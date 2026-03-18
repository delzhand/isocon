using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunSheet : VisualElement
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

        [UxmlAttribute]
        public string side
        {
            get => m_Side;
            set
            {
                m_Side = value;
                SetSide(value);
            }
        }

        private bool m_IsOpen = false;
        private string m_Side = "right";
        private VisualElement m_Overlay;
        private VisualElement m_SheetContainer;
        private bool m_IsInitialized = false;
        private bool m_StylesheetsInitialized = false;

        public ShunSheet()
        {
            AddToClassList("shun-sheet");
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;

            // Find the trigger (optional)
            var trigger = this.Query<ShunSheetTrigger>().First();

            // Find the content wrapper
            var sheetContent = this.Query<ShunSheetContent>().First();
            if (sheetContent == null) return;

            // Move content wrapper if it's a direct child
            if (sheetContent.parent == this)
            {
                sheetContent.RemoveFromHierarchy();

                // Create overlay immediately (for UI Builder preview)
                m_Overlay = new VisualElement();
                m_Overlay.AddToClassList("shun-sheet__overlay");
                m_Overlay.style.position = Position.Absolute;
                m_Overlay.style.display = DisplayStyle.None; // Hidden by default
                m_Overlay.pickingMode = PickingMode.Ignore; // Allow clicks to pass through when hidden
                hierarchy.Add(m_Overlay);

                // Create sheet container
                m_SheetContainer = new VisualElement();
                m_SheetContainer.AddToClassList("shun-sheet__container");
                m_SheetContainer.pickingMode = PickingMode.Position; // Re-enable picking for sheet content
                m_Overlay.Add(m_SheetContainer);

                // Move the content wrapper into the sheet container
                m_SheetContainer.Add(sheetContent);

                UpdateVisibility();
                
                // Wire up all buttons in the sheet to close it
                schedule.Execute(() => {
                    WireSheetButtons();
                }).ExecuteLater(0);
                
                // Ensure overlay has stylesheet references
                EnsureOverlayStylesheets();
                
                schedule.Execute(() => {
                    SetSide(m_Side);
                    SetupOverlayPosition();
                });
            }
        }
        
        private void WireSheetButtons()
        {
            if (m_SheetContainer == null) return;
            
            // Find all buttons in the footer
            var footerContainers = m_SheetContainer.Query<VisualElement>(className: "shun-sheet__footer").ToList();
            foreach (var footer in footerContainers)
            {
                var buttons = footer.Query<ShunButton>().ToList();
                foreach (var button in buttons)
                {
                    // Wire close functionality - including ShunSheetClose since it may not find ancestor after move
                    if (button is ShunSheetClose closeButton)
                    {
                        // Store reference to this sheet for close buttons
                        closeButton.SetSheet(this);
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

            // Check if click is inside the sheet container
            if (m_SheetContainer != null && m_SheetContainer.worldBound.Contains(evt.position))
            {
                return; // Click inside sheet, do nothing
            }

            // Click outside sheet, close it
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

        private void SetSide(string value)
        {
            RemoveFromClassList("shun-sheet--left");
            RemoveFromClassList("shun-sheet--right");
            RemoveFromClassList("shun-sheet--top");
            RemoveFromClassList("shun-sheet--bottom");

            if (m_SheetContainer != null)
            {
                m_SheetContainer.RemoveFromClassList("shun-sheet--left");
                m_SheetContainer.RemoveFromClassList("shun-sheet--right");
                m_SheetContainer.RemoveFromClassList("shun-sheet--top");
                m_SheetContainer.RemoveFromClassList("shun-sheet--bottom");
            }

            string className = "shun-sheet--right";
            switch (value.ToLower())
            {
                case "left": className = "shun-sheet--left"; break;
                case "right": className = "shun-sheet--right"; break;
                case "top": className = "shun-sheet--top"; break;
                case "bottom": className = "shun-sheet--bottom"; break;
            }
            
            AddToClassList(className);
            if (m_SheetContainer != null)
            {
                m_SheetContainer.AddToClassList(className);
            }
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
                    // This ensures the initial state (translate off-screen) is applied before transitioning
                    schedule.Execute(() =>
                    {
                        // Check if we are still open
                        if (!m_IsOpen) return;

                        if (m_SheetContainer != null)
                        {
                            m_SheetContainer.AddToClassList("shun-sheet__container--open");
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
                    if (m_SheetContainer != null)
                    {
                        m_SheetContainer.RemoveFromClassList("shun-sheet__container--open");
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
    public partial class ShunSheetTrigger : ShunButton
    {
        public ShunSheetTrigger()
        {
            AddToClassList("shun-sheet__trigger");
            clicked += OnClicked;
        }

        private void OnClicked()
        {
            var sheet = GetFirstAncestorOfType<ShunSheet>();
            if (sheet != null)
            {
                sheet.Open();
            }
        }
    }

    [UxmlElement]
    public partial class ShunSheetContent : VisualElement
    {
        public ShunSheetContent()
        {
            AddToClassList("shun-sheet__content");
        }
    }

    [UxmlElement]
    public partial class ShunSheetClose : ShunButton
    {
        private ShunSheet m_Sheet;

        public ShunSheetClose()
        {
            clicked += OnClicked;
        }

        public void SetSheet(ShunSheet sheet)
        {
            m_Sheet = sheet;
        }

        private void OnClicked()
        {
            // Try stored reference first
            if (m_Sheet != null)
            {
                m_Sheet.Close();
                return;
            }

            // Fallback to ancestor search (for cases where sheet wasn't moved)
            var sheet = GetFirstAncestorOfType<ShunSheet>();
            if (sheet != null)
            {
                sheet.Close();
            }
        }
    }
}
