using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunLabel : Label
    {
        private VisualElement _targetElement;

        [UxmlAttribute]
        public string targetId
        {
            get => _targetElement?.name ?? "";
            set
            {
                if (panel != null)
                {
                _targetElement = panel.visualTree.Q<VisualElement>(value);
                SetupTargetInteraction();
            }
        }
    }

    public ShunLabel()
    {
        Initialize();
    }

    private void Initialize()
    {
        AddToClassList("shun-label");

        RegisterCallback<ClickEvent>(OnLabelClick);
        
        RegisterCallback<AttachToPanelEvent>(evt => {
            // Try to find target by ID after attaching to panel
            if (!string.IsNullOrEmpty(targetId))
            {
                _targetElement = panel.visualTree.Q<VisualElement>(targetId);
                SetupTargetInteraction();
            }
        });
    }

    public void SetTarget(VisualElement target)
    {
        _targetElement = target;
        SetupTargetInteraction();
    }

    private void SetupTargetInteraction()
    {
        if (_targetElement == null) return;

        // Add cursor pointer to indicate clickable
        AddToClassList("shun-label--clickable");
    }

    private void OnLabelClick(ClickEvent evt)
    {
        if (_targetElement == null) return;

        // Handle different types of input elements
        if (_targetElement is Toggle toggle)
        {
            toggle.value = !toggle.value;
        }
        else if (_targetElement is ShunSwitch shunSwitch)
        {
            shunSwitch.value = !shunSwitch.value;
        }
        else if (_targetElement is ShunCheckbox checkbox)
        {
            checkbox.value = !checkbox.value;
        }
        else if (_targetElement is TextField textField)
        {
            textField.Focus();
        }
        else if (_targetElement is ShunInput shunInput)
        {
            shunInput.Focus();
        }
        else if (_targetElement is ShunTextArea textArea)
        {
            textArea.Focus();
        }
        else if (_targetElement is Button button)
        {
            // Simulate button click
            using (var clickEvent = ClickEvent.GetPooled())
            {
                clickEvent.target = button;
                button.SendEvent(clickEvent);
            }
        }
        else if (_targetElement is INotifyValueChanged<bool> boolInput)
        {
            boolInput.value = !boolInput.value;
        }
        else
        {
            // For other focusable elements, just focus them
            if (_targetElement.focusable)
            {
                _targetElement.Focus();
            }
        }
    }
}
}
