using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ShunUI.Primitives;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunNavigationMenu : VisualElement
    {
        private VisualElement m_MenuContainer;

        public override VisualElement contentContainer => m_MenuContainer ?? this;

        public ShunNavigationMenu()
        {
            AddToClassList("shun-navigation-menu");

            m_MenuContainer = new VisualElement();
            m_MenuContainer.AddToClassList("shun-navigation-menu__container");
            hierarchy.Add(m_MenuContainer);
        }
    }

    [UxmlElement]
    public partial class ShunNavigationMenuItem : ShunPopup
    {
        [UxmlAttribute]
        public string label { get; set; }

        [UxmlAttribute]
        public Texture2D icon { get; set; }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg { get; set; }
#endif

        [UxmlAttribute]
        public new bool isOpen
        {
            get => base.isOpen;
            set => base.isOpen = value;
        }

        private ShunButton m_TriggerButton;
        private VisualElement m_IconElement;
        private Label m_LabelElement;
        private IVisualElementScheduledItem m_CloseScheduledItem;

        public ShunNavigationMenuItem()
        {
            AddToClassList("shun-navigation-menu__item");

            m_TriggerButton = new ShunButton();
            m_TriggerButton.variant = ButtonVariant.Ghost;
            m_TriggerButton.clicked += OnTriggerClick;
            m_TriggerButton.AddToClassList("shun-navigation-menu__trigger");
            
            // Create label element for text
            m_LabelElement = new Label();
            m_LabelElement.AddToClassList("shun-navigation-menu__trigger-label");
            m_TriggerButton.Add(m_LabelElement);
            
            // Create icon element (on the right)
            m_IconElement = new VisualElement();
            m_IconElement.AddToClassList("shun-navigation-menu__trigger-icon");
            m_IconElement.style.display = DisplayStyle.None;
            m_TriggerButton.Add(m_IconElement);
            
            hierarchy.Add(m_TriggerButton);

            _content = new VisualElement();
            _content.AddToClassList("shun-navigation-menu__content");
            _content.style.display = DisplayStyle.None;
            _content.style.position = Position.Absolute;
            // Don't add to hierarchy yet - will be added to root when opened

            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("shun-navigation-menu__items");
            _content.Add(_itemsContainer);

            // Add hover events for better UX
            m_TriggerButton.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            m_TriggerButton.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            _content.RegisterCallback<MouseEnterEvent>(OnContentMouseEnter);
            _content.RegisterCallback<MouseLeaveEvent>(OnContentMouseLeave);

            UpdatePositionOnGeometryChange();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        public override VisualElement contentContainer => _itemsContainer ?? this;

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            schedule.Execute(() =>
            {
                if (!string.IsNullOrEmpty(label))
                {
                    m_LabelElement.text = label;
                }

                // Handle icon
                bool hasIcon = icon != null;
#if UNITY_6000_3_OR_NEWER
                hasIcon = hasIcon || iconSvg != null;
                if (iconSvg != null)
                {
                    m_IconElement.style.backgroundImage = new StyleBackground(iconSvg);
                    m_IconElement.style.display = DisplayStyle.Flex;
                }
                else
#endif
                if (icon != null)
                {
                    m_IconElement.style.backgroundImage = new StyleBackground(icon);
                    m_IconElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_IconElement.style.backgroundImage = null;
                    m_IconElement.style.display = DisplayStyle.None;
                }

                // Check if this item has content (submenu)
                var hasContent = _itemsContainer.childCount > 0;
                if (hasContent)
                {
                    m_TriggerButton.AddToClassList("shun-navigation-menu__trigger--has-content");
                }
            }).StartingIn(50);
        }

        private void OnTriggerClick()
        {
            var hasContent = _itemsContainer.childCount > 0;
            if (hasContent)
            {
                Toggle();
            }
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            var hasContent = _itemsContainer.childCount > 0;
            if (hasContent)
            {
                // Cancel any pending close
                m_CloseScheduledItem?.Pause();

                // Close any other open navigation menu items first
                CloseOtherNavigationMenus();

                if (!isOpen)
                {
                    Open();
                }
            }
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            var hasContent = _itemsContainer.childCount > 0;
            if (hasContent && isOpen)
            {
                ScheduleClose();
            }
        }

        private void OnContentMouseEnter(MouseEnterEvent evt)
        {
            m_CloseScheduledItem?.Pause();
        }

        private void OnContentMouseLeave(MouseLeaveEvent evt)
        {
            if (isOpen)
            {
                ScheduleClose();
            }
        }

        private void ScheduleClose()
        {
            m_CloseScheduledItem?.Pause();
            m_CloseScheduledItem = schedule.Execute(() => Close()).StartingIn(100);
        }

        protected override void PositionContent()
        {
            if (_content == null || m_TriggerButton == null) return;

            var viewportHeight = panel?.visualTree?.layout.height ?? 0;
            if (viewportHeight == 0) return;

            var triggerRect = m_TriggerButton.worldBound;
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
                _content.style.bottom = viewportHeight - triggerRect.yMin;
            }
            else
            {
                // Open below (default)
                _content.style.top = triggerRect.yMax;
                _content.style.bottom = StyleKeyword.Auto;
            }
        }

        protected override bool IsClickInside(VisualElement clickedElement)
        {
            return (_content != null && _content.Contains(clickedElement)) ||
                   (m_TriggerButton != null && m_TriggerButton.Contains(clickedElement));
        }

        private void CloseOtherNavigationMenus()
        {
            // Find parent navigation menu
            var navigationMenu = GetFirstAncestorOfType<ShunNavigationMenu>();
            if (navigationMenu == null) return;

            // Close all other menu items
            var allMenuItems = navigationMenu.Query<ShunNavigationMenuItem>().ToList();
            foreach (var item in allMenuItems)
            {
                if (item != this && item.isOpen)
                {
                    item.Close();
                }
            }
        }
    }

    [UxmlElement]
    public partial class ShunNavigationMenuList : VisualElement
    {
        public ShunNavigationMenuList()
        {
            AddToClassList("shun-navigation-menu__list");
        }
    }

    [UxmlElement]
    public partial class ShunNavigationMenuListItem : VisualElement
    {
        [UxmlAttribute]
        public string title { get; set; }

        [UxmlAttribute]
        public string description { get; set; }

        [UxmlAttribute]
        public Texture2D icon { get; set; }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg { get; set; }
#endif

        private VisualElement m_IconElement;
        private Label m_TitleLabel;
        private Label m_DescriptionLabel;

        public ShunNavigationMenuListItem()
        {
            AddToClassList("shun-navigation-menu__list-item");

            // Create icon element
            m_IconElement = new VisualElement();
            m_IconElement.AddToClassList("shun-navigation-menu__list-item-icon");
            m_IconElement.style.display = DisplayStyle.None;
            Add(m_IconElement);

            // Create text container
            var textContainer = new VisualElement();
            textContainer.AddToClassList("shun-navigation-menu__list-item-text");
            Add(textContainer);

            m_TitleLabel = new Label();
            m_TitleLabel.AddToClassList("shun-navigation-menu__list-item-title");
            textContainer.Add(m_TitleLabel);

            m_DescriptionLabel = new Label();
            m_DescriptionLabel.AddToClassList("shun-navigation-menu__list-item-description");
            textContainer.Add(m_DescriptionLabel);

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                schedule.Execute(() =>
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        m_TitleLabel.text = title;
                    }
                    
                    // Handle icon
                    bool hasIcon = icon != null;
#if UNITY_6000_3_OR_NEWER
                    hasIcon = hasIcon || iconSvg != null;
                    if (iconSvg != null)
                    {
                        m_IconElement.style.backgroundImage = new StyleBackground(iconSvg);
                        m_IconElement.style.display = DisplayStyle.Flex;
                    }
                    else
#endif
                    if (icon != null)
                    {
                        m_IconElement.style.backgroundImage = new StyleBackground(icon);
                        m_IconElement.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        m_IconElement.style.backgroundImage = null;
                        m_IconElement.style.display = DisplayStyle.None;
                    }
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        m_DescriptionLabel.text = description;
                        m_DescriptionLabel.style.display = DisplayStyle.Flex;
                        m_TitleLabel.style.marginBottom = 4;
                    }
                    else
                    {
                        m_DescriptionLabel.style.display = DisplayStyle.None;
                        m_TitleLabel.style.marginBottom = 0;
                        AddToClassList("shun-navigation-menu__list-item--compact");
                    }
                }).StartingIn(50);
            });
        }
    }

    [UxmlElement]
    public partial class ShunNavigationMenuLink : ShunButton
    {
        public ShunNavigationMenuLink()
        {
            AddToClassList("shun-navigation-menu__link");
            variant = ButtonVariant.Ghost;
            alignment = ButtonAlignment.Left;
        }
    }

    [UxmlElement]
    public partial class ShunNavigationMenuFeature : VisualElement
    {
        [UxmlAttribute]
        public string title { get; set; }

        [UxmlAttribute]
        public string description { get; set; }

        private Label m_TitleLabel;
        private Label m_DescriptionLabel;

        public ShunNavigationMenuFeature()
        {
            AddToClassList("shun-navigation-menu__feature");

            m_TitleLabel = new Label();
            m_TitleLabel.AddToClassList("shun-navigation-menu__feature-title");
            Add(m_TitleLabel);

            m_DescriptionLabel = new Label();
            m_DescriptionLabel.AddToClassList("shun-navigation-menu__feature-description");
            Add(m_DescriptionLabel);

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                schedule.Execute(() =>
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        m_TitleLabel.text = title;
                    }
                    if (!string.IsNullOrEmpty(description))
                    {
                        m_DescriptionLabel.text = description;
                    }
                }).StartingIn(50);
            });
        }
    }
}
