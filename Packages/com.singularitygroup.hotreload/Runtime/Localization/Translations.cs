#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)

namespace SingularityGroup.HotReload.Localization {
    public static class Locale {
        public const string SimplifiedChinese = "zh";
        public const string English = "en";
    }
    
    internal static partial class Translations {
        static string loadedLocale;
        static Translations() {
            LoadDefaultLocalization();
        }

        public static void LoadDefaultLocalization() {
            LoadLocalization(PackageConst.DefaultLocale);
        }

        static void LoadLocalization(string locale) {
            if (loadedLocale == locale) {
                return;
            }
            if (locale == Locale.SimplifiedChinese) {
                LoadSimplifiedChinese();
            } else {
                LoadEnglish();
            }
            loadedLocale = locale;
        }
        
        public static void LoadEnglish() {
            // Load strings from subclasses
            Common.LoadEnglish();
            Dialogs.LoadEnglish();
            Errors.LoadEnglish();
            Settings.LoadEnglish();
            Logging.LoadEnglish();
            Utility.LoadSimplifiedChinese();
        }
        
        static void LoadSimplifiedChinese() {
            Common.LoadSimplifiedChinese();
            Dialogs.LoadSimplifiedChinese();
            Errors.LoadSimplifiedChinese();
            Settings.LoadSimplifiedChinese();
            Logging.LoadSimplifiedChinese();
            Utility.LoadSimplifiedChinese();
        }
    }
}
#endif
