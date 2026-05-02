using UnityEngine;
using UnityEngine.UIElements;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunResizable : VisualElement
    {
        [UxmlAttribute]
        public string direction
        {
            get => m_Direction;
            set
            {
                m_Direction = value;
                SetDirection(value);
            }
        }

        private string m_Direction = "horizontal";

        public ShunResizable()
        {
            AddToClassList("shun-resizable");
            SetDirection(m_Direction);
        }

        private void SetDirection(string value)
        {
            RemoveFromClassList("shun-resizable--horizontal");
            RemoveFromClassList("shun-resizable--vertical");

            switch (value.ToLower())
            {
                case "horizontal":
                    AddToClassList("shun-resizable--horizontal");
                    break;
                case "vertical":
                    AddToClassList("shun-resizable--vertical");
                    break;
            }
        }
    }

    [UxmlElement]
    public partial class ShunResizablePanel : VisualElement
    {
        [UxmlAttribute]
        public float minSize { get; set; } = 50f;

        [UxmlAttribute]
        public float maxSize { get; set; } = -1f; // -1 means no max

        public ShunResizablePanel()
        {
            AddToClassList("shun-resizable__panel");
        }

        public void StartResize()
        {
            // Lock flex behavior during resize
            style.flexGrow = 0;
            style.flexShrink = 0;
        }

        public void SetSize(float size)
        {
            if (maxSize > 0)
            {
                size = Mathf.Min(size, maxSize);
            }
            size = Mathf.Max(size, minSize);
            
            var resizableParent = GetFirstAncestorOfType<ShunResizable>();
            if (resizableParent != null)
            {
                if (resizableParent.direction == "horizontal")
                {
                    style.width = size;
                }
                else
                {
                    style.height = size;
                }
            }
        }

        public float GetCurrentSize()
        {
            var resizableParent = GetFirstAncestorOfType<ShunResizable>();
            if (resizableParent != null)
            {
                if (resizableParent.direction == "horizontal")
                {
                    return resolvedStyle.width;
                }
                else
                {
                    return resolvedStyle.height;
                }
            }
            return 0;
        }
    }

    [UxmlElement]
    public partial class ShunResizableHandle : VisualElement
    {
        private bool m_IsDragging = false;
        private float m_LastMousePos;
        private float m_StartSize;
        private ShunResizablePanel m_PreviousPanel;
        private ShunResizable m_Resizable;

        public ShunResizableHandle()
        {
            AddToClassList("shun-resizable__handle");

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            
            pickingMode = PickingMode.Position;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            // Find the resizable container and previous panel
            m_Resizable = GetFirstAncestorOfType<ShunResizable>();
            if (m_Resizable != null)
            {
                var index = parent.IndexOf(this);
                if (index > 0)
                {
                    m_PreviousPanel = parent[index - 1] as ShunResizablePanel;
                    if (m_PreviousPanel != null)
                    {
                        // Lock the panel for resizing and capture its current size
                        m_PreviousPanel.StartResize();
                        m_StartSize = m_PreviousPanel.GetCurrentSize();
                        
                        // Use world/screen coordinates which don't shift during resize
                        if (m_Resizable.direction == "horizontal")
                        {
                            m_LastMousePos = evt.mousePosition.x;
                        }
                        else
                        {
                            m_LastMousePos = evt.mousePosition.y;
                        }
                        
                        m_IsDragging = true;
                    }
                }
            }

            this.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_IsDragging || m_PreviousPanel == null || m_Resizable == null) return;

            float currentMousePos;
            if (m_Resizable.direction == "horizontal")
            {
                currentMousePos = evt.mousePosition.x;
            }
            else
            {
                currentMousePos = evt.mousePosition.y;
            }

            // Calculate delta from start
            float delta = currentMousePos - m_LastMousePos;
            float newSize = m_StartSize + delta;
            
            // Calculate maximum size based on container and siblings
            float containerSize;
            if (m_Resizable.direction == "horizontal")
            {
                containerSize = m_Resizable.resolvedStyle.width;
            }
            else
            {
                containerSize = m_Resizable.resolvedStyle.height;
            }
            
            // Calculate total size of siblings (other panels and handles)
            float siblingsMinSize = 0f;
            int panelIndex = parent.IndexOf(m_PreviousPanel);
            
            for (int i = 0; i < parent.childCount; i++)
            {
                if (i == panelIndex) continue; // Skip the panel we're resizing
                
                var child = parent[i];
                if (child is ShunResizablePanel otherPanel)
                {
                    siblingsMinSize += otherPanel.minSize;
                }
                else if (child is ShunResizableHandle)
                {
                    // Add handle size
                    if (m_Resizable.direction == "horizontal")
                    {
                        siblingsMinSize += child.resolvedStyle.width;
                    }
                    else
                    {
                        siblingsMinSize += child.resolvedStyle.height;
                    }
                }
            }
            
            // Maximum size is container size minus minimum sizes of siblings
            float dynamicMaxSize = containerSize - siblingsMinSize;
            
            // Apply user-defined max size if set, otherwise use dynamic max
            float effectiveMaxSize = m_PreviousPanel.maxSize > 0 
                ? Mathf.Min(m_PreviousPanel.maxSize, dynamicMaxSize) 
                : dynamicMaxSize;
            
            // Clamp to min and max
            newSize = Mathf.Clamp(newSize, m_PreviousPanel.minSize, effectiveMaxSize);
            
            // Apply size
            float oldSize = m_PreviousPanel.GetCurrentSize();
            m_PreviousPanel.SetSize(newSize);
            
            // Check if size actually changed (might hit min/max limit)
            float actualSize = m_PreviousPanel.GetCurrentSize();
            if (Mathf.Abs(actualSize - oldSize) > 0.01f)
            {
                // Size changed, update start reference for next delta
                m_StartSize = actualSize;
                m_LastMousePos = currentMousePos;
            }
            
            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button != 0) return;

            m_IsDragging = false;
            m_Resizable = null;
            m_PreviousPanel = null;
            this.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}
