using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using ShunUI.Primitives;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunPopover : ShunPopup
    {
        private bool m_IsInitialized = false;

        public ShunPopover()
        {
            AddToClassList("shun-popover");
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;

            // Find the trigger
            var trigger = this.Query<ShunPopoverTrigger>().First();
            if (trigger == null) return;

            // Find the content wrapper
            var popoverContent = this.Query<ShunPopoverContent>().First();
            if (popoverContent == null) return;

            // Move content wrapper if it's a direct child
            if (popoverContent.parent == this)
            {
                popoverContent.RemoveFromHierarchy();
                
                // Create the popup content container
                _content = new VisualElement();
                _content.AddToClassList("shun-popover__content");
                _content.style.display = DisplayStyle.None;
                _content.style.position = Position.Absolute;
                // Don't add to hierarchy yet - will be added to root when opened

                // Move the content wrapper into the popup container
                _content.Add(popoverContent);

                // Register click event on trigger
                trigger.RegisterCallback<ClickEvent>(e => Toggle());

                UpdatePositionOnGeometryChange();
            }
        }

        protected override void PositionContent()
        {
            if (_content == null) return;

            var trigger = this.Query<ShunPopoverTrigger>().First();
            if (trigger == null) return;

            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            var triggerRect = trigger.worldBound;
            var popoverHeight = 200; // Estimated max height

            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            bool openUpward = spaceBelow < popoverHeight && spaceAbove > spaceBelow;

            // Position content absolutely at trigger position
            _content.style.position = Position.Absolute;
            _content.style.left = triggerRect.xMin;

            if (openUpward)
            {
                // Open above
                _content.style.top = StyleKeyword.Auto;
                _content.style.bottom = viewportHeight - triggerRect.yMin + 8;
            }
            else
            {
                // Open below (default)
                _content.style.top = triggerRect.yMax + 8;
                _content.style.bottom = StyleKeyword.Auto;
            }
        }

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            var trigger = this.Query<ShunPopoverTrigger>().First();
            return (_content != null && _content.Contains(clickedElement)) ||
                   (trigger != null && trigger.Contains(clickedElement));
        }
    }

    [UxmlElement]
    public partial class ShunPopoverTrigger : ShunButton
    {
        public ShunPopoverTrigger()
        {
            AddToClassList("shun-popover__trigger");
            variant = ButtonVariant.Outline;
        }
    }

    [UxmlElement]
    public partial class ShunPopoverContent : VisualElement
    {
        public ShunPopoverContent()
        {
            AddToClassList("shun-popover__inner");
        }
    }
}
