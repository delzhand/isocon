using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ShunUI;
using ShunUI.Primitives;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunHoverMenu : ShunPopup
    {
        private ShunButton _trigger;
        private IVisualElementScheduledItem _openScheduledItem;
        private IVisualElementScheduledItem _closeScheduledItem;
        private const int HOVER_DELAY = 300; // ms

        [UxmlAttribute]
        public string label
        {
            get => _trigger?.text ?? "";
            set
            {
                if (_trigger != null)
                    _trigger.text = value;
            }
        }

        [UxmlAttribute]
        public Texture2D icon
        {
            get => _trigger?.icon;
            set
            {
                if (_trigger != null)
                    _trigger.icon = value;
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg
        {
            get => _trigger?.iconSvg;
            set
            {
                if (_trigger != null)
                    _trigger.iconSvg = value;
            }
        }
#endif

        public ShunHoverMenu()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("hover-menu");

            // Create trigger as Button
            _trigger = new ShunButton();
            _trigger.AddToClassList("menu-trigger");
            _trigger.variant = ButtonVariant.Outline;
            hierarchy.Add(_trigger);

            // Create menu container using base class _content
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            // Create items container
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("menu-items");
            _content.Add(_itemsContainer);

            // Register hover events
            _trigger.RegisterCallback<MouseEnterEvent>(evt => ScheduleOpen());
            _trigger.RegisterCallback<MouseLeaveEvent>(evt => ScheduleClose());
            _content.RegisterCallback<MouseEnterEvent>(evt => CancelClose());
            _content.RegisterCallback<MouseLeaveEvent>(evt => ScheduleClose());

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectContentFromUXML();
            });

            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (isOpen) PositionContent();
            });
        }

        private void CollectContentFromUXML()
        {
            // First child goes to trigger, rest go to menu
            var children = new List<VisualElement>();
            foreach (var child in Children())
            {
                if (child != _trigger && child != _content)
                {
                    children.Add(child);
                }
            }

            if (children.Count > 0)
            {
                // First element is the trigger content
                children[0].RemoveFromHierarchy();
                _trigger.Add(children[0]);

                // Remaining elements are menu items
                for (int i = 1; i < children.Count; i++)
                {
                    children[i].RemoveFromHierarchy();
                    _itemsContainer.Add(children[i]);

                    if (children[i] is ShunMenuItemBase menuItem)
                    {
                        menuItem.clicked += Close;
                    }
                }
            }
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

            item.clicked += Close;
            _itemsContainer.Add(item);

            return item;
        }

        public void AddSeparator()
        {
            var separator = new ShunMenuSeparator();
            _itemsContainer.Add(separator);
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
            if (_content == null || _trigger == null) return;

            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            var triggerRect = _trigger.worldBound;
            var menuHeight = 300;

            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            bool openUpward = spaceBelow < menuHeight && spaceAbove > spaceBelow;

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
            return (_content != null && _content.Contains(clickedElement)) ||
                   (_trigger != null && _trigger.Contains(clickedElement));
        }
    }
}

