#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class Dialogs {
			public static string Information;
			public static string ContinueButtonText;
			public static string CancelButtonText;
			public static string DifferentProjectSummary;
			public static string DifferentProjectSuggestion;
			public static string DifferentCommitSummary;
			public static string DifferentCommitSuggestion;
			public static string ConnectionStateConnecting;
			public static string ConnectionStateHandshaking;
			public static string ConnectionStateDifferencesFound;
			public static string ConnectionStateConnected;
			public static string ConnectionStateCancelled;
			public static string Patches;
			public static string IsConnected;
			public static string NoWiFiNetwork;
			public static string WaitForCompiling;
			public static string TargetNetworkIsReachable;
			public static string AutoPairEncounteredIssue;
			public static string ConnectionFailed;
			public static string TryingToReconnect;
			public static string Disconnected;
			public static string PatchesStatus;
            
			public static void LoadEnglish() {
				Information = "Information";
				ContinueButtonText = "Continue";
				CancelButtonText = "Cancel";
				DifferentProjectSummary = "Hot Reload was started from a different project";
				DifferentProjectSuggestion = "Please run Hot Reload from the matching Unity project";
				DifferentCommitSummary = "Editor and current build are on different commits";
				DifferentCommitSuggestion = "This can cause errors when the build was made on an old commit.";
				ConnectionStateConnecting = "Connecting ...";
				ConnectionStateHandshaking = "Handshaking ...";
				ConnectionStateDifferencesFound = "Differences found";
				ConnectionStateConnected = "Connected!";
				ConnectionStateCancelled = "Cancelled";
				Patches = "Patches";
				IsConnected = "Is this device connected to {0}?";
				NoWiFiNetwork = "WiFi";
				WaitForCompiling = "Wait for compiling to finish before trying again";
				TargetNetworkIsReachable = "Make sure you're on the same {0} network. Also ensure Hot Reload is running";
				AutoPairEncounteredIssue = "Auto-pair encountered an issue";
				ConnectionFailed = "Connection failed";
				TryingToReconnect = "Trying to reconnect ...";
				Disconnected = "Disconnected";
				PatchesStatus = "Patches: {0} pending, {1} applied";
			}
            
			public static void LoadSimplifiedChinese() {
				Information = "信息";
				ContinueButtonText = "继续";
				CancelButtonText = "取消";
				DifferentProjectSummary = "Hot Reload 从不同的项目启动";
				DifferentProjectSuggestion = "请从匹配的 Unity 项目运行 Hot Reload";
				DifferentCommitSummary = "编辑器和当前构建在不同的提交上";
				DifferentCommitSuggestion = "当构建是在旧的提交上进行时，这可能会导致错误。";
				ConnectionStateConnecting = "正在连接 ...";
				ConnectionStateHandshaking = "正在握手 ...";
				ConnectionStateDifferencesFound = "发现差异";
				ConnectionStateConnected = "已连接！";
				ConnectionStateCancelled = "已取消";
				Patches = "补丁";
				IsConnected = "此设备是否已连接到 {0}？";
				NoWiFiNetwork = "WiFi";
				WaitForCompiling = "请等待编译完成后再试";
				TargetNetworkIsReachable = "请确保您在同一个 {0} 网络中。还要确保 Hot Reload 正在运行";
				AutoPairEncounteredIssue = "自动配对遇到问题";
				ConnectionFailed = "连接失败";
				TryingToReconnect = "正在尝试重新连接 ...";
				Disconnected = "已断开连接";
				PatchesStatus = "补丁：{0} 待处理，{1} 已应用";
			}
		}
	}

}
#endif
