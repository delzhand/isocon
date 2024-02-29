using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public class TextInput : VisualElement
    {
        private VisualElement rootElement;
        private Label labelElement;
        private TextInput inputElement;

        private string _labelText;
        public string label
        {
            get => _labelText;
            set
            {
                _labelText = value;
                labelElement.text = _labelText;
            }
        }

        private LabelPosition _labelPosition;
        public LabelPosition labelPosition
        {
            get => _labelPosition;
            set
            {
                _labelPosition = value;
                if (_labelPosition == LabelPosition.inline)
                {
                    rootElement.AddToClassList("inline");
                }
                else
                {
                    rootElement.RemoveFromClassList("inline");
                }
            }
        }

        public TextInput()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/TextInput");
            rootElement = element.Q("TextInput");
            labelElement = element.Q<Label>("Label");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<TextInput, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "Label" };
            UxmlEnumAttributeDescription<LabelPosition> _labelPosition = new() { name = "label-position", defaultValue = LabelPosition.normal };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var textInput = visualElement as TextInput;
                textInput.label = _labelText.GetValueFromBag(bag, context);
                textInput.labelPosition = _labelPosition.GetValueFromBag(bag, context);
            }
        }

    }

}