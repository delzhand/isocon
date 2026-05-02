using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class SimpleMainMenu : VisualElement
    {
        private VisualElement mainMenuPanel;
        private VisualElement settingsPanel;
        private ShunButton settingsButton;
        private ShunButton closeSettingsButton;

        public SimpleMainMenu()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Find the panels
            mainMenuPanel = this.Q<VisualElement>("MainMenu");
            settingsPanel = this.Q<VisualElement>("Settings");

            // Find the settings button
            settingsButton = mainMenuPanel?.Query<ShunButton>("BtnSettings").First();

            // Find the close button
            closeSettingsButton = settingsPanel?.Query<ShunButton>("BtnClose").First();

            // Wire up button clicks
            if (settingsButton != null)
            {
                settingsButton.clicked += ShowSettings;
            }

            if (closeSettingsButton != null)
            {
                closeSettingsButton.clicked += ShowMainMenu;
            }

            // Show main menu by default
            ShowMainMenu();
        }

        private void ShowSettings()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.style.display = DisplayStyle.None;
            }

            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.Flex;
            }
        }

        private void ShowMainMenu()
        {
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.None;
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.style.display = DisplayStyle.Flex;
            }
        }
    }
}
