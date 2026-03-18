using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunIcon : VisualElement
    {
        private Texture2D m_Icon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage m_IconSvg;
#endif

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

        public ShunIcon()
        {
            AddToClassList("shun-icon");
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            bool hasIcon = m_Icon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || m_IconSvg != null;
#endif

            if (hasIcon)
            {
#if UNITY_6000_3_OR_NEWER
                if (m_IconSvg != null)
                    style.backgroundImage = new StyleBackground(m_IconSvg);
                else
#endif
                    style.backgroundImage = new StyleBackground(m_Icon);
                style.display = DisplayStyle.Flex;
            }
            else
            {
                style.backgroundImage = null;
                style.display = DisplayStyle.None;
            }
        }
    }
}
