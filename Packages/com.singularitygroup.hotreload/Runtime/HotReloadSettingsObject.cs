using System;
using JetBrains.Annotations;
using System.IO;
using UnityEngine;

namespace SingularityGroup.HotReload {
    /// <summary>
    /// HotReload runtime settings. These can be changed while the app is running.
    /// </summary>
    /// <remarks>
    /// ScriptableObject that may be included in Resources/ folder.
    /// See also Editor/PrebuildIncludeResources.cs
    /// </remarks>
    [Serializable]
    class HotReloadSettingsObject : ScriptableObject {
        #region singleton
        private static HotReloadSettingsObject _I;
        public static HotReloadSettingsObject I {
            get {
                if (_I == null) {
                    _I = LoadSettingsOrDefault();
                }
                return _I;
            }
        }

        /// <summary>Create settings inside Assets/ because user cannot edit files that are included inside a Unity package</summary>
        /// <remarks>
        /// You can change this in a build script if you want it created somewhere else.
        /// </remarks>
        public static string editorAssetPath = "Assets/HotReload/Resources/HotReloadSettingsObject.asset";

        private static string resourceName => Path.GetFileNameWithoutExtension(editorAssetPath);
        
        public static bool TryLoadSettings(out HotReloadSettingsObject settings) {
            try {
                settings = LoadSettings();
                return settings != null;
            } catch(FileNotFoundException) {
                settings = null;
                return false;
            }
        }

        [NotNull]
        private static HotReloadSettingsObject LoadSettingsOrDefault() {
            var settings = LoadSettings();
            if (settings == null) {
                // load defaults
                settings = CreateInstance<HotReloadSettingsObject>();
            }
            return settings;
        }

        [CanBeNull]
        private static HotReloadSettingsObject LoadSettings() {
            HotReloadSettingsObject settings;
            if (Application.isEditor) {
                settings = HotReloadSettingsHelper.GetSettingsObject(editorAssetPath);
            } else {
                // load from Resources (assumes that build includes the resource)
                settings = Resources.Load<HotReloadSettingsObject>(resourceName);
            }
            return settings;
        }
        #endregion

        #region settings

        /// <summary>Set default values.</summary>
        /// <remarks>
        /// This is called by the Unity editor when the ScriptableObject is first created.
        /// This function is only called in editor mode.
        /// </remarks>
        private void Reset() {
            EnsurePrefabSetCorrectly();
        }

        /// <summary>
        /// Path to the prefab asset file.
        /// </summary>
        const string prefabAssetPath = "Packages/com.singularitygroup.hotreload/Runtime/HotReloadPrompts.prefab";
        
        // Call this during build, just to be sure the field is correct. (I had some issues with it while editing the prefab)
        public void EnsurePrefabSetCorrectly() {
            PromptsPrefab = HotReloadSettingsHelper.GetOrCreateSettingsPrefab(prefabAssetPath);
        }

        public void EnsurePrefabNotInBuild() {
            PromptsPrefab = null;
        }

        
        // put the stored settings here

        [Header(Localization.Translations.MenuItems.BuildSettings)]
        [Tooltip(Localization.Translations.MenuItems.IncludeInBuildTooltip)]
        public bool IncludeInBuild = true;

        [Header(Localization.Translations.MenuItems.PlayerSettings)]
        public bool AllowAndroidAppToMakeHttpRequests = false;

        #region hidden

        /// Reference to the Prefab, for loading it at runtime
        [HideInInspector]
        public GameObject PromptsPrefab;
        #endregion
        
        #endregion settings
    }
}
