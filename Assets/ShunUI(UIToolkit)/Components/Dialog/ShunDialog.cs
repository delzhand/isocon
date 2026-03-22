using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunDialog : VisualElement
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
        private VisualElement m_DialogContainer;
        private VisualElement m_ContentContainer;
        private bool m_IsInitialized = false;
        private bool m_StylesheetsInitialized = false;

        public Action CloseAction;

        public ShunDialog()
        {
            AddToClassList("shun-dialog");

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;

            // Find the ShunDialogContent element
            var dialogContent = this.Query<ShunDialogContent>().First();
            if (dialogContent == null) return;

            // Set a name so we can find this element later
            dialogContent.name = $"{parent.name}-Contents";

            // Create overlay when needed
            if (m_Overlay == null)
            {
                m_Overlay = new VisualElement();
                m_Overlay.AddToClassList("shun-dialog__overlay");
                m_Overlay.style.position = Position.Absolute;
                m_Overlay.style.display = DisplayStyle.None;
                m_Overlay.pickingMode = PickingMode.Ignore;

                // Create dialog container
                m_DialogContainer = new VisualElement();
                m_DialogContainer.AddToClassList("shun-dialog__container");
                m_DialogContainer.pickingMode = PickingMode.Position;
                m_Overlay.Add(m_DialogContainer);

                // Create content container
                m_ContentContainer = new VisualElement();
                m_ContentContainer.AddToClassList("shun-dialog__content");
                m_DialogContainer.Add(m_ContentContainer);
            }

            // Move the dialog content to the overlay (only once)
            if (dialogContent.parent == this)
            {
                dialogContent.RemoveFromHierarchy();
                m_ContentContainer.Add(dialogContent);

                // Add overlay to this element
                hierarchy.Add(m_Overlay);
            }

            // Wire up all buttons in the dialog to close it
            schedule.Execute(() =>
            {
                WireDialogButtons();
            }).ExecuteLater(0);

            // Ensure overlay has stylesheet references
            EnsureOverlayStylesheets();

            schedule.Execute(() =>
            {
                SetupOverlayPosition();
            });

            UpdateVisibility();
        }

        private void WireDialogButtons()
        {
            if (m_ContentContainer == null) return;

            // Find all buttons in the footer
            var footerContainers = m_ContentContainer.Query<VisualElement>(className: "shun-dialog__footer").ToList();
            foreach (var footer in footerContainers)
            {
                var buttons = footer.Query<ShunButton>().ToList();
                foreach (var button in buttons)
                {
                    // Wire close functionality - including ShunDialogClose since it may not find ancestor after move
                    if (button is ShunDialogClose closeButton)
                    {
                        // Store reference to this dialog for close buttons
                        closeButton.SetDialog(this);
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

            // Check if click is inside the dialog container
            if (m_DialogContainer != null && m_DialogContainer.worldBound.Contains(evt.position))
            {
                return; // Click inside dialog, do nothing
            }

            // Click outside dialog, close it
            Close();
        }

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
            if (CloseAction != null)
            {
                CloseAction.Invoke();
                CloseAction = null;
            }
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

                    // Force layout update
                    schedule.Execute(() =>
                    {
                        if (m_Overlay != null)
                        {
                            m_Overlay.MarkDirtyRepaint();
                        }
                    }).ExecuteLater(0);
                }
                else
                {
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
    public partial class ShunDialogTrigger : ShunButton
    {
        public ShunDialogTrigger()
        {
            AddToClassList("shun-dialog__trigger");
            clicked += OnClicked;
        }

        private void OnClicked()
        {
            var dialog = GetFirstAncestorOfType<ShunDialog>();
            if (dialog != null)
            {
                dialog.Open();
            }
        }
    }

    [UxmlElement]
    public partial class ShunDialogContent : VisualElement
    {
        public ShunDialogContent()
        {
            AddToClassList("shun-dialog__content-wrapper");
        }
    }

    [UxmlElement]
    public partial class ShunDialogTitle : Label
    {
        public ShunDialogTitle()
        {
            AddToClassList("shun-dialog__title");
        }
    }

    [UxmlElement]
    public partial class ShunDialogDescription : Label
    {
        public ShunDialogDescription()
        {
            AddToClassList("shun-dialog__description");
        }
    }

    [UxmlElement]
    public partial class ShunDialogClose : ShunButton
    {
        private ShunDialog m_Dialog;

        public ShunDialogClose()
        {
            clicked += OnClicked;
        }

        public void SetDialog(ShunDialog dialog)
        {
            m_Dialog = dialog;
        }

        private void OnClicked()
        {
            // Try stored reference first
            if (m_Dialog != null)
            {
                m_Dialog.Close();
                return;
            }

            // Fallback to ancestor search (for cases where dialog wasn't moved)
            var dialog = GetFirstAncestorOfType<ShunDialog>();
            if (dialog != null)
            {
                dialog.Close();
            }
        }
    }
}
