using SingularityGroup.HotReload.Editor.Localization;
using UnityEditor;

namespace SingularityGroup.HotReload.Editor {
    internal class IncludeInBuildOption : ProjectOptionBase, ISerializedProjectOption {
        static IncludeInBuildOption _I;
        public static IncludeInBuildOption I = _I ?? (_I = new IncludeInBuildOption());
        public override string ShortSummary => Translations.Settings.OptionIncludeInBuild;
        public override string Summary => ShortSummary;

        public override string ObjectPropertyName =>
            nameof(HotReloadSettingsObject.IncludeInBuild);

        public override void InnerOnGUI(SerializedObject so) {
            string description;
            if (GetValue(so)) {
                description = Translations.Settings.OptionIncludeInBuildDescriptionEnabled;
            } else {
                description = Translations.Settings.OptionIncludeInBuildDescriptionDisabled;
            }
            description += Translations.Settings.OptionIncludeInBuildDescriptionSuffix;
            EditorGUILayout.LabelField(description, HotReloadWindowStyles.WrapStyle);
        }
    }
}