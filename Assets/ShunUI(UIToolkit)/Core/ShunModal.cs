using UnityEngine.UIElements;

namespace ShunUI.Primitives
{
    public abstract class ShunModal : VisualElement
    {
        protected VisualElement _overlay;
        protected VisualElement _container;
        protected VisualElement _content;
        protected ShunButton _trigger;
        private bool _isOpen = false;
        private bool _closeOnBackdropClick = true;

        [UxmlAttribute]
        public virtual bool isOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    UpdateVisibility();
                }
            }
        }

        [UxmlAttribute]
        public bool closeOnBackdropClick
        {
            get => _closeOnBackdropClick;
            set => _closeOnBackdropClick = value;
        }

        protected ShunModal()
        {
            // Position relative to allow overlay positioning
            style.position = Position.Relative;
        }

        protected void CreateTrigger(string triggerClass = null)
        {
            _trigger = new ShunButton();
            if (!string.IsNullOrEmpty(triggerClass))
            {
                _trigger.AddToClassList(triggerClass);
            }
            _trigger.clicked += Toggle;
            hierarchy.Add(_trigger);
        }

        protected void CreateOverlay(string overlayClass, string containerClass, string contentClass)
        {
            // Create overlay (full screen backdrop)
            _overlay = new VisualElement();
            _overlay.AddToClassList(overlayClass);
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0;
            _overlay.style.top = 0;
            _overlay.style.right = 0;
            _overlay.style.bottom = 0;
            _overlay.style.display = DisplayStyle.None;
            hierarchy.Add(_overlay);

            // Register backdrop click
            _overlay.RegisterCallback<PointerDownEvent>(OnBackdropClick, TrickleDown.TrickleDown);

            // Create container (modal box)
            _container = new VisualElement();
            _container.AddToClassList(containerClass);
            _container.pickingMode = PickingMode.Position;
            _overlay.Add(_container);

            // Prevent backdrop clicks from reaching the modal content
            _container.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

            // Create content container
            _content = new VisualElement();
            _content.AddToClassList(contentClass);
            _container.Add(_content);
        }

        public virtual void Open()
        {
            isOpen = true;
        }

        public virtual void Close()
        {
            isOpen = false;
        }

        public virtual void Toggle()
        {
            isOpen = !isOpen;
        }

        protected virtual void UpdateVisibility()
        {
            if (_overlay == null) return;

            if (_isOpen)
            {
                _overlay.style.display = DisplayStyle.Flex;
                _overlay.pickingMode = PickingMode.Position;
                OnOpened();
            }
            else
            {
                _overlay.style.display = DisplayStyle.None;
                _overlay.pickingMode = PickingMode.Ignore;
                OnClosed();
            }
        }

        protected virtual void OnBackdropClick(PointerDownEvent evt)
        {
            if (!_closeOnBackdropClick) return;

            var clickedElement = evt.target as VisualElement;

            // Only close if clicking directly on the overlay (backdrop)
            if (clickedElement == _overlay)
            {
                Close();
                evt.StopPropagation();
            }
        }

        protected virtual void OnOpened()
        {
            // Override in derived classes for custom open logic
        }

        protected virtual void OnClosed()
        {
            // Override in derived classes for custom close logic
        }

        /// <summary>
        /// Moves the overlay to the root visual tree for proper z-index and full-screen coverage
        /// </summary>
        protected void MoveOverlayToRoot()
        {
            if (_overlay == null || panel == null) return;

            var root = panel.visualTree;
            if (root != null && _overlay.parent != root)
            {
                _overlay.RemoveFromHierarchy();
                root.Add(_overlay);

                // Ensure overlay covers the entire viewport
                _overlay.style.position = Position.Absolute;
                _overlay.style.left = 0;
                _overlay.style.top = 0;
                _overlay.style.right = 0;
                _overlay.style.bottom = 0;
            }
        }
    }
}
