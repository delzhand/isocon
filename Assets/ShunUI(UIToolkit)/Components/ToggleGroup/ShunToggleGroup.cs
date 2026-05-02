using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunToggleGroup : VisualElement
    {
        private List<ShunToggle> _toggles = new List<ShunToggle>();
        private bool _allowMultiple = false;

        [UxmlAttribute]
        public bool allowMultiple
        {
            get => _allowMultiple;
            set => _allowMultiple = value;
        }

        public ShunToggleGroup()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base toggle-group class
            AddToClassList("toggle-group");

            // Set layout
            style.flexDirection = FlexDirection.Row;

            // Register callback to track child toggles
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                RegisterChildToggles();
                UpdateTogglePositions();
            });

            // Also register for when children are added
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                UpdateTogglePositions();
            });
        }

        private void RegisterChildToggles()
        {
            // Find all ShunToggle children
            var toggles = this.Query<ShunToggle>().ToList();

            foreach (var toggle in toggles)
            {
                if (!_toggles.Contains(toggle))
                {
                    _toggles.Add(toggle);

                    // Subscribe to toggle clicks
                    toggle.clicked += () => OnToggleClicked(toggle);
                }
            }
        }

        private void UpdateTogglePositions()
        {
            var toggles = this.Query<ShunToggle>().ToList();

            for (int i = 0; i < toggles.Count; i++)
            {
                var toggle = toggles[i];

                // Remove all position classes
                toggle.RemoveFromClassList("toggle-first");
                toggle.RemoveFromClassList("toggle-middle");
                toggle.RemoveFromClassList("toggle-last");

                // Add appropriate position class
                if (toggles.Count == 1)
                {
                    // Single toggle, don't add any position class
                }
                else if (i == 0)
                {
                    toggle.AddToClassList("toggle-first");
                }
                else if (i == toggles.Count - 1)
                {
                    toggle.AddToClassList("toggle-last");
                }
                else
                {
                    toggle.AddToClassList("toggle-middle");
                }
            }
        }

        private void OnToggleClicked(ShunToggle clickedToggle)
        {
            if (!_allowMultiple && !clickedToggle.isOn)
            {
                // Turn off other toggles
                foreach (var toggle in _toggles)
                {
                    if (toggle != clickedToggle && toggle.isOn)
                    {
                        toggle.isOn = false;
                    }
                }
            }
        }

        public void Add(ShunToggle toggle)
        {
            base.Add(toggle);

            if (!_toggles.Contains(toggle))
            {
                _toggles.Add(toggle);
                toggle.clicked += () => OnToggleClicked(toggle);
            }

            UpdateTogglePositions();
        }
    }
}
