using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A cell element for the ShunDataTable component.
    /// Displays a single value in a table cell.
    /// </summary>
    [UxmlElement]
    public partial class ShunDataTableCell : VisualElement
    {
        private Label _label;
        private string _text = "";

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

        public ShunDataTableCell()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("data-table__cell");

            _label = new Label();
            _label.AddToClassList("data-table__cell-text");
            _label.pickingMode = PickingMode.Ignore;
            hierarchy.Add(_label);

            UpdateText();
        }

        private void UpdateText()
        {
            if (_label != null)
            {
                _label.text = _text;
            }
        }

        /// <summary>
        /// Set the text content of the cell
        /// </summary>
        public void SetText(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Get the text content of the cell
        /// </summary>
        public string GetText()
        {
            return _text;
        }
    }
}
