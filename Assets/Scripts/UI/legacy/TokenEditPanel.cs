using System;
using System.Collections;
using System.Collections.Generic;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenEditPanel : MonoBehaviour
{
    public static int HPOld;
    public static TokenData Data;

    // Start is called before the first frame update
    void Start()
    {
        UI.SetBlocking(UI.System, new string[]{"EditPanel"});
    }

    void Update() {
        UI.ToggleDisplay("EditPanel", UnitMenu.ActiveMenuItem == "Edit");
    }

    public static void Show(TokenData data) {
        SetPosition();
        CloseFoldouts();
        Data = data;
        SyncValues();
    }

    public static void SyncValues() {
        if (Data != null) {
            GameSystem.Current().SyncEditValues(Data);
        }        
    }

    private static void CloseFoldouts() {
        UI.System.Q("EditPanel").Query(null, "unity-foldout").ForEach(item => {
            (item as Foldout).value = false;
        });        
    }

    private static void SetPosition() {
        IResolvedStyle menuStyle = UI.System.Q("UnitMenu").resolvedStyle;
        UI.System.Q("EditPanel").style.left = menuStyle.left + menuStyle.width + 4;
        UI.System.Q("EditPanel").style.bottom = 80;
    }
}
