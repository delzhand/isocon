#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class Common {
			public static string UnknownException;
			public static string HotReloadIsRunning;
			public static string HotReloadIsNotRunning;
			public static string UnableToResolveMethod;
			public static string UnableToResolveType;
			public static string UnableToResolveField;
			public static string HotReloadUnreachable;
			public static string TryingToReconnect;
			public static string Disconnected;
			public static string Unknown;
            
			public static void LoadEnglish() {
				UnknownException = "unknown exception";
				HotReloadIsRunning = "Hot Reload is running";
				HotReloadIsNotRunning = "Hot Reload is not running";
				UnableToResolveMethod = "Unable to resolve method";
				UnableToResolveType = "Unable to resolve type";
				UnableToResolveField = "Unable to resolve field";
				HotReloadUnreachable = "Hot Reload was unreachable for 5 seconds, trying to reconnect...";
				TryingToReconnect = "Trying to reconnect...";
				Disconnected = "Disconnected";
				Unknown = "unknown";
			}
            
			public static void LoadSimplifiedChinese() {
				UnknownException = "未知异常";
				HotReloadIsRunning = "Hot Reload 正在运行";
				HotReloadIsNotRunning = "Hot Reload 未运行";
				UnableToResolveMethod = "无法解析方法";
				UnableToResolveType = "无法解析类型";
				UnableToResolveField = "无法解析字段";
				HotReloadUnreachable = "Hot Reload 5 秒内无法访问，正在尝试重新连接...";
				TryingToReconnect = "正在尝试重新连接...";
				Disconnected = "已断开连接";
				Unknown = "未知";
			}
		}
	}

}
#endif
