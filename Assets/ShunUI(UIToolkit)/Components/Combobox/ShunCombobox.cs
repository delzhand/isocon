using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ShunUI.Primitives;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunCombobox : ShunPopup
    {
        private ShunButton _trigger;
        private Label _valueLabel;
        private VisualElement _dropdownIcon;
        private VisualElement _searchContainer;
        private VisualElement _searchIcon;
        private TextField _searchField;
        private Label _placeholderLabel;
        private Label _noResultsLabel;

        private List<string> _options = new List<string>();
        private List<ShunButton> _optionButtons = new List<ShunButton>();
        private List<VisualElement> _checkmarkIcons = new List<VisualElement>();
        private string _selectedValue = "";
        private string _placeholder = "Select an option";
        private string _searchPlaceholder = "Search...";
        private string _noResultsMessage = "No results found";
        private Texture2D _selectedIcon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _selectedIconSvg;
        private VectorImage _searchIconSvg;
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
        public string searchPlaceholder
        {
            get => _searchPlaceholder;
            set
            {
                _searchPlaceholder = value;
                UpdateSearchPlaceholder();
            }
        }

        [UxmlAttribute]
        public string noResultsMessage
        {
            get => _noResultsMessage;
            set
            {
                _noResultsMessage = value;
                if (_noResultsLabel != null)
                {
                    _noResultsLabel.text = value;
                }
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
        public Texture2D searchIcon
        {
            get => _searchIcon?.style.backgroundImage.value.texture;
            set
            {
#if UNITY_6000_3_OR_NEWER
                if (value != null) _searchIconSvg = null;
#endif
                ApplySearchIcon(value != null ? new StyleBackground(value) : null, value != null);
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage searchIconSvg
        {
            get => _searchIconSvg;
            set
            {
                _searchIconSvg = value;
                if (value != null)
                {
                    // Clear Texture2D version - but we can't set the auto-property backing directly
                    // so we just apply the SVG
                }
                ApplySearchIcon(value != null ? new StyleBackground(value) : null, value != null);
            }
        }
#endif

        private void ApplySearchIcon(StyleBackground? background, bool hasIcon)
        {
            if (_searchIcon == null) return;

            if (hasIcon && background.HasValue)
            {
                _searchIcon.style.backgroundImage = background.Value;
                _searchIcon.style.display = DisplayStyle.Flex;

                var textInput = _searchField?.Q(className: "unity-text-field__input");
                if (textInput != null)
                    textInput.style.paddingLeft = 28;
                if (_placeholderLabel != null)
                    _placeholderLabel.style.left = 28;
            }
            else
            {
                _searchIcon.style.backgroundImage = null;
                _searchIcon.style.display = DisplayStyle.None;

                var textInput = _searchField?.Q(className: "unity-text-field__input");
                if (textInput != null)
                    textInput.style.paddingLeft = 0;
                if (_placeholderLabel != null)
                    _placeholderLabel.style.left = 0;
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

        public ShunCombobox()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("combobox");

            // Create trigger button using menu-trigger primitive
            _trigger = new ShunButton();
            _trigger.AddToClassList("menu-trigger");
            _trigger.variant = ButtonVariant.Outline;
            _trigger.clicked += Toggle;
            _trigger.style.width = Length.Percent(100);
            hierarchy.Add(_trigger);

            // Create value label
            _valueLabel = new Label(_placeholder);
            _valueLabel.AddToClassList("combobox__value");
            _trigger.hierarchy.Add(_valueLabel);

            // Create dropdown icon (chevron)
            _dropdownIcon = new VisualElement();
            _dropdownIcon.AddToClassList("combobox__icon");
            _trigger.hierarchy.Add(_dropdownIcon);

            // Create dropdown container using base class _content
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened
            
            // Create search container
            _searchContainer = new VisualElement();
            _searchContainer.AddToClassList("combobox__search-container");
            _content.hierarchy.Add(_searchContainer);

            // Create search icon
            _searchIcon = new VisualElement();
            _searchIcon.AddToClassList("combobox__search-icon");
            _searchIcon.style.display = DisplayStyle.None; // Hidden by default
            _searchContainer.hierarchy.Add(_searchIcon);

            // Create search field with placeholder
            _searchField = new TextField();
            _searchField.AddToClassList("combobox__search");

            // Set placeholder using the text input element
            var textInput = _searchField.Q(className: "unity-text-field__input");
            if (textInput != null)
            {
                _placeholderLabel = new Label(_searchPlaceholder);
                _placeholderLabel.AddToClassList("combobox__search-placeholder");
                _placeholderLabel.pickingMode = PickingMode.Ignore;
                textInput.Add(_placeholderLabel);

                // Hide placeholder when there's text
                _searchField.RegisterValueChangedCallback(evt =>
                {
                    _placeholderLabel.style.display = string.IsNullOrEmpty(evt.newValue) ? DisplayStyle.Flex : DisplayStyle.None;
                    FilterOptions(evt.newValue);
                });
            }

            _searchContainer.hierarchy.Add(_searchField);

            // Create separator
            var separator = new VisualElement();
            separator.AddToClassList("combobox__separator");
            _content.hierarchy.Add(separator);

            // Create options container using base class _itemsContainer
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("menu-items");
            _content.hierarchy.Add(_itemsContainer);

            // Create no results label
            _noResultsLabel = new Label(_noResultsMessage);
            _noResultsLabel.AddToClassList("combobox__no-results");
            _noResultsLabel.style.display = DisplayStyle.None;
            _content.hierarchy.Add(_noResultsLabel);

            // Register to collect options from UXML
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectOptionsFromUXML();
            });

            UpdatePositionOnGeometryChange();
        }

        private void CollectOptionsFromUXML()
        {
            // Find any buttons with combobox__option class that were added in UXML
            var existingOptions = this.Query<ShunButton>(className: "combobox__option").ToList();

            foreach (var option in existingOptions)
            {
                // Remove from current parent
                option.RemoveFromHierarchy();

                // Add to options container
                var optionText = option.text;
                option.clicked += () => SelectOption(optionText);

                // Create checkmark icon
                var checkmarkIcon = new VisualElement();
                checkmarkIcon.AddToClassList("combobox__option-checkmark");
                checkmarkIcon.style.display = DisplayStyle.None;
                option.Add(checkmarkIcon);

                _itemsContainer.Add(option);
                _options.Add(optionText);
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
            optionButton.AddToClassList("combobox__option");

            // Create checkmark icon
            var checkmarkIcon = new VisualElement();
            checkmarkIcon.AddToClassList("combobox__option-checkmark");
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

        public override void Open()
        {
            base.Open();
            _trigger?.AddToClassList("trigger--open");
            _searchField?.Focus();
            if (_searchField != null)
            {
                _searchField.value = "";
            }
            FilterOptions("");
        }

        public override void Close()
        {
            base.Close();
            _trigger?.RemoveFromClassList("trigger--open");
        }

        protected override void PositionContent()
        {
            if (_content == null || _trigger == null) return;

            // Get the viewport height
            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            // Get trigger position in world space
            var triggerRect = _trigger.worldBound;
            var dropdownHeight = 260; // Max dropdown height

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
                    _valueLabel.AddToClassList("combobox__value--placeholder");
                }
                else
                {
                    _valueLabel.RemoveFromClassList("combobox__value--placeholder");
                }
            }
        }

        private void UpdateSearchPlaceholder()
        {
            if (_placeholderLabel != null)
            {
                _placeholderLabel.text = _searchPlaceholder;
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

        private void FilterOptions(string searchText)
        {
            int visibleCount = 0;

            if (string.IsNullOrEmpty(searchText))
            {
                // Show all options
                foreach (var button in _optionButtons)
                {
                    button.style.display = DisplayStyle.Flex;
                    visibleCount++;
                }
            }
            else
            {
                // Filter options based on search text
                searchText = searchText.ToLower();
                foreach (var button in _optionButtons)
                {
                    bool matches = button.text.ToLower().Contains(searchText);
                    button.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
                    if (matches) visibleCount++;
                }
            }

            // Show/hide no results message
            if (_noResultsLabel != null)
            {
                if (visibleCount == 0)
                {
                    _noResultsLabel.style.display = DisplayStyle.Flex;
                    _itemsContainer.style.display = DisplayStyle.None;
                }
                else
                {
                    _noResultsLabel.style.display = DisplayStyle.None;
                    _itemsContainer.style.display = DisplayStyle.Flex;
                }
            }
        }

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            return (_content != null && _content.Contains(clickedElement)) ||
                   (_trigger != null && _trigger.Contains(clickedElement));
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunComboboxItem : ShunButton
    {
        [UxmlAttribute]
        public string label
        {
            get => text;
            set => text = value;
        }

        [UxmlAttribute]
        public string value { get; set; }

        public ShunComboboxItem()
        {
            AddToClassList("combobox__option");
            variant = ButtonVariant.Ghost;
            alignment = ButtonAlignment.Left;
        }
    }

}
