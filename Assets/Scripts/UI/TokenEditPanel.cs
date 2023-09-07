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
        HP();
        Vigor();
        Wounds();
        Resolve();
        ClassFeatures();
    }

    void Update() {
        UI.ToggleDisplay("Icon1_5EditPanel", UnitMenu.ActiveMenuItem == "Edit");
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
        // UI.System.Q<NumberNudger>("e_PartyResolve").AddValueChangedCallback((evt) => {
        //     Player.Self().CmdRequestTokenDataSetValue(Data, "PartyResolve", evt);
        // });        
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
    }

    public static void Show(TokenData data) {
        CloseFoldouts();
        Data = data as Icon_v1_5TokenData;
        SyncValues();
    }

    public static void SyncValues() {
        if (Data != null) {
            UI.System.Q<Label>("e_CurrentHP").text = $"{Data.CurrentHP}";
            UI.System.Q<SliderInt>("e_CurrentHPSlider").value = Data.CurrentHP;
            UI.System.Q<SliderInt>("e_CurrentHPSlider").highValue = Data.MaxHP;

            UI.System.Q<Label>("e_Vigor").text = $"{Data.Vigor}";
            UI.System.Q<SliderInt>("e_VigorSlider").value = Data.Vigor;
            UI.System.Q<SliderInt>("e_VigorSlider").highValue = Data.MaxHP;
        }        
    }

    private static void CloseFoldouts() {
        UI.System.Q("Icon1_5EditPanel").Query(null, "unity-foldout").ForEach(item => {
            (item as Foldout).value = false;
        });        
    }
}
