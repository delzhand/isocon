using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    public enum AlertVariant
    {
        Default,
        Destructive,
        Attention
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunAlert : VisualElement
    {
        private VisualElement _icon;
        private VisualElement _content;
        private Label _title;
        private Label _description;
        private AlertVariant _variant = AlertVariant.Default;
        private Texture2D _iconTexture = null;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _iconSvg = null;
#endif

        [UxmlAttribute]
        public AlertVariant variant
        {
            get => _variant;
            set
            {
                if (_variant != value)
                {
                    _variant = value;
                    UpdateVariantClass();
                    UpdateIcon();
                }
            }
        }

        [UxmlAttribute]
        public string title { get; set; } = "Alert Title";

        [UxmlAttribute]
        public string description { get; set; } = "Alert description";

        [UxmlAttribute]
        public Texture2D icon
        {
            get => _iconTexture;
            set
            {
                if (_iconTexture != value)
                {
                    _iconTexture = value;
#if UNITY_6000_3_OR_NEWER
                    if (value != null) _iconSvg = null;
#endif
                    UpdateIcon();
                }
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg
        {
            get => _iconSvg;
            set
            {
                if (_iconSvg != value)
                {
                    _iconSvg = value;
                    if (value != null) _iconTexture = null;
                    UpdateIcon();
                }
            }
        }
#endif

        public ShunAlert()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("alert");

            // Create header container (icon + title)
            VisualElement header = new VisualElement();
            header.AddToClassList("alert__header");

            // Create icon
            _icon = new VisualElement();
            _icon.AddToClassList("alert__icon");
            header.Add(_icon);

            // Create title
            _title = new Label(title);
            _title.AddToClassList("alert__title");
            header.Add(_title);

            Add(header);

            // Create description
            _description = new Label(description);
            _description.AddToClassList("alert__description");
            Add(_description);

            // Apply initial variant
            UpdateVariantClass();
            UpdateIcon();

            // Register for events
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateContent();
                UpdateIcon();
            });
        }

        private void UpdateVariantClass()
        {
            // Remove all variant classes
            RemoveFromClassList("alert-default");
            RemoveFromClassList("alert-destructive");
            RemoveFromClassList("alert-attention");

            // Add the current variant class
            string variantClass = _variant switch
            {
                AlertVariant.Default => "alert-default",
                AlertVariant.Destructive => "alert-destructive",
                AlertVariant.Attention => "alert-attention",
                _ => "alert-default"
            };

            AddToClassList(variantClass);
        }

        private void UpdateIcon()
        {
            if (_icon != null)
            {
                bool hasCustomIcon = _iconTexture != null;
#if UNITY_6000_3_OR_NEWER
                hasCustomIcon = hasCustomIcon || _iconSvg != null;
#endif

                if (hasCustomIcon)
                {
#if UNITY_6000_3_OR_NEWER
                    if (_iconSvg != null)
                        _icon.style.backgroundImage = new StyleBackground(_iconSvg);
                    else
#endif
                        _icon.style.backgroundImage = new StyleBackground(_iconTexture);
                    _icon.style.unityBackgroundImageTintColor = new StyleColor(StyleKeyword.Null);
                }
                else
                {
                    // Clear custom icon, let CSS handle default icon based on variant
                    _icon.style.backgroundImage = StyleKeyword.Null;
                    _icon.style.unityBackgroundImageTintColor = new StyleColor(StyleKeyword.Null);
                }
            }
        }

        private void UpdateContent()
        {
            if (_title != null && !string.IsNullOrEmpty(title))
            {
                _title.text = title;
            }

            if (_description != null && !string.IsNullOrEmpty(description))
            {
                _description.text = description;
            }
        }

        public void SetVariant(AlertVariant variant)
        {
            this.variant = variant;
        }
    }
}