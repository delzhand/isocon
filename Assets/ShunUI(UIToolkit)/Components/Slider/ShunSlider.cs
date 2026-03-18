using UnityEngine.UIElements;
using UnityEngine;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunSlider : VisualElement
    {
        private VisualElement _track;
        private VisualElement _fill;
        private VisualElement _thumb;

        private float _value = 50f;
        private float _min = 0f;
        private float _max = 100f;
        private bool _isDragging = false;

        [UxmlAttribute]
        public float min
        {
            get => _min;
            set
            {
                _min = value;
                UpdateSlider();
            }
        }

        [UxmlAttribute]
        public float max
        {
            get => _max;
            set
            {
                _max = value;
                UpdateSlider();
            }
        }

        [UxmlAttribute]
        public float value
        {
            get => _value;
            set
            {
                _value = Mathf.Clamp(value, _min, _max);
                UpdateSlider();
            }
        }

        public ShunSlider()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("slider");

            // Create track (gray background)
            _track = new VisualElement();
            _track.AddToClassList("slider__track");
            hierarchy.Add(_track);

            // Create fill (black filled portion)
            _fill = new VisualElement();
            _fill.AddToClassList("slider__fill");
            _track.hierarchy.Add(_fill);

            // Create thumb (white circle)
            _thumb = new VisualElement();
            _thumb.AddToClassList("slider__thumb");
            hierarchy.Add(_thumb);

            // Register events
            RegisterCallback<GeometryChangedEvent>(evt => UpdateSlider());
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDragging = true;
            this.CapturePointer(evt.pointerId);
            UpdateValueFromPosition(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isDragging)
            {
                UpdateValueFromPosition(evt.localPosition.x);
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _isDragging = false;
        }

        private void UpdateValueFromPosition(float posX)
        {
            float trackWidth = resolvedStyle.width;
            if (trackWidth <= 0) return;

            float normalizedValue = Mathf.Clamp01(posX / trackWidth);
            _value = _min + normalizedValue * (_max - _min);
            UpdateSlider();
        }

        private void UpdateSlider()
        {
            if (_track == null || _fill == null || _thumb == null) return;

            float trackWidth = resolvedStyle.width;
            if (trackWidth <= 0) return;

            float normalizedValue = (_value - _min) / (_max - _min);
            float fillWidth = trackWidth * normalizedValue;

            // Update fill width
            _fill.style.width = fillWidth;

            // Update thumb position
            _thumb.style.left = fillWidth - 8; // Center thumb on fill edge (8px = half of 16px thumb)
        }
    }
}