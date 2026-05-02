using UnityEngine.UIElements;

namespace ShunUI.Primitives
{
    /// <summary>
    /// Base class for all trigger/button elements that can have an open/close state
    /// Used by menu triggers, select triggers, etc.
    /// </summary>
    public abstract class ShunTrigger : ShunButton
    {
        protected bool _isOpen = false;

        /// <summary>
        /// Gets or sets the open state of the trigger
        /// </summary>
        public virtual bool isOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    UpdateOpenState();
                }
            }
        }

        /// <summary>
        /// Updates the visual state based on the open state
        /// </summary>
        protected virtual void UpdateOpenState()
        {
            if (_isOpen)
                AddToClassList("trigger--open");
            else
                RemoveFromClassList("trigger--open");
        }
    }

    /// <summary>
    /// Standard implementation of ShunTrigger for general use
    /// </summary>
    [UxmlElement]
    public partial class ShunMenuTrigger : ShunTrigger
    {
        public ShunMenuTrigger()
        {
            AddToClassList("menu-trigger");
        }
    }
}
