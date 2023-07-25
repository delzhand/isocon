using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IsoconUILibrary {

    public class SlideToggle : BaseField<bool>
    {
        public new class UxmlFactory : UxmlFactory<SlideToggle, UxmlTraits> {}
        public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription> {}

        // In the spirit of the BEM standard, the SlideToggle has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly new string ussClassName = "slide-toggle";
        public static readonly new string inputUssClassName = "slide-toggle__input";
        public static readonly string inputKnobUssClassName = "slide-toggle__input-knob";
        public static readonly string inputCheckedUssClassName = "slide-toggle__input--checked";

        VisualElement m_Input;
        VisualElement m_Knob;

        public SlideToggle() : this(null) { }

        public SlideToggle(string label) : base(label, null) {
            AddToClassList(ussClassName);

            // This is the toggle-like element
            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);

            // This shows whether it's active or not
            m_Knob = new();
            m_Knob.AddToClassList(inputKnobUssClassName);
            m_Input.Add(m_Knob);


            RegisterCallback<ClickEvent>(evt => OnClick(evt));
            RegisterCallback<KeyDownEvent>(evt => OnKeydownEvent(evt));
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt));
        }

        static void OnClick(ClickEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;

            // NavigationSubmitEvent event already covers keydown events at runtime, so this method shouldn't handle
            // them.
            if (slideToggle.panel?.contextType == ContextType.Player)
                return;

            // Toggle the value only when the user presses Enter, Return, or Space.
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
            }
        }

        // All three callbacks call this method.
        void ToggleValue()
        {
            value = !value;
        }

        // Because ToggleValue() sets the value property, the BaseField class dispatches a ChangeEvent. This results in a
        // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // enabled.
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);

            //This line of code styles the input element to look enabled or disabled.
            m_Input.EnableInClassList(inputCheckedUssClassName, newValue);
        }
    }
}
