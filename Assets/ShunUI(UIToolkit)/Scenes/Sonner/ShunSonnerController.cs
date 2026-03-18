using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    /// <summary>
    /// Runtime controller for ShunSonner.uxml demo.
    /// Attach this to the GameObject with UIDocument component.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ShunSonnerController : MonoBehaviour
    {
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Wait a bit for the UI to fully initialize
            StartCoroutine(InitializeAfterDelay(root));
        }

        private System.Collections.IEnumerator InitializeAfterDelay(VisualElement root)
        {
            // Wait for ShunSonnerManager to create instances (it uses 100ms delay)
            yield return new WaitForSeconds(0.2f);

            // Default - Only title, no icon, no description
            var btnDefault = root.Q<ShunButton>("btn-default");
            if (btnDefault != null)
            {
                btnDefault.clicked += () => ShunSonner.Toast("", "Event has been created", showIcon: false, showDescription: false);
            }

            // Description - Title + description, no icon
            var btnDescription = root.Q<ShunButton>("btn-description");
            if (btnDescription != null)
            {
                btnDescription.clicked += () => ShunSonner.Description("You can use this component to display toast notifications to users. They will automatically dismiss after a few seconds.", "Event Created");
            }
            
            // Success - Icon + title + message (description hidden)
            var btnSuccess = root.Q<ShunButton>("btn-success");
            if (btnSuccess != null)
            {
                btnSuccess.clicked += () => ShunSonner.Success("Your changes have been saved successfully!", "Success");
            }
            
            // Info - Icon + title + message (description hidden)
            var btnInfo = root.Q<ShunButton>("btn-info");
            if (btnInfo != null)
            {
                btnInfo.clicked += () => ShunSonner.Info("New features are now available in the dashboard", "Info");
            }
            
            // Warning - Icon + title + message (description hidden)
            var btnWarning = root.Q<ShunButton>("btn-warning");
            if (btnWarning != null)
            {
                btnWarning.clicked += () => ShunSonner.Warning("Your session will expire in 5 minutes", "Warning");
            }
            
            // Error - Icon + title + message (description hidden)
            var btnError = root.Q<ShunButton>("btn-error");
            if (btnError != null)
            {
                btnError.clicked += () => ShunSonner.Error("Failed to connect to the server", "Error");
            }
            
            // Action - Title + action button
            var btnAction = root.Q<ShunButton>("btn-action");
            if (btnAction != null)
            {
                btnAction.clicked += () => ShunSonner.Action("", "File Deleted", "Undo", () => Debug.Log("Undo clicked!"));
            }

            // Position buttons
            var btnTopLeft = root.Q<ShunButton>("btn-top-left");
            if (btnTopLeft != null)
            {
                btnTopLeft.clicked += () => ShunSonner.Info("This toast appears in the top left corner", "Top Left", ToastPosition.TopLeft);
            }
            
            var btnTopCenter = root.Q<ShunButton>("btn-top-center");
            if (btnTopCenter != null)
            {
                btnTopCenter.clicked += () => ShunSonner.Info("This toast appears in the top center", "Top Center", ToastPosition.TopCenter);
            }
            
            var btnTopRight = root.Q<ShunButton>("btn-top-right");
            if (btnTopRight != null)
            {
                btnTopRight.clicked += () => ShunSonner.Info("This toast appears in the top right corner", "Top Right", ToastPosition.TopRight);
            }
            
            var btnBottomLeft = root.Q<ShunButton>("btn-bottom-left");
            if (btnBottomLeft != null)
            {
                btnBottomLeft.clicked += () => ShunSonner.Success("This toast appears in the bottom left corner", "Bottom Left", ToastPosition.BottomLeft);
            }
            
            var btnBottomCenter = root.Q<ShunButton>("btn-bottom-center");
            if (btnBottomCenter != null)
            {
                btnBottomCenter.clicked += () => ShunSonner.Success("This toast appears in the bottom center", "Bottom Center", ToastPosition.BottomCenter);
            }
            
            var btnBottomRight = root.Q<ShunButton>("btn-bottom-right");
            if (btnBottomRight != null)
            {
                btnBottomRight.clicked += () => ShunSonner.Success("This toast appears in the bottom right corner", "Bottom Right", ToastPosition.BottomRight);
            }
        }
    }
}
