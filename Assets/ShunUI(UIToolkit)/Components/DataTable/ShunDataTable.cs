using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// Column definition for the DataTable
    /// </summary>
    [Serializable]
    public class ShunDataTableColumn
    {
        public string title;
        public float width = 100f;
        public bool isFlexible = true;
        public bool isSortable = true;
    }

    /// <summary>
    /// Row data for the DataTable
    /// </summary>
    [Serializable]
    public class ShunDataTableRowData
    {
        public List<string> cells = new List<string>();
        public object userData;
    }

    public enum DataTableVariant
    {
        Default,
        Outline
    }

    /// <summary>
    /// A data table component for UI Toolkit that displays tabular data with sorting and pagination.
    /// </summary>
    [UxmlElement]
    public partial class ShunDataTable : VisualElement
    {
        private DataTableVariant _variant = DataTableVariant.Default;
        private bool _showHeader = true;
        private bool _stripedRows = false;
        private int _itemsPerPage = 10;
        private float _headerHeight = 40f;
        private float _rowHeight = 40f;
        private Texture2D _previousPageIcon;
        private Texture2D _nextPageIcon;

        private VisualElement _headerContainer;
        private VisualElement _bodyContainer;
        private ScrollView _scrollView;
        private ShunDataTablePagination _pagination;
        
        private List<ShunDataTableHeaderCell> _headerCells = new List<ShunDataTableHeaderCell>();
        private List<ShunDataTableRow> _rows = new List<ShunDataTableRow>();
        
        private int _currentPage = 1;
        private int _sortedColumnIndex = -1;
        private bool _isAscending = true;
        
        /// <summary>
        /// Column definitions for the data table. Modify this list and call Refresh() to update the table.
        /// </summary>
        public List<ShunDataTableColumn> columns = new List<ShunDataTableColumn>();
        
        /// <summary>
        /// Row data for the data table. Modify this list and call Refresh() to update the table.
        /// </summary>
        public List<ShunDataTableRowData> data = new List<ShunDataTableRowData>();
        
        public event Action<int, ShunDataTableRowData> OnRowClicked;
        public event Action<int, bool> OnColumnSorted;
        public event Action<int> OnPageChanged;

        [UxmlAttribute]
        public DataTableVariant variant
        {
            get => _variant;
            set
            {
                if (_variant != value)
                {
                    _variant = value;
                    UpdateVariantClass();
                }
            }
        }

        [UxmlAttribute]
        public bool showHeader
        {
            get => _showHeader;
            set
            {
                if (_showHeader != value)
                {
                    _showHeader = value;
                    UpdateHeaderVisibility();
                }
            }
        }

        [UxmlAttribute]
        public bool stripedRows
        {
            get => _stripedRows;
            set
            {
                if (_stripedRows != value)
                {
                    _stripedRows = value;
                    RefreshView();
                }
            }
        }

        [UxmlAttribute]
        public int itemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (_itemsPerPage != value)
                {
                    _itemsPerPage = Mathf.Max(1, value);
                    _currentPage = 1;
                    RefreshView();
                }
            }
        }

        [UxmlAttribute]
        public float headerHeight
        {
            get => _headerHeight;
            set
            {
                if (!Mathf.Approximately(_headerHeight, value))
                {
                    _headerHeight = value;
                    UpdateHeaderHeight();
                }
            }
        }

        [UxmlAttribute]
        public float rowHeight
        {
            get => _rowHeight;
            set
            {
                if (!Mathf.Approximately(_rowHeight, value))
                {
                    _rowHeight = value;
                    RefreshView();
                }
            }
        }

        [UxmlAttribute]
        public Texture2D previousPageIcon
        {
            get => _previousPageIcon;
            set
            {
                _previousPageIcon = value;
                if (_pagination != null)
                {
                    _pagination.previousIcon = value;
                }
            }
        }

        [UxmlAttribute]
        public Texture2D nextPageIcon
        {
            get => _nextPageIcon;
            set
            {
                _nextPageIcon = value;
                if (_pagination != null)
                {
                    _pagination.nextIcon = value;
                }
            }
        }

        public int CurrentPage => _currentPage;
        public int TotalPages => Mathf.CeilToInt((float)data.Count / _itemsPerPage);
        public int TotalItems => data.Count;
        public int SortedColumnIndex => _sortedColumnIndex;
        public bool IsSortAscending => _isAscending;

        public ShunDataTable()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("data-table");
            UpdateVariantClass();

            // Create header container
            _headerContainer = new VisualElement();
            _headerContainer.AddToClassList("data-table__header");
            hierarchy.Add(_headerContainer);

            // Create scroll view for body
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("data-table__scroll");
            hierarchy.Add(_scrollView);

            // Create body container inside scroll view
            _bodyContainer = new VisualElement();
            _bodyContainer.AddToClassList("data-table__body");
            _scrollView.Add(_bodyContainer);

            // Create pagination
            _pagination = new ShunDataTablePagination();
            _pagination.AddToClassList("data-table__pagination");
            _pagination.OnPreviousPage += PreviousPage;
            _pagination.OnNextPage += NextPage;
            _pagination.OnFirstPage += GoToFirstPage;
            _pagination.OnLastPage += GoToLastPage;
            
            // Load default icons from Resources if not explicitly set
#if UNITY_6000_3_OR_NEWER
            var prevSvg = Resources.Load<VectorImage>("Icons/chevron-left");
            var nextSvg = Resources.Load<VectorImage>("Icons/chevron-right");
            if (prevSvg != null)
                _pagination.previousIconSvg = prevSvg;
            else
#endif
            {
                if (_previousPageIcon == null)
                    _previousPageIcon = Resources.Load<Texture2D>("Icons/chevron-left");
                if (_previousPageIcon != null)
                    _pagination.previousIcon = _previousPageIcon;
            }
#if UNITY_6000_3_OR_NEWER
            if (nextSvg != null)
                _pagination.nextIconSvg = nextSvg;
            else
#endif
            {
                if (_nextPageIcon == null)
                    _nextPageIcon = Resources.Load<Texture2D>("Icons/chevron-right");
                if (_nextPageIcon != null)
                    _pagination.nextIcon = _nextPageIcon;
            }
                
            hierarchy.Add(_pagination);

            UpdateHeaderVisibility();
        }

        private void UpdateVariantClass()
        {
            RemoveFromClassList("data-table--default");
            RemoveFromClassList("data-table--outline");

            string variantClass = _variant switch
            {
                DataTableVariant.Default => "data-table--default",
                DataTableVariant.Outline => "data-table--outline",
                _ => "data-table--default"
            };

            AddToClassList(variantClass);
        }

        private void UpdateHeaderVisibility()
        {
            if (_headerContainer != null)
            {
                _headerContainer.style.display = _showHeader ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateHeaderHeight()
        {
            if (_headerContainer != null)
            {
                _headerContainer.style.height = _headerHeight;
            }
            foreach (var cell in _headerCells)
            {
                cell.style.height = _headerHeight;
            }
        }

        /// <summary>
        /// Set the columns for the data table
        /// </summary>
        public void SetColumns(List<ShunDataTableColumn> newColumns)
        {
            columns = newColumns ?? new List<ShunDataTableColumn>();
            _sortedColumnIndex = -1;
            RebuildHeaders();
            RefreshView();
        }

        /// <summary>
        /// Set the data for the data table
        /// </summary>
        public void SetData(List<ShunDataTableRowData> newData)
        {
            data = newData ?? new List<ShunDataTableRowData>();
            _currentPage = 1;
            RefreshView();
        }

        /// <summary>
        /// Set data from a simple list of string arrays
        /// </summary>
        public void SetData(List<List<string>> newData)
        {
            data.Clear();
            if (newData != null)
            {
                foreach (var row in newData)
                {
                    data.Add(new ShunDataTableRowData { cells = row });
                }
            }
            _currentPage = 1;
            RefreshView();
        }

        /// <summary>
        /// Add a single row of data
        /// </summary>
        public void AddRow(ShunDataTableRowData rowData)
        {
            data.Add(rowData);
            RefreshView();
        }

        /// <summary>
        /// Remove a row at the specified index
        /// </summary>
        public void RemoveRow(int index)
        {
            if (index >= 0 && index < data.Count)
            {
                data.RemoveAt(index);
                RefreshView();
            }
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void ClearData()
        {
            data.Clear();
            _currentPage = 1;
            RefreshView();
        }

        /// <summary>
        /// Refresh the table display. Call this after modifying the columns or data fields directly.
        /// </summary>
        public void Refresh()
        {
            RebuildHeaders();
            RefreshView();
        }

        /// <summary>
        /// Sort the table by a specific column
        /// </summary>
        public void SortByColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= columns.Count) return;
            if (!columns[columnIndex].isSortable) return;

            if (columnIndex == _sortedColumnIndex)
            {
                _isAscending = !_isAscending;
            }
            else
            {
                _sortedColumnIndex = columnIndex;
                _isAscending = true;
            }

            // Sort the data
            data.Sort((a, b) =>
            {
                string valA = (columnIndex < a.cells.Count) ? a.cells[columnIndex] : "";
                string valB = (columnIndex < b.cells.Count) ? b.cells[columnIndex] : "";

                // Try numeric/currency sort first
                string cleanA = valA.Replace("$", "").Replace("€", "").Replace("£", "").Replace(",", "").Trim();
                string cleanB = valB.Replace("$", "").Replace("€", "").Replace("£", "").Replace(",", "").Trim();

                if (double.TryParse(cleanA, out double numA) && double.TryParse(cleanB, out double numB))
                {
                    return _isAscending ? numA.CompareTo(numB) : numB.CompareTo(numA);
                }

                // Fallback to string sort
                return _isAscending ? string.Compare(valA, valB, StringComparison.OrdinalIgnoreCase)
                                    : string.Compare(valB, valA, StringComparison.OrdinalIgnoreCase);
            });

            // Update header sort indicators
            for (int i = 0; i < _headerCells.Count; i++)
            {
                _headerCells[i].SetSortState(i == _sortedColumnIndex, _isAscending);
            }

            OnColumnSorted?.Invoke(_sortedColumnIndex, _isAscending);
            RefreshView();
        }

        /// <summary>
        /// Go to the previous page
        /// </summary>
        public void PreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                OnPageChanged?.Invoke(_currentPage);
                RefreshView();
            }
        }

        /// <summary>
        /// Go to the next page
        /// </summary>
        public void NextPage()
        {
            if (_currentPage < TotalPages)
            {
                _currentPage++;
                OnPageChanged?.Invoke(_currentPage);
                RefreshView();
            }
        }

        /// <summary>
        /// Go to the first page
        /// </summary>
        public void GoToFirstPage()
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                OnPageChanged?.Invoke(_currentPage);
                RefreshView();
            }
        }

        /// <summary>
        /// Go to the last page
        /// </summary>
        public void GoToLastPage()
        {
            int totalPages = TotalPages;
            if (_currentPage != totalPages)
            {
                _currentPage = totalPages;
                OnPageChanged?.Invoke(_currentPage);
                RefreshView();
            }
        }

        /// <summary>
        /// Go to a specific page
        /// </summary>
        public void GoToPage(int page)
        {
            int totalPages = TotalPages;
            page = Mathf.Clamp(page, 1, Mathf.Max(1, totalPages));
            if (_currentPage != page)
            {
                _currentPage = page;
                OnPageChanged?.Invoke(_currentPage);
                RefreshView();
            }
        }

        private void RebuildHeaders()
        {
            _headerContainer.Clear();
            _headerCells.Clear();

            foreach (var column in columns)
            {
                var headerCell = new ShunDataTableHeaderCell();
                headerCell.SetText(column.title);
                headerCell.SetSortable(column.isSortable);
                headerCell.style.height = _headerHeight;

                // Setup width
                if (column.isFlexible)
                {
                    headerCell.style.flexGrow = column.width;
                    headerCell.style.minWidth = 50;
                }
                else
                {
                    headerCell.style.width = column.width;
                    headerCell.style.flexGrow = 0;
                }

                // Setup click handler for sorting
                int colIndex = _headerCells.Count;
                headerCell.OnClick += () => SortByColumn(colIndex);

                _headerContainer.Add(headerCell);
                _headerCells.Add(headerCell);
            }
        }

        private void RefreshView()
        {
            ClearRows();

            int startIndex = (_currentPage - 1) * _itemsPerPage;
            int count = Mathf.Min(_itemsPerPage, data.Count - startIndex);

            for (int i = 0; i < count; i++)
            {
                int dataIndex = startIndex + i;
                var rowData = data[dataIndex];
                var row = CreateRow(rowData, dataIndex, i);
                _bodyContainer.Add(row);
                _rows.Add(row);
            }

            // Update pagination
            UpdatePagination();
        }

        private ShunDataTableRow CreateRow(ShunDataTableRowData rowData, int dataIndex, int displayIndex)
        {
            var row = new ShunDataTableRow();
            row.style.height = _rowHeight;

            // Apply striped style
            if (_stripedRows && displayIndex % 2 == 1)
            {
                row.AddToClassList("data-table__row--striped");
            }

            // Create cells
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = new ShunDataTableCell();
                string cellText = (i < rowData.cells.Count) ? rowData.cells[i] : "";
                cell.SetText(cellText);

                // Setup width to match header
                var column = columns[i];
                if (column.isFlexible)
                {
                    cell.style.flexGrow = column.width;
                    cell.style.minWidth = 50;
                }
                else
                {
                    cell.style.width = column.width;
                    cell.style.flexGrow = 0;
                }

                row.Add(cell);
            }

            // Setup click handler
            row.RegisterCallback<ClickEvent>(evt =>
            {
                OnRowClicked?.Invoke(dataIndex, rowData);
            });

            return row;
        }

        private void ClearRows()
        {
            _bodyContainer.Clear();
            _rows.Clear();
        }

        private void UpdatePagination()
        {
            int totalPages = TotalPages;
            int startItem = (_currentPage - 1) * _itemsPerPage + 1;
            int endItem = Mathf.Min(_currentPage * _itemsPerPage, data.Count);

            _pagination.UpdateInfo(_currentPage, totalPages, startItem, endItem, data.Count);
            
            // Hide pagination if there's only one page or no data
            _pagination.style.display = (totalPages <= 1) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
