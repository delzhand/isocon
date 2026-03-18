using UnityEngine.UIElements;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunProgress : VisualElement
    {
        private VisualElement _indicator;

        [UxmlAttribute]
        public float value
        {
            get => _value;
            set
            {
                _value = UnityEngine.Mathf.Clamp(value, 0f, 100f);
                UpdateProgress();
            }
        }
        private float _value = 0f;

        public ShunProgress()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("progress");

            // Create indicator
            _indicator = new VisualElement();
            _indicator.AddToClassList("progress__indicator");
            hierarchy.Add(_indicator);

            // Initial update
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (_indicator != null)
            {
                _indicator.style.width = Length.Percent(_value);
            }
        }
    }
}