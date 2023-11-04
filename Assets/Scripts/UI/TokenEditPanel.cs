using System;
using System.Collections;
using System.Collections.Generic;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenEditPanel : MonoBehaviour
{
    private static int HPOld;
    private static Icon_v1_5TokenData Data;

    // Start is called before the first frame update
    void Start()
    {
        UI.SetBlocking(UI.System, new string[]{"EditPanel"});
        HP();
        Vigor();
        Wounds();
        Resolve();
        ClassFeatures();
        Status();
    }

    void Update() {
        UI.ToggleDisplay("EditPanel", UnitMenu.ActiveMenuItem == "Edit");
    }

    private void HP() {
        UI.System.Q<SliderInt>("e_CurrentHPSlider").RegisterValueChangedCallback((evt) => {
            UI.System.Q<Label>("e_CurrentHP").text = evt.newValue.ToString();
        });
        UI.System.Q<SliderInt>("e_CurrentHPSlider").RegisterCallback<MouseEnterEvent>((evt) => {
            HPOld = UI.System.Q<SliderInt>("e_CurrentHPSlider").value;
        });
        UI.System.Q<SliderInt>("e_CurrentHPSlider").RegisterCallback<MouseLeaveEvent>((evt) => {
            int HPNew = UI.System.Q<SliderInt>("e_CurrentHPSlider").value;
            int HPDiff = -(HPOld - HPNew);
            if (HPDiff != 0) {
                Player.Self().CmdRequestTokenDataSetValue(Data, "CurrentHP", HPNew);
            }
        });
    }

    private void Vigor() {
        UI.System.Q<SliderInt>("e_VigorSlider").RegisterValueChangedCallback((evt) => {
            UI.System.Q<Label>("e_Vigor").text = evt.newValue.ToString();
        });
        UI.System.Q<SliderInt>("e_VigorSlider").RegisterCallback<MouseEnterEvent>((evt) => {
            HPOld = UI.System.Q<SliderInt>("e_VigorSlider").value;
        });
        UI.System.Q<SliderInt>("e_VigorSlider").RegisterCallback<MouseLeaveEvent>((evt) => {
            int HPNew = UI.System.Q<SliderInt>("e_VigorSlider").value;
            int HPDiff = -(HPOld - HPNew);
            if (HPDiff != 0) {
                Player.Self().CmdRequestTokenDataSetValue(Data, "Vigor", HPNew);
            }
        });        
    }
    private void Wounds() {
        UI.System.Q<NumberNudger>("e_Wounds").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Wounds", Math.Clamp(evt, 0, 4));
        });
    }

    private void Resolve() {
        UI.System.Q<NumberNudger>("e_Resolve").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Resolve", evt);
        });
        UI.System.Q<NumberNudger>("e_PartyResolve").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestGameDataSetValue("PartyResolve", evt);
            // Player.Self().CmdRequestTokenDataSetValue(Data, "PartyResolve", evt);
        });        
    }

    private void ClassFeatures() {
        UI.System.Q<NumberNudger>("e_Aether").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Aether", evt);
        });
        UI.System.Q<NumberNudger>("e_Vigilance").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Vigilance", evt);
        }); 
        UI.System.Q<NumberNudger>("e_Blessings").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Blessings", evt);
        });
        UI.System.Q<Toggle>("e_StackedDie").RegisterValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Status", "Stacked Die|pos");
        });        
        UI.System.Q<DropdownField>("e_Stance").RegisterValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(Data, "Stance", evt.newValue);
        });
    }

    private void Status() {
        UI.System.Q<Button>("ToggleBuffButton").RegisterCallback<ClickEvent>((evt) => {
            string status = UI.System.Q<DropdownField>("e_BuffDropdown").value;
            Player.Self().CmdRequestTokenDataSetValue(Data, "Status", $"{status}|pos");
        });

        UI.System.Q<Button>("ToggleDebuffButton").RegisterCallback<ClickEvent>((evt) => {
            string status = UI.System.Q<DropdownField>("e_DebuffDropdown").value;
            Player.Self().CmdRequestTokenDataSetValue(Data, "Status", $"{status}|neg");
        });

        UI.System.Q<TextField>("e_Marked").RegisterCallback<BlurEvent>((evt) => {
            string s = UI.System.Q<TextField>("e_Marked").value;
            Player.Self().CmdRequestTokenDataSetValue(Data, "Marked", s);
        });

        UI.System.Q<TextField>("e_Hatred").RegisterCallback<BlurEvent>((evt) => {
            string s = UI.System.Q<TextField>("e_Hatred").value;
            Player.Self().CmdRequestTokenDataSetValue(Data, "Hatred", s);
        });

    }

    public static void Show(TokenData data) {
        SetPosition();
        CloseFoldouts();
        Data = data as Icon_v1_5TokenData;
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
