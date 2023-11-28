using System;
using UnityEngine.UIElements;

public class TokenMenu
{
    public static void ShowMenu() {
        Block.DeselectAll();
        Block.DehighlightAll();
        TokenData data = Token.GetSelected().onlineDataObject.GetComponent<TokenData>();
        MenuItem[] items = GameSystem.Current().GetTokenMenuItems(data);
        SelectionMenu.Reset("Token Menu");
        foreach (MenuItem m in items) {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
    }

    public static void StartPlace(ClickEvent evt) {
        Block.DeselectAll();
        Block.UnfocusAll();
        Cursor.Mode = CursorMode.Placing;
        Token.GetSelected().SetPlacing();
        SelectionMenu.Find().Q("Place").AddToClassList("active");
    }

    public static void DoPlace(Block b) {
        Token.GetSelected().Place(b);
        EndPlace();
    }

    public static void EndPlace() {
        Block.DehighlightAll();
        ShowMenu();
    }

    public static void StartMove(ClickEvent evt) {
        Block.DeselectAll();
        Block.UnfocusAll();
        Cursor.Mode = CursorMode.Moving;
        Token.GetSelected().SetMoving();
        SelectionMenu.Find().Q("Move").AddToClassList("active");
    }

    public static void DoMove(Block b) {
        Token.GetSelected().Move(b);
    }

    public static void EndMoving() {
        Block.DehighlightAll();
        ShowMenu();
    }

    // private static void Move(ClickEvent evt) {
    //     ClearCurrentActive();
    //     if (ActiveMenuItem != "Moving") {
    //         ActiveMenuItem = "Moving";
    //         Block.DeselectAll();
    //         Block.UnfocusAll();
    //         Cursor.Mode = ClickMode.Moving;
    //         Data.TokenObject.GetComponent<Token>().SetMoving();
    //         UI.System.Q("MoveMenuItem").AddToClassList("active");
    //         return;
    //     }
    //     else {
    //         ActiveMenuItem = "";
    //         Cursor.Mode = ClickMode.Default;
    //     }
    //     Data.TokenObject.GetComponent<Token>().SetNeutral();
    // }
    
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

    // private static void Remove(ClickEvent evt) {
    //     ClearCurrentActive();
    //     Data.OnField = false;
    //     Data.TokenObject.GetComponent<Token>().SetNeutral();
    // }

    // private static void Delete(ClickEvent evt) {
    //     Player.Self().CmdRequestDeleteToken(Data);
    // }

    // private static void EndTurn(ClickEvent evt) {
    //     Player.Self().CmdRequestTokenDataSetValue(Data, "Status", "Turn Ended|neu");
    //     TokenController.Deselect();
    // }

    // public static void HideMenu() {
    //     Block.DehighlightAll();
    //     ClearCurrentActive();
    //     ActiveMenuItem = "";
    //     Data = null;
    //     UI.ToggleDisplay("UnitMenu", false);
    // }

    // private static void ClearCurrentActive() {
    //     UI.System.Query(null, "unit-menu-item").ForEach(item => {
    //         item.RemoveFromClassList("active");
    //     });        
    // }

    // public static void DonePlacing() {
    //     ActiveMenuItem = "";
    //     ClearCurrentActive();
    //     Block.DehighlightAll();
    // }
}
