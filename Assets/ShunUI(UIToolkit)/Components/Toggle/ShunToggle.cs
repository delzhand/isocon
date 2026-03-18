using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    // Toggle component for ShunUI
    public enum ToggleVariant
    {
        Default,
        Outline
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunToggle : ShunButton
    {
        private bool _isOn = false;
        private VisualElement _icon;
        private ToggleVariant _variant = ToggleVariant.Outline;

        [UxmlAttribute]
        public bool isOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    UpdateState();
                }
            }
        }

        [UxmlAttribute]
        public ToggleVariant toggleVariant
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

        public ShunToggle()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            // Add base toggle class
            AddToClassList("toggle");

            // Handle click to toggle state
            this.RegisterCallback<ClickEvent>(evt => {
                isOn = !isOn;
            });

            // Set initial state
            UpdateVariantClass();
            UpdateState();
        }

        protected override void UpdateVariantClass()
        {
            // Remove all variant classes
            RemoveFromClassList("toggle-default");
            RemoveFromClassList("toggle-outline");

            // Add the current variant class
            string variantClass = _variant switch
            {
                ToggleVariant.Default => "toggle-default",
                ToggleVariant.Outline => "toggle-outline",
                _ => "toggle-outline"
            };

            AddToClassList(variantClass);
        }

        private void UpdateState()
        {
            if (_isOn)
            {
                AddToClassList("toggle--on");
            }
            else
            {
                RemoveFromClassList("toggle--on");
            }
        }

        protected override void UpdateIcon()
        {
            bool hasIcon = icon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || iconSvg != null;
#endif

            if (!hasIcon)
            {
                // No icon, remove icon element if it exists
                if (_icon != null && _icon.parent != null)
                {
                    _icon.RemoveFromHierarchy();
                    _icon = null;
                }
                return;
            }

            // Create icon element if it doesn't exist
            if (_icon == null)
            {
                _icon = new VisualElement();
                _icon.AddToClassList("toggle__icon");
                Insert(0, _icon); // Insert at beginning, before text
            }

#if UNITY_6000_3_OR_NEWER
            if (iconSvg != null)
                _icon.style.backgroundImage = new StyleBackground(iconSvg);
            else
#endif
                _icon.style.backgroundImage = new StyleBackground(icon);
            _icon.style.display = DisplayStyle.Flex;
        }
    }
}
