namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class UI {
            // Run Tab
            public static string RunTabTitle;
            public static string RunTabTooltip;
            public static string TapToShowStacktrace;
            public static string CompileErrorMessage;
            public static string UnsupportedChangeMessage;
            public static string TapHereToSeeMore;
            public static string ClickableDescription;
            public static string SessionRefreshString;
            public static string RecompileButtonLabel;
            public static string StartButtonLabel;
            public static string StopButtonLabel;
            
            // License Messages
            public static string TrialLicenseMessage;
            public static string IndieLicenseMessage;
            public static string LicenseRenewalMessage;
            public static string BusinessLicenseMessage;
            
            // Startup Messages
            public static string StartingHotReloadMessage;

            public static string OverlayPanelName;
            
            public static void LoadEnglish() {
                // Run Tab
                RunTabTitle = "Run";
                RunTabTooltip = "Run and monitor the current Hot Reload session.";
                TapToShowStacktrace = "Tap to show stacktrace";
                CompileErrorMessage = "Compile error";
                UnsupportedChangeMessage = "Unsupported change detected";
                TapHereToSeeMore = "tap here to see more.";
                ClickableDescription = "Unsupported change";
                SessionRefreshString = "Next Session: {0}h {1}min";
                RecompileButtonLabel = " Recompile";
                StartButtonLabel = " Start";
                StopButtonLabel = " Stop";
                
                // License Messages
                TrialLicenseMessage = "Using Trial license, valid until {0}";
                IndieLicenseMessage = " Indie license active";
                LicenseRenewalMessage = "License will renew on {0}.";
                BusinessLicenseMessage = " Business license active";
                
                // Startup Messages
                StartingHotReloadMessage = "Starting Hot Reload";
                
                // Overlay
                OverlayPanelName = "Hot Reload Indication";
            }
            
            public static void LoadSimplifiedChinese() {
                // Run Tab
                RunTabTitle = "运行";
                RunTabTooltip = "运行并监控当前的 Hot Reload 会话。";
                TapToShowStacktrace = "点击以显示堆栈跟踪";
                CompileErrorMessage = "编译错误";
                UnsupportedChangeMessage = "检测到不支持的更改";
                TapHereToSeeMore = "点击此处查看更多。";
                ClickableDescription = "不支持的更改";
                SessionRefreshString = "下一会话：{0}h {1}min";
                RecompileButtonLabel = " 重新编译";
                StartButtonLabel = " 开始";
                StopButtonLabel = " 停止";

                // License Messages
                TrialLicenseMessage = "正在使用试用许可证，有效期至 {0}";
                IndieLicenseMessage = " 独立开发者许可证已激活";
                LicenseRenewalMessage = "许可证将于 {0} 续订。";
                BusinessLicenseMessage = " 商业许可证已激活";

                // Startup Messages
                StartingHotReloadMessage = "正在启动 Hot Reload";

                // Overlay
                OverlayPanelName = "Hot Reload 指示";
            }
        }
    }
}

