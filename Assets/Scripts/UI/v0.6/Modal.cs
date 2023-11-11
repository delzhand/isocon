using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Modal
{
    public delegate void ConfirmCallback();
    
    private static ConfirmCallback _doubleConfirm;

    public static void Setup() {
        UI.SetBlocking(UI.System, new string[]{"GeneralModal", "Backdrop", "DoubleConfirmModal"});
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

        UI.ToggleDisplay("Backdrop", true);
    }

    public static void AddContents(VisualElement e) {
        VisualElement modal = Find();
        modal.Q("Contents").Add(e);
    }

    public static void AddButton(VisualElement e) {
        VisualElement modal = Find();
        modal.Q("Buttons").Add(e);
    }

    public static void Close() {
        VisualElement modal = Find();
        UI.ToggleDisplay(modal, false);
        UI.ToggleDisplay("Backdrop", false);
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
    }

    private static void DoubleConfirmConfirmed(ClickEvent evt) {
        _doubleConfirm?.Invoke();
        VisualElement modal = Find();
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(modal, false);
        UI.ToggleDisplay(dcModal, false);
        UI.ToggleDisplay("Backdrop", false);
    }

    private static void DoubleConfirmCancelled(ClickEvent evt) {
        VisualElement modal = Find();
        VisualElement dcModal = FindDoubleConfirm();
        UI.ToggleDisplay(modal, true);
        UI.ToggleDisplay(dcModal, false);
    }

}
