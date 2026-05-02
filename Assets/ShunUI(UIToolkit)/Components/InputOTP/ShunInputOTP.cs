using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace ShunUI
{
    [UxmlElement]
    public partial class ShunInputOTP : VisualElement
    {
        private int m_MaxLength = 6;
        private int m_GroupSize = 0; // 0 = no grouping
        private List<TextField> m_Fields = new List<TextField>();
        private VisualElement m_Container;

        [UxmlAttribute]
        public int maxLength
        {
            get => m_MaxLength;
            set
            {
                if (m_MaxLength != value && value > 0)
                {
                    m_MaxLength = value;
                    RebuildFields();
                }
            }
        }

        [UxmlAttribute]
        public int groupSize
        {
            get => m_GroupSize;
            set
            {
                if (m_GroupSize != value && value >= 0)
                {
                    m_GroupSize = value;
                    RebuildFields();
                }
            }
        }

        [UxmlAttribute]
        public string value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public ShunInputOTP()
        {
            AddToClassList("input-otp");

            m_Container = new VisualElement();
            m_Container.AddToClassList("input-otp__container");
            Add(m_Container);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            RebuildFields();
        }

        private void RebuildFields()
        {
            if (m_Container == null) return;

            // Clear existing fields
            m_Container.Clear();
            m_Fields.Clear();

            int fieldsCreated = 0;
            int currentGroupSize = 0;
            VisualElement currentGroup = null;
            List<TextField> currentGroupFields = new List<TextField>();

            for (int i = 0; i < m_MaxLength; i++)
            {
                // Create new group if needed
                if (m_GroupSize > 0 && (currentGroup == null || currentGroupSize >= m_GroupSize))
                {
                    // Mark first/last fields in previous group
                    if (currentGroupFields.Count > 0)
                    {
                        currentGroupFields[0].AddToClassList("input-otp__field--first");
                        currentGroupFields[currentGroupFields.Count - 1].AddToClassList("input-otp__field--last");
                    }
                    
                    currentGroup = new VisualElement();
                    currentGroup.AddToClassList("input-otp__group");
                    m_Container.Add(currentGroup);
                    currentGroupSize = 0;
                    currentGroupFields.Clear();

                    // Add separator between groups (but not before first group)
                    if (fieldsCreated > 0)
                    {
                        var separator = new Label("-");
                        separator.AddToClassList("input-otp__separator");
                        m_Container.Insert(m_Container.IndexOf(currentGroup), separator);
                    }
                }

                // Create field
                var field = new TextField();
                field.AddToClassList("input-otp__field");
                field.maxLength = 1;
                field.isDelayed = false;

                // Add to appropriate container
                if (m_GroupSize > 0 && currentGroup != null)
                {
                    currentGroup.Add(field);
                    currentGroupSize++;
                    currentGroupFields.Add(field);
                }
                else
                {
                    m_Container.Add(field);
                }

                m_Fields.Add(field);

                // Setup event handlers
                int fieldIndex = i;
                field.RegisterCallback<KeyDownEvent>(evt => OnKeyDown(evt, fieldIndex));
                field.RegisterValueChangedCallback(evt => OnValueChanged(evt, fieldIndex));
                field.RegisterCallback<FocusInEvent>(evt => OnFocusIn(fieldIndex));
                
                // Setup placeholder styling
                field.RegisterCallback<AttachToPanelEvent>(evt => UpdateFieldPlaceholderStyle(field));
                field.RegisterCallback<ChangeEvent<string>>(evt => UpdateFieldPlaceholderStyle(field));

                fieldsCreated++;
            }

            // Mark first/last fields in the last group or ungrouped fields
            if (m_GroupSize > 0 && currentGroupFields.Count > 0)
            {
                currentGroupFields[0].AddToClassList("input-otp__field--first");
                currentGroupFields[currentGroupFields.Count - 1].AddToClassList("input-otp__field--last");
            }
            else if (m_GroupSize == 0 && m_Fields.Count > 0)
            {
                // For ungrouped, mark first and last in the entire container
                m_Fields[0].AddToClassList("input-otp__field--first");
                m_Fields[m_Fields.Count - 1].AddToClassList("input-otp__field--last");
            }
        }

        private void OnKeyDown(KeyDownEvent evt, int index)
        {
            var field = m_Fields[index];

            // Handle backspace
            if (evt.keyCode == KeyCode.Backspace)
            {
                if (string.IsNullOrEmpty(field.value) && index > 0)
                {
                    // Move to previous field and clear it
                    m_Fields[index - 1].value = "";
                    m_Fields[index - 1].Focus();
                    evt.StopPropagation();
                }
            }
            // Handle left arrow
            else if (evt.keyCode == KeyCode.LeftArrow && index > 0)
            {
                m_Fields[index - 1].Focus();
                evt.StopPropagation();
            }
            // Handle right arrow
            else if (evt.keyCode == KeyCode.RightArrow && index < m_Fields.Count - 1)
            {
                m_Fields[index + 1].Focus();
                evt.StopPropagation();
            }
        }

        private void OnValueChanged(ChangeEvent<string> evt, int index)
        {
            var field = m_Fields[index];
            var newValue = evt.newValue;

            // Only allow single character
            if (newValue.Length > 1)
            {
                field.value = newValue.Substring(newValue.Length - 1, 1);
                return;
            }

            // Auto-focus next field when character is entered
            if (!string.IsNullOrEmpty(newValue) && index < m_Fields.Count - 1)
            {
                m_Fields[index + 1].Focus();
            }

            // Dispatch change event
            using (var changeEvent = ChangeEvent<string>.GetPooled(GetOldValue(), GetValue()))
            {
                changeEvent.target = this;
                SendEvent(changeEvent);
            }
        }

        private void OnFocusIn(int index)
        {
            // Select all text when focused
            var field = m_Fields[index];
            field.SelectAll();
        }

        private void UpdateFieldPlaceholderStyle(TextField field)
        {
            var textElement = field.Q<TextElement>(className: "unity-text-element");
            if (textElement != null)
            {
                // Add placeholder class when field is empty
                if (string.IsNullOrEmpty(field.value))
                {
                    textElement.AddToClassList("unity-text-element--placeholder");
                }
                else
                {
                    textElement.RemoveFromClassList("unity-text-element--placeholder");
                }
            }
        }

        private string GetValue()
        {
            return string.Join("", m_Fields.Select(f => f.value));
        }

        private string GetOldValue()
        {
            // This is a simplified version - in production you'd track the old value
            return GetValue();
        }

        private void SetValue(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                foreach (var field in m_Fields)
                {
                    field.value = "";
                }
                return;
            }

            for (int i = 0; i < m_Fields.Count; i++)
            {
                m_Fields[i].value = i < newValue.Length ? newValue[i].ToString() : "";
            }
        }

        public new void Clear()
        {
            SetValue("");
            if (m_Fields.Count > 0)
            {
                m_Fields[0].Focus();
            }
        }

        public bool IsFilled()
        {
            return m_Fields.All(f => !string.IsNullOrEmpty(f.value));
        }
    }
}
