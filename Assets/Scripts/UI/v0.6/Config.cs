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

        Modal.AddTextField("Console", "Console", "");

        Button execute = new Button();
        execute.text = "Execute Console Command";
        execute.RegisterCallback<ClickEvent>(ConsoleExecute);
        Modal.AddButton(execute);

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(CloseModal);
        confirm.AddToClassList("preferred");

        Modal.AddButton(confirm);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

    private static void ConsoleExecute(ClickEvent evt) {
        string command = Modal.Find().Q<TextField>("Console").value;
        if (command == "Ada") {
            string json = "{\"Name\":\"Ada\",\"GraphicHash\":\"df6ee698a739576676d5f99c113a61fecdd1f2a66cc3fd7fc1b8ac21f3ba4067\",\"Size\":1,\"Class\":\"Stalwart\",\"Job\":\"Bastion\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
        }
        if (command.StartsWith("SelectedToken|")) {
            command = command.Replace("SelectedToken|", "");
            TokenData data = Token.GetSelectedData().GetComponent<TokenData>();
            Player.Self().CmdRequestTokenDataSetValue(data, command);
            Toast.Add("Console command executed on selected token.");
        }
    }

}
