using System;
using UnityEngine.UIElements;

public class TokenMenu
{
    public static TokenData Data;
    private static string ActiveMenuItem;

    public static void Setup() {
        UI.SetBlocking(UI.System, new string[]{"UnitMenu"});
        MenuItemSetup("PlaceMenuItem", Place);
        MenuItemSetup("RemoveMenuItem", Remove);
        MenuItemSetup("MoveMenuItem", Move);
        // MenuItemSetup("EditMenuItem", Edit);
        MenuItemSetup("EndTurnMenuItem", EndTurn);
        MenuItemSetup("DeleteMenuItem", Delete);
        // MenuItemSetup("AlterHPMenuItem", AlterHp);
    }

    private static void MenuItemSetup(string name, Action<ClickEvent> clickHandler) {
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

    private static void Place(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "Placing") {
            ActiveMenuItem = "Placing";
            Block.DeselectAll();
            Block.UnfocusAll();
            Cursor.Mode = ClickMode.Placing;
            Data.TokenObject.GetComponent<Token>().SetPlacing();
            UI.System.Q("PlaceMenuItem").AddToClassList("active");
            return;
        }
        else {
            ActiveMenuItem = "";
            Cursor.Mode = ClickMode.Default;
        }
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private static void Move(ClickEvent evt) {
        ClearCurrentActive();
        if (ActiveMenuItem != "Moving") {
            ActiveMenuItem = "Moving";
            Block.DeselectAll();
            Block.UnfocusAll();
            Cursor.Mode = ClickMode.Moving;
            Data.TokenObject.GetComponent<Token>().SetMoving();
            UI.System.Q("MoveMenuItem").AddToClassList("active");
            return;
        }
        else {
            ActiveMenuItem = "";
            Cursor.Mode = ClickMode.Default;
        }
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }
    
    // private static void Edit(ClickEvent evt) {
    //     ClearCurrentActive();
    //     if (ActiveMenuItem != "Edit") {
    //         ActiveMenuItem = "Edit";
    //         TokenEditPanel.Show(Data);
    //         UI.ToggleActiveClass("EditMenuItem", true);
    //         return;
    //     }
    //     else {
    //         ActiveMenuItem = "";
    //         Cursor.Mode = ClickMode.Default;
    //     }
    //     Data.TokenObject.GetComponent<Token>().SetNeutral();
    // }

    // private static void AlterHp(ClickEvent evt) {
    //     ClearCurrentActive();
    //     if (ActiveMenuItem != "AlterHP") {
    //         ActiveMenuItem = "AlterHP";
    //         Debug.Log(ActiveMenuItem);
    //         LifeEditPanel.Show(Data);
    //         UI.ToggleActiveClass("AlterHPMenuItem", true);
    //         return;
    //     }
    //     else {
    //         ActiveMenuItem = "";
    //     }
    //     Data.TokenObject.GetComponent<Token>().SetNeutral();
    // }

    private static void Remove(ClickEvent evt) {
        ClearCurrentActive();
        Data.OnField = false;
        Data.TokenObject.GetComponent<Token>().SetNeutral();
    }

    private static void Delete(ClickEvent evt) {
        Player.Self().CmdRequestDeleteToken(Data);
    }

    private static void EndTurn(ClickEvent evt) {
        Player.Self().CmdRequestTokenDataSetValue(Data, "Status", "Turn Ended|neu");
        TokenController.Deselect();
    }

    public static void ShowMenu(TokenData data) {
        Block.DeselectAll();
        Block.DehighlightAll();
        ClearCurrentActive();
        Data = data;
        Data.TokenObject.GetComponent<Token>().SetNeutral();
        UI.ToggleDisplay("UnitMenu", true);
    }

    public static void HideMenu() {
        Block.DehighlightAll();
        ClearCurrentActive();
        ActiveMenuItem = "";
        Data = null;
        UI.ToggleDisplay("UnitMenu", false);
    }

    private static void ClearCurrentActive() {
        UI.System.Query(null, "unit-menu-item").ForEach(item => {
            item.RemoveFromClassList("active");
        });        
    }

    public static void DonePlacing() {
        ActiveMenuItem = "";
        ClearCurrentActive();
        Block.DehighlightAll();
    }
}
