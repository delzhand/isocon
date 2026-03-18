using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ShunUI.Primitives;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunContextMenu : ShunPopup
    {
        private VisualElement _targetElement;
        private Vector2 _mousePosition;
        private List<ShunMenuItemBase> _items;

        public ShunContextMenu()
        {
            _items = new List<ShunMenuItemBase>();

            AddToClassList("context-menu");

            // Set _content for ShunPopup base class
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("menu-items");
            _content.Add(_itemsContainer);

            // Don't hide the container itself, only the content
            _content.style.display = DisplayStyle.None;

            RegisterCallback<PointerDownEvent>(OnMouseDown);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            // Collect menu items from UXML
            schedule.Execute(() =>
            {
                CollectItemsFromUXML();
                AttachToTriggerArea();
            });
        }

        private void CollectItemsFromUXML()
        {
            var childrenToMove = new List<VisualElement>();

            foreach (var child in Children())
            {
                if (child != _content)
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
                    _items.Add(menuItem);

                    // Close menu when item is clicked (only if it doesn't have a submenu)
                    menuItem.clicked += () =>
                    {
                        if (!menuItem.hasArrow)
                        {
                            Close();
                        }
                    };
                }
            }
        }

        private void AttachToTriggerArea()
        {
            // Find the trigger area in the parent's children (sibling elements)
            var parent = this.parent;

            if (parent != null)
            {
                // First try to find by name in parent's children
                var triggerArea = parent.Q("trigger-area");

                if (triggerArea != null && triggerArea != this)
                {
                    AttachTo(triggerArea);
                    return;
                }

                // If not found, try to find any element with the trigger class
                var triggerByClass = parent.Q(className: "context-menu-trigger-area");

                if (triggerByClass != null && triggerByClass != this)
                {
                    AttachTo(triggerByClass);
                    return;
                }

                // Fallback: attach to parent itself
                AttachTo(parent);
            }
        }

        public void AttachTo(VisualElement target)
        {
            _targetElement = target;

            target.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.RightMouse)
                {
                    evt.StopPropagation();

                    // Use evt.position which is already in world coordinates
                    _mousePosition = evt.position;
                    Open();
                }
            });
        }

        public ShunMenuItemBase AddItem(string label, string shortcut = "", System.Action onClick = null)
        {
            var item = new ShunMenuItemBase
            {
                label = label,
                shortcut = shortcut
            };

            if (onClick != null)
            {
                item.clicked += onClick;
            }

            _items.Add(item);
            _itemsContainer.Add(item);

            // Close menu when item is clicked (only if it doesn't have a submenu)
            item.clicked += () =>
            {
                // Check if item has submenu by checking if arrow is visible
                if (!item.hasArrow)
                {
                    Close();
                }
            };

            return item;
        }

        public ShunMenuItemBase AddCheckableItem(string label, bool isChecked = false, System.Action<bool> onToggle = null)
        {
            var item = new ShunMenuItemBase
            {
                label = label,
                hasCheckmark = true
            };

            // Set initial checked state
            if (isChecked)
            {
                item.AddToClassList("menu-item--checked");
            }

            _items.Add(item);
            _itemsContainer.Add(item);

            // Toggle checkmark on click
            item.clicked += () =>
            {
                bool newChecked = !item.ClassListContains("menu-item--checked");
                if (newChecked)
                {
                    item.AddToClassList("menu-item--checked");
                }
                else
                {
                    item.RemoveFromClassList("menu-item--checked");
                }

                onToggle?.Invoke(newChecked);

                // Close menu after toggle
                if (!item.hasArrow)
                {
                    Close();
                }
            };

            return item;
        }

        public void AddSeparator()
        {
            var separator = new ShunMenuSeparator();
            _itemsContainer.Add(separator);
        }

        public void ClearItems()
        {
            foreach (var item in _items)
            {
                _itemsContainer.Remove(item);
            }
            _items.Clear();

            _itemsContainer.Clear();
        }

        public override void Open()
        {
            if (isOpen)
            {
                return;
            }

            _isOpen = true;

            if (_content != null)
            {
                // Move content to root to ensure it appears on top
                if (panel != null)
                {
                    ShunOverlayManager.AddOverlay(_content, panel, this);
                }
                else if (_content.parent != panel?.visualTree)
                {
                    _content.RemoveFromHierarchy();
                    panel?.visualTree?.Add(_content);
                }

                // Ensure content has stylesheet references
                EnsureContentStylesheets();

                // Hide initially to avoid seeing the movement
                _content.style.visibility = Visibility.Hidden;
                _content.style.display = DisplayStyle.Flex;

                // Wait 2 frames for styles and layout to be fully calculated
                schedule.Execute(() =>
                {
                    schedule.Execute(() =>
                    {
                        PositionContent();
                        // Show after positioning
                        _content.style.visibility = Visibility.Visible;
                    });
                });
            }

            RegisterRootClick();
        }

        protected override void PositionContent()
        {
            if (_content == null) return;

            var root = panel?.visualTree;
            if (root == null)
            {
                return;
            }

            // Position the content using absolute positioning
            _content.style.position = Position.Absolute;

            var contentRect = _content.layout;
            var rootRect = root.layout;

            float finalLeft = _mousePosition.x;
            float finalTop = _mousePosition.y;

            // Check if menu would go off right edge (relative to root viewport)
            if (_mousePosition.x + contentRect.width > rootRect.width)
            {
                finalLeft = _mousePosition.x - contentRect.width;
            }

            // Check if menu would go off bottom edge
            if (_mousePosition.y + contentRect.height > rootRect.height)
            {
                finalTop = _mousePosition.y - contentRect.height;
            }

            _content.style.left = finalLeft;
            _content.style.top = finalTop;
        }

        public override void Close()
        {
            if (!isOpen) return;

            foreach (var item in _items)
            {
                item.HideSubmenu();
            }

            // Hide the content
            if (_content != null)
            {
                _content.style.display = DisplayStyle.None;
            }

            base.Close();
        }

        private void OnMouseDown(PointerDownEvent evt)
        {
            evt.StopPropagation();
        }

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            // Check if click is in the context menu content
            if (_content != null && _content.Contains(clickedElement))
            {
                return true;
            }

            // Check if click is in any submenu
            foreach (var item in _items)
            {
                // Use a helper method to recursively check submenus
                if (IsClickInSubmenu(item, clickedElement))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsClickInSubmenu(ShunMenuItemBase item, VisualElement clickedElement)
        {
            // Check direct submenu
            var submenu = item.Q(className: "menu-submenu");
            if (submenu != null && submenu.Contains(clickedElement))
            {
                return true;
            }

            // Recursively check nested submenus
            var submenuItems = submenu?.Query<ShunMenuItemBase>().ToList();
            if (submenuItems != null)
            {
                foreach (var subItem in submenuItems)
                {
                    if (IsClickInSubmenu(subItem, clickedElement))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
