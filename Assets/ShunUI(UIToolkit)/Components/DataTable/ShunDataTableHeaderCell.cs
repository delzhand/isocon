using System;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A header cell element for the ShunDataTable component.
    /// Supports sorting with visual indicators.
    /// </summary>
    [UxmlElement]
    public partial class ShunDataTableHeaderCell : VisualElement
    {
        private Label _label;
        private VisualElement _sortIcon;
        private string _text = "";
        private bool _isSorted = false;
        private bool _isAscending = true;
        private bool _isSortable = true;

        public event Action OnClick;

        [UxmlAttribute]
        public string text
        {
            get => _text;
            set
            {
                _text = value;
                UpdateText();
            }
        }

        [UxmlAttribute]
        public bool isSortable
        {
            get => _isSortable;
            set
            {
                _isSortable = value;
                UpdateSortableState();
            }
        }

        public ShunDataTableHeaderCell()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("data-table__header-cell");

            // Create content container
            var content = new VisualElement();
            content.AddToClassList("data-table__header-cell-content");
            content.pickingMode = PickingMode.Ignore;
            hierarchy.Add(content);

            // Create label
            _label = new Label();
            _label.AddToClassList("data-table__header-cell-text");
            _label.pickingMode = PickingMode.Ignore;
            content.Add(_label);

            UpdateText();
        }

        private void OnClickHandler(ClickEvent evt)
        {
            if (_isSortable)
            {
                OnClick?.Invoke();
            }
        }

        private void UpdateText()
        {
            if (_label != null)
            {
                _label.text = _text;
            }
        }

        private void UpdateSortIndicator()
        {
            if (_sortIcon == null) return;

            _sortIcon.RemoveFromClassList("data-table__header-cell-sort-icon--asc");
            _sortIcon.RemoveFromClassList("data-table__header-cell-sort-icon--desc");
            _sortIcon.RemoveFromClassList("data-table__header-cell-sort-icon--active");

            if (_isSorted)
            {
                _sortIcon.AddToClassList("data-table__header-cell-sort-icon--active");
                if (_isAscending)
                {
                    _sortIcon.AddToClassList("data-table__header-cell-sort-icon--asc");
                }
                else
                {
                    _sortIcon.AddToClassList("data-table__header-cell-sort-icon--desc");
                }
            }
        }

        private void UpdateSortableState()
        {
            if (_isSortable)
            {
                AddToClassList("data-table__header-cell--sortable");
                if (_sortIcon != null)
                {
                    _sortIcon.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                RemoveFromClassList("data-table__header-cell--sortable");
                if (_sortIcon != null)
                {
                    _sortIcon.style.display = DisplayStyle.None;
                }
            }
        }

        /// <summary>
        /// Set the text content of the header cell
        /// </summary>
        public void SetText(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Set whether this column is sortable
        /// </summary>
        public void SetSortable(bool sortable)
        {
            isSortable = sortable;
        }

        /// <summary>
        /// Set the sort state of this header cell
        /// </summary>
        public void SetSortState(bool isSorted, bool isAscending)
        {
            _isSorted = isSorted;
            _isAscending = isAscending;
            UpdateSortIndicator();
        }
    }
}
