using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public enum GrayButtonDescPosition
    {
        top,
        right,
        bottom,
        left
    }

    public class GrayButtonLarge : VisualElement
    {
        private Label labelElement;
        private Label descriptionElement;
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

        private string _descriptionText;
        public string description
        {
            get => _descriptionText;
            set
            {
                _descriptionText = value;
                descriptionElement.text = _descriptionText;
            }
        }

        private Texture2D _iconGraphic;
        public Texture2D icon
        {
            get => _iconGraphic;
            set
            {
                _iconGraphic = value;
                iconElement.style.backgroundImage = _iconGraphic;
            }
        }

        private GrayButtonDescPosition _descPosition;
        public GrayButtonDescPosition descriptionPosition
        {
            get => _descPosition;
            set
            {
                _descPosition = value;
                descriptionElement.RemoveFromClassList("top");
                descriptionElement.RemoveFromClassList("right");
                descriptionElement.RemoveFromClassList("bottom");
                descriptionElement.RemoveFromClassList("left");
                string descClass = value.ToString();
                descriptionElement.AddToClassList(descClass);
            }
        }

        public GrayButtonLarge()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/GrayButtonLarge");
            labelElement = element.Q<Label>("Label");
            descriptionElement = element.Q<Label>("Description");
            iconElement = element.Q("Icon");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<GrayButtonLarge, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "Label" };
            UxmlStringAttributeDescription _descriptionText = new UxmlStringAttributeDescription { name = "description", defaultValue = "Description" };
            UxmlAssetAttributeDescription<Texture2D> _iconGraphic = new UxmlAssetAttributeDescription<Texture2D> { name = "icon", defaultValue = null };
            UxmlEnumAttributeDescription<GrayButtonDescPosition> _descriptionPosition = new() { name = "description-position", defaultValue = GrayButtonDescPosition.bottom };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var grayButtonLarge = visualElement as GrayButtonLarge;
                grayButtonLarge.label = _labelText.GetValueFromBag(bag, context);
                grayButtonLarge.description = _descriptionText.GetValueFromBag(bag, context);
                grayButtonLarge.icon = _iconGraphic.GetValueFromBag(bag, context);
                grayButtonLarge.descriptionPosition = _descriptionPosition.GetValueFromBag(bag, context);
            }
        }
    }

}
