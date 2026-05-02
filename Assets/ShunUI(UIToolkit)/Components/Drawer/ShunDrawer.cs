using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunDrawer : VisualElement
    {
        [UxmlAttribute]
        public bool isOpen
        {
            get => m_IsOpen;
            set
            {
                m_IsOpen = value;
                UpdateVisibility();
            }
        }

        private bool m_IsOpen = false;
        private VisualElement m_Overlay;
        private VisualElement m_DrawerContainer;
        private VisualElement m_ContentContainer;
        private bool m_IsInitialized = false;
        private bool m_StylesheetsInitialized = false;

        public ShunDrawer()
        {
            AddToClassList("shun-drawer");
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;
            
            // Find the ShunDrawerContent element
            var drawerContent = this.Query<ShunDrawerContent>().First();
            if (drawerContent == null) return;
            
            // Create overlay when needed
            if (m_Overlay == null)
            {
                m_Overlay = new VisualElement();
                m_Overlay.AddToClassList("shun-drawer__overlay");
                m_Overlay.style.position = Position.Absolute;
                m_Overlay.style.display = DisplayStyle.None;
                m_Overlay.pickingMode = PickingMode.Ignore;

                // Create drawer container
                m_DrawerContainer = new VisualElement();
                m_DrawerContainer.AddToClassList("shun-drawer__container");
                m_DrawerContainer.pickingMode = PickingMode.Position;
                m_Overlay.Add(m_DrawerContainer);

                // Create content container
                m_ContentContainer = new VisualElement();
                m_ContentContainer.AddToClassList("shun-drawer__content");
                m_DrawerContainer.Add(m_ContentContainer);
            }
            
            // Move the drawer content to the overlay (only once)
            if (drawerContent.parent == this)
            {
                drawerContent.RemoveFromHierarchy();
                m_ContentContainer.Add(drawerContent);
                
                // Add overlay to this element
                hierarchy.Add(m_Overlay);
            }
            
            // Wire up all buttons in the drawer to close it (except those in header/body)
            schedule.Execute(() => {
                WireDrawerButtons();
            }).ExecuteLater(0);
            
            // Ensure overlay has stylesheet references
            EnsureOverlayStylesheets();
            
            schedule.Execute(() => {
                SetupOverlayPosition();
            });
            
            UpdateVisibility();
        }

        private void WireDrawerButtons()
        {
            if (m_ContentContainer == null) return;
            
            // Find all buttons in the footer
            var footerContainers = m_ContentContainer.Query<VisualElement>(className: "shun-drawer__footer").ToList();
            foreach (var footer in footerContainers)
            {
                var buttons = footer.Query<ShunButton>().ToList();
                foreach (var button in buttons)
                {
                    // Wire close functionality - including ShunDrawerClose since it may not find ancestor after move
                    if (button is ShunDrawerClose closeButton)
                    {
                        // Store reference to this drawer for close buttons
                        closeButton.SetDrawer(this);
                    }
                    else
                    {
                        button.clicked += () => Close();
                    }
                }
            }
        }
        
        private void SetupOverlayPosition()
        {
            if (m_Overlay == null) return;
            
            m_Overlay.style.position = Position.Absolute;
            m_Overlay.style.left = 0;
            m_Overlay.style.top = 0;
            m_Overlay.style.right = 0;
            m_Overlay.style.bottom = 0;
            
            // Update overlay size based on parent container geometry changes
            void UpdateOverlaySize()
            {
                if (m_Overlay == null) return;
                
                // At runtime in global container, use pixel-based sizing
                if (Application.isPlaying && m_Overlay.parent != this)
                {
                    var sizeSource = Vector2.zero;
                    
                    if (m_Overlay.parent != null)
                    {
                        var parentBounds = m_Overlay.parent.layout;
                        sizeSource = new Vector2(parentBounds.width, parentBounds.height);
                    }
                    
                    if (sizeSource == Vector2.zero)
                    {
                        sizeSource = new Vector2(1920, 1080);
                    }
                    
                    m_Overlay.style.width = sizeSource.x;
                    m_Overlay.style.height = sizeSource.y;
                }
                // In UI Builder or when child of this element, use flexible sizing
                else
                {
                    m_Overlay.style.width = StyleKeyword.Auto;
                    m_Overlay.style.height = StyleKeyword.Auto;
                }
            }
            
            // Listen to geometry changes on the overlay
            m_Overlay.RegisterCallback<GeometryChangedEvent>(evt => UpdateOverlaySize());
            
            // Also listen to this element's geometry changes (for UI Builder)
            RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlaySize());
            
            // Listen to parent geometry changes
            if (parent != null)
            {
                parent.RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlaySize());
            }
            
            // Set initial values
            schedule.Execute(UpdateOverlaySize).ExecuteLater(0);
            
            // Register click handlers
            m_Overlay.RegisterCallback<ClickEvent>(OnRootClick, TrickleDown.TrickleDown);
        }

        private void OnRootClick(ClickEvent evt)
        {
            if (!m_IsOpen) return;

            // Check if click is inside the drawer container
            if (m_DrawerContainer != null && m_DrawerContainer.worldBound.Contains(evt.position))
            {
                return; // Click inside drawer, do nothing
            }

            // Click outside drawer, close it
            Close();
        }

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
        }
        
        private void EnsureOverlayStylesheets()
        {
            if (m_Overlay == null || m_StylesheetsInitialized) return;
            
            // Load ShunStyle stylesheet from Resources (works at runtime)
            var shunStyle = Resources.Load<StyleSheet>("ShunStyle");
            if (shunStyle != null && !m_Overlay.styleSheets.Contains(shunStyle))
            {
                m_Overlay.styleSheets.Add(shunStyle);
            }
            
            // Copy stylesheets from panel root (works in UI Builder)
            if (panel != null && panel.visualTree != null)
            {
                var root = panel.visualTree;
                for (int i = 0; i < root.styleSheets.count; i++)
                {
                    var styleSheet = root.styleSheets[i];
                    // Exclude Themes in Play Mode as they are managed by ShunThemeManager
                    if (Application.isPlaying && styleSheet.name.EndsWith("Theme", System.StringComparison.OrdinalIgnoreCase)) continue;

                    if (!m_Overlay.styleSheets.Contains(styleSheet))
                    {
                        m_Overlay.styleSheets.Add(styleSheet);
                    }
                }
            }
            
            // Fallback: Copy from this element (for UI Builder preview)
            if (m_Overlay.styleSheets.count == 0)
            {
                for (int i = 0; i < styleSheets.count; i++)
                {
                    var styleSheet = styleSheets[i];
                    if (!m_Overlay.styleSheets.Contains(styleSheet))
                    {
                        m_Overlay.styleSheets.Add(styleSheet);
                    }
                }
            }
            
            m_StylesheetsInitialized = true;
        }

        private void UpdateVisibility()
        {
            if (m_Overlay != null)
            {
                if (m_IsOpen)
                {
                    if (Application.isPlaying)
                    {
                        // Clear and re-ensure stylesheets to pick up any theme changes
                        ClearOverlayStylesheets();
                        EnsureOverlayStylesheets();

                        // At runtime, move overlay to global container for full-screen coverage
                        if (panel != null)
                        {
                            ShunOverlayManager.AddOverlay(m_Overlay, panel, this);
                        }
                    }
                    else
                    {
                        // In UI Builder, find the canvas element that contains the preview
                        VisualElement canvasParent = FindUIBuilderCanvas();
                        
                        if (canvasParent != null && m_Overlay.parent != canvasParent)
                        {
                            EnsureOverlayStylesheets();
                            canvasParent.Add(m_Overlay);
                        }
                    }
                    
                    m_Overlay.style.display = DisplayStyle.Flex;
                    m_Overlay.pickingMode = PickingMode.Position;
                    
                    // Force layout update before adding the open class
                    // This ensures the initial state (translate 0 100%) is applied before transitioning
                    schedule.Execute(() =>
                    {
                        // Check if we are still open
                        if (!m_IsOpen) return;

                        if (m_DrawerContainer != null)
                        {
                            m_DrawerContainer.AddToClassList("shun-drawer__container--open");
                        }
                        
                        if (m_Overlay != null)
                        {
                            m_Overlay.MarkDirtyRepaint();
                        }
                    }).ExecuteLater(10);
                }
                else
                {
                    // Remove open class
                    if (m_DrawerContainer != null)
                    {
                        m_DrawerContainer.RemoveFromClassList("shun-drawer__container--open");
                    }
                    
                    // Delay hiding to allow close animation
                    schedule.Execute(() =>
                    {
                        // Check if we are still closed
                        if (m_IsOpen) return;

                        // Remove overlay and move back to this element
                        if (Application.isPlaying && panel != null)
                        {
                            ShunOverlayManager.RemoveOverlay(m_Overlay);
                        }
                        
                        // Move back to this element
                        if (m_Overlay.parent != this)
                        {
                            hierarchy.Add(m_Overlay);
                        }
                        
                        // Clear stylesheets when closing so we don't hold onto old themes
                        ClearOverlayStylesheets();
                        
                        m_Overlay.style.display = DisplayStyle.None;
                        m_Overlay.pickingMode = PickingMode.Ignore;
                    }).ExecuteLater(300); // Match transition duration
                }
            }
        }

        private void ClearOverlayStylesheets()
        {
            if (m_Overlay == null) return;
            m_Overlay.styleSheets.Clear();
            m_StylesheetsInitialized = false;
        }

        /// <summary>
        /// Finds the UI Builder document element by traversing up the parent hierarchy.
        /// Returns the "document" TemplateContainer which represents the full canvas area.
        /// </summary>
        private VisualElement FindUIBuilderCanvas()
        {
            VisualElement current = this;
            while (current != null)
            {
                // Look for the document element (TemplateContainer with name "document")
                if (current.name == "document" && current is TemplateContainer)
                {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }
    }

    [UxmlElement]
    public partial class ShunDrawerTrigger : ShunButton
    {
        public ShunDrawerTrigger()
        {
            AddToClassList("shun-drawer__trigger");
            clicked += OnClicked;
        }

        private void OnClicked()
        {
            var drawer = GetFirstAncestorOfType<ShunDrawer>();
            if (drawer != null)
            {
                drawer.Open();
            }
        }
    }

    [UxmlElement]
    public partial class ShunDrawerContent : VisualElement
    {
        public ShunDrawerContent()
        {
            AddToClassList("shun-drawer__content-wrapper");
        }
    }

    [UxmlElement]
    public partial class ShunDrawerTitle : Label
    {
        public ShunDrawerTitle()
        {
            AddToClassList("shun-drawer__title");
        }
    }

    [UxmlElement]
    public partial class ShunDrawerDescription : Label
    {
        public ShunDrawerDescription()
        {
            AddToClassList("shun-drawer__description");
        }
    }

    [UxmlElement]
    public partial class ShunDrawerClose : ShunButton
    {
        private ShunDrawer m_Drawer;

        public ShunDrawerClose()
        {
            clicked += OnClicked;
        }

        public void SetDrawer(ShunDrawer drawer)
        {
            m_Drawer = drawer;
        }

        private void OnClicked()
        {
            // Try stored reference first
            if (m_Drawer != null)
            {
                m_Drawer.Close();
                return;
            }

            // Fallback to ancestor search (for cases where drawer wasn't moved)
            var drawer = GetFirstAncestorOfType<ShunDrawer>();
            if (drawer != null)
            {
                drawer.Close();
            }
        }
    }
}
