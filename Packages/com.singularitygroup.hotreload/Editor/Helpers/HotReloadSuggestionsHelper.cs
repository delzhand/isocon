using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Editor.Localization;
using SingularityGroup.HotReload.Localization;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Translations = SingularityGroup.HotReload.Editor.Localization.Translations;

namespace SingularityGroup.HotReload.Editor {

	internal static class HotReloadSuggestionsHelper {
        internal static void SetSuggestionsShown(HotReloadSuggestionKind hotReloadSuggestionKind) {
            if (EditorPrefs.GetBool($"HotReloadWindow.SuggestionsShown.{hotReloadSuggestionKind}")) {
                return;
            }
            EditorPrefs.SetBool($"HotReloadWindow.SuggestionsActive.{hotReloadSuggestionKind}", true);
            EditorPrefs.SetBool($"HotReloadWindow.SuggestionsShown.{hotReloadSuggestionKind}", true);
            AlertEntry entry;
            if (suggestionMap.TryGetValue(hotReloadSuggestionKind, out entry) && !HotReloadTimelineHelper.Suggestions.Contains(entry)) {
                HotReloadTimelineHelper.Suggestions.Insert(0, entry);
                HotReloadState.ShowingRedDot = true;
            }
        }
        
        internal static bool CheckSuggestionActive(HotReloadSuggestionKind hotReloadSuggestionKind) {
            return EditorPrefs.GetBool($"HotReloadWindow.SuggestionsActive.{hotReloadSuggestionKind}");
        }
        
        internal static bool CheckSuggestionShown(HotReloadSuggestionKind hotReloadSuggestionKind) {
            return EditorPrefs.GetBool($"HotReloadWindow.SuggestionsShown.{hotReloadSuggestionKind}");
        }

        internal static bool CanShowServerSuggestion(HotReloadSuggestionKind hotReloadSuggestionKind) {
            if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerWithSideEffects) {
                return !HotReloadState.ShowedFieldInitializerWithSideEffects;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited) {
                return !HotReloadState.ShowedFieldInitializerExistingInstancesEdited;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerExistingInstancesUnedited) {
                return !HotReloadState.ShowedFieldInitializerExistingInstancesUnedited;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.AddMonobehaviourMethod) {
                return !HotReloadState.ShowedAddMonobehaviourMethods;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.DetailedErrorReportingIsEnabled) {
                return !CheckSuggestionShown(HotReloadSuggestionKind.DetailedErrorReportingIsEnabled);
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.UTF8EncodingRequired) {
                return true;
            }
            return false;
        }
        
        internal static void SetServerSuggestionShown(HotReloadSuggestionKind hotReloadSuggestionKind) {
            if (hotReloadSuggestionKind == HotReloadSuggestionKind.DetailedErrorReportingIsEnabled) {
                HotReloadSuggestionsHelper.SetSuggestionsShown(hotReloadSuggestionKind);
                return;
            } 
            if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerWithSideEffects) {
                HotReloadState.ShowedFieldInitializerWithSideEffects = true;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited) {
                HotReloadState.ShowedFieldInitializerExistingInstancesEdited = true;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.FieldInitializerExistingInstancesUnedited) {
                HotReloadState.ShowedFieldInitializerExistingInstancesUnedited = true;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.AddMonobehaviourMethod) {
                HotReloadState.ShowedAddMonobehaviourMethods = true;
            } else if (hotReloadSuggestionKind == HotReloadSuggestionKind.UTF8EncodingRequired) {
                // Allow showing it multiple times
            } else {
                return;
            }
            HotReloadSuggestionsHelper.SetSuggestionActive(hotReloadSuggestionKind);
        }
        
        // used for cases where suggestion might need to be shown more than once
        internal static void SetSuggestionActive(HotReloadSuggestionKind hotReloadSuggestionKind) {
            if (EditorPrefs.GetBool($"HotReloadWindow.SuggestionsShown.{hotReloadSuggestionKind}")) {
                return;
            }
            EditorPrefs.SetBool($"HotReloadWindow.SuggestionsActive.{hotReloadSuggestionKind}", true);
            
            AlertEntry entry;
            if (suggestionMap.TryGetValue(hotReloadSuggestionKind, out entry) && !HotReloadTimelineHelper.Suggestions.Contains(entry)) {
                HotReloadTimelineHelper.Suggestions.Insert(0, entry);
                HotReloadState.ShowingRedDot = true;
            }
        }
        
        internal static void SetSuggestionInactive(HotReloadSuggestionKind hotReloadSuggestionKind) {
            EditorPrefs.SetBool($"HotReloadWindow.SuggestionsActive.{hotReloadSuggestionKind}", false);
            AlertEntry entry;
            if (suggestionMap.TryGetValue(hotReloadSuggestionKind, out entry)) {
                HotReloadTimelineHelper.Suggestions.Remove(entry);
            }
        }
        
        internal static void InitSuggestions() {
            foreach (HotReloadSuggestionKind value in Enum.GetValues(typeof(HotReloadSuggestionKind))) {
                if (!CheckSuggestionActive(value)) {
                    continue;
                }
                AlertEntry entry;
                if (suggestionMap.TryGetValue(value, out entry) && !HotReloadTimelineHelper.Suggestions.Contains(entry)) {
                    HotReloadTimelineHelper.Suggestions.Insert(0, entry);
                }
            }
        }
        
        internal static HotReloadSuggestionKind? FindSuggestionKind(AlertEntry targetEntry) {
            foreach (KeyValuePair<HotReloadSuggestionKind, AlertEntry> pair in suggestionMap) {
                if (pair.Value.Equals(targetEntry)) {
                    return pair.Key;
                }
            }
            return null;
        }
        
        internal static readonly OpenURLButton recompileTroubleshootingButton = new OpenURLButton(Translations.Suggestions.ButtonDocs, Constants.RecompileTroubleshootingURL);
        internal static readonly OpenURLButton featuresDocumentationButton = new OpenURLButton(Translations.Suggestions.ButtonDocs, Constants.FeaturesDocumentationURL);
        internal static readonly OpenURLButton multipleEditorsDocumentationButton = new OpenURLButton(Translations.Suggestions.ButtonDocs, Constants.MultipleEditorsURL);
        internal static readonly OpenURLButton debuggerDocumentationButton = new OpenURLButton(Translations.Suggestions.ButtonMoreInfo, Constants.DebuggerURL);
        public static Dictionary<HotReloadSuggestionKind, AlertEntry> suggestionMap = new Dictionary<HotReloadSuggestionKind, AlertEntry> {
            { HotReloadSuggestionKind.UnityBestDevelopmentToolAward2023, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.Award2023Title, 
                Translations.Suggestions.Award2023Message,
                actionData: () => {
                    GUILayout.Space(6f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonVote)) {
                            Application.OpenURL(Constants.VoteForAwardURL);
                            SetSuggestionInactive(HotReloadSuggestionKind.UnityBestDevelopmentToolAward2023);
                        }
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout
            )},
            { HotReloadSuggestionKind.UnsupportedChanges, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.UnsupportedChangesTitle, 
                Translations.Suggestions.UnsupportedChangesMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        featuresDocumentationButton.OnGUI();
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout
            )},
            { HotReloadSuggestionKind.UnsupportedPackages, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.UnsupportedPackagesTitle,
                Translations.Suggestions.UnsupportedPackagesMessage,
                iconType: AlertType.UnsupportedChange,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        HotReloadAboutTab.contactButton.OnGUI();
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout
            )},
            { HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.AutoRecompiledPlaymodeTitle,
                Translations.Suggestions.AutoRecompiledPlaymodeMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        recompileTroubleshootingButton.OnGUI();
                        GUILayout.Space(5f);
                        HotReloadAboutTab.discordButton.OnGUI();
                        GUILayout.Space(5f);
                        HotReloadAboutTab.contactButton.OnGUI();
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout
            )},
#if UNITY_2022_1_OR_NEWER
            { HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.AutoRecompiled2022Title,
                Translations.Suggestions.AutoRecompiled2022Message,
                iconType: AlertType.UnsupportedChange,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonUseBuildTimeOnlyAtlas)) {
                            if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) {
                                EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2Build;
                            } else {
                                EditorSettings.spritePackerMode = SpritePackerMode.BuildTimeOnlyAtlas;
                            }
                        }
                        if (GUILayout.Button(Translations.Suggestions.ButtonOpenSettings)) {
                            SettingsService.OpenProjectSettings("Project/Editor");
                        }
                        if (GUILayout.Button(Translations.Suggestions.ButtonIgnoreSuggestion)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022);
                        }
                        
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                hasExitButton: false
            )},
#endif
            { HotReloadSuggestionKind.MultidimensionalArrays, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.MultidimensionalArraysTitle,
                Translations.Suggestions.MultidimensionalArraysMessage,
                iconType: AlertType.UnsupportedChange,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonLearnMore)) {
                            string url;
                            if (PackageConst.DefaultLocaleField == Locale.SimplifiedChinese) {
                                url = "https://learn.microsoft.com/zh-cn/dotnet/fundamentals/code-analysis/quality-rules/ca1814";
                            } else {
                                url = "https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1814";
                            }
                            Application.OpenURL(url);
                        }
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout
            )},
            { HotReloadSuggestionKind.EditorsWithoutHRRunning, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.EditorsWithoutHRTitle,
                Translations.Suggestions.EditorsWithoutHRMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonStopHotReload)) {
                            EditorCodePatcher.StopCodePatcher().Forget();
                        }
                        GUILayout.Space(5f);
                        
                        multipleEditorsDocumentationButton.OnGUI();
                        GUILayout.Space(5f);
                        
                        if (GUILayout.Button(Translations.Suggestions.ButtonDontShowAgain)) {
                            HotReloadSuggestionsHelper.SetSuggestionsShown(HotReloadSuggestionKind.EditorsWithoutHRRunning);
                            HotReloadSuggestionsHelper.SetSuggestionInactive(HotReloadSuggestionKind.EditorsWithoutHRRunning);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.UnsupportedChange
            )},
            // Not in use (never reported from the server)
            { HotReloadSuggestionKind.FieldInitializerWithSideEffects, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.FieldInitializerSideEffectsTitle,
                Translations.Suggestions.FieldInitializerSideEffectsMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonOK)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.FieldInitializerWithSideEffects);
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Translations.Suggestions.ButtonDontShowAgain)) {
                            SetSuggestionsShown(HotReloadSuggestionKind.FieldInitializerWithSideEffects);
                            SetSuggestionInactive(HotReloadSuggestionKind.FieldInitializerWithSideEffects);
                        }
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
            { HotReloadSuggestionKind.DetailedErrorReportingIsEnabled, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.DetailedErrorReportingTitle,
                Translations.Suggestions.DetailedErrorReportingMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.Space(4f);
                        if (GUILayout.Button(Translations.Suggestions.ButtonOKPadded)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.DetailedErrorReportingIsEnabled);
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Translations.Suggestions.ButtonDisable)) {
                            HotReloadSettingsTab.DisableDetailedErrorReportingInner(true);
                            SetSuggestionInactive(HotReloadSuggestionKind.DetailedErrorReportingIsEnabled);
                        }
                        GUILayout.Space(10f);
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
            // Not in use (never reported from the server)
            { HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.FieldInitializerEditedTitle,
                Translations.Suggestions.FieldInitializerEditedMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonTurnOff)) {
                            #pragma warning disable CS0618
                            HotReloadSettingsTab.ApplyApplyFieldInitializerEditsToExistingClassInstances(false);
                            #pragma warning restore CS0618
                            SetSuggestionInactive(HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited);
                        }
                        if (GUILayout.Button(Translations.Suggestions.ButtonOpenSettings)) {
                            HotReloadWindow.Current.SelectTab(typeof(HotReloadSettingsTab));
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Translations.Suggestions.ButtonDontShowAgain)) {
                            SetSuggestionsShown(HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited);
                            SetSuggestionInactive(HotReloadSuggestionKind.FieldInitializerExistingInstancesEdited);
                        }
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
            { HotReloadSuggestionKind.FieldInitializerExistingInstancesUnedited, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.FieldInitializerUneditedTitle,
                Translations.Suggestions.FieldInitializerUneditedMessage,
                actionData: () => {
                    GUILayout.Space(8f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonOK)) {
                            SetSuggestionsShown(HotReloadSuggestionKind.FieldInitializerExistingInstancesUnedited);
                            SetSuggestionInactive(HotReloadSuggestionKind.FieldInitializerExistingInstancesUnedited);
                        }
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
            { HotReloadSuggestionKind.AddMonobehaviourMethod, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.AddMonobehaviourMethodTitle,
                Translations.Suggestions.AddMonobehaviourMethodMessage,
                actionData: () => {
                    GUILayout.Space(8f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonOK)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.AddMonobehaviourMethod);
                        }
                        if (GUILayout.Button(Translations.Suggestions.ButtonAutoRecompile)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.AddMonobehaviourMethod);
                            HotReloadPrefs.AutoRecompilePartiallyUnsupportedChanges = true;
                            HotReloadPrefs.DisplayNewMonobehaviourMethodsAsPartiallySupported = true;
                            HotReloadRunTab.RecompileWithChecks();
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Translations.Suggestions.ButtonDontShowAgain)) {
                            SetSuggestionsShown(HotReloadSuggestionKind.AddMonobehaviourMethod);
                            SetSuggestionInactive(HotReloadSuggestionKind.AddMonobehaviourMethod);
                        }
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
#if UNITY_2020_1_OR_NEWER
            { HotReloadSuggestionKind.SwitchToDebugModeForInlinedMethods, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.SwitchToDebugModeTitle,
                Translations.Suggestions.SwitchToDebugModeMessage,
                actionData: () => {
                    GUILayout.Space(10f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonSwitchToDebugMode) && HotReloadRunTab.ConfirmExitPlaymode(Translations.Suggestions.SwitchToDebugModeConfirmation)) {
                            HotReloadRunTab.SwitchToDebugMode();
                        }
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.UnsupportedChange
            )},
#endif
            { HotReloadSuggestionKind.HotReloadWhileDebuggerIsAttached, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.DebuggerAttachedTitle,
                Translations.Suggestions.DebuggerAttachedMessage,
                actionData: () => {
                    GUILayout.Space(8f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonKeepEnabledDuringDebugging)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.HotReloadWhileDebuggerIsAttached);
                            HotReloadPrefs.AutoDisableHotReloadWithDebugger = false;
                        }
                        GUILayout.FlexibleSpace();
                        debuggerDocumentationButton.OnGUI();
                        if (GUILayout.Button(Translations.Suggestions.ButtonDontShowAgain)) {
                            SetSuggestionsShown(HotReloadSuggestionKind.HotReloadWhileDebuggerIsAttached);
                            SetSuggestionInactive(HotReloadSuggestionKind.HotReloadWhileDebuggerIsAttached);
                        }
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.Suggestion
            )},
            { HotReloadSuggestionKind.HotReloadedMethodsWhenDebuggerIsAttached, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.DebuggerMethodsTitle,
                Translations.Suggestions.DebuggerMethodsMessage,
                actionData: () => {
                    GUILayout.Space(8f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonRecompile)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.HotReloadedMethodsWhenDebuggerIsAttached);
                            if (HotReloadRunTab.ConfirmExitPlaymode(Translations.Suggestions.DebuggerMethodsConfirmation)) {
                                HotReloadRunTab.Recompile();
                            }
                        }
                        GUILayout.FlexibleSpace();
                        debuggerDocumentationButton.OnGUI();
                        GUILayout.Space(8f);
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.UnsupportedChange,
                hasExitButton: false
            )},
            { HotReloadSuggestionKind.UTF8EncodingRequired, new AlertEntry(
                AlertType.Suggestion, 
                Translations.Suggestions.UTF8EncodingRequiredTitle,
                Translations.Suggestions.UTF8EncodingRequiredMessage,
                actionData: () => {
                    GUILayout.Space(8f);
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button(Translations.Suggestions.ButtonOK)) {
                            SetSuggestionInactive(HotReloadSuggestionKind.UTF8EncodingRequired);
                        }
                        GUILayout.FlexibleSpace();
                    }
                },
                timestamp: DateTime.Now,
                entryType: EntryType.Foldout,
                iconType: AlertType.UnsupportedChange,
                hasExitButton: false
            )},
        };
        
        static ListRequest listRequest;
        static string[] unsupportedPackages = new[] {
            "com.unity.entities",
            "com.firstgeargames.fishnet",
        };
        static List<string> unsupportedPackagesList;
        static DateTime lastPlaymodeChange;
        
        public static void Init() {
            listRequest = Client.List(offlineMode: false, includeIndirectDependencies: true);

            EditorApplication.playModeStateChanged += state => {
                lastPlaymodeChange = DateTime.UtcNow;
            };
            CompilationPipeline.compilationStarted += obj => {
                if (DateTime.UtcNow - lastPlaymodeChange < TimeSpan.FromSeconds(1) && !HotReloadState.RecompiledUnsupportedChangesOnExitPlaymode) {
                    
#if UNITY_2022_1_OR_NEWER
                    SetSuggestionsShown(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022);
#else
                    SetSuggestionsShown(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges);
#endif
                }
                HotReloadState.RecompiledUnsupportedChangesOnExitPlaymode = false;
            };
            InitSuggestions();
        }

        private static DateTime lastCheckedUnityInstances = DateTime.UtcNow;
        public static void Check() {
            if (listRequest.IsCompleted && 
                unsupportedPackagesList == null) 
            {
                unsupportedPackagesList = new List<string>();
                if (listRequest.Result != null) {
                    foreach (var packageInfo in listRequest.Result) {
                        if (unsupportedPackages.Contains(packageInfo.name)) {
                            unsupportedPackagesList.Add(packageInfo.name);
                        }
                    }
                }
                if (unsupportedPackagesList.Count > 0) {
                    SetSuggestionsShown(HotReloadSuggestionKind.UnsupportedPackages);
                }
            }
            
            CheckEditorsWithoutHR();

#if UNITY_2022_1_OR_NEWER
            if (EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas || EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) {
                SetSuggestionsShown(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022);
            } else if (CheckSuggestionActive(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022)) { 
                SetSuggestionInactive(HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022);
                EditorPrefs.SetBool($"HotReloadWindow.SuggestionsShown.{HotReloadSuggestionKind.AutoRecompiledWhenPlaymodeStateChanges2022}", false);
            }
#endif
        }
        
        private static void CheckEditorsWithoutHR() {
            if (!ServerHealthCheck.I.IsServerHealthy) {
                HotReloadSuggestionsHelper.SetSuggestionInactive(HotReloadSuggestionKind.EditorsWithoutHRRunning);
                return;
            }
            if (checkingEditorsWihtoutHR || 
                (DateTime.UtcNow - lastCheckedUnityInstances).TotalSeconds < 5)
            {
                return;
            }
            CheckEditorsWithoutHRAsync().Forget();
        }

        static bool checkingEditorsWihtoutHR;
        private static async Task CheckEditorsWithoutHRAsync() {
            try {
                checkingEditorsWihtoutHR = true;
                var editorsWithoutHr = await RequestHelper.RequestEditorsWithoutHRRunning();
                if (editorsWithoutHr == null) {
                    return;
                }
                var showSuggestion = editorsWithoutHr.editorsWithoutHRRunning;
                if (!showSuggestion) {
                    HotReloadSuggestionsHelper.SetSuggestionInactive(HotReloadSuggestionKind.EditorsWithoutHRRunning);
                    return;
                }
                if (!HotReloadState.ShowedEditorsWithoutHR && ServerHealthCheck.I.IsServerHealthy) {
                    HotReloadSuggestionsHelper.SetSuggestionActive(HotReloadSuggestionKind.EditorsWithoutHRRunning);
                    HotReloadState.ShowedEditorsWithoutHR = true;
                }
            } finally {
                checkingEditorsWihtoutHR = false;
                lastCheckedUnityInstances = DateTime.UtcNow;
            }
        }
	}
}
