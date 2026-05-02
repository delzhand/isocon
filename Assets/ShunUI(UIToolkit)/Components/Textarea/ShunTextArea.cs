using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunTextArea : TextField
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

        public ShunTextArea()
        {
            Initialize();
        }

        public ShunTextArea(string label) : base(label)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base textarea class
            AddToClassList("textarea");

            // Make it multiline
            multiline = true;

            // Register for events to ensure styling updates
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (!ClassListContains("textarea"))
                    AddToClassList("textarea");

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
                    ((ITextEdition)textInput).placeholder = _placeholder;
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
                if (string.IsNullOrEmpty(value))
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