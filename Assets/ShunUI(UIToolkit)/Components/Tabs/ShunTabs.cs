using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ShunUI
{
    [System.Serializable]
    [UxmlElement]
    public partial class ShunTabs : VisualElement
    {
        private VisualElement _tabList;
        private List<ShunTab> _tabs = new List<ShunTab>();
        private List<ShunTabPanel> _tabPanels = new List<ShunTabPanel>();
        private string _activeTabId = "";
        private bool m_IsInitialized = false;

        public ShunTabs()
        {
            AddToClassList("tabs");
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_IsInitialized) return;
            m_IsInitialized = true;

            // Create tab list container
            _tabList = new VisualElement();
            _tabList.AddToClassList("tabs__list");
            hierarchy.Add(_tabList);

            // Find the content wrapper
            var tabsContent = this.Query<ShunTabsContent>().First();
            if (tabsContent == null) return;

            // Move content wrapper if it's a direct child
            if (tabsContent.parent == this)
            {
                tabsContent.RemoveFromHierarchy();

                // Collect tabs from direct children
                var tabs = this.Query<ShunTab>().ToList();
                foreach (var tab in tabs)
                {
                    if (tab.parent == this)
                    {
                        tab.RemoveFromHierarchy();
                        _tabList.Add(tab);

                        if (!_tabs.Contains(tab))
                        {
                            _tabs.Add(tab);
                            tab.clicked += () => ActivateTab(tab.tabId);
                        }
                    }
                }

                // Add content wrapper back
                hierarchy.Add(tabsContent);

                // Collect panels from content wrapper
                var panels = tabsContent.Query<ShunTabPanel>().ToList();
                foreach (var panel in panels)
                {
                    if (!_tabPanels.Contains(panel))
                    {
                        _tabPanels.Add(panel);
                    }
                }

                // Activate the first active tab or the first tab
                var activeTab = _tabs.FirstOrDefault(t => t.isActive);
                if (activeTab != null)
                {
                    ActivateTab(activeTab.tabId);
                }
                else if (_tabs.Count > 0)
                {
                    ActivateTab(_tabs[0].tabId);
                }
            }
        }

        private void ActivateTab(string tabId)
        {
            if (string.IsNullOrEmpty(tabId)) return;

            _activeTabId = tabId;

            // Update tab states
            foreach (var tab in _tabs)
            {
                tab.isActive = (tab.tabId == tabId);
            }

            // Update panel visibility
            foreach (var panel in _tabPanels)
            {
                // Match panel by tabId attribute
                bool isActive = panel.tabId == tabId;
                panel.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    [UxmlElement]
    public partial class ShunTabsContent : VisualElement
    {
        public ShunTabsContent()
        {
            AddToClassList("tabs__content");
        }
    }

    [UxmlElement]
    public partial class ShunTabPanel : VisualElement
    {
        private string _tabId = "";

        [UxmlAttribute]
        public string tabId
        {
            get => _tabId;
            set => _tabId = value;
        }

        public ShunTabPanel()
        {
            AddToClassList("tabs__panel");
        }
    }
}
