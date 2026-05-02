using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement] 
    public partial class ShunCheckbox : Toggle
    {
        private Label _customLabel;
        private string _labelText = "";

        [UxmlAttribute]
        public string labelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                UpdateLabel();
            }
        }

        public ShunCheckbox()
        {
            Initialize();
        }

        public ShunCheckbox(string label) : base(label)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base checkbox class immediately
            AddToClassList("checkbox");
            
            // Hide the built-in label text
            this.text = "";
            
            // Register for events to ensure styling updates in UI Builder
            RegisterCallback<AttachToPanelEvent>(evt => {
                // Ensure class is applied
                if (!ClassListContains("checkbox"))
                    AddToClassList("checkbox");
                    
                // Create custom label if needed
                if (!string.IsNullOrEmpty(_labelText) && _customLabel == null)
                {
                    CreateCustomLabel();
                }
            });
        }

        private void CreateCustomLabel()
        {
            if (_customLabel != null) return;

            _customLabel = new Label(_labelText);
            _customLabel.AddToClassList("checkbox__label");
            
            // Make label clickable to toggle checkbox
            _customLabel.RegisterCallback<ClickEvent>(evt =>
            {
                this.value = !this.value;
                evt.StopPropagation();
            });

            // Add label after the checkbox
            hierarchy.Add(_customLabel);
        }

        private void UpdateLabel()
        {
            if (string.IsNullOrEmpty(_labelText))
            {
                // Remove custom label if text is empty
                if (_customLabel != null)
                {
                    _customLabel.RemoveFromHierarchy();
                    _customLabel = null;
                }
            }
            else
            {
                // Update existing label or create new one
                if (_customLabel != null)
                {
                    _customLabel.text = _labelText;
                }
                else if (panel != null)
                {
                    CreateCustomLabel();
                }
            }
        }
    }
}
