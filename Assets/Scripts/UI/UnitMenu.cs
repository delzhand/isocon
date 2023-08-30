using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitMenu : MonoBehaviour
{
    public static TokenData Data;

    void Start() {
        MenuItemSetup("PlaceMenuItem");
        MenuItemSetup("RemoveMenuItem");
        MenuItemSetup("MoveMenuItem");
    }

    void Update() {
        UI.ToggleDisplay("UnitMenu", Data != null);
        UI.ToggleDisplay("PlaceMenuItem", Data != null && !Data.OnField);
        UI.ToggleDisplay("RemoveMenuItem", Data != null && Data.OnField);
    }

    private void MenuItemSetup(string name) {
        UI.System.Q(name).RegisterCallback<MouseEnterEvent>((evt) => {
            UI.System.Q(name).AddToClassList("hover");
        });
        UI.System.Q(name).RegisterCallback<MouseLeaveEvent>((evt) => {
            UI.System.Q(name).RemoveFromClassList("hover");
        });
        UI.System.Q(name).RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("foo");
        });

    }

    public static void ShowMenu(TokenData data) {
        Data = data;
        UI.System.Q("UnitMenu").style.left = Data.Element.resolvedStyle.left;
        UI.System.Q("UnitMenu").style.bottom = 80;
    }

    public static void HideMenu() {
        Data = null;
    }
}
