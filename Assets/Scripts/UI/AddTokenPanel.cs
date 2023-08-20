using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AddTokenPanel : MonoBehaviour
{
    void Awake() {
        UI.SetBlocking(UI.System, new string[]{"AddTokenPanel"});

        UI.System.Q<Button>("AddTokenButton").RegisterCallback<ClickEvent>((evt) => {
            UpdateGraphicsList();
            UI.ToggleDisplay("AddTokenPanel");
        });

        UI.System.Q("AddTokenCreateButton").RegisterCallback<ClickEvent>(CreateToken);

        UI.System.Q("AddTokenSaveCreateButton").RegisterCallback<ClickEvent>(SaveCreateToken);
        UI.System.Q("AddTokenCancelButton").RegisterCallback<ClickEvent>(ClosePanel);

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        UI.System.Q<Label>("DataPath").text = "Graphics for tokens should be placed in " + path + "/tokens/";

    }

    void Update() {
        UI.OverDropdown();
    }

    private void CreateToken(ClickEvent evt) {
        string json = GameSystem.Current().GetTokenData();
        Player.CreateTokenData(json, new Vector3(3, .25f, 3));
        UI.ToggleDisplay("AddTokenPanel", false);
    }

    private void SaveCreateToken(ClickEvent evt) {
        throw new NotImplementedException();
    }

    private void ClosePanel(ClickEvent evt) {
        UI.ToggleDisplay("AddTokenPanel", false);
    }

    private void UpdateGraphicsList() {
        List<string> customGraphics = GetCustomGraphics();
        UI.System.Q<DropdownField>("GraphicDropdown").choices = customGraphics;
        UI.System.Q<DropdownField>("GraphicDropdown").value = customGraphics[0];
    }

    private List<string> GetCustomGraphics() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        List<string> graphics = new() { };
        DirectoryInfo info = new(path + "/tokens/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                graphics.Add(fileInfo[i].Name);
                if (i == 0) {
                    UI.System.Q<DropdownField>("GraphicDropdown").value = fileInfo[i].Name;
                }
            }        
        }
        return graphics;
    }
}
