using UnityEngine.UIElements;

namespace ShunUI
{
    // Tooltip component for ShunUI
    [System.Serializable]
    [UxmlElement]
    public partial class ShunTooltip : VisualElement
    {
        private VisualElement _trigger;
        private VisualElement _tooltip;
        private Label _tooltipLabel;
        private bool _isVisible = false;
        private IVisualElementScheduledItem _showScheduledItem;
        private IVisualElementScheduledItem _hideScheduledItem;
    private const int HOVER_DELAY = 400; // ms

    [UxmlAttribute]
    public string text
    {
        get => _tooltipLabel?.text ?? "";
        set
        {
            if (_tooltipLabel != null)
                _tooltipLabel.text = value;
        }
    }

    public ShunTooltip()
    {
        Initialize();
    }

    private void Initialize()
    {
        AddToClassList("tooltip");

        // Create trigger container
        _trigger = new VisualElement();
        _trigger.AddToClassList("tooltip__trigger");
        hierarchy.Add(_trigger);

        // Create tooltip container
        _tooltip = new VisualElement();
        _tooltip.AddToClassList("tooltip__content");
        _tooltip.style.display = DisplayStyle.None;
        hierarchy.Add(_tooltip);

        // Create tooltip label
        _tooltipLabel = new Label();
        _tooltipLabel.AddToClassList("tooltip__text");
        _tooltip.hierarchy.Add(_tooltipLabel);

        // Register hover events
        _trigger.RegisterCallback<MouseEnterEvent>(evt => ScheduleShow());
        _trigger.RegisterCallback<MouseLeaveEvent>(evt => ScheduleHide());
        _tooltip.RegisterCallback<MouseEnterEvent>(evt => CancelHide());
        _tooltip.RegisterCallback<MouseLeaveEvent>(evt => ScheduleHide());

        RegisterCallback<AttachToPanelEvent>(evt => {
            CollectTriggerFromUXML();
        });

        RegisterCallback<GeometryChangedEvent>(evt => UpdateTooltipPosition());
    }

    private void CollectTriggerFromUXML()
    {
        // First child becomes the trigger content
        var children = new System.Collections.Generic.List<VisualElement>();
        foreach (var child in Children())
        {
            if (child != _trigger && child != _tooltip)
            {
                children.Add(child);
            }
        }

        if (children.Count > 0)
        {
            children[0].RemoveFromHierarchy();
            _trigger.Add(children[0]);
        }
    }

    private void ScheduleShow()
    {
        _hideScheduledItem?.Pause();
        _showScheduledItem?.Pause();
        
        _showScheduledItem = schedule.Execute(() => ShowTooltip()).StartingIn(HOVER_DELAY);
    }

    private void ScheduleHide()
    {
        _showScheduledItem?.Pause();
        _hideScheduledItem?.Pause();
        
        _hideScheduledItem = schedule.Execute(() => HideTooltip()).StartingIn(100);
    }

    private void CancelHide()
    {
        _hideScheduledItem?.Pause();
    }

    private void ShowTooltip()
    {
        _isVisible = true;
        _tooltip.style.display = DisplayStyle.Flex;
        UpdateTooltipPosition();
    }

    private void HideTooltip()
    {
        _isVisible = false;
        _tooltip.style.display = DisplayStyle.None;
    }

    private void UpdateTooltipPosition()
    {
        if (!_isVisible || _tooltip == null) return;

        var viewportHeight = panel?.visualTree?.layout.height ?? 0;
        if (viewportHeight == 0) return;

        var triggerRect = _trigger.worldBound;
        var tooltipHeight = 40;

        var spaceAbove = triggerRect.yMin;
        var spaceBelow = viewportHeight - triggerRect.yMax;

        // Prefer showing above the trigger
        bool showAbove = spaceAbove > tooltipHeight || spaceAbove > spaceBelow;

        if (showAbove)
        {
            _tooltip.style.bottom = _trigger.layout.height + 8;
            _tooltip.style.top = StyleKeyword.Auto;
        }
        else
        {
            _tooltip.style.top = _trigger.layout.height + 8;
            _tooltip.style.bottom = StyleKeyword.Auto;
        }
    }
}
}
