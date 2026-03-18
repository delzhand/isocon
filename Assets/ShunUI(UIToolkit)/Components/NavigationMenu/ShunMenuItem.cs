using UnityEngine.UIElements;
using ShunUI.Primitives;

namespace ShunUI
{
    /// <summary>
    /// Navigation menu item - alias for ShunMenuItemBase for backward compatibility
    /// </summary>
    [UxmlElement]
    public partial class ShunMenuItem : ShunMenuItemBase
    {
        public ShunMenuItem() : base()
        {
            // Inherits all functionality from ShunMenuItemBase
        }
    }
}
