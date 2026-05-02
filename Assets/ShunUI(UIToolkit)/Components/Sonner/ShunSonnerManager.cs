using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A utility component that automatically creates ShunSonner instances for all positions.
    /// Add this to your UXML to ensure all toast positions are available.
    /// Example: <shun:ShunSonnerManager />
    /// </summary>
    [UxmlElement]
    public partial class ShunSonnerManager : VisualElement
    {
        private ShunSonner m_TopLeft;
        private ShunSonner m_TopCenter;
        private ShunSonner m_TopRight;
        private ShunSonner m_BottomLeft;
        private ShunSonner m_BottomCenter;
        private ShunSonner m_BottomRight;

        [UxmlAttribute]
        public Texture2D successIcon { get; set; }

        [UxmlAttribute]
        public Texture2D errorIcon { get; set; }

        [UxmlAttribute]
        public Texture2D warningIcon { get; set; }

        [UxmlAttribute]
        public Texture2D infoIcon { get; set; }

        [UxmlAttribute]
        public Texture2D closeIcon { get; set; }

        public ShunSonnerManager()
        {
            // Make this element invisible and non-interactive
            style.position = Position.Absolute;
            style.width = 0;
            style.height = 0;
            style.display = DisplayStyle.None;
            pickingMode = PickingMode.Ignore;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Create all Sonner instances with a small delay to ensure panel is ready
            schedule.Execute(CreateAllSonnerInstances).ExecuteLater(100);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            // Clean up references - don't try to remove from hierarchy as they may already be removed
            m_TopLeft = null;
            m_TopCenter = null;
            m_TopRight = null;
            m_BottomLeft = null;
            m_BottomCenter = null;
            m_BottomRight = null;
        }

        private void CreateAllSonnerInstances()
        {
            if (panel == null) return;

            // Don't create if already created
            if (m_TopLeft != null) return;

            // CRITICAL: Add to parent where this manager is placed, NOT to panel.visualTree
            // This ensures toasts are positioned relative to the correct container
            var container = parent ?? panel.visualTree;

            // Create Sonner instances for all positions
            m_TopLeft = CreateSonner(container, ToastPosition.TopLeft);
            m_TopCenter = CreateSonner(container, ToastPosition.TopCenter);
            m_TopRight = CreateSonner(container, ToastPosition.TopRight);
            m_BottomLeft = CreateSonner(container, ToastPosition.BottomLeft);
            m_BottomCenter = CreateSonner(container, ToastPosition.BottomCenter);
            m_BottomRight = CreateSonner(container, ToastPosition.BottomRight);
        }

        private ShunSonner CreateSonner(VisualElement root, ToastPosition position)
        {
            var sonner = new ShunSonner
            {
                position = position,
                successIcon = successIcon,
                errorIcon = errorIcon,
                warningIcon = warningIcon,
                infoIcon = infoIcon,
                closeIcon = closeIcon
            };
            root.Add(sonner);
            return sonner;
        }
    }
}
