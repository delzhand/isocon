using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunBreadcrumb : VisualElement
    {
    private VisualElement _container;
    private List<ShunBreadcrumbItem> _items = new List<ShunBreadcrumbItem>();

    public ShunBreadcrumb()
    {
        Initialize();
    }

    private void Initialize()
    {
        AddToClassList("breadcrumb");

        _container = new VisualElement();
        _container.AddToClassList("breadcrumb__list");
        hierarchy.Add(_container);

        RegisterCallback<AttachToPanelEvent>(evt => {
            CollectItemsFromUXML();
        });
    }

    private void CollectItemsFromUXML()
    {
        var childrenToMove = new List<VisualElement>();
        
        foreach (var child in Children())
        {
            if (child != _container)
            {
                childrenToMove.Add(child);
            }
        }
        
        foreach (var child in childrenToMove)
        {
            child.RemoveFromHierarchy();
            _container.Add(child);
            
            if (child is ShunBreadcrumbItem item)
            {
                _items.Add(item);
            }
            else if (child is ShunBreadcrumbEllipsis ellipsis)
            {
                // Ellipsis stays as is
            }
            
            // Add separator except for last item
            if (child != childrenToMove[childrenToMove.Count - 1])
            {
                var separator = new VisualElement();
                separator.AddToClassList("breadcrumb__separator");
                _container.Add(separator);
            }
        }
    }

    public void AddItem(string label, System.Action onClick = null, bool isCurrentPage = false)
    {
        var item = new ShunBreadcrumbItem();
        item.label = label;
        item.isCurrentPage = isCurrentPage;
        if (onClick != null)
        {
            item.clicked += onClick;
        }
        
        // Add separator before new item if not first
        if (_container.childCount > 0)
        {
            var separator = new VisualElement();
            separator.AddToClassList("breadcrumb__separator");
            _container.Add(separator);
        }
        
        _container.Add(item);
        _items.Add(item);
    }
}
[System.Serializable]
[UxmlElement]
public partial class ShunBreadcrumbItem : ShunButton
{
    [UxmlAttribute]
    public string label
    {
        get => text;
        set => text = value;
    }

    [UxmlAttribute]
    public bool isCurrentPage
    {
        get => ClassListContains("breadcrumb__item--current");
        set
        {
            if (value)
            {
                AddToClassList("breadcrumb__item--current");
                SetEnabled(false);
            }
            else
            {
                RemoveFromClassList("breadcrumb__item--current");
                SetEnabled(true);
            }
        }
    }

    public ShunBreadcrumbItem()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        AddToClassList("breadcrumb__item");
        variant = ButtonVariant.Ghost;
    }
}
[System.Serializable]
[UxmlElement]
public partial class ShunBreadcrumbEllipsis : VisualElement
{
    private ShunButton _trigger;
    private VisualElement _dropdown;
    private VisualElement _itemsContainer;
    private bool _isOpen = false;
    private bool _rootClickRegistered = false;
    private UnityEngine.Texture2D _icon;
#if UNITY_6000_3_OR_NEWER
    private UnityEngine.UIElements.VectorImage _iconSvg;
#endif

    [UxmlAttribute]
    public UnityEngine.Texture2D icon
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
    public UnityEngine.UIElements.VectorImage iconSvg
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

    public ShunBreadcrumbEllipsis()
    {
        Initialize();
    }

    private void Initialize()
    {
        AddToClassList("breadcrumb__ellipsis");

        // Create trigger with ghost variant and icon only
        _trigger = new ShunButton();
        _trigger.variant = ButtonVariant.Ghost;
        _trigger.size = ButtonSize.Icon;
        _trigger.AddToClassList("breadcrumb__ellipsis-trigger");
        _trigger.clicked += ToggleDropdown;
        hierarchy.Add(_trigger);
        
        // Load default ellipsis icon
#if UNITY_6000_3_OR_NEWER
        var defaultSvg = UnityEngine.Resources.Load<UnityEngine.UIElements.VectorImage>("Icons/ellipsis");
        if (defaultSvg != null)
            _iconSvg = defaultSvg;
        else
#endif
        if (_icon == null)
            _icon = UnityEngine.Resources.Load<UnityEngine.Texture2D>("Icons/ellipsis");
        UpdateIcon();

        // Create dropdown
        _dropdown = new VisualElement();
        _dropdown.AddToClassList("breadcrumb__ellipsis-dropdown");
        _dropdown.style.display = DisplayStyle.None;
        hierarchy.Add(_dropdown);

        // Create items container
        _itemsContainer = new VisualElement();
        _itemsContainer.AddToClassList("breadcrumb__ellipsis-items");
        _dropdown.hierarchy.Add(_itemsContainer);

        RegisterCallback<AttachToPanelEvent>(evt => {
            CollectItemsFromUXML();
        });
    }

    private void CollectItemsFromUXML()
    {
        var childrenToMove = new List<VisualElement>();
        
        foreach (var child in Children())
        {
            if (child != _trigger && child != _dropdown)
            {
                childrenToMove.Add(child);
            }
        }
        
        foreach (var child in childrenToMove)
        {
            child.RemoveFromHierarchy();
            _itemsContainer.Add(child);
            
            if (child is ShunBreadcrumbEllipsisItem item)
            {
                item.clicked += CloseDropdown;
            }
        }
    }

    private void UpdateIcon()
    {
        if (_trigger == null) return;

#if UNITY_6000_3_OR_NEWER
        if (_iconSvg != null)
        {
            _trigger.icon = null;
            _trigger.iconSvg = _iconSvg;
        }
        else
#endif
        if (_icon != null)
        {
            _trigger.icon = _icon;
        }
    }

    public void AddItem(string label, System.Action onClick)
    {
        var item = new ShunBreadcrumbEllipsisItem();
        item.label = label;
        if (onClick != null)
        {
            item.clicked += onClick;
        }
        item.clicked += CloseDropdown;
        _itemsContainer.Add(item);
    }

    private void ToggleDropdown()
    {
        if (_isOpen)
            CloseDropdown();
        else
            OpenDropdown();
    }

    private void OpenDropdown()
    {
        _isOpen = true;
        _dropdown.style.display = DisplayStyle.Flex;
        RegisterRootClick();
    }

    private void CloseDropdown()
    {
        _isOpen = false;
        _dropdown.style.display = DisplayStyle.None;
        UnregisterRootClick();
    }

    private void RegisterRootClick()
    {
        if (panel == null || _rootClickRegistered) return;
        
        panel.visualTree.RegisterCallback<PointerDownEvent>(OnRootClick, TrickleDown.TrickleDown);
        _rootClickRegistered = true;
    }

    private void UnregisterRootClick()
    {
        if (panel == null || !_rootClickRegistered) return;
        
        panel.visualTree.UnregisterCallback<PointerDownEvent>(OnRootClick, TrickleDown.TrickleDown);
        _rootClickRegistered = false;
    }

    private void OnRootClick(PointerDownEvent evt)
    {
        if (!_isOpen) return;
        
        var clickedElement = evt.target as VisualElement;
        if (clickedElement != null)
        {
            // Don't close if clicking inside the dropdown or trigger
            if (_dropdown.Contains(clickedElement) || _trigger.Contains(clickedElement))
            {
                return;
            }
        }
        
        CloseDropdown();
    }
}
[System.Serializable]
[UxmlElement]
public partial class ShunBreadcrumbEllipsisItem : ShunButton
{
    [UxmlAttribute]
    public string label
    {
        get => text;
        set => text = value;
    }

    public ShunBreadcrumbEllipsisItem()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        AddToClassList("breadcrumb__ellipsis-item");
        variant = ButtonVariant.Ghost;
    }
}
[System.Serializable]
[UxmlElement]
public partial class ShunBreadcrumbSeparator : VisualElement
{
    public ShunBreadcrumbSeparator()
    {
        AddToClassList("breadcrumb__separator");
    }
}

} 