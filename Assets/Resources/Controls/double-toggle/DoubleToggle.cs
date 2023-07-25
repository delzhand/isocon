using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary {

    public class DoubleToggle : VisualElement 
    {
        public new class UxmlFactory : UxmlFactory<DoubleToggle, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits {
            UxmlStringAttributeDescription m_String = new UxmlStringAttributeDescription {name = "label", defaultValue="Label"};
            UxmlBoolAttributeDescription m_Bool1 = new UxmlBoolAttributeDescription {name = "value1", defaultValue = false};
            UxmlBoolAttributeDescription m_Bool2 = new UxmlBoolAttributeDescription {name = "value2", defaultValue = false};
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var ate = ve as DoubleToggle;
                ate.label = m_String.GetValueFromBag(bag, cc);
                ate.value1 = m_Bool1.GetValueFromBag(bag, cc);
                ate.value2 = m_Bool2.GetValueFromBag(bag, cc);
            }
        }

        private Label m_Label;
        private string m_LabelText; // Custom property for label text
        public string label
        {
            get => m_LabelText;
            set
            {
                m_LabelText = value;
                m_Label.text = m_LabelText; // Update the text of m_Label manually
            }
        }

        private Label m_Toggle1;
        private bool m_Value1;
        public bool value1
        {
            get => m_Value1;
            set
            {
                m_Value1 = value;
                m_Toggle1.text = value ? "active" : "off";
            }
        }

        private Label m_Toggle2;
        private bool m_Value2;
        public bool value2
        {
            get => m_Value2;
            set
            {
                m_Value2 = value;
                m_Toggle2.text = value ? "ongoing" : "off";
            }
        }

        public DoubleToggle() {
            this.AddToClassList("unity-base-field");

            m_Label = new Label();
            m_Label.text = "Label";
            m_Label.AddToClassList("unity-base-field__label");
            Add(m_Label);

            m_Toggle1 = new Label();
            m_Toggle1.text = "Off";
            m_Toggle1.RegisterCallback<ClickEvent>(Toggle1Click);
            Add(m_Toggle1);

            m_Toggle2 = new Label();
            m_Toggle2.text = "Off";
            m_Toggle2.RegisterCallback<ClickEvent>(Toggle2Click);
            Add(m_Toggle2);
        }

        public void Toggle1Click(ClickEvent evt) {
            value1 = !value1;
            evt.StopPropagation();
        }

        public void Toggle2Click(ClickEvent evt) {
            value2 = !value2;
            evt.StopPropagation();
        }

    }
}
