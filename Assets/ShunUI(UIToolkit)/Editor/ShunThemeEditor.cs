using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Text;
using System.Globalization;

namespace ShunUI.Editor
{
    [CustomEditor(typeof(ShunTheme))]
    public class ShunThemeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ShunTheme theme = (ShunTheme)target;

            if (GUILayout.Button("Regenerate StyleSheet"))
            {
                GenerateStyleSheet(theme);
            }
        }

        public static void GenerateStyleSheet(ShunTheme theme)
        {
            if (theme == null) return;

            string assetPath = AssetDatabase.GetAssetPath(theme);
            if (string.IsNullOrEmpty(assetPath)) return;

            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string ussPath = Path.Combine(directory, $"{fileName}.uss");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(":root {");
            
            // Base Colors
            sb.AppendLine($"    --color-background: {ToRgba(theme.Background)};");
            sb.AppendLine($"    --color-foreground: {ToRgba(theme.Foreground)};");
            sb.AppendLine($"    --color-border: {ToRgba(theme.Border)};");
            sb.AppendLine($"    --color-ring: {ToRgba(theme.Ring)};");
            sb.AppendLine($"    --color-input: {ToRgba(theme.Input)};");

            // Primary Colors
            sb.AppendLine($"    --color-primary: {ToRgba(theme.Primary)};");
            sb.AppendLine($"    --color-primary-foreground: {ToRgba(theme.PrimaryForeground)};");

            // Secondary Colors
            sb.AppendLine($"    --color-secondary: {ToRgba(theme.Secondary)};");
            sb.AppendLine($"    --color-secondary-foreground: {ToRgba(theme.SecondaryForeground)};");

            // Accent Colors
            sb.AppendLine($"    --color-accent: {ToRgba(theme.Accent)};");
            sb.AppendLine($"    --color-accent-foreground: {ToRgba(theme.AccentForeground)};");

            // Muted Colors
            sb.AppendLine($"    --color-muted: {ToRgba(theme.Muted)};");
            sb.AppendLine($"    --color-muted-foreground: {ToRgba(theme.MutedForeground)};");

            // UI Components
            sb.AppendLine($"    --color-card: {ToRgba(theme.Card)};");
            sb.AppendLine($"    --color-card-foreground: {ToRgba(theme.CardForeground)};");
            sb.AppendLine($"    --color-popover: {ToRgba(theme.Popover)};");
            sb.AppendLine($"    --color-popover-foreground: {ToRgba(theme.PopoverForeground)};");

            // State Colors
            sb.AppendLine($"    --color-destructive: {ToRgba(theme.Destructive)};");
            sb.AppendLine($"    --color-destructive-foreground: {ToRgba(theme.DestructiveForeground)};");

            // Layout
            sb.AppendLine($"    --border-radius: {theme.BorderRadius.ToString(CultureInfo.InvariantCulture)}px;");
            sb.AppendLine($"    --border-radius-md: {theme.BorderRadius.ToString(CultureInfo.InvariantCulture)}px;");
            sb.AppendLine($"    --border-radius-lg: {(theme.BorderRadius + 2f).ToString(CultureInfo.InvariantCulture)}px;");
            sb.AppendLine($"    --border-radius-sm: {Mathf.Max(0, theme.BorderRadius - 2f).ToString(CultureInfo.InvariantCulture)}px;");
            sb.AppendLine($"    --border-width: {theme.BorderWidth.ToString(CultureInfo.InvariantCulture)}px;");

            sb.AppendLine("}");

            File.WriteAllText(ussPath, sb.ToString(), Encoding.UTF8);
        }

        public static void LinkStyleSheet(ShunTheme theme)
        {
            if (theme == null) return;

            string assetPath = AssetDatabase.GetAssetPath(theme);
            if (string.IsNullOrEmpty(assetPath)) return;

            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string ussPath = Path.Combine(directory, $"{fileName}.uss");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            if (styleSheet != null)
            {
                theme.StyleSheet = styleSheet;
                EditorUtility.SetDirty(theme);
            }
            else
            {
                Debug.LogWarning($"Could not load StyleSheet at {ussPath}");
            }
        }

        private static string ToRgba(Color color)
        {
            return $"rgba({(int)(color.r * 255)}, {(int)(color.g * 255)}, {(int)(color.b * 255)}, {color.a.ToString(CultureInfo.InvariantCulture)})";
        }
    }
}
