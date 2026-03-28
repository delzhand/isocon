using UnityEngine.UIElements;
using UnityEngine;
using ShunUI;

namespace ShunUI.Primitives
{
    public abstract class ShunPopup : VisualElement
    {
        protected VisualElement _content;
        protected VisualElement _itemsContainer;
        protected bool _isOpen = false;
        protected bool _rootClickRegistered = false;
        private bool _stylesheetsInitialized = false;

        public virtual bool isOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    if (value) Open();
                    else Close();
                }
            }
        }

        public virtual void Open()
        {
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
                _content.style.display = DisplayStyle.Flex;
            }
            RegisterRootClick();
            PositionContent();
        }

        public virtual void Close()
        {
            _isOpen = false;
            if (_content != null)
            {
                _content.style.display = DisplayStyle.None;
                _stylesheetsInitialized = false; // Reset so we refresh styles on next open
            }
            UnregisterRootClick();
        }

        public virtual void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        protected abstract void PositionContent();

        protected void RegisterRootClick()
        {
            if (panel == null || _rootClickRegistered) return;

            panel.visualTree.RegisterCallback<PointerDownEvent>(OnRootClick, TrickleDown.TrickleDown);
            _rootClickRegistered = true;
        }

        protected void UnregisterRootClick()
        {
            if (panel == null || !_rootClickRegistered) return;

            panel.visualTree.UnregisterCallback<PointerDownEvent>(OnRootClick, TrickleDown.TrickleDown);
            _rootClickRegistered = false;
        }

        protected virtual void OnRootClick(PointerDownEvent evt)
        {
            if (!_isOpen) return;

            var clickedElement = evt.target as VisualElement;
            if (clickedElement != null && !IsClickInside(clickedElement))
            {
                Close();
            }
        }

        protected virtual bool IsClickInside(VisualElement clickedElement)
        {
            if (_content != null && _content.Contains(clickedElement))
                return true;

            // Check reparented submenus — ShowSubmenu() moves them to the root
            // overlay, so they are no longer in _content's subtree.
            if (_itemsContainer != null && IsClickInSubmenu(_itemsContainer, clickedElement))
                return true;

            return false;
        }

        /// <summary>
        /// Recursively checks whether the click landed inside a reparented submenu
        /// of any ShunMenuItemBase within the given container.
        /// </summary>
        protected static bool IsClickInSubmenu(VisualElement container, VisualElement clickedElement)
        {
            foreach (var child in container.Children())
            {
                if (child is ShunMenuItemBase menuItem)
                {
                    var submenu = menuItem.Submenu;
                    if (submenu != null)
                    {
                        if (submenu.Contains(clickedElement))
                            return true;

                        // Recurse into nested submenu items
                        if (IsClickInSubmenu(submenu, clickedElement))
                            return true;
                    }
                }
                else
                {
                    // Recurse through wrapper containers (e.g. _submenuItems)
                    // so we can reach ShunMenuItemBase children at any depth
                    if (IsClickInSubmenu(child, clickedElement))
                        return true;
                }
            }
            return false;
        }

        protected void UpdatePositionOnGeometryChange()
        {
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (_isOpen)
                {
                    PositionContent();
                }
            });
        }

        protected bool ShouldOpenUpward(float triggerBottom, float menuHeight)
        {
            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return false;

            var spaceBelow = viewportHeight - triggerBottom;
            var spaceAbove = triggerBottom;

            return spaceBelow < menuHeight && spaceAbove > spaceBelow;
        }

        protected void EnsureContentStylesheets()
        {
            if (_content == null || _stylesheetsInitialized) return;

            // Clear any existing stylesheets first
            _content.styleSheets.Clear();

            // Strategy: Copy stylesheets from the closest UIDocument root, not panel.visualTree
            // This ensures we get the UIDocument's stylesheets (ShunStyle.uss)

            // Find the UIDocument root by walking up the tree
            VisualElement uiDocumentRoot = FindUIDocumentRoot(this);

            if (uiDocumentRoot != null)
            {
                // Copy all stylesheets from UIDocument root (this includes ShunStyle.uss)
                for (int i = 0; i < uiDocumentRoot.styleSheets.count; i++)
                {
                    var styleSheet = uiDocumentRoot.styleSheets[i];
                    if (!_content.styleSheets.Contains(styleSheet))
                    {
                        _content.styleSheets.Add(styleSheet);
                    }
                }
            }
            else
            {
                // Fallback 1: Load ShunStyle stylesheet from Resources
                var shunStyle = Resources.Load<StyleSheet>("ShunStyle");
                if (shunStyle != null && !_content.styleSheets.Contains(shunStyle))
                {
                    _content.styleSheets.Add(shunStyle);
                }

                // Fallback 2: Copy stylesheets from panel root
                var root = panel?.visualTree;
                if (root != null)
                {
                    for (int i = 0; i < root.styleSheets.count; i++)
                    {
                        var styleSheet = root.styleSheets[i];
                        // Exclude Themes in Play Mode as they are managed by ShunThemeManager
                        if (Application.isPlaying && styleSheet.name.EndsWith("Theme", System.StringComparison.OrdinalIgnoreCase)) continue;

                        if (!_content.styleSheets.Contains(styleSheet))
                        {
                            _content.styleSheets.Add(styleSheet);
                        }
                    }
                }
            }

            // Fallback 3: Copy from the component itself
            if (_content.styleSheets.count == 0)
            {
                for (int i = 0; i < styleSheets.count; i++)
                {
                    var styleSheet = styleSheets[i];
                    if (!_content.styleSheets.Contains(styleSheet))
                    {
                        _content.styleSheets.Add(styleSheet);
                    }
                }
            }

            _stylesheetsInitialized = true;
        }

        /// <summary>
        /// Find the UIDocument root element by walking up the hierarchy
        /// </summary>
        private VisualElement FindUIDocumentRoot(VisualElement element)
        {
            if (element == null) return null;

            VisualElement current = element;
            VisualElement lastWithStylesheets = null;

            // Walk up the tree to find the root element that has stylesheets
            while (current != null)
            {
                // Check if this element has stylesheets (likely a UIDocument root)
                if (current.styleSheets.count > 0 && current.parent != null)
                {
                    lastWithStylesheets = current;
                }
                current = current.parent;
            }

            return lastWithStylesheets;
        }

        /// <summary>
        /// Force refresh the content stylesheets from the panel root.
        /// Call this to update popup appearance.
        /// </summary>
        /// <param name="baseStyleSheets">Optional base stylesheets to apply (like ShunStyle.uss)</param>
        public virtual void RefreshStylesheets(System.Collections.Generic.List<UnityEngine.UIElements.StyleSheet> baseStyleSheets = null)
        {
            if (_content == null) return;

            // Clear existing stylesheets
            _content.styleSheets.Clear();

            // If base stylesheets provided, use them
            if (baseStyleSheets != null && baseStyleSheets.Count > 0)
            {
                foreach (var sheet in baseStyleSheets)
                {
                    if (!_content.styleSheets.Contains(sheet))
                    {
                        _content.styleSheets.Add(sheet);
                    }
                }
            }
            else
            {
                // Try to find UIDocument root and copy its stylesheets
                VisualElement uiDocumentRoot = FindUIDocumentRoot(this);

                if (uiDocumentRoot != null)
                {
                    // Copy all stylesheets from UIDocument root
                    for (int i = 0; i < uiDocumentRoot.styleSheets.count; i++)
                    {
                        var styleSheet = uiDocumentRoot.styleSheets[i];
                        if (!_content.styleSheets.Contains(styleSheet))
                        {
                            _content.styleSheets.Add(styleSheet);
                        }
                    }
                }
                else
                {
                    // Fallback: Load ShunStyle from Resources
                    var shunStyle = Resources.Load<StyleSheet>("ShunStyle");
                    if (shunStyle != null)
                    {
                        _content.styleSheets.Add(shunStyle);
                    }

                    // Also copy from panel root
                    var root = panel?.visualTree;
                    if (root != null)
                    {
                        for (int i = 0; i < root.styleSheets.count; i++)
                        {
                            var styleSheet = root.styleSheets[i];
                            if (!_content.styleSheets.Contains(styleSheet))
                            {
                                _content.styleSheets.Add(styleSheet);
                            }
                        }
                    }
                }
            }

            // Reset the initialization flag so it can be re-initialized if needed
            _stylesheetsInitialized = false;
        }
    }
}