using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ShunUI;

namespace ShunUI.Primitives
{
    [UxmlElement]
    public partial class ShunMenuItemBase : ShunButton
    {
        protected Label _labelElement;
        protected Label _shortcutElement;
        protected VisualElement _menuIconElement;
        protected VisualElement _arrowIcon;
        protected VisualElement _checkmark;
        protected VisualElement _submenu;
        protected VisualElement _submenuItems;
        protected bool _submenuCollected = false;
        protected IVisualElementScheduledItem _hideSubmenuScheduler;
        protected IVisualElementScheduledItem _showSubmenuScheduler;
        protected bool _submenuVisible = false;
        protected bool _mouseOverParent = false;
        protected bool _mouseOverSubmenu = false;

        private const int HOVER_DELAY = 200; // ms

        [UxmlAttribute]
        public string label
        {
            get => _labelElement?.text ?? "";
            set
            {
                if (_labelElement != null)
                    _labelElement.text = value;
            }
        }

        [UxmlAttribute]
        public string shortcut
        {
            get => _shortcutElement?.text ?? "";
            set
            {
                if (_shortcutElement != null)
                {
                    _shortcutElement.text = value;
                    _shortcutElement.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        protected override void UpdateIcon()
        {
            if (_menuIconElement == null) return;

            bool hasIcon = icon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || iconSvg != null;
#endif

            if (hasIcon)
            {
#if UNITY_6000_3_OR_NEWER
                if (iconSvg != null)
                    _menuIconElement.style.backgroundImage = new StyleBackground(iconSvg);
                else
#endif
                    _menuIconElement.style.backgroundImage = new StyleBackground(icon);
                _menuIconElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                _menuIconElement.style.backgroundImage = null;
                _menuIconElement.style.display = DisplayStyle.None;
            }
        }

        [UxmlAttribute]
        public bool hasArrow
        {
            get => _arrowIcon?.style.display == DisplayStyle.Flex;
            set
            {
                if (_arrowIcon != null)
                {
                    _arrowIcon.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;

                    // If hasArrow is set to true via UXML, ensure we check for submenu items
                    if (value && _submenu == null)
                    {
                        schedule.Execute(() =>
                        {
                            if (_submenu == null)
                            {
                                CollectSubmenuItems();
                            }
                        });
                    }
                }
            }
        }

        [UxmlAttribute]
        public bool hasCheckmark
        {
            get => _checkmark?.style.display == DisplayStyle.Flex;
            set
            {
                if (_checkmark != null)
                    _checkmark.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        [UxmlAttribute]
        public bool disabled
        {
            get => ClassListContains("menu-item--disabled");
            set
            {
                if (value)
                    AddToClassList("menu-item--disabled");
                else
                    RemoveFromClassList("menu-item--disabled");
                SetEnabled(!value);
            }
        }

        public ShunMenuItemBase()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Clear any default children added by ShunButton (like the default label and icon)
            // This prevents them from being mistaken as submenu items
            Clear();

            // Set default style for menu items
            variant = ButtonVariant.Ghost;
            alignment = ButtonAlignment.Left;

            AddToClassList("menu-item");

            // Important: Position relatively so submenu can be positioned absolutely
            style.position = Position.Relative;

            // Create checkmark (for selected items)
            _checkmark = new VisualElement();
            _checkmark.AddToClassList("menu-item__checkmark");
            _checkmark.style.display = DisplayStyle.None;
            hierarchy.Add(_checkmark);

            // Create icon space
            _menuIconElement = new VisualElement();
            _menuIconElement.AddToClassList("menu-item__icon");
            _menuIconElement.style.display = DisplayStyle.None;
            hierarchy.Add(_menuIconElement);

            // Sync icon state
            UpdateIcon();

            // Create label
            _labelElement = new Label();
            _labelElement.AddToClassList("menu-item__label");
            hierarchy.Add(_labelElement);

            // Create shortcut
            _shortcutElement = new Label();
            _shortcutElement.AddToClassList("menu-item__shortcut");
            _shortcutElement.style.display = DisplayStyle.None;
            hierarchy.Add(_shortcutElement);

            // Create arrow icon (for sub-menus)
            _arrowIcon = new VisualElement();
            _arrowIcon.AddToClassList("menu-item__arrow");
            _arrowIcon.style.display = DisplayStyle.None;
            hierarchy.Add(_arrowIcon);

            // Register callback to handle submenu items
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);

            // Register mouse events for submenu behavior
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        protected virtual void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            // Always schedule collection on attach, but use flag to prevent duplicate collection
            schedule.Execute(() =>
            {
                if (!_submenuCollected)
                {
                    CollectSubmenuItems();
                    _submenuCollected = true;
                }
            });
        }

        protected virtual void OnMouseEnter(MouseEnterEvent evt)
        {
            _mouseOverParent = true;

            // Cancel any pending hide
            _hideSubmenuScheduler?.Pause();

            // If hasArrow is true but submenu not collected yet, collect now
            if (hasArrow && _submenu == null && !_submenuCollected)
            {
                CollectSubmenuItems();
                _submenuCollected = true;
            }

            if (_submenu != null && _submenuItems != null && _submenuItems.childCount > 0)
            {
                // Show submenu after delay
                _showSubmenuScheduler?.Pause();
                _showSubmenuScheduler = schedule.Execute(() => ShowSubmenu()).StartingIn(HOVER_DELAY);
            }
        }

        protected virtual void OnMouseLeave(MouseLeaveEvent evt)
        {
            _mouseOverParent = false;

            // Cancel any pending show
            _showSubmenuScheduler?.Pause();

            if (_submenu != null && _submenuVisible)
            {
                // Check if mouse is over submenu after a short delay
                schedule.Execute(() =>
                {
                    if (!_mouseOverParent && !_mouseOverSubmenu)
                    {
                        HideSubmenu();
                    }
                }).ExecuteLater(100);
            }
        }

        protected virtual void ShowSubmenu()
        {
            if (_submenu != null)
            {
                // Move submenu to root to avoid clipping issues
                MoveSubmenuToRoot();

                _submenu.style.display = DisplayStyle.Flex;
                _submenuVisible = true;
                UpdateSubmenuPosition();
            }
        }

        private void MoveSubmenuToRoot()
        {
            if (_submenu == null) return;

            if (panel != null)
            {
                ShunOverlayManager.AddOverlay(_submenu, panel, this);
                _submenu.MarkDirtyRepaint();
            }
        }

        public virtual void HideSubmenu()
        {
            if (_submenu != null)
            {
                _submenu.style.display = DisplayStyle.None;
                _submenuVisible = false;

                // Optionally move back to original parent to clean up
                // (Keeping it in root for performance, just hide it)
            }
        }

        protected virtual void CollectSubmenuItems()
        {
            // Check if this item has child menu items
            var submenuChildren = new List<VisualElement>();
            foreach (var child in Children())
            {
                if (child != _labelElement && child != _shortcutElement &&
                    child != _arrowIcon && child != _submenu && child != _checkmark && child != _menuIconElement)
                {
                    submenuChildren.Add(child);
                }
            }

            // If we have submenu items, create the submenu container
            if (submenuChildren.Count > 0)
            {
                CreateSubmenu();

                // Move children to submenu
                foreach (var child in submenuChildren)
                {
                    child.RemoveFromHierarchy();
                    _submenuItems.Add(child);
                }
            }
        }

        protected virtual void CreateSubmenu()
        {
            if (_submenu != null) return;

            hasArrow = true;

            // Create submenu container
            _submenu = new VisualElement();
            _submenu.AddToClassList("menu-submenu");
            _submenu.style.display = DisplayStyle.None;
            _submenu.style.position = Position.Absolute;
            _submenu.pickingMode = PickingMode.Position;

            // Add submenu to the menu item initially
            hierarchy.Add(_submenu);

            // Create submenu items container
            _submenuItems = new VisualElement();
            _submenuItems.AddToClassList("menu-items");

            // IMPORTANT: Make sure submenu items container allows overflow
            _submenuItems.style.overflow = Overflow.Visible;

            _submenu.Add(_submenuItems);

            // Register mouse events on submenu
            _submenu.RegisterCallback<MouseEnterEvent>(evt =>
            {
                _mouseOverSubmenu = true;
                _hideSubmenuScheduler?.Pause();
            });

            _submenu.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _mouseOverSubmenu = false;
                schedule.Execute(() =>
                {
                    if (!_mouseOverParent && !_mouseOverSubmenu)
                    {
                        HideSubmenu();
                    }
                }).ExecuteLater(100);
            });
        }

        public virtual void AddSubmenuItem(ShunMenuItemBase item)
        {
            if (_submenu == null)
            {
                CreateSubmenu();
            }

            _submenuItems.Add(item);
        }

        public virtual void AddSubmenuSeparator()
        {
            if (_submenu == null)
            {
                CreateSubmenu();
            }

            var separator = new VisualElement();
            separator.AddToClassList("menu-separator");
            _submenuItems.Add(separator);
        }

        protected virtual void UpdateSubmenuPosition()
        {
            if (_submenu == null || !_submenuVisible) return;

            // Ensure parent has relative positioning (though submenu is now in root)
            if (style.position != Position.Relative)
            {
                style.position = Position.Relative;
            }

            schedule.Execute(() =>
            {
                // Position submenu using world coordinates since it's at root level
                _submenu.style.position = Position.Absolute;

                var itemWorldBounds = worldBound;
                var root = panel?.visualTree;

                if (root == null) return;

                // Position to the right of this item
                float leftX = itemWorldBounds.xMax - 4; // Slight overlap
                float topY = itemWorldBounds.yMin - 4; // Align with top of parent item

                // Check if submenu would go off the right edge
                float submenuWidth = _submenu.layout.width;
                if (submenuWidth == 0)
                {
                    // Layout not ready yet, use min-width from CSS
                    submenuWidth = 180;
                }

                float submenuRight = leftX + submenuWidth;
                float viewportWidth = root.layout.width;

                // If submenu goes off-screen, position it to the left instead
                if (submenuRight > viewportWidth)
                {
                    leftX = itemWorldBounds.xMin - submenuWidth + 4;
                }

                _submenu.style.left = leftX;
                _submenu.style.top = topY;
            });
        }

        protected VisualElement GetRootVisualElement()
        {
            var current = parent;
            while (current != null && current.parent != null)
            {
                current = current.parent;
            }
            return current;
        }
    }

    [UxmlElement]
    public partial class ShunMenuSeparator : VisualElement
    {
        public ShunMenuSeparator()
        {
            AddToClassList("menu-separator");
        }
    }
}
