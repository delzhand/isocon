using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A pagination component for the ShunDataTable.
    /// Displays page navigation controls and information.
    /// </summary>
    [UxmlElement]
    public partial class ShunDataTablePagination : VisualElement
    {
        private Label _pageInfoLabel;
        private ShunButton _previousButton;
        private ShunButton _nextButton;

        private int _currentPage = 1;
        private int _totalPages = 1;
        
        private Texture2D _previousIcon;
        private Texture2D _nextIcon;
#if UNITY_6000_3_OR_NEWER
        private VectorImage _previousIconSvg;
        private VectorImage _nextIconSvg;
#endif

        public event Action OnFirstPage;
        public event Action OnPreviousPage;
        public event Action OnNextPage;
        public event Action OnLastPage;

        [UxmlAttribute]
        public Texture2D previousIcon
        {
            get => _previousIcon;
            set
            {
                _previousIcon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) _previousIconSvg = null;
#endif
                UpdatePreviousIcon();
            }
        }

        [UxmlAttribute]
        public Texture2D nextIcon
        {
            get => _nextIcon;
            set
            {
                _nextIcon = value;
#if UNITY_6000_3_OR_NEWER
                if (value != null) _nextIconSvg = null;
#endif
                UpdateNextIcon();
            }
        }

#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute]
        public VectorImage previousIconSvg
        {
            get => _previousIconSvg;
            set
            {
                _previousIconSvg = value;
                if (value != null) _previousIcon = null;
                UpdatePreviousIcon();
            }
        }

        [UxmlAttribute]
        public VectorImage nextIconSvg
        {
            get => _nextIconSvg;
            set
            {
                _nextIconSvg = value;
                if (value != null) _nextIcon = null;
                UpdateNextIcon();
            }
        }
#endif

        public ShunDataTablePagination()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("data-table__pagination");

            // Create right section with navigation controls (aligned right)
            var rightSection = new VisualElement();
            rightSection.AddToClassList("data-table__pagination-right");
            hierarchy.Add(rightSection);

            // Navigation buttons container
            var navButtons = new VisualElement();
            navButtons.AddToClassList("data-table__pagination-buttons");
            rightSection.Add(navButtons);

            // Previous page button
            _previousButton = new ShunButton();
            _previousButton.AddToClassList("data-table__pagination-button");
            _previousButton.AddToClassList("data-table__pagination-button--prev");
            _previousButton.variant = ButtonVariant.Outline;
            _previousButton.size = ButtonSize.Icon;
            _previousButton.text = "‹";
            _previousButton.tooltip = "Previous page";
            _previousButton.RegisterCallback<ClickEvent>(evt => OnPreviousPage?.Invoke());
            navButtons.Add(_previousButton);

            // Page info label (between buttons)
            _pageInfoLabel = new Label();
            _pageInfoLabel.AddToClassList("data-table__pagination-page-info");
            navButtons.Add(_pageInfoLabel);

            // Next page button
            _nextButton = new ShunButton();
            _nextButton.AddToClassList("data-table__pagination-button");
            _nextButton.AddToClassList("data-table__pagination-button--next");
            _nextButton.variant = ButtonVariant.Outline;
            _nextButton.size = ButtonSize.Icon;
            _nextButton.text = "›";
            _nextButton.tooltip = "Next page";
            _nextButton.RegisterCallback<ClickEvent>(evt => OnNextPage?.Invoke());
            navButtons.Add(_nextButton);

            // Load default icons from Resources if not already set
            LoadDefaultIcons();

            UpdateButtonStates();
        }

        private void LoadDefaultIcons()
        {
#if UNITY_6000_3_OR_NEWER
            if (_previousIcon == null && _previousIconSvg == null)
            {
                var svgPrev = Resources.Load<VectorImage>("Icons/chevron-left");
                if (svgPrev != null) { _previousIconSvg = svgPrev; UpdatePreviousIcon(); }
                else
#endif
                if (_previousIcon == null)
                {
                    var defaultPrevIcon = Resources.Load<Texture2D>("Icons/chevron-left");
                    if (defaultPrevIcon != null) { _previousIcon = defaultPrevIcon; UpdatePreviousIcon(); }
                }
#if UNITY_6000_3_OR_NEWER
            }

            if (_nextIcon == null && _nextIconSvg == null)
            {
                var svgNext = Resources.Load<VectorImage>("Icons/chevron-right");
                if (svgNext != null) { _nextIconSvg = svgNext; UpdateNextIcon(); }
                else
#endif
                if (_nextIcon == null)
                {
                    var defaultNextIcon = Resources.Load<Texture2D>("Icons/chevron-right");
                    if (defaultNextIcon != null) { _nextIcon = defaultNextIcon; UpdateNextIcon(); }
                }
#if UNITY_6000_3_OR_NEWER
            }
#endif
        }

        private void UpdatePreviousIcon()
        {
            if (_previousButton == null) return;

            bool hasIcon = _previousIcon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || _previousIconSvg != null;
            if (_previousIconSvg != null)
            {
                _previousButton.iconSvg = _previousIconSvg;
                _previousButton.text = "";
            }
            else
#endif
            if (_previousIcon != null)
            {
                _previousButton.icon = _previousIcon;
                _previousButton.text = "";
            }
            else
            {
                _previousButton.icon = null;
                _previousButton.text = "‹";
            }
        }

        private void UpdateNextIcon()
        {
            if (_nextButton == null) return;

            bool hasIcon = _nextIcon != null;
#if UNITY_6000_3_OR_NEWER
            hasIcon = hasIcon || _nextIconSvg != null;
            if (_nextIconSvg != null)
            {
                _nextButton.iconSvg = _nextIconSvg;
                _nextButton.text = "";
            }
            else
#endif
            if (_nextIcon != null)
            {
                _nextButton.icon = _nextIcon;
                _nextButton.text = "";
            }
            else
            {
                _nextButton.icon = null;
                _nextButton.text = "›";
            }
        }

        /// <summary>
        /// Update the pagination display with current page information
        /// </summary>
        public void UpdateInfo(int currentPage, int totalPages, int startItem, int endItem, int totalItems)
        {
            _currentPage = currentPage;
            _totalPages = totalPages;

            if (_pageInfoLabel != null)
            {
                _pageInfoLabel.text = $"Page {currentPage} of {totalPages}";
            }

            UpdateButtonStates();
        }

        /// <summary>
        /// Update the pagination display with basic page information
        /// </summary>
        public void UpdateInfo(int currentPage, int totalPages)
        {
            _currentPage = currentPage;
            _totalPages = totalPages;

            if (_pageInfoLabel != null)
            {
                _pageInfoLabel.text = $"Page {currentPage} of {totalPages}";
            }

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool canGoPrevious = _currentPage > 1;
            bool canGoNext = _currentPage < _totalPages;

            if (_previousButton != null)
            {
                _previousButton.SetEnabled(canGoPrevious);
            }
            if (_nextButton != null)
            {
                _nextButton.SetEnabled(canGoNext);
            }
        }
    }
}
