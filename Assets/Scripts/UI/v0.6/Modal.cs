using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Modal
{
    private static bool isModalOpen = false;
    private static bool isConfirmOpen = false;
    
    public delegate void ConfirmCallback();
    
    private static ConfirmCallback _doubleConfirm;

    private static EventCallback<ClickEvent> preferredAction;

    public static void Setup() {
        FindDoubleConfirm().Q<Button>("Confirm").RegisterCallback<ClickEvent>(DoubleConfirmConfirmed);
        FindDoubleConfirm().Q<Button>("Cancel").RegisterCallback<ClickEvent>(DoubleConfirmCancelled);
    }

    public static VisualElement Find() {
        return UI.System.Q("GeneralModal");
    }

    private static VisualElement FindDoubleConfirm() {
        return UI.System.Q("DoubleConfirmModal");
    }

    public static void Reset(string title) {
        VisualElement modal = Find();
        UI.ToggleDisplay(modal, true);
        modal.Q<Label>("Title").text = title;
        modal.Q("Contents").Clear();
        modal.Q("Buttons").Clear();
        preferredAction = null;

        UI.ToggleDisplay("Backdrop", true);
        isModalOpen = true;
    }

    private static void AddContents(VisualElement e) {
        VisualElement modal = Find();
        modal.Q("Contents").Add(e);
        e.SendToBack();
    }

    public static void AddButton(string label, EventCallback<ClickEvent> onClick) {
        VisualElement modal = Find();
        Button button = new Button();
        button.text = label;
        button.RegisterCallback<ClickEvent>(onClick);
        modal.Q("Buttons").Add(button);
    }

    public static void AddPreferredButton(string label, EventCallback<ClickEvent> onClick) {
        preferredAction = onClick;
        VisualElement modal = Find();
        Button button = new Button();
        button.text = label;
        button.AddToClassList("preferred");
        button.RegisterCallback<ClickEvent>(onClick);
        modal.Q("Buttons").Add(button);
    }

    public static void Close() {
        if (isConfirmOpen) {
            DoubleConfirmCancelled(new ClickEvent());
        }
        else if (isModalOpen) {
            VisualElement modal = Find();
            modal.Q<Label>("Title").text = "";
            modal.Q("Contents").Clear();
            modal.Q("Buttons").Clear();
            preferredAction = null;
            UI.ToggleDisplay(modal, false);
            UI.ToggleDisplay("Backdrop", false);
            isModalOpen = false;
        }
    }

    public static void CloseEvent(ClickEvent evt) {
        Close();
    }

    public static bool IsOpen() {
        return isModalOpen || isConfirmOpen;
    }

    public static void Activate() {
        if (isConfirmOpen) {
            DoubleConfirmConfirmed(new ClickEvent());
        }
        else if (isModalOpen && preferredAction != null) {
            preferredAction.Invoke(new ClickEvent());
        }
    }

    public static void DoubleConfirm(string title, string message, ConfirmCallback confirm) {
        VisualElement modal = Find();
        VisualElement dcModal = FindDoubleConfirm();
        dcModal.Q<Label>("Title").text = title;
        dcModal.Q<Label>("Message").text = message;
        UI.ToggleDisplay(modal, false);
        UI.ToggleDisplay(dcModal, true);
        _doubleConfirm = null;
        _doubleConfirm += confirm;
        UI.ToggleDisplay("Backdrop", true);
        isConfirmOpen = true;
    }

    private static void DoubleConfirmConfirmed(ClickEvent evt) {
        _doubleConfirm?.Invoke();
        VisualElement modal = Find();
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(modal, false);
        UI.ToggleDisplay(dcModal, false);
        UI.ToggleDisplay("Backdrop", false);
        isModalOpen = false;
        isConfirmOpen = false;
    }

    private static void DoubleConfirmCancelled(ClickEvent evt) {
        VisualElement modal = Find();
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(dcModal, false);
        isConfirmOpen = false;
        if (isModalOpen) {
            UI.ToggleDisplay(modal, true);
        }
        else {
            UI.ToggleDisplay("Backdrop", false);
        }
    }

    public static void AddTextField(string name, string label, string defaultValue, EventCallback<ChangeEvent<string>> onChange = null) {
        TextField field = new(label);
        field.value = defaultValue;
     
        field.name = name;
        if (onChange != null) {
            field.RegisterValueChangedCallback<string>(onChange);
        }
        field.AddToClassList("no-margin");
        Modal.AddContents(field);
    }

    public static void AddDropdownField(string name, string label, string defaultValue, string[] options, EventCallback<ChangeEvent<string>> onChange = null) {
        DropdownField field = new(label);
        field.choices = options.ToList<string>();
        field.value = defaultValue; 

        field.name = name;
        if (onChange != null) {
            field.RegisterValueChangedCallback<string>(onChange);            
        }
        field.AddToClassList("no-margin");
        field.focusable = false;
        Modal.AddContents(field);
    }

    public static void AddToggleField(string name, string label, bool defaultValue, EventCallback<ChangeEvent<bool>> onChange = null){
        Toggle field = new(label);
        field.value = defaultValue;
        
        field.name = name;
        if (onChange != null) {
            field.RegisterValueChangedCallback<bool>(onChange);            
        }
        field.AddToClassList("no-margin");
        field.focusable = false;
        Modal.AddContents(field);    
    }

    public static void AddIntField(string name, string label, int defaultValue, EventCallback<ChangeEvent<int>> onChange = null){
        IntegerField field = new(label);
        field.value = defaultValue;
        
        field.name = name;
        if (onChange != null) {
            field.RegisterValueChangedCallback<int>(onChange);            
        }
        field.AddToClassList("no-margin");
        Modal.AddContents(field);    
    }

    public static void AddSearchField(string name, string label, string defaultValue, string[] options) {
        VisualElement searchField = SearchField.Create(options, label);
        searchField.name = name;
        Modal.AddContents(searchField);
    }
}
