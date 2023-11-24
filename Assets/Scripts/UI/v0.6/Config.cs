using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(ClickEvent evt) {

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        TextField dataPathField = new TextField();
        dataPathField.label = "Data Path";
        dataPathField.value = path;
        dataPathField.name = "DataPath";
        dataPathField.AddToClassList("no-margin");
        dataPathField.RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("DataFolder", evt.newValue);
        });

        string uiScale = PlayerPrefs.GetString("UIScale", "100%");
        DropdownField uiScaleField = new DropdownField();
        uiScaleField.label = "UI Scale";
        uiScaleField.name = "UIScaleField";
        uiScaleField.value = uiScale;
        uiScaleField.focusable = false;
        uiScaleField.AddToClassList("no-margin");
        uiScaleField.choices = new List<string>();
        for (int i = 75; i <= 250; i += 25) {
            uiScaleField.choices.Add(i + "%");
        }
        uiScaleField.RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("UIScale", evt.newValue);
            float value = float.Parse(evt.newValue.Replace("%", "")) / 100f;
            GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = value;
        });

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(CloseModal);
        confirm.AddToClassList("preferred");

        Modal.Reset("Open Map");
        Modal.AddContents(dataPathField);
        Modal.AddContents(uiScaleField);
        Modal.AddButton(confirm);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

}
