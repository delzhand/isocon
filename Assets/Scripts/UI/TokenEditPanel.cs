using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenEditPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UI.System.Q<SliderInt>("e_CurrentHPSlider").RegisterValueChangedCallback((evt) => {
            Debug.Log(evt.newValue);
        });
        UI.System.Q<SliderInt>("e_CurrentHPSlider").RegisterCallback<PointerUpEvent>((evt) => {
            Debug.Log(UI.System.Q<SliderInt>("e_CurrentHPSlider").value);
        });
    }

    // Update is called once per frame
    void Update()
    {
        UI.ToggleDisplay("Icon1_5EditPanel", UnitMenu.ActiveMenuItem == "Edit");
    }

    public static void Show(TokenData data) {
        CloseFoldouts();
    }

    private static void CloseFoldouts() {
        UI.System.Q("Icon1_5EditPanel").Query(null, "unity-foldout").ForEach(item => {
            (item as Foldout).value = false;
        });        
    }
}
