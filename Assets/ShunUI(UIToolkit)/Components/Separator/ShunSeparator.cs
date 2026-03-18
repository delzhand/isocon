using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunSeparator : VisualElement
    {
        private Orientation m_Orientation = Orientation.Horizontal;

        [UxmlAttribute]
        public Orientation orientation
        {
            get => m_Orientation;
            set => SetOrientation(value);
        }

        public ShunSeparator()
        {
            AddToClassList("shun-separator");
            SetOrientation(Orientation.Horizontal);
        }

        public void SetOrientation(Orientation orientation)
        {
            m_Orientation = orientation;

            RemoveFromClassList("shun-separator--horizontal");
            RemoveFromClassList("shun-separator--vertical");

            if (orientation == Orientation.Horizontal)
            {
                AddToClassList("shun-separator--horizontal");
            }
            else
            {
                AddToClassList("shun-separator--vertical");
            }
        }

        public enum Orientation
        {
            Horizontal,
            Vertical
        }
    }
}
