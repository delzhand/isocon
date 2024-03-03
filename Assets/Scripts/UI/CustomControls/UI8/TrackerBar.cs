using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary
{
    public class TrackerBar : VisualElement
    {
        private Label labelElement;
        private Label valueElement;
        private ProgressBar mainBarElement;
        private ProgressBar overBarElement;

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

        private int _mainValue;
        public int mainValue
        {
            get => _mainValue;
            set
            {
                _mainValue = value;
                redraw();
            }
        }

        private int _maxValue;
        public int maxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                redraw();
            }
        }

        private int _overValue;
        public int overValue
        {
            get => _overValue;
            set
            {
                _overValue = value;
                redraw();
            }
        }

        private Color _mainBackground;
        public Color mainBackground
        {
            get => _mainBackground;
            set
            {
                _mainBackground = value;
                mainBarElement.Query(null, "unity-progress-bar__background").First().style.backgroundColor = _mainBackground;
            }
        }

        private Color _mainColor;
        public Color mainColor
        {
            get => _mainColor;
            set
            {
                _mainColor = value;
                mainBarElement.Query(null, "unity-progress-bar__progress").First().style.backgroundColor = _mainColor;
            }
        }

        private Color _overColor;
        public Color overColor
        {
            get => _overColor;
            set
            {
                _overColor = value;
                overBarElement.Query(null, "unity-progress-bar__progress").First().style.backgroundColor = _overColor;
                redraw();
            }
        }

        private void redraw()
        {
            if (_overValue > 0)
            {
                valueElement.text = $"{_mainValue}<color={ColorUtility.GetHex(_overColor)}>+{_overValue}</color>/{_maxValue}";
            }
            else
            {
                valueElement.text = $"{_mainValue}/{_maxValue}";
            }
            mainBarElement.highValue = _maxValue;
            mainBarElement.value = _mainValue;
            overBarElement.highValue = _maxValue;
            overBarElement.value = _overValue;
        }

        public TrackerBar()
        {
            VisualElement element = UI.CreateFromTemplate("UITemplates/UI8/TrackerBar");
            labelElement = element.Q<Label>("Label");
            valueElement = element.Q<Label>("Value");
            mainBarElement = element.Q<ProgressBar>("MainBar");
            overBarElement = element.Q<ProgressBar>("OverBar");
            Add(element);
        }

        public new class UxmlFactory : UxmlFactory<TrackerBar, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _labelText = new UxmlStringAttributeDescription { name = "label", defaultValue = "LABEL" };
            UxmlIntAttributeDescription _mainValue = new UxmlIntAttributeDescription { name = "main-value", defaultValue = 73 };
            UxmlIntAttributeDescription _overValue = new UxmlIntAttributeDescription { name = "over-value", defaultValue = 11 };
            UxmlIntAttributeDescription _maxValue = new UxmlIntAttributeDescription { name = "max-value", defaultValue = 100 };
            UxmlColorAttributeDescription _mainBackgroundColor = new() { name = "main-background", defaultValue = ColorUtility.GetColor("#bcbcbc") };
            UxmlColorAttributeDescription _mainForegroundColor = new() { name = "main-color", defaultValue = ColorUtility.GetColor("#e7e7e7") };
            UxmlColorAttributeDescription _overForegroundColor = new() { name = "over-color", defaultValue = ColorUtility.GetColor("#666666") };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);
                var trackerBar = visualElement as TrackerBar;
                trackerBar.label = _labelText.GetValueFromBag(bag, context);
                trackerBar.mainValue = _mainValue.GetValueFromBag(bag, context);
                trackerBar.maxValue = _maxValue.GetValueFromBag(bag, context);
                trackerBar.overValue = _overValue.GetValueFromBag(bag, context);
                trackerBar.mainBackground = _mainBackgroundColor.GetValueFromBag(bag, context);
                trackerBar.mainColor = _mainForegroundColor.GetValueFromBag(bag, context);
                trackerBar.overColor = _overForegroundColor.GetValueFromBag(bag, context);
            }
        }

    }
}