using SingularityGroup.HotReload.Localization;

namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class MenuItems {
            public const string OpenHotReload = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Window/Hot Reload/打开 &#H" : "Window/Hot Reload/Open &#H";
            public const string RecompileHotReload = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Window/Hot Reload/重新编译" : "Window/Hot Reload/Recompile";
            public const string OverlayDescription = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "Hot Reload" : "Hot Reload";
            public const string NotImplementedObsolete = PackageConst.DefaultLocale == Locale.SimplifiedChinese ? "未实现" : "Not implemented";
        }
    }
}

