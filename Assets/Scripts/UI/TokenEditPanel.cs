using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenEditPanel : MonoBehaviour
{
    private static int HPOld;
    private static Icon_v1_5TokenData Data;

    // Start is called before the first frame update
    void Start()
    {
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
                Player.Self().CmdRequestTokenDataChange(Data, "CurrentHP", HPDiff);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        UI.ToggleDisplay("Icon1_5EditPanel", UnitMenu.ActiveMenuItem == "Edit");
    }

    public static void Show(TokenData data) {
        CloseFoldouts();
        Data = data as Icon_v1_5TokenData;
    }

    private static void CloseFoldouts() {
        UI.System.Q("Icon1_5EditPanel").Query(null, "unity-foldout").ForEach(item => {
            (item as Foldout).value = false;
        });        
    }
}
