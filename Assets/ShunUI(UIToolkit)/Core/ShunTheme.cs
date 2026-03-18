using UnityEngine;

namespace ShunUI
{
    [CreateAssetMenu(fileName = "ShunTheme", menuName = "ShunUI/Theme")]
    public class ShunTheme : ScriptableObject
    {
        [Header("Layout & Styling")]
        public float BorderRadius = 6f;
        public float BorderWidth = 2f;

        [Header("Base Colors")]
        public Color Background = new Color(1f, 1f, 1f, 1f);
        public Color Foreground = new Color(0.145f, 0.145f, 0.145f, 1f);
        public Color Border = new Color(0.922f, 0.922f, 0.922f, 1f);
        public Color Ring = new Color(0.708f, 0.708f, 0.708f, 1f);
        public Color Input = new Color(0.922f, 0.922f, 0.922f, 1f);

        [Header("Primary Theme Colors")]
        public Color Primary = new Color(0.205f, 0.205f, 0.205f, 1f);
        public Color PrimaryForeground = new Color(0.985f, 0.985f, 0.985f, 1f);

        [Header("Secondary Theme Colors")]
        public Color Secondary = new Color(0.97f, 0.97f, 0.97f, 1f);
        public Color SecondaryForeground = new Color(0.205f, 0.205f, 0.205f, 1f);

        [Header("Accent Colors")]
        public Color Accent = new Color(0.97f, 0.97f, 0.97f, 1f);
        public Color AccentForeground = new Color(0.205f, 0.205f, 0.205f, 1f);

        [Header("Muted Colors")]
        public Color Muted = new Color(0.97f, 0.97f, 0.97f, 1f);
        public Color MutedForeground = new Color(0.556f, 0.556f, 0.556f, 1f);

        [Header("UI Components")]
        public Color Card = new Color(1f, 1f, 1f, 1f);
        public Color CardForeground = new Color(0.145f, 0.145f, 0.145f, 1f);
        public Color Popover = new Color(1f, 1f, 1f, 1f);
        public Color PopoverForeground = new Color(0.145f, 0.145f, 0.145f, 1f);

        [Header("State Colors")]
        public Color Destructive = new Color(0.796f, 0.298f, 0.255f, 1f);
        public Color DestructiveForeground = new Color(0.985f, 0.985f, 0.985f, 1f);

        [Header("Generated")]
        public UnityEngine.UIElements.StyleSheet StyleSheet;
    }
}
