using System;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public class Modal2
{
    // We need to maintain references to ShunDialogContent elements because in the process of being created,
    // they get reparented to the overlay in a way that makes it impossible to positively locate them using
    // Q functions.
    private static Dictionary<string, ShunDialogContent> _sdc = new();

    private static string _targetDialogName;

    // public static void SetCurrentDialog(string name, out ShunDialog dialog, out ShunDialogContent contents)
    // {
    //     _targetDialogName = name;
    //     _sdc[name] = UI.System.Q(name).Q<ShunDialogContent>();
    //     contents = Contents(name);
    //     contents.Clear();
    //     dialog = CurrentDialog;
    // }

    public static ShunDialog SetCurrentDialog(string name)
    {
        _targetDialogName = name;
        _sdc[name] = UI.System.Q(name).Q<ShunDialogContent>();
        Contents(name).Clear();
        return CurrentDialog;
    }

    public static ShunDialog CurrentDialog
    {
        get => UI.System.Q(_targetDialogName).Q<ShunDialog>();
    }

    public static ShunDialog Dialog(string dialogName)
    {
        return UI.System.Q(dialogName).Q<ShunDialog>();
    }

    public static ShunDialogContent Contents(string dialogName)
    {
        return _sdc[dialogName];
    }

    public static void Open()
    {
        CurrentDialog.Open();
    }

    public static void Close()
    {
        CurrentDialog.Close();
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
        Contents(_targetDialogName).Add(wrapper);

        var title = new ShunDialogTitle();
        title.text = value;
        wrapper.Add(title);
    }

    public static VisualElement AddDialogFooter(string cancelText = "Cancel", Action cancelAction = null)
    {
        var footer = new ShunContainer();
        footer.AddToClassList("shun-dialog__footer");
        Contents(_targetDialogName).Add(footer);

        var cancel = new ShunButton();
        cancel.text = cancelText;
        if (cancelAction != null)
        {
            cancel.clicked += cancelAction;
        }
        else
        {
            cancel.clicked += Modal2.Close;
        }
        cancel.SetVariant(ButtonVariant.Outline);
        footer.Add(cancel);

        return footer;
    }

    public static VisualElement AddFooterConfirm(string text, Action confirmAction)
    {
        var confirm = new ShunButton();
        confirm.text = text;
        confirm.clicked += confirmAction;
        confirm.SetVariant(ButtonVariant.Primary);
        Contents(_targetDialogName).Q(className: "shun-dialog__footer").Add(confirm);
        return confirm;
    }

    public static string GetTextFieldValue(string dialog, string name)
    {
        return Contents(dialog).Q<ShunInput>(name)?.value ?? null;
    }

    public static VisualElement AddAlert(string title, string description)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var hostAlert = new ShunAlert();
        hostAlert.SetVariant(AlertVariant.Default);
        hostAlert.title = title;
        hostAlert.description = description;

        wrapper.Add(hostAlert);

        return wrapper;
    }

    public static VisualElement AddLongMarkup(string content)
    {
        var label = new ShunLabel();
        label.AddToClassList("shun-dialog__label");
        label.text = content;
        label.style.whiteSpace = WhiteSpace.Normal;
        Contents(_targetDialogName).Add(label);
        return label;
    }

    public static VisualElement AddTextField(string name, string label, string defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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

    public static VisualElement AddInlineTextField(string name, string label, string defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Column;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var input = new ShunInput();
        input.name = name;
        input.value = defaultValue;
        input.AddToClassList("shun-inline-input");
        layout2.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddInlineTextAreaField(string name, string label, string defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Column;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var input = new ShunTextArea();
        // input.multiline = true;
        input.name = name;
        input.value = defaultValue;
        // input.AddToClassList("shun-inline-input");
        layout2.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    public static VisualElement AddInlineFileField(string name, string label, string defaultValue, FileBrowserType type, bool saveOp, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Row;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var input = new ShunInput();
        input.name = name;
        input.value = defaultValue;
        input.isReadOnly = true;
        input.style.flexGrow = 1;
        layout2.Add(input);

        var searchButton = new ShunButton();
        searchButton.SetVariant(ButtonVariant.Outline);
        searchButton.style.backgroundImage = Resources.Load<Texture2D>("Textures/search");
        searchButton.style.height = 40;
        searchButton.style.width = 40;
        searchButton.style.marginLeft = 16;
        searchButton.style.backgroundSize = new BackgroundSize(20, 20);
        searchButton.RegisterCallback<ClickEvent>((evt) =>
        {
            FileBrowserHelper.Open(ConfirmFileFieldSelect, name, type, saveOp);
        });
        layout2.Add(searchButton);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    private static void ConfirmFileFieldSelect()
    {
        string result = FileBrowser.Result[0];
        Debug.Log(result);
        // Contents(_targetDialogName).Q
        // UI.Modal.Q(FileBrowserHelper.FieldOrigin).Q<TextField>("File").value = result;
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
        Contents(_targetDialogName).Add(wrapper);

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

    public static List<string> GetToggleFieldValues(string dialog, string name)
    {
        var toggleField = Contents(dialog).Q<ShunToggleGroup>(name);
        if (toggleField == null)
        {
            return new List<string>();
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

    public static int GetIntFieldValue(string dialog, string name)
    {
        return Contents(dialog).Q<ShunIntInput>(name)?.value ?? 0;
    }

    public static VisualElement AddIntField(string name, string label, int defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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

    public static VisualElement AddInlineIntField(string name, string label, int defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Column;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var input = new ShunIntInput();
        input.name = name;
        input.value = defaultValue;
        input.AddToClassList("shun-inline-input");
        layout2.Add(input);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    public static string GetSelectFieldValue(string dialog, string name)
    {
        return Contents(dialog).Q<ShunSelect>(name)?.selectedValue ?? null;
    }

    public static VisualElement AddSelectField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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

    public static VisualElement AddInlineSelectField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Column;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var select = new ShunSelect();
        select.name = name;
        select.SetOptions(options);
        select.selectedValue = defaultValue;
        layout2.Add(select);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    public static bool GetSwitchFieldValue(string dialog, string name)
    {
        return Contents(dialog).Q<ShunSwitch>(name)?.value ?? false;
    }

    public static VisualElement AddSwitchField(string name, string label, bool defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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

    public static string GetComboboxFieldValue(string dialog, string name)
    {
        return Contents(dialog).Q<ShunCombobox>(name)?.selectedValue ?? null;
    }

    public static VisualElement AddComboboxField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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

    public static VisualElement AddInlineComboboxField(string name, string label, string defaultValue, List<string> options, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Column;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var select = new ShunCombobox();
        select.name = name;
        select.SetOptions(options);
        select.placeholder = "Select an option";
        select.searchPlaceholder = "Type to search...";
        select.selectedValue = defaultValue;
        layout2.Add(select);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            layout2.Add(help);
        }

        return wrapper;
    }

    public static int GetSliderFieldValue(string dialog, string name)
    {
        return Mathf.RoundToInt(Contents(dialog).Q<ShunSlider>(name).value);
    }

    public static VisualElement AddSliderField(string name, string label, int defaultValue, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

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
        input.RegisterCallback<PointerDownEvent>((val) =>
        {
            num.text = $"{Mathf.RoundToInt(input.value)}%";
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
        Contents(_targetDialogName).Add(wrapper);

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

    public static VisualElement AddScrollArea(string name)
    {
        var area = new ShunScrollArea();
        area.name = name;
        area.style.height = 400;
        Contents(_targetDialogName).Add(area);

        var areaContent = new ShunScrollAreaContent();
        area.Add(areaContent);

        return area;
    }

    public static void MoveToScrollArea(VisualElement e, VisualElement area)
    {
        e.RemoveFromHierarchy();
        area.Q<ShunScrollAreaContent>().Add(e);
    }

    public static void MoveToContainer(VisualElement e, VisualElement target)
    {
        e.RemoveFromHierarchy();
        target.Add(e);
    }

    public static VisualElement AddTokenField(string name, string label, string helpText = null)
    {
        var wrapper = new ShunContainer();
        wrapper.AddToClassList("shun-dialog__field");
        Contents(_targetDialogName).Add(wrapper);

        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row;
        layout.style.justifyContent = Justify.SpaceBetween;
        layout.style.alignItems = Align.FlexStart;
        wrapper.Add(layout);

        var fieldlabel = new Label(label);
        fieldlabel.AddToClassList("shun-dialog__label");
        layout.Add(fieldlabel);

        var layout2 = new VisualElement();
        layout2.style.flexDirection = FlexDirection.Row;
        layout2.style.alignItems = Align.Stretch;
        layout2.style.minWidth = 250;
        layout.Add(layout2);

        var select = new ShunCombobox();
        select.name = name;
        select.SetOptions(TokenLibraryModal.Options());
        select.placeholder = "Select an option";
        select.searchPlaceholder = "Type to search...";
        layout2.Add(select);

        var libLabel = new ShunButton();
        libLabel.SetVariant(ButtonVariant.Outline);
        libLabel.style.backgroundImage = Resources.Load<Texture2D>("Textures/search");
        libLabel.style.height = 40;
        libLabel.style.width = 40;
        libLabel.style.marginLeft = 16;
        libLabel.style.backgroundSize = new BackgroundSize(20, 20);
        libLabel.RegisterCallback<ClickEvent>((evt) =>
        {
            TokenLibraryModal.OpenSelect(() =>
            {
                select.selectedValue = TokenLibraryModal.GetToken().Name;
            });
        });
        layout2.Add(libLabel);

        if (helpText != null)
        {
            var help = new Label(helpText);
            help.AddToClassList("shun-dialog__description");
            wrapper.Add(help);
        }

        return wrapper;
    }
}