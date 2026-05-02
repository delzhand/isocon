using UnityEngine.UIElements;
using System.Collections.Generic;
using ShunUI.Primitives;

namespace ShunUI
{
    public enum MenuBarVariant
    {
        Default,
        Outline
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunMenuBar : VisualElement
    {
        private VisualElement _menuContainer;
        private static ShunMenuBarMenu _currentlyOpenMenu = null;
        private MenuBarVariant _variant = MenuBarVariant.Outline;

        [UxmlAttribute]
        public MenuBarVariant variant
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

        public ShunMenuBar()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("menubar");

            _menuContainer = new VisualElement();
            _menuContainer.AddToClassList("menubar__container");
            hierarchy.Add(_menuContainer);

            UpdateVariantClass();

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectMenusFromUXML();
                UpdateVariantClass();
            });
        }

        private void UpdateVariantClass()
        {
            RemoveFromClassList("menubar-default");
            RemoveFromClassList("menubar-outline");

            string variantClass = _variant switch
            {
                MenuBarVariant.Default => "menubar-default",
                MenuBarVariant.Outline => "menubar-outline",
                _ => "menubar-default"
            };

            AddToClassList(variantClass);
        }

        private void CollectMenusFromUXML()
        {
            var childrenToMove = new List<VisualElement>();

            foreach (var child in Children())
            {
                if (child != _menuContainer)
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.RemoveFromHierarchy();
                _menuContainer.Add(child);

                if (child is ShunMenuBarMenu menu)
                {
                    menu.SetMenuBar(this);
                }
            }
        }

        public ShunMenuBarMenu AddMenu(string label)
        {
            var menu = new ShunMenuBarMenu();
            menu.label = label;
            menu.SetMenuBar(this);
            _menuContainer.Add(menu);
            return menu;
        }

        internal static void NotifyMenuOpened(ShunMenuBarMenu menu)
        {
            if (_currentlyOpenMenu != null && _currentlyOpenMenu != menu)
            {
                _currentlyOpenMenu.Close();
            }
            _currentlyOpenMenu = menu;
        }

        internal static void NotifyMenuClosed(ShunMenuBarMenu menu)
        {
            if (_currentlyOpenMenu == menu)
            {
                _currentlyOpenMenu = null;
            }
        }

        internal static bool IsAnyMenuOpen()
        {
            return _currentlyOpenMenu != null;
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunMenuBarMenu : ShunPopup
    {
        private ShunButton _trigger;
        private ShunMenuBar _menuBar;

        [UxmlAttribute]
        public string label
        {
            get => _trigger?.text ?? "";
            set
            {
                if (_trigger != null)
                    _trigger.text = value;
            }
        }

        public ShunMenuBarMenu()
        {
            Initialize();
        }

        internal void SetMenuBar(ShunMenuBar menuBar)
        {
            _menuBar = menuBar;
        }

        private void Initialize()
        {
            AddToClassList("menubar__menu");

            // Create trigger using base menu-trigger style
            _trigger = new ShunButton();
            _trigger.AddToClassList("menubar__menu-trigger");
            _trigger.variant = ButtonVariant.Ghost;
            _trigger.clicked += Toggle;
            hierarchy.Add(_trigger);

            // Create content using base menu-content style
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            // Create items container using base menu-items style
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("menu-items");
            _content.hierarchy.Add(_itemsContainer);

            // Register hover events for menu switching
            _trigger.RegisterCallback<MouseEnterEvent>(OnTriggerHover);

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectItemsFromUXML();
            });

            UpdatePositionOnGeometryChange();
        }

        private void OnTriggerHover(MouseEnterEvent evt)
        {
            // If any menu is open in the menubar, switch to this one on hover
            if (ShunMenuBar.IsAnyMenuOpen() && !_isOpen)
            {
                Open();
            }
        }

        private void CollectItemsFromUXML()
        {
            var childrenToMove = new List<VisualElement>();

            foreach (var child in Children())
            {
                if (child != _trigger && child != _content)
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.RemoveFromHierarchy();
                _itemsContainer.Add(child);

                if (child is ShunMenuItemBase menuItem)
                {
                    menuItem.closeRootMenu = Close;
                    menuItem.clicked += Close;
                }
            }
        }

        public ShunMenuItemBase AddItem(string label, System.Action onClick = null, bool disabled = false)
        {
            var item = new ShunMenuItemBase();
            item.label = label;
            item.disabled = disabled;
            item.closeRootMenu = Close;
            if (onClick != null)
            {
                item.clicked += onClick;
            }
            item.clicked += Close;
            _itemsContainer.Add(item);
            return item;
        }

        public void AddSeparator()
        {
            var separator = new ShunMenuSeparator();
            _itemsContainer.Add(separator);
        }

        public override void Open()
        {
            base.Open();
            _trigger?.AddToClassList("trigger--open");
            ShunMenuBar.NotifyMenuOpened(this);
        }

        public override void Close()
        {
            if (_itemsContainer != null)
            {
                foreach (var child in _itemsContainer.Children())
                {
                    if (child is ShunMenuItemBase menuItem)
                    {
                        menuItem.HideSubmenu();
                    }
                }
            }

            base.Close();
            _trigger?.RemoveFromClassList("trigger--open");
            ShunMenuBar.NotifyMenuClosed(this);
        }

        protected override void PositionContent()
        {
            if (_content == null || _trigger == null) return;

            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            var triggerRect = _trigger.worldBound;
            var menuHeight = 300;

            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            bool openUpward = spaceBelow < menuHeight && spaceAbove > spaceBelow;

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

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            return base.IsClickInside(clickedElement) ||
                   (_trigger != null && _trigger.Contains(clickedElement));
        }
    }
}