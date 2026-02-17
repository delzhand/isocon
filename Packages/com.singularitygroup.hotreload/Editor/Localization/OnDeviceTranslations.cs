namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class OnDevice {
            // On-Device Settings
            public static string OnDeviceHeadline;
            public static string OnDeviceManualConnectFormat;
            public static string OnDeviceManualConnectWithIP;
            public static string OnDeviceNetworkNote;
            public static string OnDeviceCheckHotReloadRunning;
            public static string OnDeviceCheckHotReloadNotRunning;
            public static string OnDeviceCheckEnableExposeServer;
            public static string OnDeviceCheckPlatformSelected;
            public static string OnDeviceCheckPlatformNotSupported;
            public static string OnDeviceCheckDevelopmentEnabled;
            public static string OnDeviceCheckEnableDevelopment;
            public static string OnDeviceCheckMonoBackend;
            public static string OnDeviceCheckSetMonoBackend;
            public static string OnDeviceCheckStrippingLevel;
            public static string OnDeviceCheckStrippingSolution;
            
            public static void LoadEnglish() {
                // On-Device Settings
                OnDeviceHeadline = "Make changes to a build running on-device";
                OnDeviceManualConnectFormat = "If auto-pair fails, find your local IP in OS settings, and use this format to connect: '{ip}:{0}'";
                OnDeviceManualConnectWithIP = "If auto-pair fails, use this IP and port to connect: {0}:{1}\nMake sure you are on the same LAN/WiFi network";
                OnDeviceNetworkNote = "Make sure you are on the same LAN/WiFi network";
                OnDeviceCheckHotReloadRunning = "Hot Reload is running";
                OnDeviceCheckHotReloadNotRunning = "Hot Reload is not running";
                OnDeviceCheckEnableExposeServer = "Enable '{0}'";
                OnDeviceCheckPlatformSelected = "The {0} platform is selected";
                OnDeviceCheckPlatformNotSupported = "The current platform is {0} which is not supported";
                OnDeviceCheckDevelopmentEnabled = "Development Build is enabled";
                OnDeviceCheckEnableDevelopment = "Enable \"Development Build\"";
                OnDeviceCheckMonoBackend = "Scripting Backend is set to Mono";
                OnDeviceCheckSetMonoBackend = "Set Scripting Backend to Mono";
                OnDeviceCheckStrippingLevel = "Stripping Level = {0}";
                OnDeviceCheckStrippingSolution = "Code stripping needs to be disabled to ensure that all methods are available for patching.";
            }
            
            public static void LoadSimplifiedChinese() {
                // On-Device Settings
                OnDeviceHeadline = "对在设备上运行的构建进行更改";
                OnDeviceManualConnectFormat = "如果自动配对失败，请在操作系统设置中找到您的本地 IP，并使用此格式进行连接：'{ip}:{0}'";
                OnDeviceManualConnectWithIP = "如果自动配对失败，请使用此 IP 和端口进行连接：{0}:{1}\n确保您在同一个局域网/WiFi 网络中";
                OnDeviceNetworkNote = "确保您在同一个局域网/WiFi 网络中";
                OnDeviceCheckHotReloadRunning = "Hot Reload 正在运行";
                OnDeviceCheckHotReloadNotRunning = "Hot Reload 未运行";
                OnDeviceCheckEnableExposeServer = "启用 '{0}'";
                OnDeviceCheckPlatformSelected = "已选择 {0} 平台";
                OnDeviceCheckPlatformNotSupported = "当前平台为 {0}，不支持";
                OnDeviceCheckDevelopmentEnabled = "开发构建已启用";
                OnDeviceCheckEnableDevelopment = "启用“开发构建”";
                OnDeviceCheckMonoBackend = "脚本后端设置为 Mono";
                OnDeviceCheckSetMonoBackend = "将脚本后端设置为 Mono";
                OnDeviceCheckStrippingLevel = "剥离级别 = {0}";
                OnDeviceCheckStrippingSolution = "需要禁用代码剥离以确保所有方法都可用于修补。";
            }
        }
    }
}

