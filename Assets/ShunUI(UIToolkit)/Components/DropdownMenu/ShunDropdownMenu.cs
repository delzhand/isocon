using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ShunUI.Primitives;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunDropdownMenu : ShunPopup
    {
        private ShunButton _trigger;

        [UxmlAttribute]
        public string triggerText
        {
            get => _trigger?.text ?? "Open";
            set
            {
                if (_trigger != null)
                    _trigger.text = value;
            }
        }

        [UxmlAttribute]
        public Texture2D triggerIcon
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
        public VectorImage triggerIconSvg
        {
            get => _trigger?.iconSvg;
            set
            {
                if (_trigger != null)
                    _trigger.iconSvg = value;
            }
        }
#endif

        public ShunDropdownMenu()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("dropdown-menu");

            // Create trigger
            _trigger = new ShunButton();
            _trigger.AddToClassList("menu-trigger");
            _trigger.variant = ButtonVariant.Outline;
            _trigger.text = "Open";
            _trigger.clicked += Toggle;
            
            // Ensure button takes auto width
            _trigger.style.width = StyleKeyword.Auto;
            _trigger.style.flexShrink = 0;
            _trigger.style.flexGrow = 0;
            
            hierarchy.Add(_trigger);

            // Create content using base menu-content style
            _content = new VisualElement();
            _content.AddToClassList("menu-content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            // Create items container using base menu-items style
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("menu-items");
            _content.hierarchy.Add(_itemsContainer);

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                CollectItemsFromUXML();
            });

            UpdatePositionOnGeometryChange();
        }

        private void CollectItemsFromUXML()
        {
            var childrenToMove = new List<VisualElement>();

            foreach (var child in Children())
            {
                if (child != _trigger && child != _content)
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.RemoveFromHierarchy();
                _itemsContainer.Add(child);

                if (child is ShunMenuItemBase menuItem)
                {
                    menuItem.clicked += Close;
                }
            }
        }

        public ShunMenuItemBase AddItem(string label, System.Action onClick = null, string shortcut = "", bool disabled = false)
        {
            var item = new ShunMenuItemBase();
            item.label = label;
            item.shortcut = shortcut;
            item.disabled = disabled;
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

        public override void Open()
        {
            base.Open();
            _trigger?.AddToClassList("trigger--open");
        }

        public override void Close()
        {
            base.Close();
            _trigger?.RemoveFromClassList("trigger--open");
        }

        protected override void PositionContent()
        {
            if (_content == null || _trigger == null) return;

            // Get the viewport height
            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            // Get trigger position in world space
            var triggerRect = _trigger.worldBound;
            var menuHeight = 300; // Max menu height

            // Calculate space above and below
            var spaceBelow = viewportHeight - triggerRect.yMax;
            var spaceAbove = triggerRect.yMin;

            // Determine if dropdown should open upward or downward
            bool openUpward = spaceBelow < menuHeight && spaceAbove > spaceBelow;

            // Position content absolutely at trigger position
            _content.style.position = Position.Absolute;
            _content.style.left = triggerRect.xMin;

            if (openUpward)
            {
                // Open above
                _content.style.top = StyleKeyword.Auto;
                _content.style.bottom = viewportHeight - triggerRect.yMin + 4;
            }
            else
            {
                // Open below (default)
                _content.style.top = triggerRect.yMax + 4;
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