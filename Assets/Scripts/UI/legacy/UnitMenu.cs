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
        MenuItemSetup("EndTurnMenuItem", EndTurn);
        MenuItemSetup("DeleteMenuItem", Delete);
        MenuItemSetup("AlterHPMenuItem", AlterHp);
    }

    void Update() {
        UI.ToggleDisplay("UnitMenu", Data != null);
        UI.ToggleDisplay("PlaceMenuItem", Data != null && !Data.OnField);
        UI.ToggleDisplay("RemoveMenuItem", Data != null && Data.OnField);
        UI.ToggleDisplay("MoveMenuItem", Data != null && Data.OnField);
        UI.ToggleDisplay("EndTurnMenuItem", Data != null && !Data.CheckCondition("TurnEnded"));
        GameSystem system = GameSystem.Current();
        if (system && system.GetEditPanelName() != null) {
            UI.ToggleDisplay(GameSystem.Current().GetEditPanelName(), ActiveMenuItem == "Edit");
        }

        UI.System.Q<VisualElement>("MoveMenuItem").Q<Label>().text = (ActiveMenuItem == "Moving") ? "Stop Moving" : "Move";
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

    private void AlterHp(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "AlterHP") {
            ActiveMenuItem = "AlterHP";
            Debug.Log(ActiveMenuItem);
            LifeEditPanel.Show(Data);
            UI.ToggleActiveClass("AlterHPMenuItem", true);
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
        Player.Self().CmdRequestDeleteToken(Data);
    }

    private void EndTurn(ClickEvent evt) {
        Player.Self().CmdRequestTokenDataSetValue(Data, "Status", "Turn Ended|neu");
        TokenController.Deselect();
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
