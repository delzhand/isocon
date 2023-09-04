using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitMenu : MonoBehaviour
{
    public static TokenData Data;

    public static string ActiveMenuItem;

    void Start() {
        UI.SetBlocking(UI.System, new string[]{"UnitMenu"});
        MenuItemSetup("PlaceMenuItem", Place);
        MenuItemSetup("RemoveMenuItem", Remove);
        MenuItemSetup("MoveMenuItem", Move);
        MenuItemSetup("EditMenuItem", Edit);
        MenuItemSetup("DeleteMenuItem", Delete);
    }

    void Update() {
        UI.ToggleDisplay("UnitMenu", Data != null);
        UI.ToggleDisplay("PlaceMenuItem", Data != null && !Data.OnField);
        UI.ToggleDisplay("RemoveMenuItem", Data != null && Data.OnField);
        UI.ToggleDisplay("MoveMenuItem", Data != null && Data.OnField);
        UI.ToggleDisplay("Icon1_5EditPanel", ActiveMenuItem == "Edit");

        UI.System.Q<VisualElement>("MoveMenuItem").Q<Label>().text = (ActiveMenuItem == "Moving") ? "Stop Moving" : "Move";

        IResolvedStyle menuStyle = UI.System.Q("UnitMenu").resolvedStyle;
        UI.System.Q("Icon1_5EditPanel").style.left = menuStyle.left + menuStyle.width + 4;
        UI.System.Q("Icon1_5EditPanel").style.bottom = 80;
    }

    private void MenuItemSetup(string name, Action<ClickEvent> clickHandler) {
        UI.System.Q(name).RegisterCallback<MouseEnterEvent>((evt) => {
            UI.System.Q(name).AddToClassList("hover");
        });
        UI.System.Q(name).RegisterCallback<MouseLeaveEvent>((evt) => {
            UI.System.Q(name).RemoveFromClassList("hover");
        });
        UI.System.Q(name).RegisterCallback<ClickEvent>((evt) => {
            clickHandler.Invoke(evt);
        });
    }

    private void Place(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "Placing") {
            ActiveMenuItem = "Placing";
            Data.TokenObject.GetComponent<Token>().SetPlacing();
            UI.System.Q("PlaceMenuItem").AddToClassList("active");
            return;
        }
        else {
            ActiveMenuItem = "";
        }
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private void Move(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "Moving") {
            ActiveMenuItem = "Moving";
            Data.TokenObject.GetComponent<Token>().SetMoving();
            UI.System.Q("MoveMenuItem").AddToClassList("active");
            return;
        }
        else {
            ActiveMenuItem = "";
        }
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private void Edit(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "Edit") {
            ActiveMenuItem = "Edit";
            TokenEditPanel.Show(Data);
            UI.ToggleActiveClass("EditMenuItem", true);
            return;
        }
        else {
            ActiveMenuItem = "";
        }
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private void Remove(ClickEvent evt) {
        ClearCurrentActive();
        Data.OnField = false;
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private void Delete(ClickEvent evt) {
        // UI.System.Q("UnitBar").Remove(Data.Element);
        // UI.System.Q("Worldspace").Remove(Data.overhead);
        // Destroy(Data.TokenObject);
        
        // Destroy(Data);
        // TokenController.Deselect();
    }

    public static void ShowMenu(TokenData data) {
        ClearCurrentActive();
        Data = data;
        Data.TokenObject.GetComponent<Token>().SetNeutral();
        UI.System.Q("UnitMenu").style.left = Data.Element.resolvedStyle.left;
        UI.System.Q("UnitMenu").style.bottom = 80;
    }

    public static void HideMenu() {
        ClearCurrentActive();
        ActiveMenuItem = "";
        Data = null;
    }

    private static void ClearCurrentActive() {
        UI.System.Query(null, "unit-menu-item").ForEach(item => {
            item.RemoveFromClassList("active");
        });        
    }

    public static void DonePlacing() {
        ActiveMenuItem = "";
        ClearCurrentActive();
    }
}
