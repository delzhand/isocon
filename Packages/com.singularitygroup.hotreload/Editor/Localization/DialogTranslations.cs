namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Dialogs {
            // Dialogs
            public static string DialogTitleRecompile;
            public static string DialogMessageRecompile;
            public static string DialogTitleStopPlayMode;
            public static string DialogMessageStopPlayMode;
            public static string DialogTitleRecoverPassword;
            public static string DialogMessageRecoverPassword;
            public static string DialogTitleRateApp;
            public static string DialogMessageRateApp;
            public static string DialogTitleDeactivate;
            public static string DialogMessageDeactivate;
            public static string DialogButtonDeactivate;
            public static string DialogTitleRestartServer;
            public static string DialogMessageRestartAssetRefresh;
            public static string DialogMessageRestartConsoleWindow;
            public static string DialogMessageRestartErrorReporting;
            public static string DialogButtonRestartHotReload;
            public static string DialogButtonRestartServer;
            public static string DialogButtonDontRestart;
            public static string DialogTitleInstallComponents;
            public static string DialogMessageInstallComponents;
            public static string DialogButtonInstall;
            public static string DialogButtonMoreInfo;
            public static string DialogTitleSwitchBuildTarget;
            public static string DialogMessageSwitchBuildTarget;
            public static string DialogButtonSwitchToStandalone;
            
            // About Tab Dialog Messages
            public static string DialogManageLicenseMessage;
            public static string DialogManageAccountMessage;
            public static string DialogReportIssueMessage;
            
            // Update Dialog
            public static string DialogTitleUpdateFormat;
            public static string DialogMessageUpdateFormat;
            public static string DialogButtonUpdate;
            
            // Update Server Dialog
            public static string DialogMessageRestartUpdate;
            public static string DialogMessageRestartFieldInitializer;
            public static string DialogMessageRestartExposeServer;
            public static string DialogTitleHotReload;
            
            public static void LoadEnglish() {
                // Dialogs
                DialogTitleRecompile = "Hot Reload auto-applies changes";
                DialogMessageRecompile = "Using the Recompile button is only necessary when Hot Reload fails to apply your changes. \n\nDo you wish to proceed?";
                DialogTitleStopPlayMode = "Stop Play Mode and Recompile?";
                DialogMessageStopPlayMode = "Using the Recompile button will stop Play Mode.\n\nDo you wish to proceed?";
                DialogTitleRecoverPassword = "Recover password";
                DialogMessageRecoverPassword = "Use company code 'naughtycult' and the email you signed up with in order to recover your account.";
                DialogTitleRateApp = "Rate Hot Reload";
                DialogMessageRateApp = "Thank you for using Hot Reload!\n\nPlease consider leaving a review on the Asset Store to support us.";
                DialogTitleDeactivate = "Hot Reload";
                DialogMessageDeactivate = "Hot Reload will be completely deactivated (unusable) until you activate it again.\n\nDo you want to proceed?";
                DialogButtonDeactivate = "Deactivate";
                DialogTitleRestartServer = "Hot Reload";
                DialogMessageRestartAssetRefresh = "When changing 'Asset refresh', the Hot Reload server must be restarted for this to take effect.\nDo you want to restart it now?";
                DialogMessageRestartConsoleWindow = "When changing 'Hide console window on start', the Hot Reload server must be restarted for this to take effect.\nDo you want to restart it now?";
                DialogMessageRestartErrorReporting = "When changing 'Disable Detailed Error Reporting', the Hot Reload server must be restarted for this to take effect.\nDo you want to restart it now?";
                DialogButtonRestartHotReload = "Restart Hot Reload";
                DialogButtonRestartServer = "Restart server";
                DialogButtonDontRestart = "Don't restart";
                DialogTitleInstallComponents = "Install platform specific components";
                DialogMessageInstallComponents = "For Hot Reload to work, additional components specific to your operating system have to be installed";
                DialogButtonInstall = "Install";
                DialogButtonMoreInfo = "More Info";
                DialogTitleSwitchBuildTarget = "Switch Build Target";
                DialogMessageSwitchBuildTarget = "Switching the build target can take a while depending on project size.";
                DialogButtonSwitchToStandalone = "Switch to Standalone";
                
                // About Tab Dialog Messages
                DialogManageLicenseMessage = "Upgrade/downgrade/edit your subscription and edit payment info.";
                DialogManageAccountMessage = "Login with company code 'naughtycult'. Use the email you signed up with. Your initial password was sent to you by email.";
                DialogReportIssueMessage = "Report issue in our public issue tracker. Requires gitlab.com account (if you don't have one and are not willing to make it, please contact us by other means such as our website).";
                
                // Update Dialog
                DialogTitleUpdateFormat = "Update To v{0}";
                DialogMessageUpdateFormat = "By pressing 'Update' the Hot Reload package will be updated to v{0}";
                DialogButtonUpdate = "Update";
                
                // Update Server Dialog
                DialogMessageRestartUpdate = "When updating Hot Reload, the server must be restarted for the update to take effect.\nDo you want to restart it now?";
                DialogMessageRestartFieldInitializer = "When changing 'Apply field initializer edits to existing class instances' setting, the Hot Reload server must restart for it to take effect.\nDo you want to restart it now?";
                DialogMessageRestartExposeServer = "When changing '{0}', the Hot Reload server must be restarted for this to take effect.\nDo you want to restart it now?";
                DialogTitleHotReload = "Hot Reload";
            }
            
            public static void LoadSimplifiedChinese() {
                // Dialogs
                DialogTitleRecompile = "Hot Reload 自动应用更改";
                DialogMessageRecompile = "仅当 Hot Reload 未能应用您的更改时，才需要使用“重新编译”按钮。\n\n您希望继续吗？";
                DialogTitleStopPlayMode = "停止播放模式并重新编译？";
                DialogMessageStopPlayMode = "使用“重新编译”按钮将停止播放模式。\n\n您希望继续吗？";
                DialogTitleRecoverPassword = "恢复密码";
                DialogMessageRecoverPassword = "使用公司代码 'naughtycult' 和您注册时使用的电子邮件来恢复您的帐户。";
                DialogTitleRateApp = "为 Hot Reload 评分";
                DialogMessageRateApp = "感谢您使用 Hot Reload！\n\n请考虑在 Asset Store 上留下评论以支持我们。";
                DialogTitleDeactivate = "Hot Reload";
                DialogMessageDeactivate = "Hot Reload 将被完全停用（无法使用），直到您再次激活它。\n\n您希望继续吗？";
                DialogButtonDeactivate = "停用";
                DialogTitleRestartServer = "Hot Reload";
                DialogMessageRestartAssetRefresh = "更改“资源刷新”时，必须重新启动 Hot Reload 服务器才能生效。\n您希望现在重新启动吗？";
                DialogMessageRestartConsoleWindow = "更改“启动时隐藏控制台窗口”时，必须重新启动 Hot Reload 服务器才能生效。\n您希望现在重新启动吗？";
                DialogMessageRestartErrorReporting = "更改“禁用详细错误报告”时，必须重新启动 Hot Reload 服务器才能生效。\n您希望现在重新启动吗？";
                DialogButtonRestartHotReload = "重新启动 Hot Reload";
                DialogButtonRestartServer = "重新启动服务器";
                DialogButtonDontRestart = "不重新启动";
                DialogTitleInstallComponents = "安装特定于平台的组件";
                DialogMessageInstallComponents = "为了让 Hot Reload 工作，必须安装特定于您操作系统的附加组件";
                DialogButtonInstall = "安装";
                DialogButtonMoreInfo = "更多信息";
                DialogTitleSwitchBuildTarget = "切换构建目标";
                DialogMessageSwitchBuildTarget = "切换构建目标可能需要一段时间，具体取决于项目大小。";
                DialogButtonSwitchToStandalone = "切换到独立平台";

                // About Tab Dialog Messages
                DialogManageLicenseMessage = "升级/降级/编辑您的订阅并编辑付款信息。";
                DialogManageAccountMessage = "使用公司代码 'naughtycult' 登录。使用您注册时使用的电子邮件。您的初始密码已通过电子邮件发送给您。";
                DialogReportIssueMessage = "在我们的公共问题跟踪器中报告问题。需要 gitlab.com 帐户（如果您没有并且不愿意创建，请通过其他方式与我们联系，例如我们的网站）。";

                // Update Dialog
                DialogTitleUpdateFormat = "更新到 v{0}";
                DialogMessageUpdateFormat = "按下“更新”后，Hot Reload 软件包将更新到 v{0}";
                DialogButtonUpdate = "更新";

                // Update Server Dialog
                DialogMessageRestartUpdate = "更新 Hot Reload 时，必须重新启动服务器才能使更新生效。\n您希望现在重新启动吗？";
                DialogMessageRestartFieldInitializer = "更改“将字段初始化程序编辑应用于现有类实例”设置时，必须重新启动 Hot Reload 服务器才能生效。\n您希望现在重新启动吗？";
                DialogMessageRestartExposeServer = "更改“{0}”时，必须重新启动 Hot Reload 服务器才能生效。\n您希望现在重新启动吗？";
                DialogTitleHotReload = "Hot Reload";
            }
        }
    }
}

