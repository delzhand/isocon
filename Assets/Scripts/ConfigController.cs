using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UI.SetScale("UICanvas/SystemUI", PlayerPrefs.GetFloat("UIScale", 1f));
        registerCallbacks();
        // UI.SetScale("WorldCanvas/TokenUI", PlayerPrefs.GetFloat("InfoScale", 1f));
        // if (PlayerPrefs.GetInt("ShowHelp", 1) == 0) {
        //     UI.DisableHelp();
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void registerCallbacks() {
        UI.System.Q<DropdownField>("UIScaleDropdown").value = PlayerPrefs.GetFloat("UIScale", 1f).ToString();
        UI.System.Q<DropdownField>("UIScaleDropdown").RegisterValueChangedCallback((evt) => {
            float value = float.Parse(evt.newValue.Replace("%", ""))/100;
            PlayerPrefs.SetFloat("UIScale", value);
            UI.SetScale("SystemUI", value);
        });

        UI.System.Q<DropdownField>("IconVerDropdown").value = PlayerPrefs.GetString("IconVersion", "1.5");
        UI.System.Q<DropdownField>("IconVerDropdown").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetString("IconVersion", evt.newValue);
        });
    }
}
