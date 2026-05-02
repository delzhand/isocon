using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunCard : VisualElement
    {
        public ShunCard()
        {
            AddToClassList("card");
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunCardTitle : Label
    {
        public ShunCardTitle()
        {
            AddToClassList("card__title");
        }
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunCardDescription : Label
    {
        public ShunCardDescription()
        {
            AddToClassList("card__description");
        }
    }
}
