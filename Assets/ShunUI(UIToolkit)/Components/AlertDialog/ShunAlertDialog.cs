using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunAlertDialog : VisualElement
    {
        private VisualElement _overlay;
        private VisualElement _dialog;
        private VisualElement _content;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private VisualElement _actions;
        private ShunButton _cancelButton;
        private ShunButton _actionButton;
        private bool _isOpen;
        private bool _isInitialized;
        private bool _stylesheetsInitialized;
        private readonly HashSet<Button> _wiredButtons = new HashSet<Button>();

        [UxmlAttribute]
        public string title
        {
            get => _titleLabel?.text ?? string.Empty;
            set
            {
                if (_titleLabel != null)
                {
                    _titleLabel.text = value;
                }
            }
        }

        [UxmlAttribute]
        public string description
        {
            get => _descriptionLabel?.text ?? string.Empty;
            set
            {
                if (_descriptionLabel != null)
                {
                    _descriptionLabel.text = value;
                }
            }
        }

        [UxmlAttribute]
        public string cancelText
        {
            get => _cancelButton?.text ?? string.Empty;
            set
            {
                if (_cancelButton != null)
                {
                    _cancelButton.text = value;
                    _cancelButton.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        [UxmlAttribute]
        public string actionText
        {
            get => _actionButton?.text ?? string.Empty;
            set
            {
                if (_actionButton != null)
                {
                    _actionButton.text = value;
                    _actionButton.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        [UxmlAttribute]
        public bool isOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                _isOpen = value;

                if (_isInitialized)
                {
                    UpdateVisibility();
                }
            }
        }

        public ShunAlertDialog()
        {
            Initialize();
        }

        private void Initialize()
        {
            AddToClassList("alert-dialog");

            _overlay = new VisualElement();
            _overlay.AddToClassList("alert-dialog__overlay");
            _overlay.style.position = Position.Absolute;
            _overlay.style.display = DisplayStyle.None; // Hidden by default
            _overlay.pickingMode = PickingMode.Ignore;
            
            _overlay.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.target == _overlay)
                {
                    isOpen = false;
                }
            });
            hierarchy.Add(_overlay);

            _dialog = new VisualElement();
            _dialog.AddToClassList("alert-dialog__content");
            _overlay.hierarchy.Add(_dialog);

            _content = new VisualElement();
            _content.AddToClassList("alert-dialog__body");
            _dialog.hierarchy.Add(_content);

            _titleLabel = new Label();
            _titleLabel.AddToClassList("alert-dialog__title");
            _content.hierarchy.Add(_titleLabel);

            _descriptionLabel = new Label();
            _descriptionLabel.AddToClassList("alert-dialog__description");
            _content.hierarchy.Add(_descriptionLabel);

            _actions = new VisualElement();
            _actions.AddToClassList("alert-dialog__actions");
            _dialog.hierarchy.Add(_actions);

            // Create cancel button
            _cancelButton = new ShunButton
            {
                text = "Cancel",
                variant = ButtonVariant.Outline
            };
            _cancelButton.AddToClassList("alert-dialog__cancel");
            _cancelButton.clicked += () => isOpen = false;
            _actions.hierarchy.Add(_cancelButton);

            // Create action button
            _actionButton = new ShunButton
            {
                text = "Continue",
                variant = ButtonVariant.Primary
            };
            _actionButton.AddToClassList("alert-dialog__action");
            _actionButton.clicked += () => isOpen = false;
            _actions.hierarchy.Add(_actionButton);

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (_isInitialized)
                {
                    return;
                }

                _isInitialized = true;
                
                // Ensure overlay has stylesheet references
                EnsureOverlayStylesheets();

                schedule.Execute(() =>
                {
                    SetupOverlayPosition();
                    UpdateVisibility();
                }).ExecuteLater(0);
            });

            // For UI Builder preview support
            schedule.Execute(() =>
            {
                if (!_isInitialized && panel != null)
                {
                    _isInitialized = true;
                    EnsureOverlayStylesheets();
                    SetupOverlayPosition();
                    UpdateVisibility();
                }
            }).ExecuteLater(100);
        }
        
        private void EnsureOverlayStylesheets()
        {
            if (_overlay == null || _stylesheetsInitialized) return;
            
            // Load ShunStyle stylesheet from Resources (works at runtime)
            var shunStyle = Resources.Load<StyleSheet>("ShunStyle");
            if (shunStyle != null && !_overlay.styleSheets.Contains(shunStyle))
            {
                _overlay.styleSheets.Add(shunStyle);
            }
            
            // Copy stylesheets from panel root (works in UI Builder)
            if (panel != null && panel.visualTree != null)
            {
                var root = panel.visualTree;
                for (int i = 0; i < root.styleSheets.count; i++)
                {
                    var styleSheet = root.styleSheets[i];
                    // Exclude Themes in Play Mode as they are managed by ShunThemeManager
                    if (Application.isPlaying && styleSheet.name.EndsWith("Theme", System.StringComparison.OrdinalIgnoreCase)) continue;

                    if (!_overlay.styleSheets.Contains(styleSheet))
                    {
                        _overlay.styleSheets.Add(styleSheet);
                    }
                }
            }
            
            // Fallback: Copy from this element (for UI Builder preview)
            if (_overlay.styleSheets.count == 0)
            {
                for (int i = 0; i < styleSheets.count; i++)
                {
                    var styleSheet = styleSheets[i];
                    if (!_overlay.styleSheets.Contains(styleSheet))
                    {
                        _overlay.styleSheets.Add(styleSheet);
                    }
                }
            }
            
            _stylesheetsInitialized = true;
        }

        private void SetupOverlayPosition()
        {
            if (_overlay == null)
            {
                return;
            }

            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0;
            _overlay.style.top = 0;
            _overlay.style.right = 0;
            _overlay.style.bottom = 0;

            void UpdateOverlaySize()
            {
                if (_overlay == null)
                {
                    return;
                }

                // At runtime in global container, use pixel-based sizing
                if (Application.isPlaying && _overlay.parent != this)
                {
                    var sizeSource = Vector2.zero;

                    if (_overlay.parent != null)
                    {
                        var parentBounds = _overlay.parent.layout;
                        sizeSource = new Vector2(parentBounds.width, parentBounds.height);
                    }
                    
                    if (sizeSource == Vector2.zero)
                    {
                        sizeSource = new Vector2(1920, 1080);
                    }

                    _overlay.style.width = sizeSource.x;
                    _overlay.style.height = sizeSource.y;
                }
                // In UI Builder or when child of this element, use flexible sizing
                else
                {
                    _overlay.style.width = StyleKeyword.Auto;
                    _overlay.style.height = StyleKeyword.Auto;
                }
            }

            _overlay.RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlaySize());
            
            // Also listen to this element's geometry changes (for UI Builder)
            RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlaySize());
            
            // Listen to parent geometry changes
            if (parent != null)
            {
                parent.RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlaySize());
            }
            
            // Initial size update
            schedule.Execute(UpdateOverlaySize).ExecuteLater(0);
        }

        public void AddAction(string label, System.Action onClick, bool isPrimary = false)
        {
            var button = new ShunButton
            {
                text = label,
                variant = isPrimary ? ButtonVariant.Primary : ButtonVariant.Outline
            };

            if (onClick != null)
            {
                button.clicked += onClick;
            }

            button.clicked += () => isOpen = false;
            _actions.Add(button);
        }

        public void OpenDialog() => isOpen = true;

        public void CloseDialog() => isOpen = false;

        private void UpdateVisibility()
        {
            if (_overlay == null)
            {
                return;
            }

            if (_isOpen)
            {
                if (Application.isPlaying)
                {
                    // Clear and re-ensure stylesheets to pick up any theme changes
                    ClearOverlayStylesheets();
                    EnsureOverlayStylesheets();

                    // At runtime, move overlay to global container for full-screen coverage
                    if (panel != null)
                    {
                        ShunOverlayManager.AddOverlay(_overlay, panel, this);
                    }
                }
                else
                {
                    // In UI Builder, find the canvas element that contains the preview
                    VisualElement canvasParent = FindUIBuilderCanvas();
                    
                    if (canvasParent != null && _overlay.parent != canvasParent)
                    {
                        EnsureOverlayStylesheets();
                        canvasParent.Add(_overlay);
                    }
                }
                
                _overlay.style.display = DisplayStyle.Flex;
                _overlay.pickingMode = PickingMode.Position;
                
                // Force layout update
                schedule.Execute(() =>
                {
                    if (_overlay != null)
                    {
                        _overlay.MarkDirtyRepaint();
                    }
                }).ExecuteLater(0);
            }
            else
            {
                // Remove overlay and move back to this element
                if (Application.isPlaying && panel != null)
                {
                    ShunOverlayManager.RemoveOverlay(_overlay);
                }
                
                // Move back to this element
                if (_overlay.parent != this)
                {
                    hierarchy.Add(_overlay);
                }
                
                // Clear stylesheets when closing so we don't hold onto old themes
                ClearOverlayStylesheets();
                
                _overlay.style.display = DisplayStyle.None;
                _overlay.pickingMode = PickingMode.Ignore;
            }
        }

        private void ClearOverlayStylesheets()
        {
            if (_overlay == null) return;
            _overlay.styleSheets.Clear();
            _stylesheetsInitialized = false;
        }

        /// <summary>
        /// Finds the UI Builder document element by traversing up the parent hierarchy.
        /// Returns the "document" TemplateContainer which represents the full canvas area.
        /// </summary>
        private VisualElement FindUIBuilderCanvas()
        {
            VisualElement current = this;
            while (current != null)
            {
                // Look for the document element (TemplateContainer with name "document")
                if (current.name == "document" && current is TemplateContainer)
                {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }
    }

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogTrigger : ShunButton
{
    public ShunAlertDialogTrigger()
    {
        clicked += OnClicked;
    }

    private void OnClicked()
    {
        var dialog = GetFirstAncestorOfType<ShunAlertDialog>();
        if (dialog != null)
        {
            dialog.OpenDialog();
        }
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogHeader : VisualElement
{
    public ShunAlertDialogHeader()
    {
        AddToClassList("alert-dialog__header");
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogTitle : Label
{
    public ShunAlertDialogTitle()
    {
        AddToClassList("alert-dialog__title");
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogDescription : Label
{
    public ShunAlertDialogDescription()
    {
        AddToClassList("alert-dialog__description");
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogFooter : VisualElement
{
    public ShunAlertDialogFooter()
    {
        AddToClassList("alert-dialog__footer");
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogCancel : ShunButton
{
    public ShunAlertDialogCancel()
    {
        AddToClassList("alert-dialog__cancel");
    }
}

[System.Serializable]
[UxmlElement]
public partial class ShunAlertDialogAction : ShunButton
{
    public ShunAlertDialogAction()
    {
        AddToClassList("alert-dialog__action");
    }
}

} 
