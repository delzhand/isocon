using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public enum LabelPosition
    {
        normal,
        inline
    }

    public enum OptionColumns
    {
        two,
        three
    }

    public class OptionSelect : VisualElement
    {
        private VisualElement rootElement;
        private Label labelElement;

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

        private string _optionsList;
        public string optionsList
        {
            get => _optionsList;
            set
            {
                _optionsList = value;
                VisualElement optionsElement = rootElement.Q("Options");
                optionsElement.Clear();
                string[] options = _optionsList.Split(",");
                for (int i = 0; i < options.Length; i++)
                {
                    var button = new Button()
                    {
                        text = options[i]
                    };
                    if (i == 0)
                    {
                        button.AddToClassList("active");
                    }
                    optionsElement.Add(button);
                }
            }
        }

        private OptionColumns _optionColumns;
        public OptionColumns optionColumns
        {
            get => _optionColumns;
            set
            {
                _optionColumns = value;
                VisualElement optionsElement = rootElement.Q("Options");
                if (_optionColumns == OptionColumns.three)
                {
                    optionsElement.AddToClassList("three-up");
                }
                else
                {
                    optionsElement.RemoveFromClassList("three-up");
                }
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

        public OptionSelect()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/OptionSelect");
            rootElement = element.Q("OptionSelect");
            labelElement = element.Q<Label>("Label");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<OptionSelect, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "Label" };
            UxmlStringAttributeDescription _optionsList = new UxmlStringAttributeDescription { name = "options-list", defaultValue = "" };
            UxmlEnumAttributeDescription<LabelPosition> _labelPosition = new() { name = "label-position", defaultValue = LabelPosition.normal };
            UxmlEnumAttributeDescription<OptionColumns> _optionColumns = new() { name = "option-columns", defaultValue = OptionColumns.two };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var optionSelect = visualElement as OptionSelect;
                optionSelect.label = _labelText.GetValueFromBag(bag, context);
                optionSelect.optionsList = _optionsList.GetValueFromBag(bag, context);
                optionSelect.labelPosition = _labelPosition.GetValueFromBag(bag, context);
                optionSelect.optionColumns = _optionColumns.GetValueFromBag(bag, context);
            }
        }
    }

}
