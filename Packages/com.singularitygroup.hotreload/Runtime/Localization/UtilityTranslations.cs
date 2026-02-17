#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {
	internal static partial class Translations {
		public static class Utility {
			public static string BuildSettings;
			public static string IncludeInBuildTooltip;
			public static string PlayerSettings;
			public static string Other;
			public static string FallbackEventSystemTooltip;
			public static string NoEventSystemWarning;
			public static string OnHotReloadWarning;
			public static string MethodCallWarning;
			public static string OnHotReloadLocalCallWarning;
			public static string OnHotReloadLocalWarning;
            
			public static void LoadEnglish() {
				BuildSettings = "Build Settings";
				IncludeInBuildTooltip = "Should the Hot Reload runtime be included in development builds? HotReload is never included in release builds.";
				PlayerSettings = "Player Settings";
				Other = "Other";
				FallbackEventSystemTooltip = "Used when project does not create an EventSystem early enough";
				NoEventSystemWarning = "No EventSystem is active, enabling an EventSystem inside Hot Reload {0} prefab. A Unity EventSystem and an Input module is required for tapping buttons on the Unity UI.";
				OnHotReloadWarning = "failed. Make sure it has 0 parameters, or 1 parameter with type List<MethodPatch>. Exception:";
				MethodCallWarning = "failed. Make sure it's a method with 0 parameters either static or defined on MonoBehaviour.";
				OnHotReloadLocalCallWarning = "failed. Make sure it has 0 parameters. Exception:";
				OnHotReloadLocalWarning = "failed to find method {0}. Make sure it exists within the same class.";
			}
            
			public static void LoadSimplifiedChinese() {
				BuildSettings = "构建设置";
				IncludeInBuildTooltip = "Hot Reload 运行时是否应包含在开发版本中？HotReload 永远不会包含在发布版本中。";
				PlayerSettings = "播放器设置";
				Other = "其他";
				FallbackEventSystemTooltip = "当项目未能及早创建 EventSystem 时使用";
				NoEventSystemWarning = "没有活动的 EventSystem，正在 Hot Reload {0} 预制件内启用 EventSystem。点击 Unity UI 上的按钮需要 Unity EventSystem 和输入模块。";
				OnHotReloadWarning = "失败。请确保它有 0 个参数，或 1 个类型为 List<MethodPatch> 的参数。异常：";
				MethodCallWarning = "失败。请确保它是一个具有 0 个参数的方法，静态或在 MonoBehaviour 上定义。";
				OnHotReloadLocalCallWarning = "失败。请确保它有 0 个参数。异常：";
				OnHotReloadLocalWarning = "未能找到方法 {0}。请确保它存在于同一个类中。";
			}
		}
	}

}
#endif
