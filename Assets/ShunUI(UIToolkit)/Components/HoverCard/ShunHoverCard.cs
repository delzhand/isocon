using UnityEngine;
using UnityEngine.UIElements;
using ShunUI;
using ShunUI.Primitives;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunHoverCard : ShunPopup
    {
        private IVisualElementScheduledItem _openScheduledItem;
        private IVisualElementScheduledItem _closeScheduledItem;
        private const int HOVER_DELAY = 300; // ms
        private bool m_IsInitialized = false;

        public ShunHoverCard()
        {
            AddToClassList("hover-card");
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;

            // Find the trigger
            var trigger = this.Query<ShunHoverCardTrigger>().First();
            if (trigger == null) return;

            // Find the content wrapper
            var hoverCardContent = this.Query<ShunHoverCardContent>().First();
            if (hoverCardContent == null) return;

            // Move content wrapper if it's a direct child
            if (hoverCardContent.parent == this)
            {
                hoverCardContent.RemoveFromHierarchy();
                
                // Create the popup content container
                _content = new VisualElement();
                _content.AddToClassList("hover-card__content");
                _content.style.display = DisplayStyle.None;
                _content.style.position = Position.Absolute;
                // Don't add to hierarchy yet - will be added to root when opened

                // Move the content wrapper into the popup container
                _content.Add(hoverCardContent);

                // Register hover events
                trigger.RegisterCallback<MouseEnterEvent>(e => ScheduleOpen());
                trigger.RegisterCallback<MouseLeaveEvent>(e => ScheduleClose());
                _content.RegisterCallback<MouseEnterEvent>(e => CancelClose());
                _content.RegisterCallback<MouseLeaveEvent>(e => ScheduleClose());

                UpdatePositionOnGeometryChange();
            }
        }

        private void ScheduleOpen()
        {
            _closeScheduledItem?.Pause();
            _openScheduledItem?.Pause();

            _openScheduledItem = schedule.Execute(() => Open()).StartingIn(HOVER_DELAY);
        }

        private void ScheduleClose()
        {
            _openScheduledItem?.Pause();
            _closeScheduledItem?.Pause();

            _closeScheduledItem = schedule.Execute(() => Close()).StartingIn(200);
        }

        private void CancelClose()
        {
            _closeScheduledItem?.Pause();
        }

        protected override void PositionContent()
        {
            if (!isOpen || _content == null) return;

            var trigger = this.Query<ShunHoverCardTrigger>().First();
            if (trigger == null) return;

            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            var triggerRect = trigger.worldBound;
            var cardHeight = 200;

            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            bool openUpward = spaceBelow < cardHeight && spaceAbove > spaceBelow;

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
            var trigger = this.Query<ShunHoverCardTrigger>().First();
            return (_content != null && _content.Contains(clickedElement)) ||
                   (trigger != null && trigger.Contains(clickedElement));
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunHoverCardTrigger : ShunButton
    {
        public ShunHoverCardTrigger()
        {
            variant = ButtonVariant.Outline;
            AddToClassList("hover-card__trigger");
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunHoverCardContent : VisualElement
    {
        public ShunHoverCardContent()
        {
            AddToClassList("hover-card__inner");
        }
    }
}
