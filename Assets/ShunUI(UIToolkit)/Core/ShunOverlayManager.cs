using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ShunUI
{
    /// <summary>
    /// Global overlay manager that provides a dedicated container at the root level for overlays.
    /// This preserves USS stylesheet context while allowing overlays to escape their parent hierarchy.
    /// </summary>
    public static class ShunOverlayManager
    {
        private static StyleSheet s_ShunStyleSheet;
        private const string OVERLAY_CONTAINER_CLASS = "shun-overlay-container";
        private const string SHUN_STYLE_PATH = "ShunStyle";

        /// <summary>
        /// Gets or creates the global overlay container for the given panel.
        /// The container sits at the root level and maintains stylesheet context.
        /// </summary>
        public static VisualElement GetOverlayContainer(IPanel panel)
        {
            if (panel == null || panel.visualTree == null)
            {
                return null;
            }

            // Load ShunStyle stylesheet if not already loaded
            if (s_ShunStyleSheet == null)
            {
                s_ShunStyleSheet = Resources.Load<StyleSheet>(SHUN_STYLE_PATH);
            }

            // Check if overlay container already exists
            var root = panel.visualTree;
            var overlayContainer = root.Q(className: OVERLAY_CONTAINER_CLASS);

            if (overlayContainer == null)
            {
                // Create new overlay container
                overlayContainer = new VisualElement();
                overlayContainer.name = "ShunOverlayContainer";
                overlayContainer.AddToClassList(OVERLAY_CONTAINER_CLASS);
                
                // Position at root level, taking full screen
                overlayContainer.style.position = Position.Absolute;
                overlayContainer.style.left = 0;
                overlayContainer.style.top = 0;
                overlayContainer.style.right = 0;
                overlayContainer.style.bottom = 0;
                overlayContainer.pickingMode = PickingMode.Ignore; // Allow clicks to pass through by default
                
                // Add ShunStyle stylesheet to container
                if (s_ShunStyleSheet != null)
                {
                    overlayContainer.styleSheets.Add(s_ShunStyleSheet);
                }
                
                // Also copy all stylesheets from root to overlay container
                // But exclude Themes, as they are managed by ShunThemeManager
                for (int i = 0; i < root.styleSheets.count; i++)
                {
                    var styleSheet = root.styleSheets[i];
                    if (styleSheet.name.EndsWith("Theme", System.StringComparison.OrdinalIgnoreCase)) continue;

                    if (!overlayContainer.styleSheets.Contains(styleSheet))
                    {
                        overlayContainer.styleSheets.Add(styleSheet);
                    }
                }
                
                // Add to root
                root.Add(overlayContainer);
            }

            return overlayContainer;
        }

        /// <summary>
        /// Adds an overlay to the global overlay container, escaping its parent hierarchy
        /// while maintaining USS stylesheet context.
        /// </summary>
        public static void AddOverlay(VisualElement overlay, IPanel panel, VisualElement sourceElement)
        {
            if (overlay == null || panel == null) return;

            var container = GetOverlayContainer(panel);
            if (container == null) return;

            // Remove from current parent if any
            overlay.RemoveFromHierarchy();

            // Add ShunStyle stylesheet directly to overlay
            if (s_ShunStyleSheet != null && !overlay.styleSheets.Contains(s_ShunStyleSheet))
            {
                overlay.styleSheets.Add(s_ShunStyleSheet);
            }

            // Also copy stylesheets from root
            // But exclude Themes, as they are managed by ShunThemeManager
            var root = panel.visualTree;
            if (root != null)
            {
                for (int i = 0; i < root.styleSheets.count; i++)
                {
                    var styleSheet = root.styleSheets[i];
                    if (styleSheet.name.EndsWith("Theme", System.StringComparison.OrdinalIgnoreCase)) continue;

                    if (!overlay.styleSheets.Contains(styleSheet))
                    {
                        overlay.styleSheets.Add(styleSheet);
                    }
                }
            }

            // Add overlay to the global container
            container.Add(overlay);
        }

        /// <summary>
        /// Removes an overlay from the global overlay container.
        /// </summary>
        public static void RemoveOverlay(VisualElement overlay)
        {
            if (overlay == null) return;
            overlay.RemoveFromHierarchy();
        }

        /// <summary>
        /// Updates the theme stylesheet on the overlay container and all active overlays.
        /// </summary>
        public static void UpdateTheme(IPanel panel, StyleSheet newTheme, List<StyleSheet> oldThemes)
        {
            var container = GetOverlayContainer(panel);
            if (container == null) return;

            // Remove old themes
            if (oldThemes != null)
            {
                foreach (var oldTheme in oldThemes)
                {
                    if (container.styleSheets.Contains(oldTheme))
                        container.styleSheets.Remove(oldTheme);
                        
                    foreach (var child in container.Children())
                    {
                        if (child.styleSheets.Contains(oldTheme))
                            child.styleSheets.Remove(oldTheme);
                    }
                }
            }
            
            // Add new theme
            if (newTheme != null)
            {
                if (!container.styleSheets.Contains(newTheme))
                    container.styleSheets.Add(newTheme);
                    
                foreach (var child in container.Children())
                {
                    if (!child.styleSheets.Contains(newTheme))
                        child.styleSheets.Add(newTheme);
                }
            }
        }
    }
}
