using UnityEngine;

namespace SingularityGroup.HotReload {
    internal static class PackageConst {
        //CI changes this property to 'true' for asset store builds.
        //Don't touch unless you know what you are doing
        public static bool IsAssetStoreBuild => true;

        
        public const string Version = "1.13.15";
        // Never higher than Version
        // Used for the download
        public const string ServerVersion = "1.13.15";
        public const string PackageName = "com.singularitygroup.hotreload";
        public const string DefaultLocale = Localization.Locale.English;
        // avoids unreachable code warnings from using const
        public static string DefaultLocaleField = DefaultLocale;
        public const string LibraryCachePath = "Library/" + PackageName;
        public const string ConfigFileName = "hot-reload-config.json";
    }
}
