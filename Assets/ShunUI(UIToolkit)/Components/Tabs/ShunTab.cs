using UnityEngine.UIElements;
using System;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunTab : ShunButton
    {
        private bool _isActive = false;
        private string _tabId = "";

        [UxmlAttribute]
        public string tabId
        {
            get => _tabId;
            set => _tabId = value;
        }

        [UxmlAttribute]
        public bool isActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    UpdateState();
                }
            }
        }

        public ShunTab()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            // Add base tab class
            AddToClassList("tab");
            
            // Update initial state
            UpdateState();
        }

        private void UpdateState()
        {
            if (_isActive)
            {
                AddToClassList("tab--active");
            }
            else
            {
                RemoveFromClassList("tab--active");
            }
        }
    }
}
