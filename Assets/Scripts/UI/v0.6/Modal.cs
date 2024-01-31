using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

public class Modal
{
    private static bool isModalOpen = false;
    private static bool isConfirmOpen = false;

    public delegate void ConfirmCallback();

    private static ConfirmCallback _doubleConfirm;

    private static EventCallback<ClickEvent> preferredAction;
    private static EventCallback<ClickEvent> cancelAction;

    public static void Setup()
    {
        UI.Modal.Q("Top").Q("Exit").RegisterCallback<ClickEvent>(Modal.CloseEvent);
        FindDoubleConfirm().Q<Button>("Confirm").RegisterCallback<ClickEvent>(DoubleConfirmConfirmed);
        FindDoubleConfirm().Q<Button>("Cancel").RegisterCallback<ClickEvent>(DoubleConfirmCancelled);
    }

    private static VisualElement FindDoubleConfirm()
    {
        return UI.System.Q("DoubleConfirmModal");
    }

    public static void Reset(string title)
    {
        UI.ToggleDisplay(UI.Modal, true);
        UI.Modal.Q<Label>("Title").text = title;
        UI.Modal.Q("Contents").Clear();
        UI.Modal.Q("Buttons").Clear();
        preferredAction = null;
        cancelAction = null;

        UI.ToggleDisplay("Backdrop", true);
        isModalOpen = true;
    }

    private static void AddContents(VisualElement e)
    {
        UI.Modal.Q("Contents").Add(e);
        e.SendToBack();
    }

    public static void AddContentButton(string name, string label, EventCallback<ClickEvent> onClick)
    {
        Button button = new Button();
        button.name = name;
        button.text = label;
        button.RegisterCallback<ClickEvent>(onClick);
        UI.Modal.Q("Contents").Add(button);
        button.SendToBack();
    }

    public static void AddButton(string label, EventCallback<ClickEvent> onClick)
    {
        Button button = new Button();
        button.text = label;
        button.RegisterCallback<ClickEvent>(onClick);
        UI.Modal.Q("Buttons").Add(button);
    }

    public static void AddPreferredButton(string label, EventCallback<ClickEvent> onClick)
    {
        preferredAction = onClick;
        Button button = new Button();
        button.text = label;
        button.AddToClassList("preferred");
        button.RegisterCallback<ClickEvent>(onClick);
        UI.Modal.Q("Buttons").Add(button);
    }

    public static void AddCloseCallback(EventCallback<ClickEvent> onClick)
    {
        cancelAction += onClick;
    }

    public static void Close()
    {
        if (isConfirmOpen)
        {
            DoubleConfirmCancelled(new ClickEvent());
        }
        else if (isModalOpen)
        {
            UI.Modal.Q<Label>("Title").text = "";
            UI.Modal.Q("Contents").Clear();
            UI.Modal.Q("Buttons").Clear();
            preferredAction = null;
            UI.ToggleDisplay(UI.Modal, false);
            UI.ToggleDisplay("Backdrop", false);
            isModalOpen = false;
            if (cancelAction != null)
            {
                cancelAction.Invoke(new ClickEvent());
                cancelAction = null;
            }
        }
    }

    public static void CloseEvent(ClickEvent evt)
    {
        Close();
    }

    public static bool IsOpen()
    {
        return isModalOpen || isConfirmOpen;
    }

    public static void Activate()
    {
        if (isConfirmOpen)
        {
            DoubleConfirmConfirmed(new ClickEvent());
        }
        else if (isModalOpen && preferredAction != null)
        {
            preferredAction.Invoke(new ClickEvent());
        }
    }

    public static void DoubleConfirm(string title, string message, ConfirmCallback confirm)
    {
        VisualElement dcModal = FindDoubleConfirm();
        dcModal.Q<Label>("Title").text = title;
        dcModal.Q<Label>("Message").text = message;
        UI.ToggleDisplay(UI.Modal, false);
        UI.ToggleDisplay(dcModal, true);
        _doubleConfirm = null;
        _doubleConfirm += confirm;
        UI.ToggleDisplay("Backdrop", true);
        isConfirmOpen = true;
    }

    private static void DoubleConfirmConfirmed(ClickEvent evt)
    {
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(UI.Modal, false);
        UI.ToggleDisplay(dcModal, false);
        UI.ToggleDisplay("Backdrop", false);
        isModalOpen = false;
        isConfirmOpen = false;
        _doubleConfirm?.Invoke();
    }

    private static void DoubleConfirmCancelled(ClickEvent evt)
    {
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(dcModal, false);
        isConfirmOpen = false;
        if (isModalOpen)
        {
            UI.ToggleDisplay(UI.Modal, true);
        }
        else
        {
            UI.ToggleDisplay("Backdrop", false);
        }
    }

    public static void AddLabel(string text, string styleClass)
    {
        Label field = new Label();
        field.text = text;
        field.AddToClassList("no-margin");
        field.AddToClassList(styleClass);
        Modal.AddContents(field);
    }

    public static void AddSeparator()
    {
        VisualElement v = new VisualElement();
        v.AddToClassList("separator");
        Modal.AddContents(v);
    }

    public static void AddColumns(string name, int count)
    {
        VisualElement v = new VisualElement();
        v.name = name;
        v.style.flexDirection = FlexDirection.Row;
        v.style.marginLeft = -8;
        v.style.marginRight = -8;
        for (int i = 0; i < count; i++)
        {
            VisualElement v2 = new();
            v2.name = $"{name}_{i}";
            v2.style.marginLeft = 8;
            v2.style.marginRight = 8;
            v.Add(v2);
        }
        Modal.AddContents(v);
    }

    public static void MoveToColumn(string columnName, string elementName)
    {
        UI.Modal.Q(columnName).Add(UI.Modal.Q(elementName));
    }

    public static void AddMarkup(string name, string contents)
    {
        Label label = new Label(contents);
        label.enableRichText = true;
        label.style.whiteSpace = WhiteSpace.Normal;

        label.name = name;
        label.AddToClassList("no-margin");
        Modal.AddContents(label);
    }

    public static void AddTextField(string name, string label, string defaultValue, EventCallback<ChangeEvent<string>> onChange = null)
    {
        TextField field = new(label);
        field.value = defaultValue;

        field.name = name;
        if (onChange != null)
        {
            field.RegisterValueChangedCallback<string>(onChange);
        }
        field.AddToClassList("no-margin");
        Modal.AddContents(field);
    }

    public static void AddDropdownField(string name, string label, string defaultValue, string[] options, EventCallback<ChangeEvent<string>> onChange = null)
    {
        DropdownField field = new(label);
        field.choices = options.ToList<string>();
        field.value = defaultValue;

        field.name = name;
        if (onChange != null)
        {
            field.RegisterValueChangedCallback<string>(onChange);
        }
        field.AddToClassList("no-margin");
        field.focusable = false;
        Modal.AddContents(field);
    }

    public static void AddToggleField(string name, string label, bool defaultValue, EventCallback<ChangeEvent<bool>> onChange = null)
    {
        Toggle field = new(label);
        field.value = defaultValue;

        field.name = name;
        if (onChange != null)
        {
            field.RegisterValueChangedCallback<bool>(onChange);
        }
        field.AddToClassList("no-margin");
        field.focusable = false;
        Modal.AddContents(field);
    }

    public static void AddNumberNudgerField(string name, string label, int defaultValue, int min, Action<int> onChange = null)
    {
        NumberNudger field = new();
        field.label = label;
        field.name = name;
        field.value = defaultValue;
        field.minValue = min;
        if (onChange != null)
        {
            field.AddValueChangedCallback(onChange);
        }
        field.AddToClassList("no-margin");
        field.focusable = false;
        Modal.AddContents(field);
    }

    public static void AddIntField(string name, string label, int defaultValue, EventCallback<ChangeEvent<int>> onChange = null)
    {
        IntegerField field = new(label);
        field.value = defaultValue;

        field.name = name;
        if (onChange != null)
        {
            field.RegisterValueChangedCallback<int>(onChange);
        }
        field.AddToClassList("no-margin");
        Modal.AddContents(field);
    }

    public static void AddFloatSlider(string name, string label, float defaultValue, float highValue, float lowValue, EventCallback<ChangeEvent<float>> onChange = null)
    {
        Slider field = new(label);
        field.lowValue = lowValue;
        field.highValue = highValue;
        field.value = defaultValue;

        field.name = name;
        if (onChange != null)
        {
            field.RegisterValueChangedCallback<float>(onChange);
        }
        field.focusable = false;
        field.AddToClassList("no-margin");
        Modal.AddContents(field);
    }

    public static void AddSearchField(string name, string label, string defaultValue, string[] options)
    {
        VisualElement field = SearchField.Create(options, label);
        field.name = name;
        Modal.AddContents(field);
    }

    public static void AddColorField(string name, Color initial)
    {
        VisualElement field = ColorField.Create(name, initial);
        field.name = name;
        Modal.AddContents(field);
    }
}
