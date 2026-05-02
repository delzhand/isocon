using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunSkeleton : VisualElement
    {
        private IVisualElementScheduledItem _pulseSchedule;
        private float _elapsed;
        private const float Duration = 2f;
        private const float Interval = 16L; // ~60fps

        public ShunSkeleton()
        {
            AddToClassList("skeleton");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            _elapsed = 0f;
            _pulseSchedule = schedule.Execute(UpdatePulse).Every((long)Interval);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            _pulseSchedule?.Pause();
            _pulseSchedule = null;
        }

        private void UpdatePulse()
        {
            _elapsed += Interval / 1000f;
            if (_elapsed >= Duration)
                _elapsed -= Duration;

            // Normalized time [0, 1]
            float t = _elapsed / Duration;

            // Sine wave: smoothly oscillates between 1.0 and 0.5
            float opacity = 0.75f + 0.25f * Mathf.Cos(t * Mathf.PI * 2f);

            style.opacity = opacity;
        }
    }
}
