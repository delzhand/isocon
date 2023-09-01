using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitMenu : MonoBehaviour
{
    public static TokenData Data;

    void Start() {
        UI.SetBlocking(UI.System, new string[]{"UnitMenu"});
        MenuItemSetup("PlaceMenuItem", Place);
        MenuItemSetup("RemoveMenuItem", Remove);
        MenuItemSetup("MoveMenuItem", Move);
        MenuItemSetup("DeleteMenuItem", Delete);
    }

    void Update() {
        UI.ToggleDisplay("UnitMenu", Data != null);
        UI.ToggleDisplay("PlaceMenuItem", Data != null && !Data.OnField);
        UI.ToggleDisplay("RemoveMenuItem", Data != null && Data.OnField);
        UI.System.Q<VisualElement>("MoveMenuItem").Q<Label>().text = (TokenController.SelectedState == SelectedState.Moving) ? "Stop Moving" : "Move";
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
        Data.TokenObject.GetComponent<Token>().SetPlacing();
        UI.System.Q("PlaceMenuItem").AddToClassList("active");
    }

    private void Move(ClickEvent evt) {
        ClearCurrentActive();
        if (TokenController.SelectedState != SelectedState.Moving) {
            Data.TokenObject.GetComponent<Token>().SetMoving();
            UI.System.Q("MoveMenuItem").AddToClassList("active");
            return;
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
        Data = null;
    }

    public static void ClearCurrentActive() {
        UI.System.Query(null, "unit-menu-item").ForEach(item => {
            item.RemoveFromClassList("active");
        });        
    }
}
