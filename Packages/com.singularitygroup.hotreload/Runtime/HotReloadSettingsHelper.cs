using System;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SingularityGroup.HotReload {

	internal static class HotReloadSettingsHelper {
		public static UnityEngine.GameObject GetOrCreateSettingsPrefab(string prefabAssetPath) {
		    #if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabAssetPath);
            if (prefab == null) {
                // when you use HotReload as a unitypackage, prefab is somewhere inside your assets folder
                var guids = AssetDatabase.FindAssets("HotReloadPrompts t:prefab", new string[]{"Assets"});
                var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
                var promptsPrefabPath = paths.FirstOrDefault(assetpath => Path.GetFileName(assetpath) == "HotReloadPrompts.prefab");
                if (promptsPrefabPath != null) {
                    prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(promptsPrefabPath);
                }
            }
            if (prefab == null) {
                throw new Exception(Localization.Translations.Errors.FailedPromptsPrefab);
            }
            return prefab;
            #else
            return null;
			#endif
		}

		public static HotReloadSettingsObject GetSettingsObject(string editorAssetPath) {
		   #if UNITY_EDITOR
		   return AssetDatabase.LoadAssetAtPath<HotReloadSettingsObject>(editorAssetPath);
		   #else
		   return null;
		   #endif
		}
		
	}

}
