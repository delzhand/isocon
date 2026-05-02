using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunAccordion : VisualElement
    {
        public ShunAccordion()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("accordion");

            // Register for events
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (!ClassListContains("accordion"))
                    AddToClassList("accordion");
            });
        }

        public void AddItem(ShunAccordionItem item)
        {
            Add(item);
        }

        public void RemoveItem(ShunAccordionItem item)
        {
            Remove(item);
        }
    }
}