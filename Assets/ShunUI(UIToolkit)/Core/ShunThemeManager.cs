using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ShunUI
{
    [ExecuteAlways]
    public class ShunThemeManager : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private ShunTheme _currentTheme;

        public ShunTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    
                    // Schedule the theme application to avoid repaint issues during runtime
                    if (Application.isPlaying && _uiDocument != null && _uiDocument.rootVisualElement != null)
                    {
                        _uiDocument.rootVisualElement.schedule.Execute(ApplyTheme);
                    }
                    else
                    {
                        ApplyTheme();
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
            
            // Delay application to ensure UIDocument is ready
            if (Application.isPlaying)
            {
                // In Play Mode, wait for Start or next frame
                // But since we need it ASAP, we try now, and if it fails (rootVisualElement null), we retry in Start
            }
            
            ApplyTheme();
        }

        private void Start()
        {
            // Ensure theme is applied if OnEnable ran before UIDocument was ready
            ApplyTheme();
        }

        private void OnValidate()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument == null) return;
            
            // If rootVisualElement is null, we can't do anything yet.
            if (_uiDocument.rootVisualElement == null) return;

            if (_currentTheme == null)
            {
                // Try to load DefaultTheme if no theme is selected
                var defaultTheme = Resources.Load<ShunTheme>("Themes/DefaultTheme");
                if (defaultTheme != null)
                {
                    _currentTheme = defaultTheme;
                }
                else
                {
                    Debug.LogWarning("[ShunThemeManager] No theme selected and DefaultTheme not found in Resources/Themes/.");
                    return;
                }
            }

            if (_currentTheme.StyleSheet == null)
            {
                Debug.LogWarning($"[ShunThemeManager] Theme '{_currentTheme.name}' has no StyleSheet assigned. Please regenerate themes.");
                return;
            }

            var root = _uiDocument.rootVisualElement;

            // Remove existing theme stylesheets
            List<StyleSheet> sheetsToRemove = new List<StyleSheet>();
            for (int i = 0; i < root.styleSheets.count; i++)
            {
                var sheet = root.styleSheets[i];
                // Check for null sheet just in case
                if (sheet != null && sheet.name.EndsWith("Theme"))
                {
                    sheetsToRemove.Add(sheet);
                }
            }

            foreach (var sheet in sheetsToRemove)
            {
                root.styleSheets.Remove(sheet);
            }
            
            // Add the new theme stylesheet
            if (!root.styleSheets.Contains(_currentTheme.StyleSheet))
            {
                root.styleSheets.Add(_currentTheme.StyleSheet);
            }

            // Update overlays
            if (root.panel != null)
            {
                ShunOverlayManager.UpdateTheme(root.panel, _currentTheme.StyleSheet, sheetsToRemove);
            }
        }
    }
}
