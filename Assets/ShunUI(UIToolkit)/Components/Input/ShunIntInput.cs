using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunIntInput : IntegerField
    {
        private string _placeholder = "";
        private TextElement _textElement;

        [UxmlAttribute]
        public string placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                UpdatePlaceholder();
            }
        }

        public ShunIntInput()
        {
            Initialize();
        }

        public ShunIntInput(string label) : base(label)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base input class immediately
            AddToClassList("input");

            // Register for events to ensure styling updates in UI Builder
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                // Ensure class is applied
                if (!ClassListContains("input"))
                    AddToClassList("input");

                UpdatePlaceholder();

                // Get the text element
                _textElement = this.Q<TextElement>(className: "unity-text-element");
                UpdatePlaceholderStyle();
            });

            // Monitor value changes to update placeholder styling
            this.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                UpdatePlaceholderStyle();
            });

            // Also monitor when the field gains/loses focus
            RegisterCallback<FocusInEvent>(evt => UpdatePlaceholderStyle());
            RegisterCallback<FocusOutEvent>(evt => UpdatePlaceholderStyle());
        }

        private void UpdatePlaceholder()
        {
            if (!string.IsNullOrEmpty(_placeholder))
            {
                // Use Unity's built-in placeholder property
                var textInput = this.Q(className: "unity-text-element");
                if (textInput != null)
                {
                    if (textInput is ITextEdition textEdition)
                    {
                        textEdition.placeholder = _placeholder;
                    }
                    UpdatePlaceholderStyle();
                }
            }
        }

        private void UpdatePlaceholderStyle()
        {
            if (_textElement == null)
                _textElement = this.Q<TextElement>(className: "unity-text-element");

            if (_textElement != null)
            {
                // Add a class when showing placeholder (empty value)
                if (string.IsNullOrEmpty($"{value}"))
                {
                    _textElement.AddToClassList("unity-text-element--placeholder");
                }
                else
                {
                    _textElement.RemoveFromClassList("unity-text-element--placeholder");
                }
            }
        }
    }
}
