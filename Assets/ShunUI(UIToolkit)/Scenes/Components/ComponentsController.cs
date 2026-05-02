using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// Controller for Components.uxml showcase scene.
    /// Handles interactions for all component demos including Sonner toasts.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ComponentsController : MonoBehaviour
    {
        [Header("Theme Management")]
        [SerializeField] private ShunThemeManager _themeManager;
        [SerializeField] private ShunTheme _defaultTheme;
        [SerializeField] private ShunTheme _darkTheme;
        [SerializeField] private ShunTheme _squareTheme;
        [SerializeField] private ShunTheme _blueTheme;
        [SerializeField] private ShunTheme _violetTheme;
        [SerializeField] private ShunTheme _roseTheme;
        [SerializeField] private ShunTheme _redTheme;
        [SerializeField] private ShunTheme _orangeTheme;
        [SerializeField] private ShunTheme _greenTheme;

        [Header("DataTable Configuration")]
        [SerializeField] private List<ShunDataTableColumn> _dataTableColumns = new List<ShunDataTableColumn>
        {
            new ShunDataTableColumn { title = "Invoice", width = 1f, isFlexible = true },
            new ShunDataTableColumn { title = "Status", width = 1f, isFlexible = true },
            new ShunDataTableColumn { title = "Method", width = 1f, isFlexible = true },
            new ShunDataTableColumn { title = "Amount", width = 1f, isFlexible = true }
        };
        
        [SerializeField] private List<ShunDataTableRowData> _dataTableData = new List<ShunDataTableRowData>
        {
            new ShunDataTableRowData { cells = new List<string> { "INV001", "Paid", "Credit Card", "$250.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV002", "Pending", "PayPal", "$150.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV003", "Unpaid", "Bank Transfer", "$350.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV004", "Paid", "Credit Card", "$450.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV005", "Paid", "PayPal", "$550.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV006", "Pending", "Bank Transfer", "$200.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV007", "Unpaid", "Credit Card", "$300.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV008", "Paid", "PayPal", "$400.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV009", "Pending", "Credit Card", "$500.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV010", "Paid", "Bank Transfer", "$600.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV011", "Unpaid", "PayPal", "$150.00" } },
            new ShunDataTableRowData { cells = new List<string> { "INV012", "Paid", "Credit Card", "$750.00" } },
        };

        private void OnEnable()
        {
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null) return;
            
            var root = uiDoc.rootVisualElement;
            if (root == null) return;

            // Theme Switcher
            root.Q<ShunButton>("btn-theme-default")?.RegisterCallback<ClickEvent>(evt => SetTheme(_defaultTheme));
            root.Q<ShunButton>("btn-theme-dark")?.RegisterCallback<ClickEvent>(evt => SetTheme(_darkTheme));
            root.Q<ShunButton>("btn-theme-square")?.RegisterCallback<ClickEvent>(evt => SetTheme(_squareTheme));
            root.Q<ShunButton>("btn-theme-blue")?.RegisterCallback<ClickEvent>(evt => SetTheme(_blueTheme));
            root.Q<ShunButton>("btn-theme-violet")?.RegisterCallback<ClickEvent>(evt => SetTheme(_violetTheme));
            root.Q<ShunButton>("btn-theme-rose")?.RegisterCallback<ClickEvent>(evt => SetTheme(_roseTheme));
            root.Q<ShunButton>("btn-theme-red")?.RegisterCallback<ClickEvent>(evt => SetTheme(_redTheme));
            root.Q<ShunButton>("btn-theme-orange")?.RegisterCallback<ClickEvent>(evt => SetTheme(_orangeTheme));
            root.Q<ShunButton>("btn-theme-green")?.RegisterCallback<ClickEvent>(evt => SetTheme(_greenTheme));

            // Data Table Demo
            SetupDataTable(root);

            // Sonner Demo - Variant buttons
            root.Q<ShunButton>("btn-sonner-default")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Toast("", "Event has been created", showIcon: false, showDescription: false));

            root.Q<ShunButton>("btn-sonner-description")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Description("You can use this component to display toast notifications to users. They will automatically dismiss after a few seconds.", "Event Created"));
            
            root.Q<ShunButton>("btn-sonner-success")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Success("Your changes have been saved successfully!", "Success"));
            
            root.Q<ShunButton>("btn-sonner-info")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Info("New features are now available in the dashboard", "Info"));
            
            root.Q<ShunButton>("btn-sonner-warning")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Warning("Your session will expire in 5 minutes", "Warning"));
            
            root.Q<ShunButton>("btn-sonner-error")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Error("Failed to connect to the server", "Error"));
            
            root.Q<ShunButton>("btn-sonner-action")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Action("", "File Deleted", "Undo", () => Debug.Log("Undo clicked!")));

            // Sonner Demo - Position buttons
            root.Q<ShunButton>("btn-sonner-top-left")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Info("This toast appears in the top left corner", "Top Left", ToastPosition.TopLeft));
            
            root.Q<ShunButton>("btn-sonner-top-center")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Info("This toast appears in the top center", "Top Center", ToastPosition.TopCenter));
            
            root.Q<ShunButton>("btn-sonner-top-right")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Info("This toast appears in the top right corner", "Top Right", ToastPosition.TopRight));
            
            root.Q<ShunButton>("btn-sonner-bottom-left")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Success("This toast appears in the bottom left corner", "Bottom Left", ToastPosition.BottomLeft));
            
            root.Q<ShunButton>("btn-sonner-bottom-center")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Success("This toast appears in the bottom center", "Bottom Center", ToastPosition.BottomCenter));
            
            root.Q<ShunButton>("btn-sonner-bottom-right")?.RegisterCallback<ClickEvent>(evt => 
                ShunSonner.Success("This toast appears in the bottom right corner", "Bottom Right", ToastPosition.BottomRight));
        }

        private void SetTheme(ShunTheme theme)
        {
            if (_themeManager != null && theme != null)
            {
                _themeManager.CurrentTheme = theme;
            }
        }

        private void SetupDataTable(VisualElement root)
        {
            var dataTable = root.Q<ShunDataTable>("demo-data-table");
            if (dataTable == null) return;

            // Use the serialized fields exposed in the Inspector
            dataTable.columns = _dataTableColumns;
            dataTable.data = _dataTableData;
            dataTable.Refresh();

            // Subscribe to events
            dataTable.OnRowClicked += (index, rowData) => 
                Debug.Log($"Row clicked: Index={index}, Invoice={rowData.cells[0]}");
        }
    }
}
