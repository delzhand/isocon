using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ShunUI.Primitives;
using System;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunSelect : ShunPopup
    {
        private ShunButton _trigger;
        private Label _valueLabel;
        private VisualElement _dropdownIcon;

        private List<string> _options = new List<string>();
        private List<ShunButton> _optionButtons = new List<ShunButton>();
        private List<VisualElement> _checkmarkIcons = new List<VisualElement>();
        private string _selectedValue = "";
        private string _placeholder = "Select an option";
        private Texture2D _selectedIcon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _selectedIconSvg;
        public Action OnSelect;

#endif

        [UxmlAttribute]
        public List<string> choices
        {
            get => _options;
            set
            {
                if (value == null || value.Count == 0) return;
                SetOptions(value);
            }
        }

        [UxmlAttribute]
        public string placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                UpdateTriggerText();
            }
        }

        [UxmlAttribute]
        public string selectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                UpdateTriggerText();
            }
        }

        [UxmlAttribute]
        public Texture2D selectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) _selectedIconSvg = null;
#endif
                UpdateCheckmarkIcons();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage selectedIconSvg
        {
            get => _selectedIconSvg;
            set
            {
                _selectedIconSvg = value;
                if (value != null) _selectedIcon = null;
                UpdateCheckmarkIcons();
            }
        }
#endif

        public ShunSelect()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("select");

            // Create trigger button
            _trigger = new ShunButton();
            _trigger.AddToClassList("menu-trigger");
            _trigger.variant = ButtonVariant.Outline;
            _trigger.clicked += Toggle;
            _trigger.style.width = Length.Percent(100);
            hierarchy.Add(_trigger);

            // Create value label
            _valueLabel = new Label(_placeholder);
            _valueLabel.AddToClassList("select__value");
            _trigger.Add(_valueLabel);

            // Create dropdown icon (chevron)
            _dropdownIcon = new VisualElement();
            _dropdownIcon.AddToClassList("select__icon");
            _trigger.Add(_dropdownIcon);

            // Create dropdown container using base class _content
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            // Create options container
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("select__options");
            _content.Add(_itemsContainer);

            // Register to collect options from UXML
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectOptionsFromUXML();
            });
        }

        private void CollectOptionsFromUXML()
        {
            // Find any buttons with select__option class that were added in UXML
            var existingOptions = this.Query<ShunButton>(className: "select__option").ToList();

            foreach (var option in existingOptions)
            {
                // Remove from current parent
                option.RemoveFromHierarchy();

                // Add to options container
                string optionText = option.text;
                string optionValue = optionText;

                if (option is ShunSelectItem selectItem)
                {
                    if (!string.IsNullOrEmpty(selectItem.value))
                    {
                        optionValue = selectItem.value;
                    }
                }

                option.clicked += () => SelectOption(optionValue);

                // Create checkmark icon
                var checkmarkIcon = new VisualElement();
                checkmarkIcon.AddToClassList("select__option-checkmark");
                checkmarkIcon.style.display = DisplayStyle.None;
                option.Add(checkmarkIcon);

                _itemsContainer.Add(option);
                _options.Add(optionValue);
                _optionButtons.Add(option);
                _checkmarkIcons.Add(checkmarkIcon);
            }
        }

        public void AddOption(string optionValue, string optionLabel = null)
        {
            if (string.IsNullOrEmpty(optionLabel))
                optionLabel = optionValue;

            _options.Add(optionValue);

            var optionButton = new ShunButton();
            optionButton.clicked += () => SelectOption(optionValue);
            optionButton.text = optionLabel;
            optionButton.variant = ButtonVariant.Ghost;
            optionButton.alignment = ButtonAlignment.Left;
            optionButton.AddToClassList("select__option");

            // Create checkmark icon
            var checkmarkIcon = new VisualElement();
            checkmarkIcon.AddToClassList("select__option-checkmark");
            checkmarkIcon.style.display = DisplayStyle.None;
            optionButton.Add(checkmarkIcon);

            _itemsContainer.Add(optionButton);
            _optionButtons.Add(optionButton);
            _checkmarkIcons.Add(checkmarkIcon);
        }

        public void SetOptions(List<string> options)
        {
            _itemsContainer.Clear();
            _options.Clear();
            _optionButtons.Clear();
            _checkmarkIcons.Clear();

            foreach (var option in options)
            {
                AddOption(option);
            }
        }

        protected override void PositionContent()
        {
            if (_content == null || _trigger == null) return;

            // Get the viewport height
            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            // Get trigger position in world space
            var triggerRect = _trigger.worldBound;
            var dropdownHeight = 200; // Max dropdown height

            // Calculate space above and below
            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            // Determine if dropdown should open upward or downward
            bool openUpward = spaceBelow < dropdownHeight && spaceAbove > spaceBelow;

            // Position content absolutely at trigger position
            _content.style.position = Position.Absolute;
            _content.style.left = triggerRect.xMin;

            if (openUpward)
            {
                // Open above
                _content.style.top = StyleKeyword.Auto;
                _content.style.bottom = viewportHeight - triggerRect.yMin + 4;
            }
            else
            {
                // Open below (default)
                _content.style.top = triggerRect.yMax + 4;
                _content.style.bottom = StyleKeyword.Auto;
            }
        }

        private void SelectOption(string value)
        {
            _selectedValue = value;
            UpdateTriggerText();
            UpdateCheckmarkIcons();
            if (OnSelect != null)
            {
                OnSelect.Invoke();
            }
            Close();
        }

        private void UpdateTriggerText()
        {
            if (_valueLabel != null)
            {
                bool isPlaceholder = string.IsNullOrEmpty(_selectedValue);
                _valueLabel.text = isPlaceholder ? _placeholder : _selectedValue;

                // Apply placeholder styling
                if (isPlaceholder)
                {
                    _valueLabel.AddToClassList("select__value--placeholder");
                }
                else
                {
                    _valueLabel.RemoveFromClassList("select__value--placeholder");
                }
            }
        }

        private void UpdateCheckmarkIcons()
        {
            for (int i = 0; i < _options.Count; i++)
            {
                if (i < _checkmarkIcons.Count)
                {
                    var checkmark = _checkmarkIcons[i];
                    bool isSelected = _options[i] == _selectedValue;

                    checkmark.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;

                    // Set icon if provided
                    if (isSelected)
                    {
#if UNITY_6000_3_OR_NEWER
                        if (_selectedIconSvg != null)
                            checkmark.style.backgroundImage = new StyleBackground(_selectedIconSvg);
                        else
#endif
                            if (_selectedIcon != null)
                                checkmark.style.backgroundImage = new StyleBackground(_selectedIcon);
                    }
                }
            }
        }

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            return (_content != null && _content.Contains(clickedElement)) ||
                   (_trigger != null && _trigger.Contains(clickedElement));
        }
    }

    [UxmlElement]
    public partial class ShunSelectItem : ShunButton
    {
        [UxmlAttribute]
        public string label
        {
            get => text;
            set => text = value;
        }

        [UxmlAttribute]
        public string value { get; set; }

        public ShunSelectItem()
        {
            AddToClassList("select__option");
            variant = ButtonVariant.Ghost;
            alignment = ButtonAlignment.Left;
        }
    }
}
