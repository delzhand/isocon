using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public enum StatusIcon
    {
        positive,
        negative,
        neutral
    }

    public class TrackerStatus : VisualElement
    {
        private VisualElement rootElement;
        private Label labelElement;
        private VisualElement iconElement;



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

        private StatusIcon _icon;
        public StatusIcon icon
        {
            get => _icon;
            set
            {
                _icon = value;
                iconElement.RemoveFromClassList("neutral");
                iconElement.RemoveFromClassList("negative");
                iconElement.RemoveFromClassList("positive");
                iconElement.AddToClassList(_icon.ToString());
            }
        }

        public TrackerStatus()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/TrackerStatus");
            rootElement = element.Q("TrackerStatus");
            labelElement = element.Q<Label>("Label");
            iconElement = element.Q("Icon");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<TrackerStatus, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "Label" };
            UxmlEnumAttributeDescription<StatusIcon> _icon = new UxmlEnumAttributeDescription<StatusIcon> { name = "icon", defaultValue = StatusIcon.neutral };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var trackerStatus = visualElement as TrackerStatus;
                trackerStatus.label = _labelText.GetValueFromBag(bag, context);
                trackerStatus.icon = _icon.GetValueFromBag(bag, context);
            }
        }

    }

}