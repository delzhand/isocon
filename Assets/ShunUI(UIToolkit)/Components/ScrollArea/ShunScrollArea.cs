using ShunUI.Primitives;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunScrollArea : ScrollView
    {
        private bool m_StylesApplied = false;
        private bool m_IsDragging = false;
        private Vector2 m_DragStartPosition;
        private Vector2 m_ScrollStartPosition;
        private bool m_IsInitialized = false;

        public ShunScrollArea()
        {
            AddToClassList("shun-scroll-area");

            // Configure default scroll mode
            mode = ScrollViewMode.Vertical;

            // Update classes based on initial mode
            UpdateOrientationClasses();

            // Set elasticity to disable bouncing
            elasticity = 0;

            // Apply custom styling to scrollers
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Register drag events for drag-to-scroll
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (!m_IsInitialized)
            {
                m_IsInitialized = true;
                OrganizeContent();
            }

            // Apply CSS classes when attached
            schedule.Execute(() =>
            {
                ApplyScrollbarStyles();
                UpdateOrientationClasses();
            }).ExecuteLater(0);
        }

        private void OrganizeContent()
        {
            // Find the content wrapper
            var scrollContent = this.Query<ShunScrollAreaContent>().First();
            if (scrollContent == null) return;

            // Move content wrapper into contentContainer if it's a direct child
            if (scrollContent.parent == this)
            {
                scrollContent.RemoveFromHierarchy();
                contentContainer.Add(scrollContent);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Ensure styles are applied after geometry changes
            if (!m_StylesApplied)
            {
                ApplyScrollbarStyles();
            }
        }

        private void ApplyScrollbarStyles()
        {
            // Apply CSS classes only - let USS handle all styling
            // Use Children() to only get direct children, avoiding nested scroll areas
            var verticalScroller = this.Query<Scroller>(className: "unity-scroller--vertical")
                .Where(s => IsDirectScrollerChild(s))
                .First();
            if (verticalScroller != null)
            {
                verticalScroller.AddToClassList("shun-scroll-area__scroller");
                verticalScroller.AddToClassList("shun-scroll-area__scroller--vertical");
            }

            var horizontalScroller = this.Query<Scroller>(className: "unity-scroller--horizontal")
                .Where(s => IsDirectScrollerChild(s))
                .First();
            if (horizontalScroller != null)
            {
                horizontalScroller.AddToClassList("shun-scroll-area__scroller");
                horizontalScroller.AddToClassList("shun-scroll-area__scroller--horizontal");
            }

            m_StylesApplied = true;
        }

        private bool IsDirectScrollerChild(Scroller scroller)
        {
            // Check if this scroller belongs directly to this ScrollView
            // and not to a nested ScrollView
            var parent = scroller.parent;
            while (parent != null)
            {
                if (parent == this)
                {
                    return true;
                }
                // If we encounter another ScrollView before reaching this one,
                // then the scroller belongs to that nested ScrollView
                if (parent is ScrollView && parent != this)
                {
                    return false;
                }
                parent = parent.parent;
            }
            return false;
        }

        private void UpdateOrientationClasses()
        {
            RemoveFromClassList("shun-scroll-area--vertical");
            RemoveFromClassList("shun-scroll-area--horizontal");
            RemoveFromClassList("shun-scroll-area--both");

            switch (mode)
            {
                case ScrollViewMode.Vertical:
                    AddToClassList("shun-scroll-area--vertical");
                    break;
                case ScrollViewMode.Horizontal:
                    AddToClassList("shun-scroll-area--horizontal");
                    break;
                case ScrollViewMode.VerticalAndHorizontal:
                    AddToClassList("shun-scroll-area--both");
                    break;
            }
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Only start drag if middle mouse button or left mouse button is pressed
            // and not clicking on a scroller
            if (evt.button != 0 && evt.button != 2) return;

            var target = evt.target as VisualElement;
            if (target != null)
            {
                // Don't start drag if clicking on scrollbar or interactive elements
                if (IsScrollbarOrInteractive(target)) return;
            }

            m_IsDragging = true;
            m_DragStartPosition = evt.mousePosition;
            m_ScrollStartPosition = new Vector2(scrollOffset.x, scrollOffset.y);

            // Change cursor to grabbing
            AddToClassList("shun-scroll-area--dragging");

            // Capture mouse to continue receiving events even if pointer leaves element
            this.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_IsDragging) return;

            Vector2 delta = evt.mousePosition - m_DragStartPosition;

            // Invert delta for natural drag behavior (drag down = scroll down)
            Vector2 newScrollOffset = m_ScrollStartPosition - delta;

            // Apply scroll based on orientation
            switch (mode)
            {
                case ScrollViewMode.Vertical:
                    scrollOffset = new Vector2(scrollOffset.x, newScrollOffset.y);
                    break;
                case ScrollViewMode.Horizontal:
                    scrollOffset = new Vector2(newScrollOffset.x, scrollOffset.y);
                    break;
                case ScrollViewMode.VerticalAndHorizontal:
                    scrollOffset = newScrollOffset;
                    break;
            }

            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!m_IsDragging) return;

            m_IsDragging = false;

            // Change cursor back to normal
            RemoveFromClassList("shun-scroll-area--dragging");

            this.ReleaseMouse();
            evt.StopPropagation();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            // Don't stop dragging when mouse leaves if we have mouse capture
            // This allows dragging beyond the scroll area boundaries
        }

        private bool IsScrollbarOrInteractive(VisualElement element)
        {
            // Check if element or any parent is a scroller, button, or other interactive element
            var current = element;
            while (current != null && current != this)
            {
                if (current is Scroller ||
                    current is Button ||
                    current is TextField ||
                    current is Toggle ||
                    current is Slider ||
                    current is ShunSwitch ||
                    current is ShunButton ||
                    current is ShunToggle ||
                    current is ShunCollapsibleTrigger ||
                    current.ClassListContains("unity-scroller") ||
                    current.ClassListContains("unity-base-slider__dragger") ||
                    current.ClassListContains("unity-base-slider__tracker") ||
                    current.ClassListContains("switch-container") ||
                    current.ClassListContains("switch-track") ||
                    current.ClassListContains("switch-label"))
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }

    [UxmlElement]
    public partial class ShunScrollAreaContent : VisualElement
    {
        public ShunScrollAreaContent()
        {
            AddToClassList("shun-scroll-area__content");
        }
    }
}
