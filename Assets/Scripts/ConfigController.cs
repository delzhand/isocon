using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigController : MonoBehaviour
{
    private VisualElement root;

    // Start is called before the first frame update
    void Start()
    {
        root = GameObject.Find("ModeUI").GetComponent<UIDocument>().rootVisualElement;
        setup();

        UI.SetScale("UICanvas/ModeUI", PlayerPrefs.GetFloat( "UIScale"));
        UI.SetScale("WorldCanvas/TokenUI", PlayerPrefs.GetFloat( "InfoScale"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void setup() {
        root.Q<DropdownField>("UIScaleDropdown").value = PlayerPrefs.GetFloat("UIScale", 1f).ToString();
        root.Q<DropdownField>("UIScaleDropdown").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetFloat("UIScale", float.Parse(evt.newValue));
            UI.SetScale("UICanvas/ModeUI", float.Parse(evt.newValue));
        });
        UI.AttachHelp(root, "UIScaleDropdown", "Make the general UI larger or smaller.");

        root.Q<Slider>("InfoScaleSlider").value = PlayerPrefs.GetFloat("InfoScale", 1f);
        root.Q<Slider>("InfoScaleSlider").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetFloat("InfoScale", evt.newValue);
            UI.SetScale("WorldCanvas/TokenUI", evt.newValue);
        });
        UI.AttachHelp(root, "InfoScaleSlider", "Make the battle information larger or smaller.");

        root.Q<DropdownField>("VersionField").value = PlayerPrefs.GetString("IconVersion", "1.5");
        root.Q<DropdownField>("VersionField").RegisterValueChangedCallback((evt) => {
            PlayerPrefs.SetString("IconVersion", evt.newValue);
        });
        UI.AttachHelp(root, "VersionField", "Select which version of the ICON ruleset to use");

    }
}
