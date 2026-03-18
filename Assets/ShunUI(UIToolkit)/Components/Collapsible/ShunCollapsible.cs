using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunCollapsible : VisualElement
    {
        [UxmlAttribute]
        public bool isOpen
        {
            get => m_IsOpen;
            set
            {
                m_IsOpen = value;
                UpdateContentVisibility();
            }
        }

        private bool m_IsOpen = false;
        private ShunContainer m_TriggerContainer;
        private ShunContainer m_ContentContainer;

        public ShunCollapsible()
        {
            AddToClassList("shun-collapsible");

            // Create trigger container (for the button/header)
            m_TriggerContainer = new ShunContainer();
            m_TriggerContainer.AddToClassList("shun-collapsible__trigger-container");
            Add(m_TriggerContainer);

            // Create content container (for collapsible content)
            m_ContentContainer = new ShunContainer();
            m_ContentContainer.AddToClassList("shun-collapsible__content");
            Add(m_ContentContainer);

            UpdateContentVisibility();
            
            // Register callback to organize children
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Move children to appropriate containers
            schedule.Execute(() => {
                var childrenToMove = new System.Collections.Generic.List<VisualElement>();
                
                foreach (var child in Children())
                {
                    if (child != m_TriggerContainer && child != m_ContentContainer)
                    {
                        childrenToMove.Add(child);
                    }
                }
                
                foreach (var child in childrenToMove)
                {
                    child.RemoveFromHierarchy();
                    
                    if (child is ShunCollapsibleTrigger)
                    {
                        m_TriggerContainer.Add(child);
                    }
                    else
                    {
                        // Everything else goes to content
                        m_ContentContainer.Add(child);
                    }
                }
            });
        }

        private void UpdateContentVisibility()
        {
            if (m_IsOpen)
            {
                m_ContentContainer.RemoveFromClassList("shun-collapsible__content--hidden");
                AddToClassList("shun-collapsible--open");
            }
            else
            {
                m_ContentContainer.AddToClassList("shun-collapsible__content--hidden");
                RemoveFromClassList("shun-collapsible--open");
            }
        }

        public void Toggle()
        {
            isOpen = !isOpen;
        }
    }

    [UxmlElement]
    public partial class ShunCollapsibleTrigger : VisualElement
    {
        private Label m_TextLabel;
        private ShunButton m_ChevronButton;

        public ShunCollapsibleTrigger()
        {
            AddToClassList("shun-collapsible__trigger");
            
            // Create text label
            m_TextLabel = new Label();
            m_TextLabel.AddToClassList("shun-collapsible__text");
            hierarchy.Add(m_TextLabel);
            
            // Create chevron button with outline variant
            m_ChevronButton = new ShunButton();
            m_ChevronButton.variant = ButtonVariant.Outline;
            m_ChevronButton.size = ButtonSize.Icon;
            m_ChevronButton.AddToClassList("shun-collapsible__chevron-button");
            hierarchy.Add(m_ChevronButton);
            
            // Load default icon
#if UNITY_6000_3_OR_NEWER
            var defaultIconSvg = Resources.Load<VectorImage>("Icons/chevrons-up-down");
            if (defaultIconSvg != null)
                m_ChevronButton.iconSvg = defaultIconSvg;
            else
#endif
            {
                var defaultIcon = Resources.Load<Texture2D>("Icons/chevrons-up-down");
                if (defaultIcon != null)
                    m_ChevronButton.icon = defaultIcon;
            }
            
            // Register click event on the trigger itself (for clicking label area)
            RegisterCallback<ClickEvent>(OnClicked);
            
            // Register button's clicked event (for proper button interaction)
            m_ChevronButton.clicked += OnButtonClicked;
        }

        [UxmlAttribute]
        public string text
        {
            get => m_TextLabel?.text ?? "";
            set
            {
                if (m_TextLabel != null)
                {
                    m_TextLabel.text = value;
                }
            }
        }

        [UxmlAttribute]
        public Texture2D icon
        {
            get => m_ChevronButton?.icon;
            set
            {
                if (m_ChevronButton != null)
                {
                    m_ChevronButton.icon = value;
                }
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg
        {
            get => m_ChevronButton?.iconSvg;
            set
            {
                if (m_ChevronButton != null)
                {
                    m_ChevronButton.iconSvg = value;
                }
            }
        }
#endif

        private void OnClicked(ClickEvent evt)
        {
            // Only handle clicks that are not on the button itself
            // (button will handle its own clicks)
            if (evt.target != m_ChevronButton)
            {
                ToggleCollapsible();
            }
        }

        private void OnButtonClicked()
        {
            // Handle button's clicked event
            ToggleCollapsible();
        }

        private void ToggleCollapsible()
        {
            var collapsible = GetFirstAncestorOfType<ShunCollapsible>();
            if (collapsible != null)
            {
                collapsible.Toggle();
            }
        }
    }
}
