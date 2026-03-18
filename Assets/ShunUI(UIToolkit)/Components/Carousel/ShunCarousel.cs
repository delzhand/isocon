using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunCarousel : VisualElement
    {
        [UxmlAttribute]
        public Orientation orientation
        {
            get => m_Orientation;
            set => SetOrientation(value);
        }

        [UxmlAttribute("show-dots")]
        public bool showDots
        {
            get => m_ShowDots;
            set => SetShowDots(value);
        }

        [UxmlAttribute("previous-icon")]
        public Texture2D previousIcon
        {
            get => m_PreviousIcon;
            set
            {
#if UNITY_6000_3_OR_NEWER
                if (value != null) m_PreviousIconSvg = null;
#endif
                SetPreviousIcon(value);
            }
        }

        [UxmlAttribute("next-icon")]
        public Texture2D nextIcon
        {
            get => m_NextIcon;
            set
            {
#if UNITY_6000_3_OR_NEWER
                if (value != null) m_NextIconSvg = null;
#endif
                SetNextIcon(value);
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute("previous-icon-svg")]
        public VectorImage previousIconSvg
        {
            get => m_PreviousIconSvg;
            set
            {
                m_PreviousIconSvg = value;
                if (value != null) m_PreviousIcon = null;
                SetPreviousIconSvg(value);
            }
        }

        [UxmlAttribute("next-icon-svg")]
        public VectorImage nextIconSvg
        {
            get => m_NextIconSvg;
            set
            {
                m_NextIconSvg = value;
                if (value != null) m_NextIcon = null;
                SetNextIconSvg(value);
            }
        }
#endif

        private VisualElement m_Container;
        private VisualElement m_ItemsContainer;
        private Button m_PreviousButton;
        private Button m_NextButton;
        private Image m_PreviousIconImage;
        private Image m_NextIconImage;
        private VisualElement m_DotsContainer;
        private Orientation m_Orientation = Orientation.Horizontal;
        private bool m_ShowDots = false;
        private int m_CurrentIndex = 0;
        private int m_ItemCount = 0;
        private Texture2D m_PreviousIcon;
        private Texture2D m_NextIcon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage m_PreviousIconSvg;
        private VectorImage m_NextIconSvg;
#endif

        // Override the content container so children are added to the items container
        public override VisualElement contentContainer => m_ItemsContainer ?? this;

        public ShunCarousel()
        {
            AddToClassList("shun-carousel");

            // Items container (scrollable) - this will receive children from UXML
            m_ItemsContainer = new VisualElement();
            m_ItemsContainer.AddToClassList("shun-carousel__items");

            // Main container
            m_Container = new VisualElement();
            m_Container.AddToClassList("shun-carousel__container");
            m_Container.Add(m_ItemsContainer);
            hierarchy.Add(m_Container);

            // Previous button
            m_PreviousButton = new Button(OnPreviousClick);
            m_PreviousButton.AddToClassList("shun-carousel__button");
            m_PreviousButton.AddToClassList("shun-carousel__button--previous");
            
            // Add icon image container - use VisualElement for background image tinting
            m_PreviousIconImage = new Image();
            m_PreviousIconImage.AddToClassList("shun-carousel__button-icon");
            m_PreviousIconImage.style.display = DisplayStyle.None;
            m_PreviousButton.Add(m_PreviousIconImage);
            
            hierarchy.Add(m_PreviousButton);

            // Next button
            m_NextButton = new Button(OnNextClick);
            m_NextButton.AddToClassList("shun-carousel__button");
            m_NextButton.AddToClassList("shun-carousel__button--next");
            
            // Add icon image container - use VisualElement for background image tinting
            m_NextIconImage = new Image();
            m_NextIconImage.AddToClassList("shun-carousel__button-icon");
            m_NextIconImage.style.display = DisplayStyle.None;
            m_NextButton.Add(m_NextIconImage);
            
            hierarchy.Add(m_NextButton);

            // Dots container (initially hidden)
            m_DotsContainer = new VisualElement();
            m_DotsContainer.AddToClassList("shun-carousel__dots");
            m_DotsContainer.style.display = DisplayStyle.None;
            hierarchy.Add(m_DotsContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void SetPreviousIcon(Texture2D icon)
        {
            m_PreviousIcon = icon;
            if (m_PreviousButton != null && m_PreviousIconImage != null)
            {
                if (icon != null)
                {
                    m_PreviousIconImage.style.backgroundImage = new StyleBackground(icon);
                    m_PreviousIconImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_PreviousIconImage.style.backgroundImage = StyleKeyword.Null;
                    m_PreviousIconImage.style.display = DisplayStyle.None;
                }
            }
        }

        private void SetNextIcon(Texture2D icon)
        {
            m_NextIcon = icon;
            if (m_NextButton != null && m_NextIconImage != null)
            {
                if (icon != null)
                {
                    m_NextIconImage.style.backgroundImage = new StyleBackground(icon);
                    m_NextIconImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_NextIconImage.style.backgroundImage = StyleKeyword.Null;
                    m_NextIconImage.style.display = DisplayStyle.None;
                }
            }
        }

#if UNITY_6000_3_OR_NEWER
        private void SetPreviousIconSvg(VectorImage icon)
        {
            if (m_PreviousButton != null && m_PreviousIconImage != null)
            {
                if (icon != null)
                {
                    m_PreviousIconImage.style.backgroundImage = new StyleBackground(icon);
                    m_PreviousIconImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_PreviousIconImage.style.backgroundImage = StyleKeyword.Null;
                    m_PreviousIconImage.style.display = DisplayStyle.None;
                }
            }
        }

        private void SetNextIconSvg(VectorImage icon)
        {
            if (m_NextButton != null && m_NextIconImage != null)
            {
                if (icon != null)
                {
                    m_NextIconImage.style.backgroundImage = new StyleBackground(icon);
                    m_NextIconImage.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_NextIconImage.style.backgroundImage = StyleKeyword.Null;
                    m_NextIconImage.style.display = DisplayStyle.None;
                }
            }
        }
#endif

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            schedule.Execute(Initialize).StartingIn(100);
        }

        private void Initialize()
        {
            // Collect carousel items
            var items = m_ItemsContainer.Query<ShunCarouselItem>().ToList();
            m_ItemCount = items.Count;

            if (m_ItemCount == 0)
            {
                return;
            }

            // Create dots if enabled
            if (m_ShowDots)
            {
                m_DotsContainer.Clear();
                for (int i = 0; i < m_ItemCount; i++)
                {
                    int index = i; // Capture for closure
                    var dot = new Button(() => GoToSlide(index));
                    dot.AddToClassList("shun-carousel__dot");
                    
                    // Mark first dot as active
                    if (i == 0)
                    {
                        dot.AddToClassList("shun-carousel__dot--active");
                    }
                    
                    m_DotsContainer.Add(dot);
                }
            }

            // Initialize to first slide
            m_CurrentIndex = 0;
            UpdateButtons();
            
            // Ensure first slide is visible
            GoToSlide(0);
        }

        public void SetOrientation(Orientation orientation)
        {
            m_Orientation = orientation;
            
            RemoveFromClassList("shun-carousel--horizontal");
            RemoveFromClassList("shun-carousel--vertical");

            if (orientation == Orientation.Horizontal)
            {
                AddToClassList("shun-carousel--horizontal");
            }
            else
            {
                AddToClassList("shun-carousel--vertical");
            }
        }

        public void SetShowDots(bool showDots)
        {
            m_ShowDots = showDots;
            if (m_DotsContainer != null)
            {
                m_DotsContainer.style.display = showDots ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnPreviousClick()
        {
            if (m_CurrentIndex > 0)
            {
                GoToSlide(m_CurrentIndex - 1);
            }
        }

        private void OnNextClick()
        {
            if (m_CurrentIndex < m_ItemCount - 1)
            {
                GoToSlide(m_CurrentIndex + 1);
            }
        }

        private void GoToSlide(int index)
        {
            if (index < 0 || index >= m_ItemCount) return;

            m_CurrentIndex = index;

            // Gap between cards in pixels (must match CSS gap property)
            const float gap = 16f;
            
            // Calculate scroll position using fixed dimensions plus gap
            if (m_Orientation == Orientation.Horizontal)
            {
                // 404px card width + 16px gap = 420px per slide
                float offset = -index * (404f + gap);
                m_ItemsContainer.style.translate = new Translate(new Length(offset, LengthUnit.Pixel), 0);
            }
            else
            {
                // 350px card height + 16px gap = 366px per slide
                float offset = -index * (350f + gap);
                m_ItemsContainer.style.translate = new Translate(0, new Length(offset, LengthUnit.Pixel));
            }

            UpdateDots();
            UpdateButtons();
        }

        private void UpdateDots()
        {
            if (!m_ShowDots) return;

            var dots = m_DotsContainer.Children().ToList();
            for (int i = 0; i < dots.Count; i++)
            {
                var dot = dots[i] as Button;
                if (dot == null) continue;

                // Toggle active class based on current index
                if (i == m_CurrentIndex)
                {
                    dot.AddToClassList("shun-carousel__dot--active");
                }
                else
                {
                    dot.RemoveFromClassList("shun-carousel__dot--active");
                }
            }
        }

        private void UpdateButtons()
        {
            m_PreviousButton.SetEnabled(m_CurrentIndex > 0);
            m_NextButton.SetEnabled(m_CurrentIndex < m_ItemCount - 1);
        }

        public enum Orientation
        {
            Horizontal,
            Vertical
        }
    }

    [UxmlElement]
    public partial class ShunCarouselItem : VisualElement
    {
        public ShunCarouselItem()
        {
            AddToClassList("shun-carousel__item");
        }
    }
}
