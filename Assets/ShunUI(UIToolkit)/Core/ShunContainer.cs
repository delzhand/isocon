using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// A generic container primitive that exposes its content in UXML.
    /// This allows users to drag and drop visual elements in the UI Builder.
    /// </summary>
    [System.Serializable]
    [UxmlElement]
    public partial class ShunContainer : VisualElement
    {
        public ShunContainer()
        {
            AddToClassList("shun-container");
        }

        // By default, VisualElement already accepts children in UXML
        // This container serves as a semantic primitive for ShunUI components
    }
}
