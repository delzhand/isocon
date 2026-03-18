using UnityEngine.UIElements;
using ShunUI.Primitives;

namespace ShunUI
{
    /// <summary>
    /// Hover menu item - alias for ShunMenuItemBase for backward compatibility
    /// </summary>
    [UxmlElement]
    public partial class ShunHoverMenuItem : ShunMenuItemBase
    {
        public ShunHoverMenuItem()
        {
            // Inherits all functionality from ShunMenuItemBase
        }
    }
}
