using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public class StatLine : VisualElement
    {
        private Label labelElement;
        private Label valueElement;

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

        private string _valueText;
        public string value
        {
            get => _valueText;
            set
            {
                _valueText = value;
                valueElement.text = _valueText;
            }
        }

        public StatLine()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/StatLine");
            labelElement = element.Q<Label>("Label");
            valueElement = element.Q<Label>("Value");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<StatLine, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "LABEL" };
            UxmlStringAttributeDescription _valueText = new UxmlStringAttributeDescription { name = "value", defaultValue = "1" };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var statLine = visualElement as StatLine;
                statLine.label = _labelText.GetValueFromBag(bag, context);
                statLine.value = _valueText.GetValueFromBag(bag, context);
            }
        }

    }

}