using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    public enum ButtonVariant
    {
        Primary,
        Secondary,
        Destructive,
        Outline,
        Ghost,
        Link
    }

    public enum ButtonSize
    {
        Small,
        Default,
        Large,
        Icon
    }

    public enum ButtonAlignment
    {
        Left,
        Center,
        Right
    }

    [UxmlElement]
    public partial class ShunButton : VisualElement
    {
        private ButtonVariant _variant = ButtonVariant.Primary;
        private ButtonSize _size = ButtonSize.Default;
        private ButtonAlignment _alignment = ButtonAlignment.Center;
        [UxmlAttribute]
        public ButtonAlignment alignment
        {
            get => _alignment;
            set
            {
                if (_alignment != value)
                {
                    _alignment = value;
                    UpdateAlignment();
                }
            }
        }
        private string _text = string.Empty;
        private Texture2D _icon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _iconSvg;
#endif
        private Label _label;
        private VisualElement _iconElement;

        [UxmlAttribute]
        public ButtonVariant variant
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

        [UxmlAttribute]
        public ButtonSize size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    UpdateSizeClass();
                }
            }
        }

        [UxmlAttribute]
        public string text
        {
            get => _text;
            set
            {
                _text = value;
                UpdateText();
            }
        }

        [UxmlAttribute]
        public Texture2D icon
        {
            get => _icon;
            set
            {
                _icon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) _iconSvg = null;
#endif
                UpdateIcon();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg
        {
            get => _iconSvg;
            set
            {
                _iconSvg = value;
                if (value != null) _icon = null;
                UpdateIcon();
            }
        }
#endif

        public ShunButton()
        {
            Initialize();
        }

        public ShunButton(string text, ButtonVariant variant = ButtonVariant.Primary, ButtonSize size = ButtonSize.Default)
        {
            Initialize();
            this.text = text;
            this.variant = variant;
            this.size = size;
        }

        protected virtual void Initialize()
        {
            // Add base button class immediately
            AddToClassList("button");

            // Make it focusable and clickable like a button
            focusable = true;

            // Create label for text
            _label = new Label();
            _label.pickingMode = PickingMode.Ignore; // Let clicks pass through to button
            hierarchy.Add(_label);

            // Apply initial styling
            UpdateVariantClass();
            UpdateSizeClass();
            UpdateText();
            UpdateIcon();
            UpdateAlignment();

            // Register for events to ensure styling updates in UI Builder
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateVariantClass();
                UpdateSizeClass();
                UpdateText();
                UpdateIcon();
                UpdateAlignment();
            });

            // Handle click events
            RegisterCallback<ClickEvent>(OnClick);
        }
        protected virtual void UpdateAlignment()
        {
            // Remove all alignment classes
            RemoveFromClassList("button-align-left");
            RemoveFromClassList("button-align-center");
            RemoveFromClassList("button-align-right");

            // Add the current alignment class
            string alignClass = _alignment switch
            {
                ButtonAlignment.Left => "button-align-left",
                ButtonAlignment.Center => "button-align-center",
                ButtonAlignment.Right => "button-align-right",
                _ => "button-align-center"
            };
            AddToClassList(alignClass);
        }

        protected virtual void UpdateText()
        {
            if (_label != null)
            {
                _label.text = _text;
            }
        }

        protected virtual void UpdateIcon()
        {
            bool hasIcon = _icon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || _iconSvg != null;
#endif

            if (!hasIcon)
            {
                // No icon, remove icon element if it exists
                if (_iconElement != null)
                {
                    _iconElement.RemoveFromHierarchy();
                    _iconElement = null;
                }
                return;
            }

            // Create icon element if it doesn't exist
            if (_iconElement == null)
            {
                _iconElement = new VisualElement();
                _iconElement.AddToClassList("button__icon");
                _iconElement.pickingMode = PickingMode.Ignore;
                hierarchy.Insert(0, _iconElement); // Insert at beginning, before text
            }

#if UNITY_6000_3_OR_NEWER
            if (_iconSvg != null)
                _iconElement.style.backgroundImage = new StyleBackground(_iconSvg);
            else
#endif
                _iconElement.style.backgroundImage = new StyleBackground(_icon);
        }

        protected virtual void UpdateVariantClass()
        {
            // Remove all variant classes
            RemoveFromClassList("button-primary");
            RemoveFromClassList("button-secondary");
            RemoveFromClassList("button-destructive");
            RemoveFromClassList("button-outline");
            RemoveFromClassList("button-ghost");
            RemoveFromClassList("button-link");

            // Add the current variant class
            string variantClass = _variant switch
            {
                ButtonVariant.Primary => "button-primary",
                ButtonVariant.Secondary => "button-secondary",
                ButtonVariant.Destructive => "button-destructive",
                ButtonVariant.Outline => "button-outline",
                ButtonVariant.Ghost => "button-ghost",
                ButtonVariant.Link => "button-link",
                _ => "button-primary"
            };

            AddToClassList(variantClass);
        }

        protected virtual void UpdateSizeClass()
        {
            // Remove all size classes
            RemoveFromClassList("button-sm");
            RemoveFromClassList("button-lg");
            RemoveFromClassList("button-icon");

            // Add the current size class
            string sizeClass = _size switch
            {
                ButtonSize.Small => "button-sm",
                ButtonSize.Large => "button-lg",
                ButtonSize.Icon => "button-icon",
                ButtonSize.Default => "", // No additional class for default size
                _ => ""
            };

            if (!string.IsNullOrEmpty(sizeClass))
                AddToClassList(sizeClass);
        }

        // Public methods for programmatic access
        public void SetVariant(ButtonVariant variant)
        {
            this.variant = variant;
        }

        public void SetSize(ButtonSize size)
        {
            this.size = size;
        }

        // Add click handler (similar to Button's clicked event)
        public event System.Action clicked;

        protected void OnClick(ClickEvent evt)
        {
            clicked?.Invoke();
        }
    }
}