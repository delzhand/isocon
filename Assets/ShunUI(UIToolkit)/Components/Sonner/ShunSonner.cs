using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace ShunUI
{
    public enum ToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum ToastVariant
    {
        Default,
        Success,
        Error,
        Warning,
        Info
    }



    [UxmlElement]
    public partial class ShunSonner : VisualElement
    {
        private static Dictionary<ToastPosition, ShunSonner> s_Instances;
        private List<ToastItem> m_ActiveToasts = new List<ToastItem>();
        private const int MAX_VISIBLE_TOASTS = 3;
        private const float DEFAULT_DURATION = 5000f; // milliseconds
        private bool m_IsInitialized = false;

        private ToastPosition m_Position = ToastPosition.BottomRight;
        private Texture2D m_SuccessIcon = null;
        private Texture2D m_ErrorIcon = null;
        private Texture2D m_WarningIcon = null;
        private Texture2D m_InfoIcon = null;
        private Texture2D m_CloseIcon = null;

        [UxmlAttribute]
        public ToastPosition position
        {
            get => m_Position;
            set
            {
                if (m_Position != value)
                {
                    // Unregister from old position
                    if (s_Instances.ContainsKey(m_Position) && s_Instances[m_Position] == this)
                    {
                        s_Instances.Remove(m_Position);
                    }

                    m_Position = value;

                    // Register in new position first
                    s_Instances[m_Position] = this;

                    // Then update visual position
                    if (m_IsInitialized)
                    {
                        UpdatePosition();
                    }
                }
            }
        }

        /// <summary>
        /// Default icon for Success toasts. Set in UI Builder or via code.
        /// </summary>
        [UxmlAttribute]
        public Texture2D successIcon
        {
            get => m_SuccessIcon;
            set => m_SuccessIcon = value;
        }

        /// <summary>
        /// Default icon for Error toasts. Set in UI Builder or via code.
        /// </summary>
        [UxmlAttribute]
        public Texture2D errorIcon
        {
            get => m_ErrorIcon;
            set => m_ErrorIcon = value;
        }

        /// <summary>
        /// Default icon for Warning toasts. Set in UI Builder or via code.
        /// </summary>
        [UxmlAttribute]
        public Texture2D warningIcon
        {
            get => m_WarningIcon;
            set => m_WarningIcon = value;
        }

        /// <summary>
        /// Default icon for Info toasts. Set in UI Builder or via code.
        /// </summary>
        [UxmlAttribute]
        public Texture2D infoIcon
        {
            get => m_InfoIcon;
            set => m_InfoIcon = value;
        }

        /// <summary>
        /// Icon for the close button. Set in UI Builder or via code.
        /// </summary>
        [UxmlAttribute]
        public Texture2D closeIcon
        {
            get => m_CloseIcon;
            set => m_CloseIcon = value;
        }

        public ShunSonner()
        {
            if (s_Instances == null)
            {
                s_Instances = new Dictionary<ToastPosition, ShunSonner>();
            }

            AddToClassList("sonner");

            style.position = Position.Absolute;
            pickingMode = PickingMode.Ignore; // Container doesn't block clicks, but children (toasts) will

            // Make visible by default
            style.display = DisplayStyle.Flex;

            // Register hover callbacks to pause/resume timers
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            PauseAllToasts();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            ResumeAllToasts();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;

            // Register this instance for static toast calls FIRST
            s_Instances[m_Position] = this;

            // Setup immediately - no delays
            UpdatePosition();
            m_IsInitialized = true;
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            // Unregister this instance
            if (s_Instances.ContainsKey(m_Position) && s_Instances[m_Position] == this)
            {
                s_Instances.Remove(m_Position);
            }
        }

        private void UpdatePosition()
        {
            RemoveFromClassList("sonner--top-left");
            RemoveFromClassList("sonner--top-center");
            RemoveFromClassList("sonner--top-right");
            RemoveFromClassList("sonner--bottom-left");
            RemoveFromClassList("sonner--bottom-center");
            RemoveFromClassList("sonner--bottom-right");

            string positionClass = m_Position switch
            {
                ToastPosition.TopLeft => "sonner--top-left",
                ToastPosition.TopCenter => "sonner--top-center",
                ToastPosition.TopRight => "sonner--top-right",
                ToastPosition.BottomLeft => "sonner--bottom-left",
                ToastPosition.BottomCenter => "sonner--bottom-center",
                ToastPosition.BottomRight => "sonner--bottom-right",
                _ => "sonner--bottom-right"
            };

            AddToClassList(positionClass);
        }

        public void ShowToast(
            string message,
            string title = null,
            ToastVariant variant = ToastVariant.Default,
            float duration = DEFAULT_DURATION,
            bool showIcon = true,
            Texture2D customIcon = null,
            bool showDescription = true,
            bool showActionButton = false,
            string actionButtonText = "Action",
            System.Action onActionClick = null)
        {
            var toast = new ToastItem(this, message, title, variant, duration, showIcon, customIcon, showDescription, showActionButton, actionButtonText);

            // Always add to the end - newest toasts are at the end of the list
            Add(toast.element);
            m_ActiveToasts.Add(toast);

            UpdateStackPositions();

            // Schedule auto-dismiss (stored so we can pause/resume)
            toast.dismissSchedule = toast.element.schedule.Execute(() => DismissToast(toast)).StartingIn((long)duration);

            // Setup dismiss button using ShunToast's method
            toast.element.RegisterCloseCallback(() => DismissToast(toast));

            // Setup action button callback if provided
            if (onActionClick != null)
            {
                toast.element.RegisterActionCallback(onActionClick);
            }
        }

        private void PauseAllToasts()
        {
            foreach (var toast in m_ActiveToasts)
            {
                toast.dismissSchedule?.Pause();
            }
        }

        private void ResumeAllToasts()
        {
            foreach (var toast in m_ActiveToasts)
            {
                toast.dismissSchedule?.Resume();
            }
        }

        private void DismissToast(ToastItem toast)
        {
            if (!m_ActiveToasts.Contains(toast)) return;

            // Remove immediately without animation to avoid positioning glitches
            if (toast.element.parent != null)
            {
                toast.element.RemoveFromHierarchy();
            }
            m_ActiveToasts.Remove(toast);
            UpdateStackPositions();
        }

        private void UpdateStackPositions()
        {
            bool isTopPosition = m_Position == ToastPosition.TopLeft ||
                                m_Position == ToastPosition.TopCenter ||
                                m_Position == ToastPosition.TopRight;

            // Keep only the most recent MAX_VISIBLE_TOASTS, dismiss the rest
            if (m_ActiveToasts.Count > MAX_VISIBLE_TOASTS)
            {
                // Get the oldest toasts (beyond max visible)
                var toastsToDismiss = m_ActiveToasts.Take(m_ActiveToasts.Count - MAX_VISIBLE_TOASTS).ToList();
                foreach (var oldToast in toastsToDismiss)
                {
                    DismissToast(oldToast);
                }
                return; // UpdateStackPositions will be called again after dismissals
            }

            int count = m_ActiveToasts.Count;

            // Starting from a common Y coordinate (0), position each toast
            // Newer toasts get higher positions (appear on top)
            float visibleGap = 12f; // Gap between stacked toasts

            for (int i = 0; i < count; i++)
            {
                var toast = m_ActiveToasts[i];
                toast.element.style.display = DisplayStyle.Flex;

                // Use absolute positioning from a common reference point
                toast.element.style.position = Position.Absolute;
                // Don't set left/right - let the container's alignment handle horizontal positioning
                toast.element.style.left = StyleKeyword.Auto;
                toast.element.style.right = StyleKeyword.Auto;

                // Calculate visual index: newest toast (last in list) = 0, oldest (first in list) = count-1
                int visualIndex = count - 1 - i;

                // Position ALL toasts from base Y=0, offset by their visual index
                // Toast 0 (newest): yOffset = 0 * 12 = 0
                // Toast 1 (middle): yOffset = 1 * 12 = 12
                // Toast 2 (oldest): yOffset = 2 * 12 = 24
                float yOffset = visualIndex * visibleGap;

                if (isTopPosition)
                {
                    toast.element.style.top = yOffset;
                    toast.element.style.bottom = StyleKeyword.Auto;
                }
                else
                {
                    toast.element.style.top = StyleKeyword.Auto;
                    toast.element.style.bottom = yOffset;
                }

                // REVERSED: Place older toasts BEHIND newer ones
                // Newer toasts (higher index) should be on top
                if (i < count - 1)
                {
                    // Place this older toast behind the next (newer) one
                    var newerToast = m_ActiveToasts[i + 1];
                    toast.element.PlaceBehind(newerToast.element);
                }

                // Scale down slightly for depth effect (older toasts are smaller)
                float scale = 1f - (visualIndex * 0.03f); // 3% scale reduction per position
                toast.element.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));

                // Reduce opacity slightly for older toasts
                float opacity = 1f - (visualIndex * 0.1f); // 10% opacity reduction per position
                toast.element.style.opacity = Mathf.Max(opacity, 0.8f);
            }
        }

        // Static methods for easy usage
        public static void Toast(
            string message,
            string title = null,
            ToastVariant variant = ToastVariant.Default,
            ToastPosition position = ToastPosition.BottomRight,
            float duration = DEFAULT_DURATION,
            bool showIcon = true,
            Texture2D customIcon = null,
            bool showDescription = true,
            bool showActionButton = false,
            string actionButtonText = "Action",
            System.Action onActionClick = null)
        {
            // Try to get existing instance
            if (s_Instances.TryGetValue(position, out var sonner))
            {
                sonner.ShowToast(message, title, variant, duration, showIcon, customIcon, showDescription, showActionButton, actionButtonText, onActionClick);
                return;
            }

            // If no instance exists, log a clear error
            Debug.LogError($"ShunSonner: No instance found for position {position}. " +
                          $"Please add a <shun:ShunSonner position=\"{position}\" /> element to your UXML, " +
                          $"or use <shun:ShunSonnerManager /> to create all positions automatically. " +
                          $"Available positions: {string.Join(", ", s_Instances.Keys)}");
        }

        public static void Success(string message, string title = "Success", ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION, Texture2D customIcon = null)
        {
            Toast(message, title, ToastVariant.Success, position, duration, showIcon: true, customIcon: customIcon, showDescription: false);
        }

        public static void Error(string message, string title = "Error", ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION, Texture2D customIcon = null)
        {
            Toast(message, title, ToastVariant.Error, position, duration, showIcon: true, customIcon: customIcon, showDescription: false);
        }

        public static void Warning(string message, string title = "Warning", ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION, Texture2D customIcon = null)
        {
            Toast(message, title, ToastVariant.Warning, position, duration, showIcon: true, customIcon: customIcon, showDescription: false);
        }

        public static void Info(string message, string title = "Info", ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION, Texture2D customIcon = null)
        {
            Toast(message, title, ToastVariant.Info, position, duration, showIcon: true, customIcon: customIcon, showDescription: false);
        }

        public static void Description(string message, string title, ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION)
        {
            Toast(message, title, ToastVariant.Default, position, duration, showIcon: false, showDescription: true);
        }

        public static void Action(string message, string title, string actionButtonText = "Action", System.Action onActionClick = null, ToastPosition position = ToastPosition.BottomRight, float duration = DEFAULT_DURATION)
        {
            Toast(message, title, ToastVariant.Default, position, duration, showIcon: false, showDescription: false, showActionButton: true, actionButtonText: actionButtonText, onActionClick: onActionClick);
        }

        /// <summary>
        /// Get a list of all registered toast positions. Useful for debugging.
        /// </summary>
        public static string GetRegisteredPositions()
        {
            if (s_Instances.Count == 0)
                return "No ShunSonner instances registered. Add <shun:ShunSonner> or <shun:ShunSonnerManager> to your UXML.";

            return $"Registered positions: {string.Join(", ", s_Instances.Keys)}";
        }

        /// <summary>
        /// Check if a specific position is available.
        /// </summary>
        public static bool IsPositionAvailable(ToastPosition position)
        {
            return s_Instances.ContainsKey(position);
        }

        private class ToastItem
        {
            public ShunToast element;
            public IVisualElementScheduledItem dismissSchedule;

            public ToastItem(
                ShunSonner sonner,
                string message,
                string title,
                ToastVariant variant,
                float duration,
                bool showIcon,
                Texture2D customIcon,
                bool showDescription,
                bool showActionButton,
                string actionButtonText)
            {
                // Get default icon from the instance based on variant
                Texture2D defaultIcon = variant switch
                {
                    ToastVariant.Success => sonner.m_SuccessIcon,
                    ToastVariant.Error => sonner.m_ErrorIcon,
                    ToastVariant.Warning => sonner.m_WarningIcon,
                    ToastVariant.Info => sonner.m_InfoIcon,
                    _ => null
                };

                // Use custom icon if provided, otherwise use instance default
                Texture2D iconToUse = customIcon ?? defaultIcon;

                // Determine icon visibility based on variant and user preference
                bool shouldShowIcon = showIcon && (
                    variant == ToastVariant.Success ||
                    variant == ToastVariant.Error ||
                    variant == ToastVariant.Warning ||
                    variant == ToastVariant.Info ||
                    iconToUse != null
                );

                element = new ShunToast
                {
                    variant = variant,
                    title = title ?? string.Empty,
                    description = showDescription ? (message ?? string.Empty) : string.Empty,
                    showCloseButton = true,
                    showIcon = shouldShowIcon,
                    icon = iconToUse,
                    showActionButton = showActionButton,
                    actionButtonText = actionButtonText,
                    closeIcon = sonner.m_CloseIcon
                };
            }
        }
    }
}
