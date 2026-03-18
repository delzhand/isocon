using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunRadioGroup : RadioButtonGroup
    {
        public ShunRadioGroup()
        {
            Initialize();
        }

        public ShunRadioGroup(string label) : base(label)
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("radio-group");

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (!ClassListContains("radio-group"))
                    AddToClassList("radio-group");
            });
        }
    }
}