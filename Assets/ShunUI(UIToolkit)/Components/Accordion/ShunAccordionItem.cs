using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunAccordionItem : VisualElement
    {
        private ShunButton _header;
        private Label _titleLabel;
        private VisualElement _icon;
        private ShunContainer _content;
        private bool _isExpanded = false;
        private Texture2D _iconTexture;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _iconSvg;
#endif

        [UxmlAttribute]
        public string title { get; set; } = "Accordion Item";

        [UxmlAttribute]
        public Texture2D chevronIcon
        {
            get => _iconTexture;
            set
            {
                _iconTexture = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) _iconSvg = null;
#endif
                UpdateChevronIcon();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage chevronIconSvg
        {
            get => _iconSvg;
            set
            {
                _iconSvg = value;
                if (value != null) _iconTexture = null;
                UpdateChevronIcon();
            }
        }
#endif

        [UxmlAttribute]
        public bool isExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    UpdateExpandedState();
                }
            }
        }

        public ShunAccordionItem()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("accordion-item");

            // Create header container (clickable)
            _header = new ShunButton();
            _header.AddToClassList("accordion-item__header");
            _header.variant = ButtonVariant.Link;
            _header.clicked += Toggle;

            // Clear button's default text and create custom layout
            _header.text = "";

            // Create title label
            _titleLabel = new Label(title);
            _titleLabel.AddToClassList("accordion-item__title");
            _titleLabel.style.flexGrow = 1;
            _header.Add(_titleLabel);

            // Create icon
            _icon = new VisualElement();
            _icon.AddToClassList("accordion-item__icon");
            _header.Add(_icon);

            // Create content container - this will hold user-defined content
            _content = new ShunContainer();
            _content.AddToClassList("accordion-item__content");

            Add(_header);
            Add(_content);

            // Register for events
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateHeaderText();
                UpdateChevronIcon();
                UpdateExpandedState();
                
                // Move any children that were added in UXML into the content container
                var childrenToMove = new System.Collections.Generic.List<VisualElement>();
                foreach (var child in Children())
                {
                    if (child != _header && child != _content)
                    {
                        childrenToMove.Add(child);
                    }
                }
                
                foreach (var child in childrenToMove)
                {
                    Remove(child);
                    _content.Add(child);
                }
            });
        }

        private void UpdateChevronIcon()
        {
            if (_icon == null) return;

#if UNITY_6000_3_OR_NEWER
            if (_iconSvg != null)
                _icon.style.backgroundImage = new StyleBackground(_iconSvg);
            else
#endif
            if (_iconTexture != null)
                _icon.style.backgroundImage = new StyleBackground(_iconTexture);
        }

        private void Toggle()
        {
            _isExpanded = !_isExpanded;
            UpdateExpandedState();
        }

        private void UpdateExpandedState()
        {
            if (_isExpanded)
            {
                AddToClassList("accordion-item--expanded");
            }
            else
            {
                RemoveFromClassList("accordion-item--expanded");
            }
        }

        private void UpdateHeaderText()
        {
            if (_titleLabel != null && !string.IsNullOrEmpty(title))
            {
                _titleLabel.text = title;
            }
        }

        public void SetExpanded(bool expanded)
        {
            if (_isExpanded != expanded)
            {
                Toggle();
            }
        }

        public void SetContent(VisualElement content)
        {
            _content.Clear();
            _content.Add(content);
        }
    }
}