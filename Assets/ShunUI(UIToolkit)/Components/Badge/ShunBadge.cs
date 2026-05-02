using UnityEngine.UIElements;

namespace ShunUI
{
    public enum BadgeVariant
    {
        Primary,
        Secondary,
        Destructive,
        Outline
    }

    [UxmlElement]
    public partial class ShunBadge : Label
    {
        private BadgeVariant _variant = BadgeVariant.Primary;

        [UxmlAttribute]
        public BadgeVariant variant
        {
            get => _variant;
            set
            {
                if (_variant != value)
                {
                    _variant = value;
                    UpdateVariantClass();
                }
            }
        }

        public ShunBadge()
        {
            Initialize();
        }

        public ShunBadge(string text, BadgeVariant variant = BadgeVariant.Primary)
        {
            this.text = text;
            Initialize();
            this.variant = variant;
        }

        private void Initialize()
        {
            // Add base badge class immediately
            AddToClassList("badge");

            // Ensure text is set if not already
            if (string.IsNullOrEmpty(text))
                text = "Badge";

            // Apply initial styling
            UpdateVariantClass();

            // Register for events to ensure styling updates in UI Builder
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateVariantClass();
            });
        }

        private void UpdateVariantClass()
        {
            // Remove all variant classes
            RemoveFromClassList("badge-primary");
            RemoveFromClassList("badge-secondary");
            RemoveFromClassList("badge-destructive");
            RemoveFromClassList("badge-outline");

            // Add the current variant class and apply colors directly as fallback
            string variantClass = _variant switch
            {
                BadgeVariant.Primary => "badge-primary",
                BadgeVariant.Secondary => "badge-secondary",
                BadgeVariant.Destructive => "badge-destructive",
                BadgeVariant.Outline => "badge-outline",
                _ => "badge-primary"
            };

            AddToClassList(variantClass);
        }

        // Public method for programmatic access
        public void SetVariant(BadgeVariant variant)
        {
            this.variant = variant;
        }
    }
}