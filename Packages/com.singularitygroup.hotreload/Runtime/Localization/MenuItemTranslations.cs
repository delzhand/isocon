#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class MenuItems {
			public const string UIControls = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "UI 控件" : "UI controls";
			public const string Information = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "信息" : "Information";
			public const string Other = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "其他" : "Other";
			public const string FalllbackEventSystem = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "当项目未能及早创建 EventSystem 时使用" : "Used when project does not create an EventSystem early enough";
			public const string BuildSettings = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "构建设置" : "Build Settings";
			public const string IncludeInBuildTooltip = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Hot Reload 运行时是否应包含在开发版本中？HotReload 永远不会包含在发布版本中。" : "Should the Hot Reload runtime be included in development builds? HotReload is never included in release builds.";
			public const string PlayerSettings = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "播放器设置" : "Player Settings";
		}
	}
}
#endif
