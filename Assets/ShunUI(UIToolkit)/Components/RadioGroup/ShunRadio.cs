using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunRadio : RadioButton
    {
        public ShunRadio()
        {
            Initialize();
        }

        public ShunRadio(string label) : base(label)
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("radio");

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (!ClassListContains("radio"))
                    AddToClassList("radio");
            });
        }
    }
}