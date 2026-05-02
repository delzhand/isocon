using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A row element for the ShunDataTable component.
    /// Supports hover and selection states.
    /// </summary>
    [UxmlElement]
    public partial class ShunDataTableRow : VisualElement
    {
        private bool _isSelected = false;
        private bool _isHovered = false;

        [UxmlAttribute]
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    UpdateState();
                }
            }
        }

        public ShunDataTableRow()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("data-table__row");

            // Register hover events
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            _isHovered = true;
            UpdateState();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            _isHovered = false;
            UpdateState();
        }

        private void UpdateState()
        {
            RemoveFromClassList("data-table__row--hover");
            RemoveFromClassList("data-table__row--selected");

            if (_isSelected)
            {
                AddToClassList("data-table__row--selected");
            }
            else if (_isHovered)
            {
                AddToClassList("data-table__row--hover");
            }
        }

        /// <summary>
        /// Set the selected state of the row
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }
    }
}
