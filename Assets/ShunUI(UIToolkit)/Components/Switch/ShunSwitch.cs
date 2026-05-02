using UnityEngine.UIElements;
using System;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunSwitch : VisualElement
    {
        private VisualElement _track;
        private VisualElement _thumb;
        private Label _label;
        private bool _value = false;
        private string _labelText = "Switch";

        [UxmlAttribute]
        public string label
        {
            get => _labelText;
            set
            {
                _labelText = value;
                if (_label != null)
                    _label.text = value;
            }
        }

        [UxmlAttribute]
        public bool value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    UpdateState();
                }
            }
        }

        public event Action<bool> onValueChanged;

        public ShunSwitch()
        {
            // Add base container class
            AddToClassList("switch-container");

            // Create the track (background)
            _track = new VisualElement();
            _track.AddToClassList("switch-track");
            _track.pickingMode = PickingMode.Position; // Ensure track can receive clicks
            Add(_track);

            // Create the thumb (sliding circle)
            _thumb = new VisualElement();
            _thumb.AddToClassList("switch-thumb");
            _thumb.pickingMode = PickingMode.Ignore; // Thumb doesn't need to capture clicks
            _track.Add(_thumb);

            // Create the label
            _label = new Label(_labelText);
            _label.AddToClassList("switch-label");
            Add(_label);

            // Add click handler to track and label
            _track.RegisterCallback<ClickEvent>(OnClicked);
            _label.RegisterCallback<ClickEvent>(OnClicked);

            // Register for attach event to ensure proper initialization
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            // Set initial state
            UpdateState();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            UpdateState();
            if (_label != null)
                _label.text = _labelText;
        }

        private void OnClicked(ClickEvent evt)
        {
            value = !value;
            onValueChanged?.Invoke(_value);
            evt.StopPropagation();
        }

        private void UpdateState()
        {
            if (_track == null || _thumb == null) return;

            if (_value)
            {
                _track.AddToClassList("switch-track--checked");
                _thumb.AddToClassList("switch-thumb--checked");
            }
            else
            {
                _track.RemoveFromClassList("switch-track--checked");
                _thumb.RemoveFromClassList("switch-thumb--checked");
            }
        }
    }
}