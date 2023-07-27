using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary {
    public class NumberNudger : VisualElement, INotifyValueChanged<int>
    {
        public new class UxmlFactory : UxmlFactory<NumberNudger, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits {
            UxmlStringAttributeDescription m_String = new UxmlStringAttributeDescription { name = "label", defaultValue = "Label" };
            UxmlIntAttributeDescription m_Int = new UxmlIntAttributeDescription { name = "value", defaultValue = 0 };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as NumberNudger;
                ate.label = m_String.GetValueFromBag(bag, cc);
                ate.value = m_Int.GetValueFromBag(bag, cc);
            }
        }

        private Label stringLabel;
        private string m_LabelText; // Custom property for label text
        public string label
        {
            get => m_LabelText;
            set
            {
                m_LabelText = value;
                stringLabel.text = m_LabelText; // Update the text of m_Label manually
            }
        }

        private Label valueLabel;
        private int m_ValueInt;
        public int value
        {
            get => m_ValueInt;
            set
            {
                if (m_ValueInt != value)
                {
                    int oldValue = m_ValueInt;
                    m_ValueInt = value;
                    valueLabel.text = value.ToString();
                    OnValueChanged(oldValue, value);
                }
            }
        }

        // Implementation of INotifyValueChanged<int>
        private List<Action<int>> valueChangedCallbacks;

        public void AddValueChangedCallback(Action<int> callback)
        {
            if (valueChangedCallbacks == null)
            {
                valueChangedCallbacks = new List<Action<int>>();
            }
            valueChangedCallbacks.Add(callback);
        }

        public void RemoveValueChangedCallback(Action<int> callback)
        {
            if (valueChangedCallbacks != null)
            {
                valueChangedCallbacks.Remove(callback);
            }
        }

        private void OnValueChanged(int oldValue, int newValue)
        {
            if (valueChangedCallbacks != null)
            {
                foreach (var callback in valueChangedCallbacks)
                {
                    callback(newValue);
                }
            }
        }

        public void SetValueWithoutNotify(int newValue)
        {
            m_ValueInt = newValue;
            valueLabel.text = newValue.ToString();
        }

        private Button downButton;
        private Button upButton;

        public NumberNudger()
        {
            this.AddToClassList("unity-base-field");
            this.AddToClassList("number-nudger");

            stringLabel = new Label();
            stringLabel.text = "Label";
            stringLabel.AddToClassList("unity-base-field__label");
            Add(stringLabel);

            downButton = new Button();
            downButton.text = "-";
            downButton.AddToClassList("down");
            downButton.RegisterCallback<ClickEvent>(downClick);
            Add(downButton);

            valueLabel = new Label();
            valueLabel.text = value.ToString();
            valueLabel.AddToClassList("value");
            Add(valueLabel);

            upButton = new Button();
            upButton.text = "+";
            upButton.AddToClassList("up");
            upButton.RegisterCallback<ClickEvent>(upClick);
            Add(upButton);
        }

        public void downClick(ClickEvent evt)
        {
            value = value - 1;
        }

        public void upClick(ClickEvent evt)
        {
            value = value + 1;
        }
    }
}
