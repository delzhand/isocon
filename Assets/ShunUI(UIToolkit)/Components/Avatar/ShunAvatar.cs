using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

namespace ShunUI
{
    public enum AvatarSize
    {
        Default,
        Small,
        Large
    }

    [System.Serializable]
    [UxmlElement]
    public partial class ShunAvatar : VisualElement
    {
        private VisualElement _imageContainer;
        private Label _fallbackLabel;
        private AvatarSize _size = AvatarSize.Default;
        private string _imageSrc = "https://github.com/shadcn.png";
        private string _fallbackText = "";
        private bool _isAttached = false;

        [UxmlAttribute]
        public AvatarSize size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    UpdateSizeClass();
                }
            }
        }

        [UxmlAttribute]
        public string src
        {
            get => _imageSrc;
            set
            {
                if (_imageSrc != value)
                {
                    _imageSrc = value;
                    if (_isAttached)
                        UpdateImage();
                }
            }
        }

        [UxmlAttribute]
        public string fallback
        {
            get => _fallbackText;
            set
            {
                if (_fallbackText != value)
                {
                    _fallbackText = value;
                    if (_isAttached)
                        UpdateFallback();
                }
            }
        }

        public ShunAvatar()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Add base class
            AddToClassList("avatar");

            // Create image container (hidden by default)
            _imageContainer = new VisualElement();
            _imageContainer.AddToClassList("avatar__image");
            _imageContainer.style.display = DisplayStyle.None;
            Add(_imageContainer);

            // Create fallback label (visible by default)
            _fallbackLabel = new Label();
            _fallbackLabel.AddToClassList("avatar__fallback");
            _fallbackLabel.text = string.IsNullOrEmpty(_fallbackText) ? "?" : _fallbackText;
            Add(_fallbackLabel);

            // Apply initial size
            UpdateSizeClass();

            // Register for events - only update when attached to panel
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                _isAttached = true;
                schedule.Execute(() =>
                {
                    UpdateImage();
                    UpdateFallback();
                }).ExecuteLater(50); // Delay to avoid threading issues
            });

            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                _isAttached = false;
            });
        }

        private void UpdateSizeClass()
        {
            // Remove all size classes
            RemoveFromClassList("avatar-sm");
            RemoveFromClassList("avatar-lg");

            // Add the current size class
            string sizeClass = _size switch
            {
                AvatarSize.Small => "avatar-sm",
                AvatarSize.Large => "avatar-lg",
                AvatarSize.Default => "",
                _ => ""
            };

            if (!string.IsNullOrEmpty(sizeClass))
                AddToClassList(sizeClass);
        }

        private void UpdateImage()
        {
            if (!_isAttached || string.IsNullOrEmpty(_imageSrc))
            {
                ShowFallback();
                return;
            }

            // Check if it's a URL (http/https)
            if (_imageSrc.StartsWith("http://") || _imageSrc.StartsWith("https://"))
            {
                LoadImageFromUrl(_imageSrc);
            }
            else
            {
                // Load from Resources
                LoadImageFromResources(_imageSrc);
            }
        }

        private void LoadImageFromResources(string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture != null)
            {
                if (_imageContainer != null)
                {
                    _imageContainer.style.backgroundImage = new StyleBackground(texture);
                    _imageContainer.style.display = DisplayStyle.Flex;
                }
                if (_fallbackLabel != null)
                {
                    _fallbackLabel.style.display = DisplayStyle.None;
                }
            }
            else
            {
                ShowFallback();
            }
        }

        private void LoadImageFromUrl(string url)
        {
            // Show fallback initially
            ShowFallback();

            // Start the web request
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            var operation = request.SendWebRequest();

            // Wait for completion with a single scheduled check
            schedule.Execute(() =>
            {
                if (!_isAttached || operation == null || request == null)
                {
                    request?.Dispose();
                    return;
                }

                if (!operation.isDone)
                    return; // Still loading, will check again

                // Request completed
                try
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null && _imageContainer != null && _isAttached)
                        {
                            _imageContainer.style.backgroundImage = new StyleBackground(texture);
                            _imageContainer.style.display = DisplayStyle.Flex;
                            if (_fallbackLabel != null)
                                _fallbackLabel.style.display = DisplayStyle.None;
                        }
                    }
                }
                finally
                {
                    request?.Dispose();
                }
            }).Every(100).Until(() => !_isAttached || operation == null || operation.isDone);
        }

        private void ShowFallback()
        {
            if (_imageContainer != null)
                _imageContainer.style.display = DisplayStyle.None;
            if (_fallbackLabel != null)
                _fallbackLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateFallback()
        {
            if (_fallbackLabel != null)
            {
                _fallbackLabel.text = string.IsNullOrEmpty(_fallbackText) ? "?" : _fallbackText;
            }
        }
    }
}