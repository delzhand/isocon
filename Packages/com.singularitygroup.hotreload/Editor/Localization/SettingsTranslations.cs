namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Settings {
            // Settings Tab
            public static string SettingsTitle;
            public static string SettingsConfiguration;
            public static string SettingsAdvanced;
            public static string SettingsOnDevice;
            public static string SettingsManualConnect;
            public static string SettingsBuildSettingsChecklist;
            public static string SettingsOptions;
            public static string SettingsVisualFeedback;
            public static string SettingsMisc;
            
            // Settings Descriptions
            public static string SettingsManageAutoRefreshOn;
            public static string SettingsManageAutoRefreshOff;
            public static string SettingsAssetRefreshOn;
            public static string SettingsAssetRefreshOff;
            public static string SettingsDebuggerCompatibilityOn;
            public static string SettingsDebuggerCompatibilityOff;
            public static string SettingsRefreshShadersOn;
            public static string SettingsRefreshShadersOff;
            public static string SettingsHideConsoleOn;
            public static string SettingsHideConsoleOff;
            public static string SettingsDeactivatedOn;
            public static string SettingsDeactivatedOff;
            public static string SettingsDisableErrorReportingOn;
            public static string SettingsDisableErrorReportingOff;
            public static string SettingsPauseEditModeOn;
            public static string SettingsPauseEditModeOff;
            public static string SettingsAutostartOn;
            public static string SettingsAutostartOff;
            public static string SettingsPatchingIndicationUnsupported;
            public static string SettingsPatchingIndicationOff;
            public static string SettingsPatchingIndicationOn;
            public static string SettingsCompilingIndicationUnsupported;
            public static string SettingsCompilingIndicationOff;
            public static string SettingsCompilingIndicationOn;
            public static string SettingsAutoRecompileUnsupported;
            public static string SettingsAutoRecompileOn;
            public static string SettingsAutoRecompileOff;
            public static string SettingsAutoRecompileInspectorOn;
            public static string SettingsAutoRecompileInspectorOff;
            public static string SettingsAutoRecompilePartialOn;
            public static string SettingsAutoRecompilePartialOff;
            public static string SettingsDisplayMonobehaviourOn;
            public static string SettingsDisplayMonobehaviourOff;
            public static string SettingsRecompileImmediatelyOn;
            public static string SettingsRecompileImmediatelyOff;
            public static string SettingsRecompilePlayModeOn;
            public static string SettingsRecompilePlayModeOff;
            public static string SettingsRecompileEditModeOn;
            public static string SettingsRecompileEditModeOff;
            public static string SettingsRecompileExitPlayModeOn;
            public static string SettingsRecompileExitPlayModeOff;
            public static string SettingsIndicationsUnsupported;
            
            // Settings Toggle Names
            public static string ToggleManageAutoRefresh;
            public static string ToggleAssetRefresh;
            public static string ToggleDebuggerCompatibility;
            public static string ToggleRefreshShaders;
            public static string ToggleHideConsole;
            public static string ToggleDeactivate;
            public static string ToggleDisableErrorReporting;
            public static string TogglePauseEditMode;
            public static string ToggleAutostart;
            public static string TogglePatchingIndication;
            public static string ToggleCompilingIndication;
            public static string ToggleAutoRecompile;
            public static string ToggleAutoRecompileInspector;
            public static string ToggleAutoRecompilePartial;
            public static string ToggleDisplayMonobehaviour;
            public static string ToggleRecompileImmediately;
            public static string ToggleRecompilePlayMode;
            public static string ToggleRecompileEditMode;
            public static string ToggleRecompileExitPlayMode;
            
            // Settings Options
            public static string OptionExposeServerShort;
            public static string OptionExposeServerFull;
            public static string OptionExposeServerDescriptionEnabled;
            public static string OptionExposeServerDescriptionDisabled;
            public static string OptionAllowHttpRequests;
            public static string OptionAllowHttpRequestsDescription;
            public static string OptionIncludeInBuild;
            public static string OptionIncludeInBuildDescriptionEnabled;
            public static string OptionIncludeInBuildDescriptionDisabled;
            public static string OptionIncludeInBuildDescriptionSuffix;
            
            public static void LoadEnglish() {
                // Settings Tab
                SettingsTitle = "Settings";
                SettingsConfiguration = "Settings";
                SettingsAdvanced = "Advanced";
                SettingsOnDevice = "On-Device";
                SettingsManualConnect = "Manual connect";
                SettingsBuildSettingsChecklist = "Build Settings Checklist";
                SettingsOptions = "Options";
                SettingsVisualFeedback = "Visual Feedback";
                SettingsMisc = "Misc";
                
                // Settings Descriptions
                SettingsManageAutoRefreshOn = "To avoid unnecessary recompiling, Hot Reload will automatically change Unity's Auto Refresh and Script Compilation settings. Previous settings will be restored when Hot Reload is stopped";
                SettingsManageAutoRefreshOff = "Enabled this setting to auto-manage Unity's Auto Refresh and Script Compilation settings. This reduces unncessary recompiling";
                SettingsAssetRefreshOn = "Hot Reload will refresh changed assets such as sprites, prefabs, etc";
                SettingsAssetRefreshOff = "Enable to allow Hot Reload to refresh changed assets in the project. All asset types are supported including sprites, prefabs, shaders etc";
                SettingsDebuggerCompatibilityOn = "Hot Reload automatically disables itself while a debugger is attached, as it can otherwise interfere with certain debugger features. Please read the documentation if you consider disabling this setting.";
                SettingsDebuggerCompatibilityOff = "When a debugger is attached, Hot Reload will be active, but certain debugger features might not work as expected. Please read our documentation to learn about the limitations.";
                SettingsRefreshShadersOn = "Hot Reload will auto refresh shaders. Note that enabling this setting might impact performance.";
                SettingsRefreshShadersOff = "Enable to auto-refresh shaders. Note that enabling this setting might impact performance";
                SettingsHideConsoleOn = "Hot Reload will start without creating a console window. Logs can be accessed through \"Help\" tab.";
                SettingsHideConsoleOff = "Enable to start Hot Reload without creating a console window.";
                SettingsDeactivatedOn = "Hot Reload is deactivated.";
                SettingsDeactivatedOff = "Enable to deactivate Hot Reload.";
                SettingsDisableErrorReportingOn = "Detailed error reporting is disabled.";
                SettingsDisableErrorReportingOff = "Toggle on to disable detailed error reporting.";
                SettingsPauseEditModeOn = "Hot Reload is paused in Edit mode. It is recommended to perform a full Unity recompilation manually before entering Play Mode to prevent Hot Reload becoming unusable.";
                SettingsPauseEditModeOff = "Toggle on to pause Hot Reload while in Edit mode. With this setting enabled, it is recommended to perform a full Unity recompilation manually before entering Play Mode to prevent Hot Reload becoming unusable.";
                SettingsAutostartOn = "Hot Reload will be launched when Unity project opens.";
                SettingsAutostartOff = "Enable to launch Hot Reload when Unity project opens.";
                SettingsPatchingIndicationUnsupported = "Patching Notification is not supported in the Unity version you use.";
                SettingsPatchingIndicationOff = "Enable to show GameView and SceneView indications when Patching.";
                SettingsPatchingIndicationOn = "Indications will be shown in GameView and SceneView when Patching.";
                SettingsCompilingIndicationUnsupported = "Compiling Unsupported Changes Notification is not supported in the Unity version you use.";
                SettingsCompilingIndicationOff = "Enable to show GameView and SceneView indications when compiling unsupported changes.";
                SettingsCompilingIndicationOn = "Indications will be shown in GameView and SceneView when compiling unsupported changes.";
                SettingsAutoRecompileUnsupported = "Auto recompiling unsupported changes is not supported in the Unity version you use.";
                SettingsAutoRecompileOn = "Hot Reload will recompile automatically after code changes that Hot Reload doesn't support.";
                SettingsAutoRecompileOff = "When enabled, recompile happens automatically after code changes that Hot Reload doesn't support.";
                SettingsAutoRecompileInspectorOn = "Hot Reload will trigger recompilation for inspector field changes that are not supported in Edit mode.";
                SettingsAutoRecompileInspectorOff = "Enable to trigger recompilation for inspector field changes that are not supported in Edit mode.";
                SettingsAutoRecompilePartialOn = "Hot Reload will recompile partially supported changes.";
                SettingsAutoRecompilePartialOff = "Enable to recompile partially supported changes.";
                SettingsDisplayMonobehaviourOn = "Hot Reload will display new monobehaviour methods as partially supported.";
                SettingsDisplayMonobehaviourOff = "Enable to display new monobehaviour methods as partially supported.";
                SettingsRecompileImmediatelyOn = "Unsupported changes will be recompiled immediately.";
                SettingsRecompileImmediatelyOff = "Unsupported changes will be recompiled when editor is focused. Enable to recompile immediately.";
                SettingsRecompilePlayModeOn = "Hot Reload will exit Play Mode to recompile unsupported changes.";
                SettingsRecompilePlayModeOff = "Enable to auto exit Play Mode to recompile unsupported changes.";
                SettingsRecompileEditModeOn = "Hot Reload recompile unsupported changes when in Edit Mode.";
                SettingsRecompileEditModeOff = "Enable to auto recompile unsupported changes in Edit Mode.";
                SettingsRecompileExitPlayModeOn = "Hot Reload will recompile unsupported changes when exiting Play Mode.";
                SettingsRecompileExitPlayModeOff = "Enable to recompile unsupported changes when exiting Play Mode.";
                SettingsIndicationsUnsupported = "Indications are not supported in the Unity version you use.";
                
                // Settings Toggle Names
                ToggleManageAutoRefresh = "Manage Unity auto-refresh (recommended)";
                ToggleAssetRefresh = "Asset refresh (recommended)";
                ToggleDebuggerCompatibility = "Auto-disable Hot Reload while a debugger is attached (recommended)";
                ToggleRefreshShaders = "Refresh shaders";
                ToggleHideConsole = "Hide console window on start";
                ToggleDeactivate = "Deactivate Hot Reload";
                ToggleDisableErrorReporting = "Disable Detailed Error Reporting";
                TogglePauseEditMode = "Pause Hot Reload in Edit Mode";
                ToggleAutostart = "Autostart on Unity open";
                TogglePatchingIndication = "Patching Indication";
                ToggleCompilingIndication = "Compiling Unsupported Changes Indication";
                ToggleAutoRecompile = "Auto recompile unsupported changes (recommended)";
                ToggleAutoRecompileInspector = "Auto recompile inspector field edits";
                ToggleAutoRecompilePartial = "Include partially supported changes";
                ToggleDisplayMonobehaviour = "Display new Monobehaviour methods as partially supported";
                ToggleRecompileImmediately = "Recompile immediately";
                ToggleRecompilePlayMode = "Recompile in Play Mode";
                ToggleRecompileEditMode = "Recompile in Edit Mode";
                ToggleRecompileExitPlayMode = "Recompile on exit Play Mode";
                
                // Settings Options
                OptionExposeServerShort = "Allow Devices to Connect";
                OptionExposeServerFull = "Allow Devices to Connect (WiFi)";
                OptionExposeServerDescriptionEnabled = "The HotReload server is reachable from devices on the same Wifi network";
                OptionExposeServerDescriptionDisabled = "The HotReload server is available to your computer only. Other devices cannot connect to it.";
                OptionAllowHttpRequests = "Allow app to make HTTP requests";
                OptionAllowHttpRequestsDescription = "For Hot Reload to work on-device, please allow HTTP requests";
                OptionIncludeInBuild = "Include Hot Reload in player builds";
                OptionIncludeInBuildDescriptionEnabled = "The Hot Reload runtime is included in development builds that use the Mono scripting backend.";
                OptionIncludeInBuildDescriptionDisabled = "The Hot Reload runtime will not be included in any build. Use this option to disable HotReload without removing it from your project.";
                OptionIncludeInBuildDescriptionSuffix = " This option does not affect Hot Reload usage in Playmode";
            }
            
            public static void LoadSimplifiedChinese() {
                // Settings Tab
                SettingsTitle = "设置";
                SettingsConfiguration = "配置";
                SettingsAdvanced = "高级";
                SettingsOnDevice = "在设备上";
                SettingsManualConnect = "手动连接";
                SettingsBuildSettingsChecklist = "构建设置清单";
                SettingsOptions = "选项";
                SettingsVisualFeedback = "视觉反馈";
                SettingsMisc = "杂项";

                // Settings Descriptions
                SettingsManageAutoRefreshOn = "为避免不必要的重新编译，Hot Reload 将自动更改 Unity 的自动刷新和脚本编译设置。停止 Hot Reload 后将恢复以前的设置";
                SettingsManageAutoRefreshOff = "启用此设置以自动管理 Unity 的自动刷新和脚本编译设置。这可以减少不必要的重新编译";
                SettingsAssetRefreshOn = "Hot Reload 将刷新已更改的资产，如精灵、预制件等";
                SettingsAssetRefreshOff = "启用以允许 Hot Reload 刷新项目中已更改的资产。支持所有资产类型，包括精灵、预制件、着色器等";
                SettingsDebuggerCompatibilityOn = "附加调试器时，Hot Reload 会自动禁用自身，因为它可能会干扰某些调试器功能。如果您考虑禁用此设置，请阅读文档。";
                SettingsDebuggerCompatibilityOff = "附加调试器时，Hot Reload 将处于活动状态，但某些调试器功能可能无法按预期工作。请阅读我们的文档以了解限制。";
                SettingsRefreshShadersOn = "Hot Reload 将自动刷新着色器。请注意，启用此设置可能会影响性能。";
                SettingsRefreshShadersOff = "启用以自动刷新着色器。请注意，启用此设置可能会影响性能";
                SettingsHideConsoleOn = "Hot Reload 启动时不会创建控制台窗口。可以通过“帮助”选项卡访问日志。";
                SettingsHideConsoleOff = "启用以在不创建控制台窗口的情况下启动 Hot Reload。";
                SettingsDeactivatedOn = "Hot Reload 已停用。";
                SettingsDeactivatedOff = "启用以停用 Hot Reload。";
                SettingsDisableErrorReportingOn = "详细错误报告已禁用。";
                SettingsDisableErrorReportingOff = "切换以禁用详细错误报告。";
                SettingsPauseEditModeOn = "在编辑模式下，Hot Reload 已暂停。建议在进入播放模式之前手动执行完整的 Unity 重新编译，以防止 Hot Reload 无法使用。";
                SettingsPauseEditModeOff = "切换以在编辑模式下暂停 Hot Reload。启用此设置后，建议在进入播放模式之前手动执行完整的 Unity 重新编译，以防止 Hot Reload 无法使用。";
                SettingsAutostartOn = "当 Unity 项目打开时，将启动 Hot Reload。";
                SettingsAutostartOff = "启用以在 Unity 项目打开时启动 Hot Reload。";
                SettingsPatchingIndicationUnsupported = "您使用的 Unity 版本不支持修补通知。";
                SettingsPatchingIndicationOff = "启用以在修补时显示 GameView 和 SceneView 指示。";
                SettingsPatchingIndicationOn = "修补时将在 GameView 和 SceneView 中显示指示。";
                SettingsCompilingIndicationUnsupported = "您使用的 Unity 版本不支持编译不支持的更改通知。";
                SettingsCompilingIndicationOff = "启用以在编译不支持的更改时显示 GameView 和 SceneView 指示。";
                SettingsCompilingIndicationOn = "编译不支持的更改时将在 GameView 和 SceneView 中显示指示。";
                SettingsAutoRecompileUnsupported = "您使用的 Unity 版本不支持自动重新编译不支持的更改。";
                SettingsAutoRecompileOn = "在 Hot Reload 不支持的代码更改后，Hot Reload 将自动重新编译。";
                SettingsAutoRecompileOff = "启用后，在 Hot Reload 不支持的代码更改后会自动进行重新编译。";
                SettingsAutoRecompileInspectorOn = "对于在编辑模式下不支持的检查器字段更改，Hot Reload 将触发重新编译。";
                SettingsAutoRecompileInspectorOff = "启用以在编辑模式下为不支持的检查器字段更改触发重新编译。";
                SettingsAutoRecompilePartialOn = "Hot Reload 将重新编译部分不支持的更改。";
                SettingsAutoRecompilePartialOff = "启用以重新编译部分不支持的更改。";
                SettingsDisplayMonobehaviourOn = "Hot Reload 将把新的 monobehaviour 方法显示为部分不支持。";
                SettingsDisplayMonobehaviourOff = "启用以将新的 monobehaviour 方法显示为部分不支持。";
                SettingsRecompileImmediatelyOn = "不支持的更改将立即重新编译。";
                SettingsRecompileImmediatelyOff = "当编辑器获得焦点时，将重新编译不支持的更改。启用以立即重新编译。";
                SettingsRecompilePlayModeOn = "Hot Reload 将退出播放模式以重新编译不支持的更改。";
                SettingsRecompilePlayModeOff = "启用以自动退出播放模式以重新编译不支持的更改。";
                SettingsRecompileEditModeOn = "Hot Reload 在编辑模式下重新编译不支持的更改。";
                SettingsRecompileEditModeOff = "启用以在编辑模式下自动重新编译不支持的更改。";
                SettingsRecompileExitPlayModeOn = "退出播放模式时，Hot Reload 将重新编译不支持的更改。";
                SettingsRecompileExitPlayModeOff = "启用以在退出播放模式时重新编译不支持的更改。";
                SettingsIndicationsUnsupported = "您使用的 Unity 版本不支持指示。";

                // Settings Toggle Names
                ToggleManageAutoRefresh = "管理 Unity 自动刷新（推荐）";
                ToggleAssetRefresh = "资产刷新（推荐）";
                ToggleDebuggerCompatibility = "附加调试器时自动禁用 Hot Reload（推荐）";
                ToggleRefreshShaders = "刷新着色器";
                ToggleHideConsole = "启动时隐藏控制台窗口";
                ToggleDeactivate = "停用 Hot Reload";
                ToggleDisableErrorReporting = "禁用详细错误报告";
                TogglePauseEditMode = "在编辑模式下暂停 Hot Reload";
                ToggleAutostart = "在 Unity 打开时自动启动";
                TogglePatchingIndication = "修补指示";
                ToggleCompilingIndication = "编译不支持的更改指示";
                ToggleAutoRecompile = "自动重新编译不支持的更改（推荐）";
                ToggleAutoRecompileInspector = "自动重新编译检查器字段编辑";
                ToggleAutoRecompilePartial = "包括部分不支持的更改";
                ToggleDisplayMonobehaviour = "将新的 Monobehaviour 方法显示为部分支持";
                ToggleRecompileImmediately = "立即重新编译";
                ToggleRecompilePlayMode = "在播放模式下重新编译";
                ToggleRecompileEditMode = "在编辑模式下重新编译";
                ToggleRecompileExitPlayMode = "退出播放模式时重新编译";

                // Settings Options
                OptionExposeServerShort = "允许设备连接";
                OptionExposeServerFull = "允许设备连接 (WiFi)";
                OptionExposeServerDescriptionEnabled = "HotReload 服务器可从同一 Wifi 网络上的设备访问";
                OptionExposeServerDescriptionDisabled = "HotReload 服务器仅对您的计算机可用。其他设备无法连接到它。";
                OptionAllowHttpRequests = "允许应用发出 HTTP 请求";
                OptionAllowHttpRequestsDescription = "为了让 Hot Reload 在设备上工作，请允许 HTTP 请求";
                OptionIncludeInBuild = "在播放器构建中包含 Hot Reload";
                OptionIncludeInBuildDescriptionEnabled = "Hot Reload 运行时包含在使用 Mono 脚本后端的开发构建中。";
                OptionIncludeInBuildDescriptionDisabled = "Hot Reload 运行时将不包含在任何构建中。使用此选项可在不从项目中删除 HotReload 的情况下禁用它。";
                OptionIncludeInBuildDescriptionSuffix = " 此选项不影响在播放模式下使用 Hot Reload";
            }
        }
    }
}

