using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public class GrayButtonSmall : VisualElement
    {
        private Label descriptionElement;
        private VisualElement iconElement;

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

        public GrayButtonSmall()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/GrayButtonSmall");
            descriptionElement = element.Q<Label>("Description");
            iconElement = element.Q("Icon");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<GrayButtonSmall, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _descriptionText = new UxmlStringAttributeDescription { name = "description", defaultValue = "Description" };
            UxmlAssetAttributeDescription<Texture2D> _iconGraphic = new UxmlAssetAttributeDescription<Texture2D> { name = "icon", defaultValue = null };
            UxmlEnumAttributeDescription<GrayButtonDescPosition> _descriptionPosition = new() { name = "description-position", defaultValue = GrayButtonDescPosition.bottom };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var grayButtonSmall = visualElement as GrayButtonSmall;
                grayButtonSmall.description = _descriptionText.GetValueFromBag(bag, context);
                grayButtonSmall.icon = _iconGraphic.GetValueFromBag(bag, context);
                grayButtonSmall.descriptionPosition = _descriptionPosition.GetValueFromBag(bag, context);
            }
        }
    }

}
