using System;
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

        private VisualElement toggle1;
        private bool m_Value1;
        public bool value1
        {
            get => m_Value1;
            set
            {
                if(value1 != value) {
                    (bool,bool) oldValue = (value1,value2);
                    m_Value1 = value;
                    if (value) {
                        toggle1.AddToClassList("checked");
                    }
                    else {
                        toggle1.RemoveFromClassList("checked");
                    }
                    OnValueChanged(oldValue, (value1,value2));
                }
            }
        }

        private VisualElement toggle2;
        private bool m_Value2;
        public bool value2
        {
            get => m_Value2;
            set
            {
                if (value2 != value) {
                    (bool,bool) oldValue = (value1,value2);
                    m_Value2 = value;
                    if (value) {
                        toggle2.AddToClassList("checked");
                    }
                    else {
                        toggle2.RemoveFromClassList("checked");
                    }
                    OnValueChanged(oldValue, (value1,value2));
                }
            }
        }

        private List<Action<ChangeEvent<(bool,bool)>>> valueChangedCallbacks;
        public void AddValueChangedCallback(Action<ChangeEvent<(bool,bool)>> callback) {
            if (valueChangedCallbacks == null) {
                valueChangedCallbacks = new List<Action<ChangeEvent<(bool,bool)>>>();
            }
            valueChangedCallbacks.Add(callback);
        }
        private void OnValueChanged((bool,bool) oldValue, (bool,bool) newValue) {
            if (valueChangedCallbacks != null) {
                foreach (var callback in valueChangedCallbacks) {
                    ChangeEvent<(bool,bool)> c = ChangeEvent<(bool,bool)>.GetPooled(oldValue, newValue);
                    c.target = this;
                    callback(c);
                }
            }
        }

        public void SetValuesWithoutNotify(bool v1, bool v2) {
            m_Value1 = v1;
            m_Value2 = v2;
        }

        public DoubleToggle() {
            this.AddToClassList("unity-base-field");
            this.AddToClassList("unity-toggle");
            this.AddToClassList("double-toggle");

            m_Label = new Label();
            m_Label.text = "Label";
            m_Label.AddToClassList("unity-base-field__label");
            Add(m_Label);

            toggle1 = new VisualElement();
            toggle1.AddToClassList("unity-base-field__input");
            toggle1.AddToClassList("unity-toggle__input");
            toggle1.RegisterCallback<ClickEvent>(Toggle1Click);
            Add(toggle1);

            Label ongoingLabel = new Label();
            ongoingLabel.text = "Ongoing";
            Add(ongoingLabel);

            VisualElement toggle1check = new VisualElement();
            toggle1check.AddToClassList("unity-toggle__checkmark");
            toggle1.Add(toggle1check);

            toggle2 = new VisualElement();
            toggle2.AddToClassList("unity-base-field__input");
            toggle2.AddToClassList("unity-toggle__input");
            toggle2.RegisterCallback<ClickEvent>(Toggle2Click);
            Add(toggle2);

            VisualElement toggle2check = new VisualElement();
            toggle2check.AddToClassList("unity-toggle__checkmark");
            toggle2.Add(toggle2check);
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
