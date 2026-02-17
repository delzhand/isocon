namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Miscellaneous {
            // Overlay/Toolbar
            public static string OverlayTooltipRecompile;
            
            // Notifications
            public static string NotificationPatching;
            public static string NotificationNeedsRecompile;
            
            // Update Button
            public static string ButtonUpdateToVersionFormat;
            public static string ButtonTroubleshooting;
            
            // Rate App
            public static string RateAppQuestion;
            public static string RateAppThankYou;
            
            // Compilation Messages
            public static string CompileErrorTapToSee;
            public static string UnsupportedChangeTapToSee;
            public static string TapToShowStacktrace;
            
            // Link Messages
            public static string LinkForgotPassword;
            
            // Changelog
            public static string ChangelogTitle;
            
            // Daily Session
            public static string DailySessionStart;
            public static string DailySessionTimeHoursLeft;
            public static string DailySessionTimeMinutesLeft;
            public static string DailySessionNextSessionMinutes;
            public static string DailySessionNextSessionHours;
            
            // Indication Status Messages
            public static string IndicationFinishRegistration;
            public static string IndicationStarted;
            public static string IndicationStopping;
            public static string IndicationStopped;
            public static string IndicationPaused;
            public static string IndicationInstalling;
            public static string IndicationStarting;
            public static string IndicationReloaded;
            public static string IndicationPartiallySupported;
            public static string IndicationUnsupported;
            public static string IndicationPatching;
            public static string IndicationCompiling;
            public static string IndicationCompileErrors;
            public static string IndicationActivationFailed;
            public static string IndicationLoading;
            public static string IndicationUndetected;
            
            public static void LoadEnglish() {
                // Overlay/Toolbar
                OverlayTooltipRecompile = "Recompile";
                
                // Notifications
                NotificationPatching = "[Hot Reload] Applying patches...";
                NotificationNeedsRecompile = "[Hot Reload] Unsupported Changes detected! Recompiling...";
                
                // Update Button
                ButtonUpdateToVersionFormat = "Update To v{0}";
                ButtonTroubleshooting = "Troubleshooting";
                
                // Rate App
                RateAppQuestion = "Are you enjoying using Hot Reload?";
                RateAppThankYou = "Thank you for using Hot Reload!\n\nPlease consider leaving a review on the Asset Store to support us.";
                
                // Compilation Messages
                CompileErrorTapToSee = "Compile error, tap here to see more.";
                UnsupportedChangeTapToSee = "Unsupported change detected, tap here to see more.";
                TapToShowStacktrace = "Tap to show stacktrace";
                
                // Link Messages
                LinkForgotPassword = "Forgot password?";
                
                // Changelog
                ChangelogTitle = "Changelog";
                
                // Daily Session
                DailySessionStart = "Daily Session: Make code changes to start";
                DailySessionTimeHoursLeft = "Daily Session: {0}h {1}m Left";
                DailySessionTimeMinutesLeft = "Daily Session: {0}m Left";
                DailySessionNextSessionMinutes = "Next Session: {0}m";
                DailySessionNextSessionHours = "Next Session: {0}h {1}m";
                
                // Indication Status Messages
                IndicationFinishRegistration = "Finish Registration";
                IndicationStarted = "Waiting for code changes";
                IndicationStopping = "Stopping Hot Reload";
                IndicationStopped = "Hot Reload inactive";
                IndicationPaused = "Hot Reload paused";
                IndicationInstalling = "Installing";
                IndicationStarting = "Starting Hot Reload";
                IndicationReloaded = "Reload finished";
                IndicationPartiallySupported = "Changes partially applied";
                IndicationUnsupported = "Finished with warnings";
                IndicationPatching = "Reloading";
                IndicationCompiling = "Compiling";
                IndicationCompileErrors = "Scripts have compile errors";
                IndicationActivationFailed = "Activation failed";
                IndicationLoading = "Loading";
                IndicationUndetected = "No changes applied";
            }
            
            public static void LoadSimplifiedChinese() {
                // Overlay/Toolbar
                OverlayTooltipRecompile = "重新编译";

                // Notifications
                NotificationPatching = "[Hot Reload] 正在应用补丁...";
                NotificationNeedsRecompile = "[Hot Reload] 检测到不支持的更改！正在重新编译...";

                // Update Button
                ButtonUpdateToVersionFormat = "更新到 v{0}";
                ButtonTroubleshooting = "疑难解答";

                // Rate App
                RateAppQuestion = "您喜欢使用 Hot Reload 吗？";
                RateAppThankYou = "感谢您使用 Hot Reload！\n\n请考虑在 Asset Store 上留下评论以支持我们。";

                // Compilation Messages
                CompileErrorTapToSee = "编译错误，点击此处查看更多。";
                UnsupportedChangeTapToSee = "检测到不支持的更改，点击此处查看更多。";
                TapToShowStacktrace = "点击以显示堆栈跟踪";

                // Link Messages
                LinkForgotPassword = "忘记密码？";

                // Changelog
                ChangelogTitle = "更新日志";

                // Daily Session
                DailySessionStart = "每日会话：进行代码更改以开始";
                DailySessionTimeHoursLeft = "每日会话：剩余 {0}h {1}m";
                DailySessionTimeMinutesLeft = "每日会话：剩余 {0}m";
                DailySessionNextSessionMinutes = "下一会话：{0}m";
                DailySessionNextSessionHours = "下一会话：{0}h {1}m";

                // Indication Status Messages
                IndicationFinishRegistration = "完成注册";
                IndicationStarted = "等待代码更改";
                IndicationStopping = "正在停止 Hot Reload";
                IndicationStopped = "Hot Reload 未激活";
                IndicationPaused = "Hot Reload 已暂停";
                IndicationInstalling = "正在安装";
                IndicationStarting = "正在启动 Hot Reload";
                IndicationReloaded = "重新加载完成";
                IndicationPartiallySupported = "更改部分应用";
                IndicationUnsupported = "完成但有警告";
                IndicationPatching = "正在重新加载";
                IndicationCompiling = "正在编译";
                IndicationCompileErrors = "脚本有编译错误";
                IndicationActivationFailed = "激活失败";
                IndicationLoading = "正在加载";
                IndicationUndetected = "未应用任何更改";
            }
        }
    }
}

