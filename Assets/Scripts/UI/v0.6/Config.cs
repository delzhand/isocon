using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(ClickEvent evt) {
        Modal.Reset("Configuration");

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        Modal.AddTextField("DataPath", "Data Path", path, (evt) => {
            PlayerPrefs.SetString("DataFolder", evt.newValue);
        });

        string uiScale = PlayerPrefs.GetString("UIScale", "100%");
        List<string> scaleOptions = new();
        for (int i = 75; i <= 250; i += 25) {
            scaleOptions.Add(i + "%");
        }
        Modal.AddDropdownField("UIScaleField", "UI Scale", uiScale, scaleOptions.ToArray(), (evt) => {
            PlayerPrefs.SetString("UIScale", evt.newValue);
            float value = float.Parse(evt.newValue.Replace("%", "")) / 100f;
            GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = value;
        });
        
        float tokenScale = PlayerPrefs.GetFloat("TokenScale", 1f);
        Modal.AddFloatSlider("TokenScaleField", "Token Scale", tokenScale, 2f, .5f, (evt) => {
            PlayerPrefs.SetFloat("TokenScale", evt.newValue);
        });

        string tokenOutline = PlayerPrefs.GetString("TokenOutline", "White");
        Modal.AddDropdownField("TokenOutlineField", "Token Outline", tokenOutline, StringUtility.Arr("White", "Black", "None"), (evt) => {
            PlayerPrefs.SetString("TokenOutline", evt.newValue);
        });

        Modal.AddPreferredButton("Confirm", CloseModal);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

}
