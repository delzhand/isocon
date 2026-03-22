using System;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class ShunDialogHelper
{
    private static string _targetDialogName;

    public static void SetTargetDialog(string name)
    {
        _targetDialogName = name;
    }

    public static ShunDialog Dialog
    {
        get => UI.System.Q(_targetDialogName).Q<ShunDialog>();
    }

    public static ShunDialogContent Contents
    {
        get
        {
            var dialog = Dialog;
            return dialog.Q<ShunDialogContent>();
        }
    }

    public static ShunDialogContent Results(string dialogName)
    {
        return GameObject.Find("SystemUI").GetComponent<UIDocument>().rootVisualElement.parent.Q<ShunDialogContent>($"{dialogName}-Contents");
    }

    public static void SetCloseAction(Action closeAction)
    {
        var dialog = UI.System.Q<ShunDialog>();
        dialog.CloseAction = closeAction;
    }

    public static void AddDialogHeader(string value)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__header");
        Contents.Add(wrapper);

        var title = new ShunDialogTitle();
        title.text = value;
        wrapper.Add(title);
    }

    public static VisualElement AddDialogFooter(Action cancelAction)
    {
        var footer = new ShunContainer();
        footer.AddToClassList("shun-dialog__footer");
        ShunDialogHelper.Contents.Add(footer);

        var close = new ShunDialogClose();
        close.text = "Cancel";
        close.clicked += cancelAction;
        close.SetVariant(ButtonVariant.Outline);
        footer.Add(close);

        return footer;
    }

    public static VisualElement AddTextField(string name, string label, string defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var input = new ShunInput();
        input.name = name;
        input.value = defaultValue;
        wrapper.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddToggleField(string name, string label, string defaultValue, List<string> options, bool allowMultiple, string helpText = null)
    {
        List<string> defaultValues = new();
        defaultValues.Add(defaultValue);
        return AddToggleField(name, label, defaultValues, options, allowMultiple, helpText);
    }

    public static VisualElement AddToggleField(string name, string label, List<string> defaultValues, List<string> options, bool allowMultiple, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var toggles = new ShunToggleGroup();
        toggles.name = name;
        toggles.allowMultiple = allowMultiple;

        foreach (string s in options)
        {
            var toggle = new ShunToggle();
            toggle.text = s;
            toggle.isOn = defaultValues.Contains(s);
            toggles.Add(toggle);
        }

        wrapper.Add(toggles);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static List<string> GetToggleFieldValues(ShunToggleGroup toggleField)
    {
        if (toggleField == null)
        {
            Debug.Log("null element");
        }
        List<string> active = new();
        foreach (ShunToggle s in toggleField?.Query<ShunToggle>().ToList())
        {
            if (s.isOn)
            {
                active.Add(s.text);
            }
        }
        return active;
    }

    public static VisualElement AddIntField(string name, string label, int defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var input = new ShunIntInput();
        input.name = name;
        input.value = defaultValue;
        wrapper.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddSelectField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var select = new ShunSelect();
        select.name = name;
        select.SetOptions(options);
        select.selectedValue = defaultValue;
        wrapper.Add(select);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddSwitchField(string name, string label, bool defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.Center;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var input = new ShunSwitch();
        input.name = name;
        input.value = defaultValue;
        input.label = null;
        layout.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddComboboxField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var select = new ShunCombobox();
        select.name = name;
        select.SetOptions(options);
        select.placeholder = "Select an option";
        select.searchPlaceholder = "Type to search...";
        select.selectedValue = defaultValue;
        wrapper.Add(select);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddSliderField(string name, string label, int defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.Center;
        wrapper.Add(layout);

        var num = new Label($"{defaultValue}%");
        num.AddToClassList("shun-dialog__label");
        num.style.width = 40;
        num.style.marginTop = -4;
        num.style.marginLeft = 16;

        var input = new ShunSlider();
        input.name = name;
        input.value = defaultValue;
        input.min = 0;
        input.max = 100;
        input.RegisterCallback<PointerMoveEvent>((val) =>
        {
            if (input.isDragging)
            {
                num.text = $"{Mathf.RoundToInt(input.value)}%";
            }
        });
        layout.Add(input);

        layout.Add(num);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddTabs(string name, Dictionary<string, string> tabNames)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var tabs = new ShunTabs();
        tabs.name = name;

        var tabsContent = new ShunTabsContent();
        tabs.Add(tabsContent);

        foreach (string key in tabNames.Keys)
        {
            var tab = new ShunTab();
            tab.tabId = key;
            tab.text = tabNames[key];
            tabs.Add(tab);

            var tabPanel = new ShunTabPanel();
            tabPanel.tabId = key;
            tabPanel.name = key;
            tabsContent.Add(tabPanel);
        }

        wrapper.Add(tabs);

        return wrapper;
    }

    public static void MoveToTab(VisualElement e, VisualElement tabs, string tabPanelId)
    {
        e.RemoveFromHierarchy();
        tabs.Q<ShunTabPanel>(tabPanelId).Add(e);
    }

    public static VisualElement AddTokenField(string name, string label, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents.Add(wrapper);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        wrapper.Add(fieldlabel);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.Center;
        wrapper.Add(layout);

        // ...

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }
}