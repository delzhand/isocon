using System;
using System.Diagnostics.CodeAnalysis;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Editor.Cli;
using SingularityGroup.HotReload.Editor.Localization;
using RuntimeLocalization = SingularityGroup.HotReload.Localization;
using UnityEditor;
using UnityEngine;
using EditorGUI = UnityEditor.EditorGUI;

namespace SingularityGroup.HotReload.Editor {
    internal struct HotReloadSettingsTabState {
        public readonly bool running;
        public readonly bool trialLicense;
        public readonly LoginStatusResponse loginStatus;
        public readonly bool isServerHealthy;
        public readonly bool registrationRequired;
        
        public HotReloadSettingsTabState(
            bool running,
            bool trialLicense,
            LoginStatusResponse loginStatus,
            bool isServerHealthy,
            bool registrationRequired
        ) {
            this.running = running;
            this.trialLicense = trialLicense;
            this.loginStatus = loginStatus;
            this.isServerHealthy = isServerHealthy;
            this.registrationRequired = registrationRequired;
        }
    }

    internal class HotReloadSettingsTab : HotReloadTabBase {
        private readonly HotReloadOptionsSection optionsSection;

        // cached because changing built target triggers C# domain reload
        // Also I suspect selectedBuildTargetGroup has chance to freeze Unity for several seconds (unconfirmed).
        private readonly Lazy<BuildTargetGroup> currentBuildTarget = new Lazy<BuildTargetGroup>(
            () => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

        private readonly Lazy<bool> isCurrentBuildTargetSupported = new Lazy<bool>(() => {
            var target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return HotReloadBuildHelper.IsMonoSupported(target);
        });

        // Resources.Load uses cache, so it's safe to call it every frame.
        //  Retrying Load every time fixes an issue where you import the package and constructor runs, but resources aren't loadable yet.
        private Texture iconCheck => Resources.Load<Texture>("icon_check_circle");
        private Texture iconWarning => Resources.Load<Texture>("icon_warning_circle");

        [SuppressMessage("ReSharper", "Unity.UnknownResource")] // Rider doesn't check packages
        public HotReloadSettingsTab(HotReloadWindow window) : base(window,
            Translations.Settings.SettingsTitle,
            "_Popup",
            Translations.OnDevice.OnDeviceHeadline) {
            optionsSection = new HotReloadOptionsSection();
        }

        private GUIStyle headlineStyle;
        private GUIStyle paddedStyle;
        
        private Vector2 _settingsTabScrollPos;
        
        HotReloadSettingsTabState currentState;
        public override void OnGUI() {
            // HotReloadAboutTabState ensures rendering is consistent between Layout and Repaint calls
            // Without it errors like this happen:
            // ArgumentException: Getting control 2's position in a group with only 2 controls when doing repaint
            // See thread for more context: https://answers.unity.com/questions/17718/argumentexception-getting-control-2s-position-in-a.html
            if (Event.current.type == EventType.Layout) {
                currentState = new HotReloadSettingsTabState(
                    running: EditorCodePatcher.Running,
                    trialLicense: EditorCodePatcher.Status != null && (EditorCodePatcher.Status?.isTrial == true),
                    loginStatus: EditorCodePatcher.Status,
                    isServerHealthy: ServerHealthCheck.I.IsServerHealthy,
                    registrationRequired: RedeemLicenseHelper.I.RegistrationRequired
                );
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(_settingsTabScrollPos, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(Math.Max(HotReloadWindowStyles.windowScreenHeight, 800)), GUILayout.MaxWidth(Math.Max(HotReloadWindowStyles.windowScreenWidth, 800)))) {
                _settingsTabScrollPos.x = scope.scrollPosition.x;
                _settingsTabScrollPos.y = scope.scrollPosition.y;
                using (new EditorGUILayout.VerticalScope(HotReloadWindowStyles.DynamicSectionHelpTab)) {
                    GUILayout.Space(10);
                    if (!EditorCodePatcher.LoginNotRequired
                        && !currentState.registrationRequired
                        // Delay showing login in settings to not confuse users that they need to login to use Free trial
                        && (HotReloadPrefs.RateAppShown
                            || PackageConst.IsAssetStoreBuild)
                       ) {
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBoxCompact)) {
                            using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBoxWide)) {
                                using (new EditorGUILayout.VerticalScope()) {
                                    RenderLicenseInfoSection();
                                }
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBoxCompact)) {
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBoxWide)) {
                            using (new EditorGUILayout.VerticalScope()) {
                                HotReloadPrefs.ShowConfiguration = EditorGUILayout.Foldout(HotReloadPrefs.ShowConfiguration, Translations.Settings.SettingsConfiguration, true, HotReloadWindowStyles.FoldoutStyle);
                                if (HotReloadPrefs.ShowConfiguration) {
                                    EditorGUILayout.Space();

                                    // main section
                                    RenderUnityAutoRefresh();
                                    using (new EditorGUI.DisabledScope(!EditorCodePatcher.autoRecompileUnsupportedChangesSupported)) {
                                        RenderAutoRecompileUnsupportedChanges();
                                        if (HotReloadPrefs.AutoRecompileUnsupportedChanges && EditorCodePatcher.autoRecompileUnsupportedChangesSupported) {
                                            using (new EditorGUILayout.VerticalScope(paddedStyle ?? (paddedStyle = new GUIStyle { padding = new RectOffset(20, 0, 0, 0) }))) {
                                                RenderAutoRecompileUnsupportedChangesImmediately();
                                                RenderAutoRecompileUnsupportedChangesOnExitPlayMode();
                                                RenderAutoRecompileUnsupportedChangesInPlayMode();
                                                RenderAutoRecompileInspectorFieldEdits();
                                                RenderAutoRecompilePartiallyUnsupportedChanges();
                                                RenderDisplayNewMonobehaviourMethodsAsPartiallySupported();
                                                RenderAutoRecompileUnsupportedChangesInEditMode();
                                            }
                                        }
                                        EditorGUILayout.Space();
                                    }
                                    RenderAssetRefresh();
                                    if (HotReloadPrefs.AllAssetChanges) {
                                        using (new EditorGUILayout.VerticalScope(paddedStyle ?? (paddedStyle = new GUIStyle { padding = new RectOffset(20, 0, 0, 0) }))) {
                                            RenderIncludeShaderChanges();
                                        }

                                        EditorGUILayout.Space();
                                    }
                                    RenderDebuggerCompatibility();

                                    // // fields
                                    // RenderShowFeatures();
                                    // using (new EditorGUILayout.VerticalScope(paddedStyle ?? (paddedStyle = new GUIStyle { padding = new RectOffset(20, 0, 0, 0) }))) {
                                    //     RenderShowApplyfieldInitializerEditsToExistingClassInstances();
                                    //
                                    //     EditorGUILayout.Space();
                                    // }

                                    // visual feedback
                                    if (EditorWindowHelper.supportsNotifications) {
                                        RenderShowNotifications();
                                        using (new EditorGUILayout.VerticalScope(paddedStyle ?? (paddedStyle = new GUIStyle { padding = new RectOffset(20, 0, 0, 0) }))) {
                                            RenderShowPatchingNotifications();
                                            RenderShowCompilingUnsupportedNotifications();
                                        }

                                        EditorGUILayout.Space();
                                    }

                                    // misc
                                    RenderMiscHeader();
                                    using (new EditorGUILayout.VerticalScope(paddedStyle ?? (paddedStyle = new GUIStyle { padding = new RectOffset(20, 0, 0, 0) }))) {
                                        RenderAutostart();
                                        RenderConsoleWindow();

                                        EditorGUILayout.Space();
                                    }

                                    EditorGUILayout.Space();
                                    using (new EditorGUILayout.HorizontalScope()) {
                                        GUILayout.FlexibleSpace();
                                        HotReloadWindow.RenderShowOnStartup();
                                    }
                                }
                            }
                        }
                    }

                    if (!EditorCodePatcher.LoginNotRequired && currentState.trialLicense && currentState.running) {
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBoxCompact)) {
                            using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBoxWide)) {
                                using (new EditorGUILayout.VerticalScope()) {
                                    RenderPromoCodeSection();
                                }
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBoxCompact)) {
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBoxWide)) {
                            using (new EditorGUILayout.VerticalScope()) {
                                RenderOnDevice();
                            }
                        }
                    }
                    
                    using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBoxCompact)) {
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBoxWide)) {
                            using (new EditorGUILayout.VerticalScope()) {
                                HotReloadPrefs.ShowAdvanced = EditorGUILayout.Foldout(HotReloadPrefs.ShowAdvanced, Translations.Settings.SettingsAdvanced, true, HotReloadWindowStyles.FoldoutStyle);
                                if (HotReloadPrefs.ShowAdvanced) {
                                    EditorGUILayout.Space();

                                    DeactivateHotReload();
                                    DisableDetailedErrorReporting();
                                    PauseHotReloadInEditMode();
#if UNITY_EDITOR_WIN
                                    if (PackageConst.DefaultLocale == RuntimeLocalization.Locale.English) {
                                        UseWatchman();
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }
        }

        void RenderUnityAutoRefresh() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleManageAutoRefresh), HotReloadPrefs.AllowDisableUnityAutoRefresh);
            if (newSettings != HotReloadPrefs.AllowDisableUnityAutoRefresh) {
                HotReloadPrefs.AllowDisableUnityAutoRefresh = newSettings;
            }
            string toggleDescription;
            if (HotReloadPrefs.AllowDisableUnityAutoRefresh) {
                toggleDescription = Translations.Settings.SettingsManageAutoRefreshOn;
            } else {
                toggleDescription = Translations.Settings.SettingsManageAutoRefreshOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void RenderAssetRefresh() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleAssetRefresh), HotReloadPrefs.AllAssetChanges);
            if (newSettings != HotReloadPrefs.AllAssetChanges) {
                HotReloadPrefs.AllAssetChanges = newSettings;
                // restart when setting changes
                if (ServerHealthCheck.I.IsServerHealthy) {
                    var restartServer = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleRestartServer,
                        Translations.Dialogs.DialogMessageRestartAssetRefresh,
                        Translations.Dialogs.DialogButtonRestartHotReload, Translations.Dialogs.DialogButtonDontRestart);
                    if (restartServer) {
                        EditorCodePatcher.RestartCodePatcher().Forget();
                    }
                }
            }
            string toggleDescription;
            if (HotReloadPrefs.AllAssetChanges) {
                toggleDescription = Translations.Settings.SettingsAssetRefreshOn;
            } else {
                toggleDescription = Translations.Settings.SettingsAssetRefreshOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void RenderDebuggerCompatibility() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleDebuggerCompatibility), HotReloadPrefs.AutoDisableHotReloadWithDebugger);
            if (newSettings != HotReloadPrefs.AutoDisableHotReloadWithDebugger) {
                HotReloadPrefs.AutoDisableHotReloadWithDebugger = newSettings;
                CodePatcher.I.debuggerCompatibilityEnabled = !HotReloadPrefs.AutoDisableHotReloadWithDebugger;
            }
            string toggleDescription;
            if (HotReloadPrefs.AutoDisableHotReloadWithDebugger) {
                toggleDescription = Translations.Settings.SettingsDebuggerCompatibilityOn;
            } else {
                toggleDescription = Translations.Settings.SettingsDebuggerCompatibilityOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void RenderIncludeShaderChanges() {
            HotReloadPrefs.IncludeShaderChanges = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleRefreshShaders), HotReloadPrefs.IncludeShaderChanges);
            string toggleDescription;
            if (HotReloadPrefs.IncludeShaderChanges) {
                toggleDescription = Translations.Settings.SettingsRefreshShadersOn;
            } else {
                toggleDescription = Translations.Settings.SettingsRefreshShadersOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }

        void RenderConsoleWindow() {
            if (!HotReloadCli.CanOpenInBackground) {
                return;
            }
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleHideConsole), HotReloadPrefs.DisableConsoleWindow);
            if (newSettings != HotReloadPrefs.DisableConsoleWindow) {
                HotReloadPrefs.DisableConsoleWindow = newSettings;
                // restart when setting changes
                if (ServerHealthCheck.I.IsServerHealthy) {
                    var restartServer = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleRestartServer,
                        Translations.Dialogs.DialogMessageRestartConsoleWindow,
                        Translations.Dialogs.DialogButtonRestartServer, Translations.Dialogs.DialogButtonDontRestart);
                    if (restartServer) {
                        EditorCodePatcher.RestartCodePatcher().Forget();
                    }
                }
            }
            string toggleDescription;
            if (HotReloadPrefs.DisableConsoleWindow) {
                toggleDescription = Translations.Settings.SettingsHideConsoleOn;
            } else {
                toggleDescription = Translations.Settings.SettingsHideConsoleOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void DeactivateHotReload() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleDeactivate), HotReloadPrefs.DeactivateHotReload);
            if (newSettings != HotReloadPrefs.DeactivateHotReload) {
                DeactivateHotReloadInner(newSettings);
            }
            string toggleDescription;
            if (HotReloadPrefs.DeactivateHotReload) {
                toggleDescription = Translations.Settings.SettingsDeactivatedOn;
            } else {
                toggleDescription = Translations.Settings.SettingsDeactivatedOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void DisableDetailedErrorReporting() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleDisableErrorReporting), HotReloadPrefs.DisableDetailedErrorReporting);
            DisableDetailedErrorReportingInner(newSettings);
            string toggleDescription;
            if (HotReloadPrefs.DisableDetailedErrorReporting) {
                toggleDescription = Translations.Settings.SettingsDisableErrorReportingOn;
            } else {
                toggleDescription = Translations.Settings.SettingsDisableErrorReportingOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
        void PauseHotReloadInEditMode() {
            HotReloadPrefs.PauseHotReloadInEditMode = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.TogglePauseEditMode), HotReloadPrefs.PauseHotReloadInEditMode);
            string toggleDescription;
            if (HotReloadPrefs.PauseHotReloadInEditMode) {
                toggleDescription = Translations.Settings.SettingsPauseEditModeOn;
            } else {
                toggleDescription = Translations.Settings.SettingsPauseEditModeOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
        
#if UNITY_EDITOR_WIN
        void UseWatchman() {
            HotReloadPrefs.UseWatchman = EditorGUILayout.BeginToggleGroup(new GUIContent("Use watchman"), HotReloadPrefs.UseWatchman);
            string toggleDescription;
            if (HotReloadPrefs.UseWatchman) {
                toggleDescription = "Use watchman file watcher";
            } else {
                toggleDescription = "Use default file watcher";
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space(6f);
        }
#endif

        public static void DisableDetailedErrorReportingInner(bool newSetting) {
            if (newSetting == HotReloadPrefs.DisableDetailedErrorReporting) {
                return;
            }
            HotReloadPrefs.DisableDetailedErrorReporting = newSetting;
            // restart when setting changes
            if (ServerHealthCheck.I.IsServerHealthy) {
                var restartServer = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleRestartServer,
                    Translations.Dialogs.DialogMessageRestartErrorReporting,
                    Translations.Dialogs.DialogButtonRestartServer, Translations.Dialogs.DialogButtonDontRestart);
                if (restartServer) {
                    EditorCodePatcher.RestartCodePatcher().Forget();
                }
            }
        }

        static void DeactivateHotReloadInner(bool deactivate) {
            var confirmed = !deactivate || EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleDeactivate,
                Translations.Dialogs.DialogMessageDeactivate,
                Translations.Dialogs.DialogButtonDeactivate, Translations.Common.ButtonCancel);
            if (confirmed) {
                HotReloadPrefs.DeactivateHotReload = deactivate;
                if (deactivate) {
                    EditorCodePatcher.StopCodePatcher(recompileOnDone: true).Forget();
                } else {
                    HotReloadRunTab.Recompile();
                }
            }
        }

        void RenderAutostart() {
            var newSettings = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleAutostart), HotReloadPrefs.LaunchOnEditorStart);
            if (newSettings != HotReloadPrefs.LaunchOnEditorStart) {
                HotReloadPrefs.LaunchOnEditorStart = newSettings;
            }
            string toggleDescription;
            if (HotReloadPrefs.LaunchOnEditorStart) {
                toggleDescription = Translations.Settings.SettingsAutostartOn;
            } else {
                toggleDescription = Translations.Settings.SettingsAutostartOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();
        }

        void RenderShowNotifications() {
            EditorGUILayout.Space(10f);
            GUILayout.Label(Translations.Settings.SettingsVisualFeedback, HotReloadWindowStyles.NotificationsTitleStyle);
            EditorGUILayout.Space(10f);
            
            if (!EditorWindowHelper.supportsNotifications && !UnitySettingsHelper.I.playmodeTintSupported) {
                var toggleDescription = Translations.Settings.SettingsIndicationsUnsupported;
                EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            }
        }

        // void RenderShowFields() {
        //     EditorGUILayout.Space(14f);
        //     GUILayout.Label("Fields", HotReloadWindowStyles.NotificationsTitleStyle);
        // }

        void RenderMiscHeader() {
            EditorGUILayout.Space(10f);
            GUILayout.Label(Translations.Settings.SettingsMisc, HotReloadWindowStyles.NotificationsTitleStyle);
            EditorGUILayout.Space(10f);
        }

        void RenderShowPatchingNotifications() {
            HotReloadPrefs.ShowPatchingNotifications = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.TogglePatchingIndication), HotReloadPrefs.ShowPatchingNotifications);
            string toggleDescription;
            if (!EditorWindowHelper.supportsNotifications) {
                toggleDescription = Translations.Settings.SettingsPatchingIndicationUnsupported;
            } else if (!HotReloadPrefs.ShowPatchingNotifications) {
                toggleDescription = Translations.Settings.SettingsPatchingIndicationOff;
            } else {
                toggleDescription = Translations.Settings.SettingsPatchingIndicationOn;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        // void RenderShowApplyfieldInitializerEditsToExistingClassInstances() {
        //     var newSetting = EditorGUILayout.BeginToggleGroup(new GUIContent("Apply field initializer edits to existing class instances"), HotReloadPrefs.ApplyFieldInitiailzerEditsToExistingClassInstances);
        //     ApplyApplyFieldInitializerEditsToExistingClassInstances(newSetting);
        //     string toggleDescription;
        //     if (HotReloadPrefs.ApplyFieldInitiailzerEditsToExistingClassInstances) {
        //         toggleDescription = "New field initializers with constant value will update field value of existing objects.";
        //     } else {
        //         toggleDescription = "New field initializers will not modify existing objects.";
        //     }
        //     EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
        //     EditorGUILayout.EndToggleGroup();
        // }

        [Obsolete(Translations.MenuItems.NotImplementedObsolete)]
        public static void ApplyApplyFieldInitializerEditsToExistingClassInstances(bool newSetting) {
            if (newSetting != HotReloadPrefs.ApplyFieldInitiailzerEditsToExistingClassInstances) {
                HotReloadPrefs.ApplyFieldInitiailzerEditsToExistingClassInstances = newSetting;
                // restart when setting changes
                if (ServerHealthCheck.I.IsServerHealthy) {
                    var restartServer = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleRestartServer,
                        Translations.Dialogs.DialogMessageRestartFieldInitializer,
                        Translations.Dialogs.DialogButtonRestartServer, Translations.Dialogs.DialogButtonDontRestart);
                    if (restartServer) {
                        EditorCodePatcher.RestartCodePatcher().Forget();
                    }
                }
            }
        }

        void RenderShowCompilingUnsupportedNotifications() {
            HotReloadPrefs.ShowCompilingUnsupportedNotifications = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleCompilingIndication), HotReloadPrefs.ShowCompilingUnsupportedNotifications);
            string toggleDescription;
            if (!EditorWindowHelper.supportsNotifications) {
                toggleDescription = Translations.Settings.SettingsCompilingIndicationUnsupported;
            } else if (!HotReloadPrefs.ShowCompilingUnsupportedNotifications) {
                toggleDescription = Translations.Settings.SettingsCompilingIndicationOff;
            } else {
                toggleDescription = Translations.Settings.SettingsCompilingIndicationOn;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileUnsupportedChanges() {
            HotReloadPrefs.AutoRecompileUnsupportedChanges = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleAutoRecompile), HotReloadPrefs.AutoRecompileUnsupportedChanges && EditorCodePatcher.autoRecompileUnsupportedChangesSupported);
            string toggleDescription;
            if (!EditorCodePatcher.autoRecompileUnsupportedChangesSupported) {
                toggleDescription = Translations.Settings.SettingsAutoRecompileUnsupported;
            } else if (HotReloadPrefs.AutoRecompileUnsupportedChanges) {
                toggleDescription = Translations.Settings.SettingsAutoRecompileOn;
            } else {
                toggleDescription = Translations.Settings.SettingsAutoRecompileOff;
            }
            if (!HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode && !HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode) {
                HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode = true;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileInspectorFieldEdits() {
            HotReloadPrefs.AutoRecompileInspectorFieldsEdit = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleAutoRecompileInspector), HotReloadPrefs.AutoRecompileInspectorFieldsEdit);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompileInspectorFieldsEdit) {
                toggleDescription = Translations.Settings.SettingsAutoRecompileInspectorOn;
            } else {
                toggleDescription = Translations.Settings.SettingsAutoRecompileInspectorOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompilePartiallyUnsupportedChanges() {
            HotReloadPrefs.AutoRecompilePartiallyUnsupportedChanges = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleAutoRecompilePartial), HotReloadPrefs.AutoRecompilePartiallyUnsupportedChanges);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompilePartiallyUnsupportedChanges) {
                toggleDescription = Translations.Settings.SettingsAutoRecompilePartialOn;
            } else {
                toggleDescription = Translations.Settings.SettingsAutoRecompilePartialOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderDisplayNewMonobehaviourMethodsAsPartiallySupported() {
            HotReloadPrefs.DisplayNewMonobehaviourMethodsAsPartiallySupported = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleDisplayMonobehaviour), HotReloadPrefs.DisplayNewMonobehaviourMethodsAsPartiallySupported);
            string toggleDescription;
            if (HotReloadPrefs.DisplayNewMonobehaviourMethodsAsPartiallySupported) {
                toggleDescription = Translations.Settings.SettingsDisplayMonobehaviourOn;
            } else {
                toggleDescription = Translations.Settings.SettingsDisplayMonobehaviourOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileUnsupportedChangesImmediately() {
            HotReloadPrefs.AutoRecompileUnsupportedChangesImmediately = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleRecompileImmediately), HotReloadPrefs.AutoRecompileUnsupportedChangesImmediately);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompileUnsupportedChangesImmediately) {
                toggleDescription = Translations.Settings.SettingsRecompileImmediatelyOn;
            } else {
                toggleDescription = Translations.Settings.SettingsRecompileImmediatelyOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileUnsupportedChangesInPlayMode() {
            HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleRecompilePlayMode), HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode) {
                toggleDescription = Translations.Settings.SettingsRecompilePlayModeOn;
            } else {
                toggleDescription = Translations.Settings.SettingsRecompilePlayModeOff;
            }
            if (!HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode && !HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode) {
                HotReloadPrefs.AutoRecompileUnsupportedChanges = false;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileUnsupportedChangesInEditMode() {
            HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleRecompileEditMode), HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode) {
                toggleDescription = Translations.Settings.SettingsRecompileEditModeOn;
            } else {
                toggleDescription = Translations.Settings.SettingsRecompileEditModeOff;
            }
            if (!HotReloadPrefs.AutoRecompileUnsupportedChangesInEditMode && !HotReloadPrefs.AutoRecompileUnsupportedChangesInPlayMode) {
                HotReloadPrefs.AutoRecompileUnsupportedChanges = false;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }
        
        void RenderAutoRecompileUnsupportedChangesOnExitPlayMode() {
            HotReloadPrefs.AutoRecompileUnsupportedChangesOnExitPlayMode = EditorGUILayout.BeginToggleGroup(new GUIContent(Translations.Settings.ToggleRecompileExitPlayMode), HotReloadPrefs.AutoRecompileUnsupportedChangesOnExitPlayMode);
            string toggleDescription;
            if (HotReloadPrefs.AutoRecompileUnsupportedChangesOnExitPlayMode) {
                toggleDescription = Translations.Settings.SettingsRecompileExitPlayModeOn;
            } else {
                toggleDescription = Translations.Settings.SettingsRecompileExitPlayModeOff;
            }
            EditorGUILayout.LabelField(toggleDescription, HotReloadWindowStyles.WrapStyle);
            EditorGUILayout.EndToggleGroup();
        }

        void RenderOnDevice() {
            HotReloadPrefs.ShowOnDevice = EditorGUILayout.Foldout(HotReloadPrefs.ShowOnDevice, Translations.Settings.SettingsOnDevice, true, HotReloadWindowStyles.FoldoutStyle);
            if (!HotReloadPrefs.ShowOnDevice) {
                return;
            }
            // header with explainer image
            {
                if (headlineStyle == null) {
                    // start with textArea for the background and border colors
                    headlineStyle = new GUIStyle(GUI.skin.label) {
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                    headlineStyle.normal.textColor = HotReloadWindowStyles.H2TitleStyle.normal.textColor;
            
                    // bg color
                    if (HotReloadWindowStyles.IsDarkMode) {
                        headlineStyle.normal.background = EditorTextures.DarkGray40;
                    } else {
                        headlineStyle.normal.background = EditorTextures.LightGray225;
                    }
                    // layout
                    headlineStyle.padding = new RectOffset(8, 8, 0, 0);
                    headlineStyle.margin = new RectOffset(6, 6, 6, 6);
                }
                GUILayout.Space(9f); // space between logo and headline
            
                GUILayout.Label(Translations.OnDevice.OnDeviceHeadline,
                    headlineStyle, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 1.4f));
                // image showing how Hot Reload works with a phone
                // var bannerBox = GUILayoutUtility.GetRect(flowchart.width * 0.6f, flowchart.height * 0.6f);
                // GUI.DrawTexture(bannerBox, flowchart, ScaleMode.ScaleToFit);
            }

            GUILayout.Space(16f);
            
            //ButtonToOpenBuildSettings();

            {
                GUILayout.Label(Translations.Settings.SettingsManualConnect, HotReloadWindowStyles.H3TitleStyle);
                EditorGUILayout.Space();
                
                GUILayout.BeginHorizontal();
                
                // indent all controls (this works with non-labels)
                GUILayout.Space(16f);
                GUILayout.BeginVertical();
            
                string text;
                var ip = IpHelper.GetIpAddressCached();
                if (string.IsNullOrEmpty(ip)) {
                    text = string.Format(Translations.OnDevice.OnDeviceManualConnectFormat, RequestHelper.port);
                } else {
                    text = string.Format(Translations.OnDevice.OnDeviceManualConnectWithIP, ip, RequestHelper.port);
                }
                GUILayout.Label(text, HotReloadWindowStyles.H3TitleWrapStyle);

                if (!currentState.isServerHealthy) {
                    DrawHorizontalCheck(ServerHealthCheck.I.IsServerHealthy,
                        Translations.OnDevice.OnDeviceCheckHotReloadRunning,
                        Translations.OnDevice.OnDeviceCheckHotReloadNotRunning,
                        hasFix: false);
                }
                
                if (!HotReloadPrefs.ExposeServerToLocalNetwork) {
                    var summary = string.Format(Translations.OnDevice.OnDeviceCheckEnableExposeServer, new ExposeServerOption().ShortSummary);
                    DrawHorizontalCheck(HotReloadPrefs.ExposeServerToLocalNetwork,
                        summary,
                        summary);
                }
            
                // explainer image that shows phone needs same wifi to auto connect ?
                
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(16f);
            
            // loading again is smooth, pretty sure AssetDatabase.LoadAssetAtPath is caching -Troy
            var settingsObject = HotReloadSettingsEditor.LoadSettingsOrDefault();
            var so = new SerializedObject(settingsObject);
            
            // if you build for Android now, will Hot Reload work?
            {
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Translations.Settings.SettingsBuildSettingsChecklist, HotReloadWindowStyles.H3TitleStyle);
                EditorGUI.BeginDisabledGroup(isSupported);
                // One-click to change each setting to the supported value
                if (GUILayout.Button(Translations.Common.ButtonFixAll, GUILayout.MaxWidth(90f))) {
                    FixAllUnsupportedSettings(so);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                
                
                // NOTE: After user changed some build settings, window may not immediately repaint
                // (e.g. toggle Development Build in Build Settings window)
                // We could show a refresh button (to encourage the user to click the window which makes it repaint).
                DrawSectionCheckBuildSupport(so);
            }
            

            GUILayout.Space(16f);

            // Settings checkboxes (Hot Reload options)
            {
                GUILayout.Label(Translations.Settings.SettingsOptions, HotReloadWindowStyles.H3TitleStyle);
                if (settingsObject) {
                    optionsSection.DrawGUI(so);
                }
            }
            GUILayout.FlexibleSpace(); // needed otherwise vertical scrollbar is appearing for no reason (Unity 2021 glitch perhaps)
        }
        
        private void RenderLicenseInfoSection() {
            HotReloadRunTab.RenderLicenseInfo(
                _window.RunTabState,
                currentState.loginStatus,
                verbose: true,
                allowHide: false,
                overrideActionButton:Translations.Common.ButtonActivateLicense,
                showConsumptions: true
            );
        }
        
        private void RenderPromoCodeSection() {
            _window.RunTab.RenderPromoCodes();
        }
        
        public void FocusLicenseFoldout() {
            HotReloadPrefs.ShowLogin = true;
        }

        // note: changing scripting backend does not force Unity to recreate the GUI, so need to check it when drawing.
        private ScriptingImplementation ScriptingBackend => HotReloadBuildHelper.GetCurrentScriptingBackend();
        private ManagedStrippingLevel StrippingLevel => HotReloadBuildHelper.GetCurrentStrippingLevel();
        public bool isSupported = true;

        /// <summary>
        /// These options are drawn in the On-device tab
        /// </summary>
        // new on-device options should be added here
        public static readonly IOption[] allOptions = new IOption[] {
            new ExposeServerOption(),
            IncludeInBuildOption.I,
            new AllowAndroidAppToMakeHttpRequestsOption(),
        };

        /// <summary>
        /// Change each setting to the value supported by Hot Reload
        /// </summary>
        private void FixAllUnsupportedSettings(SerializedObject so) {
            if (!isCurrentBuildTargetSupported.Value) {
                // try switch to Android platform
                // (we also support Standalone but HotReload on mobile is a better selling point)
                if (!TrySwitchToStandalone()) {
                    // skip changing other options (user won't readthe gray text) - user has to click Fix All again
                    return;
                }
            }
            
            foreach (var buildOption in allOptions) {
                if (!buildOption.GetValue(so)) {
                    buildOption.SetValue(so, true);
                }
            }
            so.ApplyModifiedProperties();
            var settingsObject = so.targetObject as HotReloadSettingsObject;
            if (settingsObject) {
                // when you click fix all, make sure to save the settings, otherwise ui does not update
                HotReloadSettingsEditor.EnsureSettingsCreated(settingsObject);
            }
            
            if (!EditorUserBuildSettings.development) {
                EditorUserBuildSettings.development = true;
            }
            
            HotReloadBuildHelper.SetCurrentScriptingBackend(ScriptingImplementation.Mono2x);
            HotReloadBuildHelper.SetCurrentStrippingLevel(ManagedStrippingLevel.Disabled);
        }

        public static bool TrySwitchToStandalone() {
            BuildTarget buildTarget;
            if (Application.platform == RuntimePlatform.LinuxEditor) {
                buildTarget = BuildTarget.StandaloneLinux64;
            } else if (Application.platform == RuntimePlatform.WindowsEditor) {
                buildTarget = BuildTarget.StandaloneWindows64;
            } else if (Application.platform == RuntimePlatform.OSXEditor) {
                buildTarget = BuildTarget.StandaloneOSX;
            } else {
                return false;
            }
            var current = EditorUserBuildSettings.activeBuildTarget;
            if (current == buildTarget) {
                return true;
            }
            var confirmed = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleSwitchBuildTarget,
                Translations.Dialogs.DialogMessageSwitchBuildTarget,
                Translations.Dialogs.DialogButtonSwitchToStandalone, Translations.Common.ButtonCancel);
            if (confirmed) {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, buildTarget);
                Log.Info(Translations.About.LogBuildTargetSwitching, buildTarget);
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Section that user can check before making a Unity Player build.
        /// </summary>
        /// <param name="so"></param>
        /// <remarks>
        /// This section is for confirming your build will work with Hot Reload.<br/>
        /// Options that can be changed after the build is made should be drawn elsewhere.
        /// </remarks>
        public void DrawSectionCheckBuildSupport(SerializedObject so) {
            isSupported = true;
            var selectedPlatform = currentBuildTarget.Value;
            DrawHorizontalCheck(isCurrentBuildTargetSupported.Value,
                string.Format(Translations.OnDevice.OnDeviceCheckPlatformSelected, selectedPlatform.ToString()),
                string.Format(Translations.OnDevice.OnDeviceCheckPlatformNotSupported, selectedPlatform.ToString()));

            using (new EditorGUI.DisabledScope(!isCurrentBuildTargetSupported.Value)) {
                foreach (var option in allOptions) {
                    DrawHorizontalCheck(option.GetValue(so),
                        string.Format(Translations.OnDevice.OnDeviceCheckEnableExposeServer, option.ShortSummary),
                        string.Format(Translations.OnDevice.OnDeviceCheckEnableExposeServer, option.ShortSummary));
                }

                DrawHorizontalCheck(EditorUserBuildSettings.development,
                    Translations.OnDevice.OnDeviceCheckDevelopmentEnabled,
                    Translations.OnDevice.OnDeviceCheckEnableDevelopment);
                
                DrawHorizontalCheck(ScriptingBackend == ScriptingImplementation.Mono2x,
                    Translations.OnDevice.OnDeviceCheckMonoBackend,
                    Translations.OnDevice.OnDeviceCheckSetMonoBackend);
                
                DrawHorizontalCheck(StrippingLevel == ManagedStrippingLevel.Disabled,
                    string.Format(Translations.OnDevice.OnDeviceCheckStrippingLevel, StrippingLevel),
                    string.Format(Translations.OnDevice.OnDeviceCheckStrippingLevel, StrippingLevel),
                    suggestedSolutionText: Translations.OnDevice.OnDeviceCheckStrippingSolution
                );
            }
        }

        /// <summary>
        /// Draw a box with a tick or warning icon on the left, with text describing the tick or warning
        /// </summary>
        /// <param name="condition">The condition to check. True to show a tick icon, False to show a warning.</param>
        /// <param name="okText">Shown when condition is true</param>
        /// <param name="notOkText">Shown when condition is false</param>
        /// <param name="suggestedSolutionText">Shown when <paramref name="condition"/> is false</param>
        void DrawHorizontalCheck(bool condition, string okText, string notOkText = null, string suggestedSolutionText = null, bool hasFix = true) {
            if (okText == null) {
                throw new ArgumentNullException(nameof(okText));
            }
            if (notOkText == null) {
                notOkText = okText;
            }

            // include some horizontal space around the icon
            var boxWidth = GUILayout.Width(EditorGUIUtility.singleLineHeight * 1.31f);
            var height = GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.01f);
            GUILayout.BeginHorizontal(HotReloadWindowStyles.BoxStyle, height, GUILayout.ExpandWidth(true));
            var style = HotReloadWindowStyles.NoPaddingMiddleLeftStyle;
            var iconRect = GUILayoutUtility.GetRect(
                Mathf.Round(EditorGUIUtility.singleLineHeight * 1.31f),
                Mathf.Round(EditorGUIUtility.singleLineHeight * 1.01f),
                style, boxWidth, height, GUILayout.ExpandWidth(false));
            // rounded so we can have pixel perfect black circle bg
            iconRect.Set(Mathf.Round(iconRect.x), Mathf.Round(iconRect.y), Mathf.CeilToInt(iconRect.width),
                Mathf.CeilToInt(iconRect.height));
            var text = condition ? okText : notOkText;
            var icon = condition ? iconCheck : iconWarning;
            if (GUI.enabled) {
                DrawBlackCircle(iconRect);
                // resource can be null when building player (Editor Resources not available)
                if (icon) {
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                }
            } else {
                // show something (instead of hiding) so that layout stays same size
                DrawDisabledCircle(iconRect);
            }
            GUILayout.Space(4f);
            GUILayout.Label(text, style, height);

            if (!condition && hasFix) {
                isSupported = false;
            }

            GUILayout.EndHorizontal();
            if (!condition && !String.IsNullOrEmpty(suggestedSolutionText)) {
                // suggest to the user how they can resolve the issue
                EditorGUI.indentLevel++;
                GUILayout.Label(suggestedSolutionText, HotReloadWindowStyles.WrapStyle);
                EditorGUI.indentLevel--;
            }
        }

        void DrawDisabledCircle(Rect rect) => DrawCircleIcon(rect,
            Resources.Load<Texture>("icon_circle_gray"),
            Color.clear); // smaller circle draws less attention

        void DrawBlackCircle(Rect rect) => DrawCircleIcon(rect,
            Resources.Load<Texture>("icon_circle_black"),
            new Color(0.14f, 0.14f, 0.14f)); // black is too dark in unity light theme

        void DrawCircleIcon(Rect rect, Texture circleIcon, Color borderColor) {
            // Note: drawing texture from resources is pixelated on the edges, so it has some transperancy around the edges.
            // While building for Android, Resources.Load returns null for our editor Resources. 
            if (circleIcon != null) {
                GUI.DrawTexture(rect, circleIcon, ScaleMode.ScaleToFit);
            }
            
            // Draw smooth circle border
            const float borderWidth = 2f;
            GUI.DrawTexture(rect, EditorTextures.White, ScaleMode.ScaleToFit, true,
                0f,
                borderColor,
                new Vector4(borderWidth, borderWidth, borderWidth, borderWidth),
                Mathf.Min(rect.height, rect.width) / 2f);
        }
    }
}
