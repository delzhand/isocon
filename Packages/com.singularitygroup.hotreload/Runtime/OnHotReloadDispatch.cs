#pragma warning disable CS0618 // obsolete warnings (stay warning-free also in newer unity versions) 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using SingularityGroup.HotReload.Localization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SingularityGroup.HotReload {

    static class Dispatch {
        // DispatchOnHotReload is called every time a patch is applied (1x per batch of filechanges)
        public static async Task OnHotReload(List<MethodPatch> patchedMethods) {
            var methods = await Task.Run(() => GetOrFillMethodsCacheThreaded());

            foreach (var m in methods) {
                if (m.IsStatic) {
                    InvokeStaticMethod(m, nameof(InvokeOnHotReload), patchedMethods);
                } else {
                    foreach (var go in GameObject.FindObjectsOfType(m.DeclaringType)) {
                        InvokeInstanceMethod(m, go, patchedMethods);
                    }
                }
            }
        }

        public static void OnHotReloadLocal(MethodBase originalMethod, MethodBase patchMethod) {
            if (!Attribute.IsDefined(originalMethod, typeof(InvokeOnHotReloadLocal))) {
                return;
            }
            var attrib = Attribute.GetCustomAttribute(originalMethod, typeof(InvokeOnHotReloadLocal)) as InvokeOnHotReloadLocal;

            if (!string.IsNullOrEmpty(attrib?.methodToInvoke)) {
                OnHotReloadLocalCustom(originalMethod, attrib);
                return;
            }
            var patchMethodParams = patchMethod.GetParameters();
            if (patchMethodParams.Length == 0) {
                InvokeStaticMethod(patchMethod, nameof(InvokeOnHotReloadLocal), null);
            } else if (typeof(MonoBehaviour).IsAssignableFrom(patchMethodParams[0].ParameterType)) {
                foreach (var go in GameObject.FindObjectsOfType(patchMethodParams[0].ParameterType)) {
                    InvokeInstanceMethodStatic(patchMethod, go);
                }
            } else {
                Log.Warning($"[{nameof(InvokeOnHotReloadLocal)}] {patchMethod.DeclaringType?.Name} {patchMethod.Name} {Localization.Translations.Utility.MethodCallWarning}");
            }
        }

        public static void OnHotReloadLocalCustom(MethodBase origianlMethod, InvokeOnHotReloadLocal attrib) {
            var reloadForType = origianlMethod.DeclaringType;
            var reloadMethod = reloadForType?.GetMethod(attrib.methodToInvoke, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (reloadMethod == null) {
                Log.Warning($"[{nameof(InvokeOnHotReloadLocal)}] {string.Format(Localization.Translations.Utility.OnHotReloadLocalWarning, attrib.methodToInvoke)}");
                return;
            }
            if (reloadMethod.IsStatic) {
                InvokeStaticMethod(reloadMethod, nameof(InvokeOnHotReloadLocal), null);
            } else if (typeof(MonoBehaviour).IsAssignableFrom(reloadForType)) {
                foreach (var go in GameObject.FindObjectsOfType(reloadForType)) {
                    InvokeInstanceMethod(reloadMethod, go, null);
                }
            } else {
                Log.Warning($"[{nameof(InvokeOnHotReloadLocal)}] {reloadMethod.DeclaringType?.Name} {reloadMethod.Name} {Localization.Translations.Utility.MethodCallWarning}");
            }
        }

        private static List<MethodInfo> methodsCache;

        private static List<MethodInfo> GetOrFillMethodsCacheThreaded() {
            if (methodsCache != null) {
                return methodsCache;
            }

#if UNITY_2019_1_OR_NEWER && UNITY_EDITOR
            var methodCollection = UnityEditor.TypeCache.GetMethodsWithAttribute(typeof(InvokeOnHotReload));
            var methods = new List<MethodInfo>();
            foreach (var m in methodCollection) {
                methods.Add(m);
            }
#else
            var methods = GetMethodsReflection();
#endif

            methodsCache = methods;
            return methods;
        }

        private static List<MethodInfo> GetMethodsReflection() {
            var methods = new List<MethodInfo>();

            try {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (asm.FullName == "System" || asm.FullName.StartsWith("System.", StringComparison.Ordinal)) {
                        continue; // big performance optimization
                    }

                    try {
                        foreach (var type in asm.GetTypes()) {
                            try {
                                foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                                    try {
                                        if (Attribute.IsDefined(m, typeof(InvokeOnHotReload))) {
                                            methods.Add(m);
                                        }
                                    } catch (BadImageFormatException) {
                                        // silently ignore (can happen, is very annoying if it spams)
                                        /*
                                            BadImageFormatException: VAR 3 (TOutput) cannot be expanded in this context with 3 instantiations
                                            System.Reflection.MonoMethod.GetBaseMethod () (at <c8d0d7b9135640958bff528a1e374758>:0)
                                            System.MonoCustomAttrs.GetBase (System.Reflection.ICustomAttributeProvider obj) (at <c8d0d7b9135640958bff528a1e374758>:0)
                                            System.MonoCustomAttrs.IsDefined (System.Reflection.ICustomAttributeProvider obj, System.Type attributeType, System.Boolean inherit) (at <c8d0d7b9135640958bff528a1e374758>:0)
                                        */
                                    } catch (TypeLoadException) {
                                        // silently ignore (can happen, is very annoying if it spams)
                                    } catch (Exception e) {
                                        ThreadUtility.LogException(new AggregateException(type.Name + "." + m.Name, e));
                                    }
                                }
                            } catch (BadImageFormatException) {
                                // silently ignore (can happen, is very annoying if it spams)
                            } catch (TypeLoadException) {
                                // silently ignore (can happen, is very annoying if it spams)
                            } catch (Exception e) {
                                ThreadUtility.LogException(new AggregateException(type.Name, e));
                            }
                        }
                    } catch (BadImageFormatException) {
                        // silently ignore (can happen, is very annoying if it spams)
                    } catch (TypeLoadException) {
                        // silently ignore (can happen, is very annoying if it spams)
                    } catch (Exception e) {
                        ThreadUtility.LogException(new AggregateException(asm.FullName, e));
                    }
                }
            } catch (Exception e) {
                ThreadUtility.LogException(e);
            }
            return methods;
        }

        private static void InvokeStaticMethod(MethodBase m, string attrName, List<MethodPatch> patchedMethods) {
            try {
                if (patchedMethods != null && m.GetParameters().Length == 1) {
                    m.Invoke(null, new object[] { patchedMethods });
                } else {
                    m.Invoke(null, new object[] { });
                }
            } catch (Exception e) {
                if (m.GetParameters().Length != 0) {
                    Log.Warning($"[{attrName}] {m.DeclaringType?.Name} {m.Name} {Localization.Translations.Utility.OnHotReloadWarning}\n{e}");
                } else {
                    Log.Warning($"[{attrName}] {m.DeclaringType?.Name} {m.Name} failed. Exception\n{e}");
                }
            }
        }

        private static void InvokeInstanceMethod(MethodBase m, Object go, List<MethodPatch> patchedMethods) {
            try {
                if (patchedMethods != null && m.GetParameters().Length == 1) {
                    m.Invoke(go, new object[] { patchedMethods });
                } else {
                    m.Invoke(go, new object[] { });
                }
            } catch (Exception e) {
                if (m.GetParameters().Length != 0) {
                    Log.Warning($"[InvokeOnHotReload] {m.DeclaringType?.Name} {m.Name} {Localization.Translations.Utility.OnHotReloadWarning}\n{e}");
                } else {
                    Log.Warning(string.Format(Localization.Translations.Logging.InvokeOnHotReloadFailed, m.DeclaringType?.Name, m.Name, e));
                }
            }
        }
        
        private static void InvokeInstanceMethodStatic(MethodBase m, Object go) {
            try {
                m.Invoke(null, new object[] { go });
            } catch (Exception e) {
                if (m.GetParameters().Length != 0) {
                    Log.Warning($"[InvokeOnHotReloadLocal] {m.DeclaringType?.Name} {m.Name} {Localization.Translations.Utility.OnHotReloadLocalCallWarning}\n{e}");
                } else {
                    Log.Warning(Localization.Translations.Logging.InvokeOnHotReloadLocalFailed, m.DeclaringType?.Name, m.Name, e);
                }
            }
        }

    }
}
