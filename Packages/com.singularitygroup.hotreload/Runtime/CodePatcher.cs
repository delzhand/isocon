using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Localization;
using JetBrains.Annotations;
using SingularityGroup.HotReload.Burst;
using SingularityGroup.HotReload.HarmonyLib;
using SingularityGroup.HotReload.JsonConverters;
using SingularityGroup.HotReload.MonoMod.Utils;
using SingularityGroup.HotReload.Newtonsoft.Json;
using SingularityGroup.HotReload.RuntimeDependencies;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("SingularityGroup.HotReload.Editor")]

namespace SingularityGroup.HotReload {
    class RegisterPatchesResult {
        // note: doesn't include removals and method definition changes (e.g. renames)
        public readonly List<MethodPatch> patchedMethods = new List<MethodPatch>();
        public List<SField> addedFields = new List<SField>();
        public readonly List<SMethod> patchedSMethods = new List<SMethod>();
        public bool inspectorModified;
        public bool inspectorFieldAdded;
        public readonly List<Tuple<SMethod, string>> patchFailures = new List<Tuple<SMethod, string>>();
        public readonly List<string> patchExceptions = new List<string>();
    }

    class FieldHandler {
        public readonly Func<Type, FieldInfo, bool> storeField;
        public readonly Action<Type, FieldInfo, FieldInfo> registerInspectorFieldAttributes;
        public readonly Func<Type, string, bool> hideField;

        public FieldHandler(Func<Type, FieldInfo, bool> storeField, Func<Type, string, bool> hideField, Action<Type, FieldInfo, FieldInfo> registerInspectorFieldAttributes) {
            this.storeField = storeField;
            this.hideField = hideField;
            this.registerInspectorFieldAttributes = registerInspectorFieldAttributes;
        }
    }
    
    class CodePatcher {
        public static readonly CodePatcher I = new CodePatcher();
        /// <summary>Tag for use in Debug.Log.</summary>
        public const string TAG = "HotReload";
        
        internal int PatchesApplied { get; private set; }
        string PersistencePath {get;}
        
        List<MethodPatchResponse> pendingPatches;
        readonly List<MethodPatchResponse> patchHistory;
        readonly HashSet<string> seenResponses = new HashSet<string>();
        string[] assemblySearchPaths;
        SymbolResolver symbolResolver;
        readonly string tmpDir;
        public FieldHandler fieldHandler;
        public bool debuggerCompatibilityEnabled;
        
        CodePatcher() {
            pendingPatches = new List<MethodPatchResponse>();
            patchHistory = new List<MethodPatchResponse>(); 
            if(UnityHelper.IsEditor) {
                tmpDir = PackageConst.LibraryCachePath;
            } else {
                tmpDir = UnityHelper.TemporaryCachePath;
            }
            if(!UnityHelper.IsEditor) {
                PersistencePath = Path.Combine(UnityHelper.PersistentDataPath, "HotReload", "patches.json");
                try {
                    LoadPatches(PersistencePath);
                } catch(Exception ex) {
                    Log.Error($"{Localization.Translations.Logging.LoadingPatchesFromDiskError}\n{ex}");
                }
            }
#if UNITY_EDITOR
            // Unity event methods are not assigned outside the scene. 
            // So we need to ensure they are added when entering play mode from edit mode
            EditorApplication.playModeStateChanged += state => {
                if (state != PlayModeStateChange.EnteredPlayMode) {
                    return;
                }
                foreach (var unityEventMethod in unityEventMethods) {
                    EnsureUnityEventMethod(unityEventMethod);
                }
            };
#endif
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeUnityEvents() {
            UnityEventHelper.Initialize();
        }

        
        void LoadPatches(string filePath) {
            PlayerLog(Localization.Translations.Logging.LoadingPatchesFromFile, filePath);
            var file = new FileInfo(filePath);
            if(file.Exists) {
                var bytes = File.ReadAllText(filePath);
                var patches = JsonConvert.DeserializeObject<List<MethodPatchResponse>>(bytes);
                PlayerLog(Localization.Translations.Logging.LoadedPatchesFromDisk, patches.Count.ToString());
                foreach (var patch in patches) {
                    RegisterPatches(patch, persist: false);
                }
            }  
        }

        
        internal IReadOnlyList<MethodPatchResponse> PendingPatches => pendingPatches;
        internal SymbolResolver SymbolResolver => symbolResolver;
        
        
        internal string[] GetAssemblySearchPaths() {
            EnsureSymbolResolver();
            return assemblySearchPaths;
        }
       
        internal RegisterPatchesResult RegisterPatches(MethodPatchResponse patches, bool persist) {
            PlayerLog(Localization.Translations.Logging.RegisterPatches, string.Join("\n", patches.failures), string.Join("\n", patches.patches.SelectMany(p => p.modifiedMethods).Select(m => m.displayName)));
            pendingPatches.Add(patches);
            return ApplyPatches(persist);
        }
        
        RegisterPatchesResult ApplyPatches(bool persist) {
            PlayerLog(Localization.Translations.Logging.ApplyPatchesPending, pendingPatches.Count);
            EnsureSymbolResolver();

            var result = new RegisterPatchesResult();
            
            try {
                int count = 0;
                foreach(var response in pendingPatches) {
                    if (seenResponses.Contains(response.id)) {
                        continue;
                    }
                    foreach (var patch in response.patches) {
                        var asm = Assembly.Load(patch.patchAssembly, patch.patchPdb);
                        SymbolResolver.AddAssembly(asm);
                    }
                    HandleRemovedUnityMethods(response.removedMethod);
#if UNITY_EDITOR
                    HandleAlteredFields(response.id, result, response.alteredFields);
#endif
                    // needs to come before RegisterNewFieldInitializers
                    RegisterNewFieldDefinitions(response);
                    // Note: order is important here. Reshaped fields require new field initializers to be added
                    // because the old initializers must override new initilaizers for existing holders.
                    // so that the initializer is not invoked twice
                    RegisterNewFieldInitializers(response);
                    HandleReshapedFields(response);
                    RemoveOldFieldInitializers(response);
#if UNITY_EDITOR
                    RegisterInspectorFieldAttributes(result, response);
#endif

                    HandleMethodPatchResponse(response, result);
                    patchHistory.Add(response);

                    seenResponses.Add(response.id);
                    count += response.patches.Length;
                }
                if (count > 0) {
                    Dispatch.OnHotReload(result.patchedMethods).Forget();
                }
            } catch(Exception ex) {
                Log.Warning($"{Localization.Translations.Logging.ExceptionHandlingMethodPatch}\n{ex}");
            } finally {
                pendingPatches.Clear();
            }
            
            if(PersistencePath != null && persist) {
                SaveAppliedPatches(PersistencePath).Forget();
            }

            PatchesApplied++;
            return result;
        }
        
        internal void ClearPatchedMethods() {
            PatchesApplied = 0;
        }

        static bool didLog;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void WarnOnSceneLoad() {
            SceneManager.sceneLoaded += (_, __) => {
                if (didLog || !UnityEventHelper.UnityMethodsAdded()) {
                    return;
                }
                Log.Warning(Localization.Translations.Logging.SceneLoadedWithNewUnityEventMethods);
                didLog = true;
            };
        }

        static HashSet<MethodBase> unityEventMethods = new HashSet<MethodBase>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneLoad() {
            SceneManager.sceneLoaded += (_, __) => {
                foreach (var unityEventMethod in unityEventMethods) {
                    EnsureUnityEventMethod(unityEventMethod);
                }
            };
        }

        static bool EnsureUnityEventMethod(MethodBase newMethod) {
            try {
                return UnityEventHelper.EnsureUnityEventMethod(newMethod);
            } catch(Exception ex) {
                Log.Warning(Localization.Translations.Logging.ExceptionEnsureUnityEventMethod, ex.GetType().Name, ex.Message);
                return false;
            }
        }

        void HandleMethodPatchResponse(MethodPatchResponse response, RegisterPatchesResult result) {
            EnsureSymbolResolver();

            foreach(var patch in response.patches) {
                try {
                    foreach(var sMethod in patch.newMethods) {
                        var newMethod = SymbolResolver.Resolve(sMethod);

                        var isUnityEvent = EnsureUnityEventMethod(newMethod);
                        if (isUnityEvent) {
                            unityEventMethods.Add(newMethod);
                        } 
                        
                        MethodUtils.DisableVisibilityChecks(newMethod);
                        if (!patch.patchMethods.Any(m => m.metadataToken == sMethod.metadataToken)) {
                            result.patchedMethods.Add(new MethodPatch(null, null, newMethod));
                            result.patchedSMethods.Add(sMethod);
                            previousPatchMethods[newMethod] = newMethod;
                            newMethods.Add(newMethod);
                        }
                    }
                    
                    for (int i = 0; i < patch.modifiedMethods.Length; i++) {
                        var sOriginalMethod = patch.modifiedMethods[i];
                        var sPatchMethod = patch.patchMethods[i];
                        var err = PatchMethod(response.id, sOriginalMethod: sOriginalMethod, sPatchMethod: sPatchMethod, containsBurstJobs: patch.unityJobs.Length > 0, patchesResult: result);
                        if (!string.IsNullOrEmpty(err)) {
                            result.patchFailures.Add(Tuple.Create(sOriginalMethod, err));
                        }
                    }
                    foreach (var job in patch.unityJobs) {
                        var type = SymbolResolver.Resolve(new SType(patch.assemblyName, job.jobKind.ToString(), job.metadataToken));
                        JobHotReloadUtility.HotReloadBurstCompiledJobs(job, type);
                    }
#if UNITY_EDITOR
                    HandleNewFields(patch.patchId, result, patch.newFields);
#endif
                } catch (Exception ex) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.Exception), new EditorExtraData {
                        { StatKey.PatchId, patch.patchId },
                        { StatKey.Detailed_Exception, ex.ToString() },
                    }).Forget();
                    result.patchExceptions.Add($"{Localization.Translations.Logging.ExceptionApplyingPatch}\nException: {ex}");
                }
            }
        }
        
        void HandleRemovedUnityMethods(SMethod[] removedMethods) {
            if (removedMethods == null) {
                return;
            }
            foreach(var sMethod in removedMethods) {
                try {
                    var oldMethod = SymbolResolver.Resolve(sMethod);
                    UnityEventHelper.RemoveUnityEventMethod(oldMethod);
                    unityEventMethods.Remove(oldMethod);
                } catch (SymbolResolvingFailedException) {
                    // ignore, not a unity event method if can't resolve
                } catch(Exception ex) {
                    Log.Warning(Localization.Translations.Logging.ExceptionRemoveUnityEventMethod, ex.GetType().Name, ex.Message);
                }
            }
        }
        
        // Important: must come before applying any patches
        void RegisterNewFieldInitializers(MethodPatchResponse resp) {
            for (var i = 0; i < resp.addedFieldInitializerFields.Length; i++) {
                var sField = resp.addedFieldInitializerFields[i];
                var sMethod = resp.addedFieldInitializerInitializers[i];
                try {
                    var declaringType = SymbolResolver.Resolve(sField.declaringType);
                    var method = SymbolResolver.Resolve(sMethod);
                    if (!(method is MethodInfo initializer)) {
                        Log.Warning(string.Format(Localization.Translations.Logging.FailedRegisteringInitializerInvalidMethod, sField.fieldName, sField.declaringType.typeName));
                        continue;
                    }
                    // We infer if the field is static by the number of parameters the method has
                    // because sField is old field
                    var isStatic = initializer.GetParameters().Length == 0;
                    MethodUtils.DisableVisibilityChecks(initializer);
                    // Initializer return type is used in place of fieldType because latter might be point to old field if the type changed
                    FieldInitializerRegister.RegisterInitializer(declaringType, sField.fieldName, initializer.ReturnType, initializer, isStatic);
                    
                } catch (Exception e) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.RegisterFieldInitializer), new EditorExtraData {
                        { StatKey.PatchId, resp.id },
                        { StatKey.Detailed_Exception, e.ToString() },
                    }).Forget();
                    Log.Warning(string.Format(Localization.Translations.Logging.FailedRegisteringInitializerException, sField.fieldName, sField.declaringType.typeName, e.Message));
                }
            }
        }
        
        void RegisterNewFieldDefinitions(MethodPatchResponse resp) {
            foreach (var sField in resp.newFieldDefinitions) {
                try {
                    var declaringType = SymbolResolver.Resolve(sField.declaringType);
                    var fieldType = SymbolResolver.Resolve(sField).FieldType;
                    FieldResolver.RegisterFieldType(declaringType, sField.fieldName, fieldType);
                } catch (Exception e) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.RegisterFieldDefinition), new EditorExtraData {
                        { StatKey.PatchId, resp.id },
                        { StatKey.Detailed_Exception, e.ToString() },
                    }).Forget();
                    Log.Warning(string.Format(Localization.Translations.Logging.FailedRegisteringNewFieldDefinitions, sField.fieldName, sField.declaringType.typeName, e.Message));
                }
            }
        }
            
        // Important: must come before applying any patches
        // Note: server might decide not to report removed field initializer at all if it can handle it
        void RemoveOldFieldInitializers(MethodPatchResponse resp) {
            foreach (var sField in resp.removedFieldInitializers) {
                try {
                    var declaringType = SymbolResolver.Resolve(sField.declaringType);
                    var fieldType = SymbolResolver.Resolve(sField.declaringType);
                    FieldInitializerRegister.UnregisterInitializer(declaringType, sField.fieldName, fieldType, sField.isStatic);
                } catch (Exception e) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.UnregisterFieldInitializer), new EditorExtraData {
                        { StatKey.PatchId, resp.id },
                        { StatKey.Detailed_Exception, e.ToString() },
                    }).Forget();
                    Log.Warning(string.Format(Localization.Translations.Logging.FailedRemovingInitializer, sField.fieldName, sField.declaringType.typeName, e.Message));
                }
            }
        }
        
        // Important: must come before applying any patches
        // Should also come after RegisterNewFieldInitializers so that new initializers are not invoked for existing objects
        internal void HandleReshapedFields(MethodPatchResponse resp) {
            foreach(var patch in resp.patches) {
                var removedReshapedFields = patch.deletedFields;
                var renamedReshapedFieldsFrom = patch.renamedFieldsFrom;
                var renamedReshapedFieldsTo = patch.renamedFieldsTo;
                
                foreach (var f in removedReshapedFields) {
                    try {
                        var declaringType = SymbolResolver.Resolve(f.declaringType);
                        var fieldType = SymbolResolver.Resolve(f).FieldType;
                        FieldResolver.ClearHolders(declaringType, f.isStatic, f.fieldName, fieldType);
                    } catch (Exception e) {
                        RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.ClearHolders), new EditorExtraData {
                            { StatKey.PatchId, resp.id },
                            { StatKey.Detailed_Exception, e.ToString() },
                        }).Forget();
                        Log.Warning(string.Format(Localization.Translations.Logging.FailedRemovingFieldValue, f.fieldName, f.declaringType.typeName, e.Message));
                    }
                }
                for (var i = 0; i < renamedReshapedFieldsFrom.Length; i++) {
                    var fromField = renamedReshapedFieldsFrom[i];
                    var toField = renamedReshapedFieldsTo[i];
                    try {
                        var declaringType = SymbolResolver.Resolve(fromField.declaringType);
                        var fieldType = SymbolResolver.Resolve(fromField).FieldType;
                        var toFieldType = SymbolResolver.Resolve(toField).FieldType;
                        if (!AreSTypesCompatible(fromField.declaringType, toField.declaringType)
                            || fieldType != toFieldType
                            || fromField.isStatic != toField.isStatic
                        ) {
                            FieldResolver.ClearHolders(declaringType, fromField.isStatic, fromField.fieldName, fieldType);
                            continue;
                        }
                        FieldResolver.MoveHolders(declaringType, fromField.fieldName, toField.fieldName, fieldType, fromField.isStatic);
                    } catch (Exception e) {
                        RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.MoveHolders), new EditorExtraData {
                            { StatKey.PatchId, resp.id },
                            { StatKey.Detailed_Exception, e.ToString() },
                        }).Forget();
                        Log.Warning(Localization.Translations.Logging.FailedMovingFieldValue, fromField, toField, toField.declaringType.typeName, e.Message);
                    }
                }
            }
        }

        internal bool AreSTypesCompatible(SType one, SType two) {
            if (one.isGenericParameter != two.isGenericParameter) {
                return false;
            }
            if (one.metadataToken != two.metadataToken) {
                return false;
            }
            if (one.assemblyName != two.assemblyName) {
                return false;
            }
            if (one.genericParameterPosition != two.genericParameterPosition) {
                return false;
            }
            if (one.typeName != two.typeName) {
                return false;
            }
            return true;
        }

#if UNITY_EDITOR
        internal void RegisterInspectorFieldAttributes(RegisterPatchesResult result, MethodPatchResponse resp) {
            foreach (var patch in resp.patches) {
                var propertyAttributesFieldOriginal = patch.propertyAttributesFieldOriginal ?? Array.Empty<SField>();
                var propertyAttributesFieldUpdated = patch.propertyAttributesFieldUpdated ?? Array.Empty<SField>();
                for (var i = 0; i < propertyAttributesFieldOriginal.Length; i++) {
                    var original = propertyAttributesFieldOriginal[i];
                    var updated = propertyAttributesFieldUpdated[i];
                    try {
                        var declaringType = SymbolResolver.Resolve(original.declaringType);
                        var originalField = SymbolResolver.Resolve(original);
                        var updatedField = SymbolResolver.Resolve(updated);
                        fieldHandler?.registerInspectorFieldAttributes?.Invoke(declaringType, originalField, updatedField);
                        result.inspectorModified = true;
                    } catch (Exception e) {
                        RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.MoveHolders), new EditorExtraData {
                            { StatKey.PatchId, resp.id },
                            { StatKey.Detailed_Exception, e.ToString() },
                        }).Forget();
                        Log.Warning(string.Format(Localization.Translations.Logging.FailedUpdatingFieldAttributes, original.fieldName, original.declaringType.typeName, e.Message));
                    }
                }
            }
        }
        
        internal void HandleNewFields(string patchId, RegisterPatchesResult result, SField[] sFields) {
            foreach (var sField in sFields) {
                if (!sField.serializable) {
                    continue;
                }
                try {
                    var declaringType = SymbolResolver.Resolve(sField.declaringType);
                    var field = SymbolResolver.Resolve(sField);
                    result.inspectorFieldAdded = fieldHandler?.storeField?.Invoke(declaringType, field) ?? false;
                    result.inspectorModified = true;
                } catch (Exception e) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.AddInspectorField), new EditorExtraData {
                        { StatKey.PatchId, patchId },
                        { StatKey.Detailed_Exception, e.ToString() },
                    }).Forget();
                    Log.Warning(string.Format(Localization.Translations.Logging.FailedAddingFieldToInspector, sField.fieldName, sField.declaringType.typeName, e.Message));
                }
            }
            result.addedFields.AddRange(sFields);
        }
        
        // IMPORTANT: must come before HandleNewFields. Might contain new fields which we don't want to hide
        internal void HandleAlteredFields(string patchId, RegisterPatchesResult result, SField[] alteredFields) {
            if (alteredFields == null) {
                return;
            }
            bool alteredFieldHidden = false;
            foreach(var sField in alteredFields) {
                try {
                    var declaringType = SymbolResolver.Resolve(sField.declaringType);
                    if (fieldHandler?.hideField?.Invoke(declaringType, sField.fieldName) == true) {
                        alteredFieldHidden = true;
                    }
                } catch(Exception e) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.HideInspectorField), new EditorExtraData {
                        { StatKey.PatchId, patchId },
                        { StatKey.Detailed_Exception, e.ToString() },
                    }).Forget();
                    Log.Warning(string.Format(Localization.Translations.Logging.FailedHidingFieldFromInspector, sField.fieldName, sField.declaringType.typeName, e.Message));
                }
            }
            if (alteredFieldHidden) {
                result.inspectorModified = true;
            }
        }
#endif

        Dictionary<MethodBase, MethodBase> previousPatchMethods = new Dictionary<MethodBase, MethodBase>();
        public IEnumerable<MethodBase> OriginalPatchMethods => previousPatchMethods.Keys;
        List<MethodBase> newMethods = new List<MethodBase>();

        string PatchMethod(string patchId, SMethod sOriginalMethod, SMethod sPatchMethod, bool containsBurstJobs, RegisterPatchesResult patchesResult) {
            try {
                var patchMethod = SymbolResolver.Resolve(sPatchMethod);
                var start = DateTime.UtcNow;
                var state = TryResolveMethod(sOriginalMethod, patchMethod);
                if (Debugger.IsAttached && !debuggerCompatibilityEnabled) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.DebuggerAttached), new EditorExtraData {
                        { StatKey.PatchId, patchId },
                    }).Forget();
                    return Localization.Translations.Logging.DebuggerAttachedNotAllowed;
                }

                if (DateTime.UtcNow - start > TimeSpan.FromMilliseconds(500)) {
                    Log.Info(Localization.Translations.Logging.HotReloadApplyTook, (DateTime.UtcNow - start).TotalMilliseconds);
                }

                if(state.match == null) {
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.MethodMismatch), new EditorExtraData {
                        { StatKey.PatchId, patchId },
                    }).Forget();
                    return string.Format(Localization.Translations.Logging.MethodMismatch, sOriginalMethod.simpleName, patchMethod.Name);
                }

                PlayerLog(Localization.Translations.Logging.DetourMethod, sOriginalMethod.metadataToken, patchMethod.Name, state.offset);
                DetourResult result;
                DetourApi.DetourMethod(state.match, patchMethod, out result);
                if (result.success) {
                    // previous method is either original method or the last patch method
                    MethodBase previousMethod;
                    if (!previousPatchMethods.TryGetValue(state.match, out previousMethod)) {
                        previousMethod = state.match;
                    }
                    MethodBase originalMethod = state.match;
                    if (newMethods.Contains(state.match)) {
                        // for function added at runtime the original method should be null
                        originalMethod = null;
                    }
                    patchesResult.patchedMethods.Add(new MethodPatch(originalMethod, previousMethod, patchMethod));
                    patchesResult.patchedSMethods.Add(sOriginalMethod);
                    previousPatchMethods[state.match] = patchMethod;
                    try {
                        Dispatch.OnHotReloadLocal(state.match, patchMethod);
                    } catch {
                        // best effort
                    }
                    return null;
                } else {
                    if(result.exception is InvalidProgramException && containsBurstJobs) {
                        //ignore. The method is likely burst compiled and can't be patched
                        return null;
                    } else {
                        RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.Failure), new EditorExtraData {
                            { StatKey.PatchId, patchId },
                            { StatKey.Detailed_Exception, result.exception.ToString() },
                        }).Forget();
                        return HandleMethodPatchFailure(sOriginalMethod, result.exception);
                    }
                }
            } catch(Exception ex) {
                RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Error, StatFeature.Patching, StatEventType.Exception), new EditorExtraData {
                    { StatKey.PatchId, patchId },
                    { StatKey.Detailed_Exception, ex.ToString() },
                }).Forget();
                return HandleMethodPatchFailure(sOriginalMethod, ex);
            }
        }
        
        struct ResolveMethodState {
            public readonly SMethod originalMethod;
            public readonly int offset;
            public readonly bool tryLowerTokens;
            public readonly bool tryHigherTokens;
            public readonly MethodBase match;
            public ResolveMethodState(SMethod originalMethod, int offset, bool tryLowerTokens, bool tryHigherTokens, MethodBase match) {
                this.originalMethod = originalMethod;
                this.offset = offset;
                this.tryLowerTokens = tryLowerTokens;
                this.tryHigherTokens = tryHigherTokens;
                this.match = match;
            }

            public ResolveMethodState With(bool? tryLowerTokens = null, bool? tryHigherTokens = null, MethodBase match = null, int? offset = null) {
                return new ResolveMethodState(
                    originalMethod, 
                    offset ?? this.offset, 
                    tryLowerTokens ?? this.tryLowerTokens,
                    tryHigherTokens ?? this.tryHigherTokens,
                    match ?? this.match);
            }
        }
        
        struct ResolveMethodResult {
            public readonly MethodBase resolvedMethod;
            public readonly bool tokenOutOfRange;
            public ResolveMethodResult(MethodBase resolvedMethod, bool tokenOutOfRange) {
                this.resolvedMethod = resolvedMethod;
                this.tokenOutOfRange = tokenOutOfRange;
            }
        }
        
        ResolveMethodState TryResolveMethod(SMethod originalMethod, MethodBase patchMethod) {
            var state = new ResolveMethodState(originalMethod, offset: 0, tryLowerTokens: true, tryHigherTokens: true, match: null);
            var result = TryResolveMethodCore(state.originalMethod, patchMethod, 0);
            if(result.resolvedMethod != null) {
                return state.With(match: result.resolvedMethod);
            }
            state = state.With(offset: 1);
            const int tries = 100000;
            while(state.offset <= tries && (state.tryHigherTokens || state.tryLowerTokens)) {
                if(state.tryHigherTokens) {
                    result = TryResolveMethodCore(originalMethod, patchMethod, state.offset);
                    if(result.resolvedMethod != null) {
                        return state.With(match: result.resolvedMethod);
                    } else if(result.tokenOutOfRange) {
                        state = state.With(tryHigherTokens: false);
                    }
                }
                if(state.tryLowerTokens) {
                    result = TryResolveMethodCore(originalMethod, patchMethod, -state.offset);
                    if(result.resolvedMethod != null) {
                        return state.With(match: result.resolvedMethod);
                    } else if(result.tokenOutOfRange) {
                        state = state.With(tryLowerTokens: false);
                    }
                }
                state = state.With(offset: state.offset + 1);
            }
            return state;
        }
        
        
        ResolveMethodResult TryResolveMethodCore(SMethod methodToResolve, MethodBase patchMethod, int offset) {
            bool tokenOutOfRange = false;
            MethodBase resolvedMethod = null;
            try {
                resolvedMethod = TryGetMethodBaseWithRelativeToken(methodToResolve, offset);
                var err = MethodCompatiblity.CheckCompatibility(resolvedMethod, patchMethod);
                if(err != null) {
                    // if (resolvedMethod.Name == patchMethod.Name) {
                    //     Log.Info(err);
                    // }
                    resolvedMethod = null;
                }
            } catch (SymbolResolvingFailedException ex) when(ex.InnerException is ArgumentOutOfRangeException) {
                tokenOutOfRange = true;
            } catch (ArgumentOutOfRangeException) {
                tokenOutOfRange = true;
            }
            return new ResolveMethodResult(resolvedMethod, tokenOutOfRange);
        }
        
        MethodBase TryGetMethodBaseWithRelativeToken(SMethod sOriginalMethod, int offset) {
            return symbolResolver.Resolve(new SMethod(sOriginalMethod.assemblyName, 
                sOriginalMethod.displayName, 
                sOriginalMethod.metadataToken + offset,
                sOriginalMethod.simpleName));
        }
    
        string HandleMethodPatchFailure(SMethod method, Exception exception) {
            return string.Format(Localization.Translations.Logging.FailedToApplyPatchForMethod, method.displayName, method.assemblyName, exception);
        }

        void EnsureSymbolResolver() {
            if (symbolResolver == null) {
                var searchPaths = new HashSet<string>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assembliesByName = new Dictionary<string, List<Assembly>>();
                for (var i = 0; i < assemblies.Length; i++) {
                    var name = assemblies[i].GetNameSafe();
                    List<Assembly> list;
                    if (!assembliesByName.TryGetValue(name, out list)) {
                        assembliesByName.Add(name, list = new List<Assembly>());
                    }
                    list.Add(assemblies[i]);
                    
                    if(assemblies[i].IsDynamic) continue;

                    var location = assemblies[i].Location;
                    if(File.Exists(location)) {
                        searchPaths.Add(Path.GetDirectoryName(Path.GetFullPath(location)));
                    }
                }
                symbolResolver = new SymbolResolver(assembliesByName);
                assemblySearchPaths = searchPaths.ToArray();
            }
        }
        
        
        //Allow one save operation at a time.
        readonly SemaphoreSlim gate = new SemaphoreSlim(1);
        public async Task SaveAppliedPatches(string filePath) {
            await gate.WaitAsync();
            try {
                await SaveAppliedPatchesNoLock(filePath);
            } finally {
                gate.Release();
            }
        }
        
        async Task SaveAppliedPatchesNoLock(string filePath) {
            if (filePath == null) {
                throw new ArgumentNullException(nameof(filePath));
            }
            filePath = Path.GetFullPath(filePath);
            var dir = Path.GetDirectoryName(filePath);
            if(string.IsNullOrEmpty(dir)) {
                throw new ArgumentException(string.Format(Localization.Translations.Logging.InvalidPath, filePath), nameof(filePath));
            }
            Directory.CreateDirectory(dir);
            var history = patchHistory.ToList();
            
            PlayerLog(Localization.Translations.Logging.SavingAppliedPatches, history.Count, filePath);

            await Task.Run(() => {
                using (FileStream fs = File.Create(filePath))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter writer = new JsonTextWriter(sw)) {
                    JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings {
                        Converters = new List<JsonConverter> { new MethodPatchResponsesConverter() }
                    });
                    serializer.Serialize(writer, history);
                }
            });
        }
        
        public void InitPatchesBlocked(string filePath) {
            seenResponses.Clear();
            var file = new FileInfo(filePath);
            if (file.Exists) {
                using(var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonReader reader = new JsonTextReader(sr)) {
                    JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings {
                        Converters = new List<JsonConverter> { new MethodPatchResponsesConverter() }
                    });
                    pendingPatches = serializer.Deserialize<List<MethodPatchResponse>>(reader);
                }
                ApplyPatches(persist: false);
            }
        }
        
        
        [StringFormatMethod("format")]
        static void PlayerLog(string format, params object[] args) {
#if !UNITY_EDITOR
            HotReload.Log.Info(format, args);
#endif //!UNITY_EDITOR
        }
        
        class SimpleMethodComparer : IEqualityComparer<SMethod> {
            public static readonly SimpleMethodComparer I = new SimpleMethodComparer();
            SimpleMethodComparer() { }
            public bool Equals(SMethod x, SMethod y) => x.metadataToken == y.metadataToken;
            public int GetHashCode(SMethod x) {
                return x.metadataToken;
            }
        }
    }
}
