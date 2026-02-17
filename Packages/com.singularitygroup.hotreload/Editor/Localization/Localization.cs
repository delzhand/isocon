using SingularityGroup.HotReload.Localization;
using UnityEditor;
using UnityEngine;

namespace SingularityGroup.HotReload.Editor.Localization {
    [InitializeOnLoad]
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

        static void LoadEnglish() {
            Common.LoadEnglish();
            Timeline.LoadEnglish();
            License.LoadEnglish();
            Errors.LoadEnglish();
            Registration.LoadEnglish();
            Dialogs.LoadEnglish();
            Settings.LoadEnglish();
            OnDevice.LoadEnglish();
            About.LoadEnglish();
            Miscellaneous.LoadEnglish();
            Suggestions.LoadEnglish();
            Utility.LoadEnglish();
            UI.LoadEnglish();
        }

        static void LoadSimplifiedChinese() {
            Common.LoadSimplifiedChinese();
            Timeline.LoadSimplifiedChinese();
            License.LoadSimplifiedChinese();
            Errors.LoadSimplifiedChinese();
            Registration.LoadSimplifiedChinese();
            Dialogs.LoadSimplifiedChinese();
            Settings.LoadSimplifiedChinese();
            OnDevice.LoadSimplifiedChinese();
            About.LoadSimplifiedChinese();
            Miscellaneous.LoadSimplifiedChinese();
            Suggestions.LoadSimplifiedChinese();
            Utility.LoadSimplifiedChinese();
            UI.LoadSimplifiedChinese();
        }
    }
}
