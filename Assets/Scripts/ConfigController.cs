using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        setup();
        UI.SetScale("UICanvas/ModeUI", PlayerPrefs.GetFloat("UIScale", 1f));
        UI.SetScale("WorldCanvas/TokenUI", PlayerPrefs.GetFloat("InfoScale", 1f));
        if (PlayerPrefs.GetInt("ShowHelp", 1) == 0) {
            UI.DisableHelp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void setup() {
        UI.System.Q<DropdownField>("UIScaleDropdown").value = PlayerPrefs.GetFloat("UIScale", 1f).ToString();
        UI.System.Q<DropdownField>("UIScaleDropdown").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetFloat("UIScale", float.Parse(evt.newValue));
            UI.SetScale("UICanvas/ModeUI", float.Parse(evt.newValue));
        });
        UI.AttachHelp(UI.System, "UIScaleDropdown", "Make the general UI larger or smaller.");

        UI.System.Q<Slider>("InfoScaleSlider").value = PlayerPrefs.GetFloat("InfoScale", 1f);
        UI.System.Q<Slider>("InfoScaleSlider").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetFloat("InfoScale", evt.newValue);
            UI.SetScale("WorldCanvas/TokenUI", evt.newValue);
        });
        UI.AttachHelp(UI.System, "InfoScaleSlider", "Make the battle information larger or smaller.");

        UI.System.Q<DropdownField>("VersionField").value = PlayerPrefs.GetString("IconVersion", "1.5");
        UI.System.Q<DropdownField>("VersionField").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetString("IconVersion", evt.newValue);
        });
        UI.AttachHelp(UI.System, "VersionField", "Select which version of the ICON ruleset to use");

        UI.System.Q<Toggle>("HelpBarToggle").RegisterValueChangedCallback<bool>((evt) => {
            if (evt.newValue) {
                UI.EnableHelp();
            }
            else {
                UI.DisableHelp();
            }
        });

    }
}
