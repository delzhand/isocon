using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigSidebar : MonoBehaviour
{
    void Awake() {
        UI.System.Q<Button>("ConfigToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("ConfigSidebar");
        });

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        UI.System.Q<TextField>("DataPathField").value = path;
        UI.System.Q<TextField>("DataPathField").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("DataFolder", evt.newValue);
        });

        string uiScale = PlayerPrefs.GetString("UIScale", "100%");
        SetScale(uiScale);
        UI.System.Q<DropdownField>("UIScaleField").value = uiScale; 
        UI.System.Q<DropdownField>("UIScaleField").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("UIScale", evt.newValue);
            SetScale(evt.newValue);
        });

        string system = PlayerPrefs.GetString("System", "Generic");
        GameSystem.Set(system);
        UI.System.Q<DropdownField>("SystemField").value = system; 
        UI.System.Q<DropdownField>("SystemField").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("System", evt.newValue);
            GameSystem.Set(evt.newValue);
        });
    }

    private void SetScale(string sValue) {
        float value = float.Parse(sValue.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = value;
    }
}
