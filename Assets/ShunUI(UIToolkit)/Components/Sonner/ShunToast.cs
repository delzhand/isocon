using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunToast : VisualElement
    {
        private VisualElement m_Content;
        private VisualElement m_IconElement;
        private VisualElement m_TextContainer;
        private Label m_Title;
        private Label m_Description;
        private ShunButton m_CloseButton;
        private ShunButton m_ActionButton;

        private ToastVariant m_Variant = ToastVariant.Default;
        private string m_TitleText = "Notification";
        private string m_DescriptionText = "This is a toast notification";
        private bool m_ShowCloseButton = true;
        private bool m_ShowActionButton = false;
        private string m_ActionButtonText = "Action";
        private Texture2D m_Icon = null;
#if UNITY_6000_3_OR_NEWER
        private VectorImage m_IconSvg = null;
        private VectorImage m_CloseIconSvg = null;
#endif
        private bool m_ShowIcon = true;
        private Texture2D m_CloseIcon = null;

        [UxmlAttribute]
        public ToastVariant variant
        {
            get => m_Variant;
            set
            {
                if (m_Variant != value)
                {
                    m_Variant = value;
                    UpdateVariant();
                }
            }
        }

        [UxmlAttribute]
        public string title
        {
            get => m_TitleText;
            set
            {
                m_TitleText = value;
                UpdateTitle();
            }
        }

        [UxmlAttribute]
        public string description
        {
            get => m_DescriptionText;
            set
            {
                m_DescriptionText = value;
                UpdateDescription();
            }
        }

        [UxmlAttribute]
        public bool showCloseButton
        {
            get => m_ShowCloseButton;
            set
            {
                m_ShowCloseButton = value;
                UpdateCloseButton();
            }
        }

        [UxmlAttribute]
        public bool showActionButton
        {
            get => m_ShowActionButton;
            set
            {
                m_ShowActionButton = value;
                UpdateActionButton();
            }
        }

        [UxmlAttribute]
        public string actionButtonText
        {
            get => m_ActionButtonText;
            set
            {
                m_ActionButtonText = value;
                UpdateActionButtonText();
            }
        }

        [UxmlAttribute]
        public Texture2D icon
        {
            get => m_Icon;
            set
            {
                m_Icon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) m_IconSvg = null;
#endif
                UpdateIcon();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage iconSvg
        {
            get => m_IconSvg;
            set
            {
                m_IconSvg = value;
                if (value != null) m_Icon = null;
                UpdateIcon();
            }
        }
#endif

        [UxmlAttribute]
        public bool showIcon
        {
            get => m_ShowIcon;
            set
            {
                m_ShowIcon = value;
                UpdateIconVisibility();
            }
        }

        [UxmlAttribute]
        public Texture2D closeIcon
        {
            get => m_CloseIcon;
            set
            {
                m_CloseIcon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) m_CloseIconSvg = null;
#endif
                UpdateCloseIcon();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage closeIconSvg
        {
            get => m_CloseIconSvg;
            set
            {
                m_CloseIconSvg = value;
                if (value != null) m_CloseIcon = null;
                UpdateCloseIcon();
            }
        }
#endif

        public ShunToast()
        {
            BuildUI();
            
            // Ensure toast can receive mouse events
            pickingMode = PickingMode.Position;
            
            UpdateVariant();
            UpdateTitle();
            UpdateDescription();
            UpdateCloseButton();
            UpdateActionButton();
            UpdateIcon();
            UpdateIconVisibility();
            UpdateCloseIcon();
        }

        private void BuildUI()
        {
            AddToClassList("toast");

            // Create content container
            m_Content = new VisualElement();
            m_Content.AddToClassList("toast__content");
            Add(m_Content);

            // Add icon
            m_IconElement = new VisualElement();
            m_IconElement.AddToClassList("toast__icon");
            m_Content.Add(m_IconElement);

            // Add text container
            m_TextContainer = new VisualElement();
            m_TextContainer.AddToClassList("toast__text");
            m_Content.Add(m_TextContainer);

            // Add title
            m_Title = new Label();
            m_Title.AddToClassList("toast__title");
            m_TextContainer.Add(m_Title);

            // Add description
            m_Description = new Label();
            m_Description.AddToClassList("toast__message");
            m_TextContainer.Add(m_Description);

            // Add action button (initially hidden) - placed in content container to be inline
            m_ActionButton = new ShunButton();
            m_ActionButton.AddToClassList("button");
            m_ActionButton.AddToClassList("button-sm");
            m_ActionButton.AddToClassList("toast__action");
            m_ActionButton.style.marginLeft = 12;
            m_ActionButton.style.flexShrink = 0; // Prevent button from shrinking
            m_Content.Add(m_ActionButton);

            // Add close button
            m_CloseButton = new ShunButton();
            m_CloseButton.name = "toast-close";
            m_CloseButton.AddToClassList("toast__close");
            m_CloseButton.text = "×";
            Add(m_CloseButton);
        }

        private void UpdateVariant()
        {
            // Remove all variant classes
            RemoveFromClassList("toast--default");
            RemoveFromClassList("toast--success");
            RemoveFromClassList("toast--error");
            RemoveFromClassList("toast--warning");
            RemoveFromClassList("toast--info");

            // Add new variant class
            string variantClass = m_Variant switch
            {
                ToastVariant.Success => "toast--success",
                ToastVariant.Error => "toast--error",
                ToastVariant.Warning => "toast--warning",
                ToastVariant.Info => "toast--info",
                _ => "toast--default"
            };
            AddToClassList(variantClass);
        }

        private void UpdateTitle()
        {
            if (m_Title != null)
            {
                m_Title.text = m_TitleText;
                m_Title.style.display = string.IsNullOrEmpty(m_TitleText) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void UpdateDescription()
        {
            if (m_Description != null)
            {
                m_Description.text = m_DescriptionText;
                m_Description.style.display = string.IsNullOrEmpty(m_DescriptionText) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void UpdateCloseButton()
        {
            if (m_CloseButton != null)
            {
                m_CloseButton.style.display = m_ShowCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateActionButton()
        {
            if (m_ActionButton != null)
            {
                m_ActionButton.style.display = m_ShowActionButton ? DisplayStyle.Flex : DisplayStyle.None;
                UpdateActionButtonText();
            }
        }

        private void UpdateActionButtonText()
        {
            if (m_ActionButton != null)
            {
                m_ActionButton.text = m_ActionButtonText;
            }
        }

        private void UpdateIcon()
        {
            if (m_IconElement != null)
            {
                bool hasCustomIcon = m_Icon != null;
#if UNITY_6000_3_OR_NEWER
                hasCustomIcon = hasCustomIcon || m_IconSvg != null;
#endif

                if (hasCustomIcon)
                {
#if UNITY_6000_3_OR_NEWER
                    if (m_IconSvg != null)
                        m_IconElement.style.backgroundImage = new StyleBackground(m_IconSvg);
                    else
#endif
                        m_IconElement.style.backgroundImage = new StyleBackground(m_Icon);
                    m_IconElement.style.unityBackgroundImageTintColor = new StyleColor(StyleKeyword.Null);
                }
                else
                {
                    // No custom icon: clear and let CSS handle default colored circle
                    m_IconElement.style.backgroundImage = StyleKeyword.Null;
                    m_IconElement.style.unityBackgroundImageTintColor = new StyleColor(StyleKeyword.Null);
                }
            }
        }

        private void UpdateIconVisibility()
        {
            if (m_IconElement != null)
            {
                m_IconElement.style.display = m_ShowIcon ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateCloseIcon()
        {
            if (m_CloseButton == null) return;

            bool hasCloseIcon = m_CloseIcon != null;
#if UNITY_6000_3_OR_NEWER
            hasCloseIcon = hasCloseIcon || m_CloseIconSvg != null;
            if (m_CloseIconSvg != null)
            {
                m_CloseButton.iconSvg = m_CloseIconSvg;
                m_CloseButton.text = string.Empty;
            }
            else
#endif
            if (m_CloseIcon != null)
            {
                m_CloseButton.icon = m_CloseIcon;
                m_CloseButton.text = string.Empty;
            }
            else
            {
                m_CloseButton.icon = null;
                m_CloseButton.text = "×";
            }
        }

        /// <summary>
        /// Register a callback for when the close button is clicked
        /// </summary>
        public void RegisterCloseCallback(System.Action callback)
        {
            if (m_CloseButton != null)
            {
                m_CloseButton.clicked += callback;
            }
        }

        /// <summary>
        /// Register a callback for when the action button is clicked
        /// </summary>
        public void RegisterActionCallback(System.Action callback)
        {
            if (m_ActionButton != null)
            {
                m_ActionButton.clicked += callback;
            }
        }
    }
}
