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

    private bool HasGraphics = false;

    void Awake() {
        UI.SetBlocking(UI.System, new string[]{"AddTokenPanel"});

        UI.System.Q<Button>("AddTokenButton").RegisterCallback<ClickEvent>((evt) => {
            UpdateGraphicsList();
            UI.ToggleDisplay("AddTokenPanel");
        });

        UI.System.Q("AddTokenCreateButton").RegisterCallback<ClickEvent>(CreateToken);

        UI.System.Q("AddTokenSaveCreateButton").RegisterCallback<ClickEvent>(SaveCreateToken);
        UI.System.Q("AddTokenCancelButton").RegisterCallback<ClickEvent>(ClosePanel);

        UpdatePathHelp();

        UI.SetBlocking(UI.System, new string[]{"DebugPanel"});
        UI.System.Q<Button>("DebugButton").RegisterCallback<ClickEvent>((evt) => {
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                g.GetComponent<TokenData>().OnField = true;
                g.GetComponent<Icon_v1_5TokenData>().CurrentHP -= 1;
            }
        });
    }

    void Update() {
        UI.ToggleDisplay("AddTokenCreateButton", HasGraphics);
    }

    public static void UpdatePathHelp() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        UI.System.Q<Label>("DataPath").text = "Graphics for tokens should be placed in " + path + "\tokens";
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
        if (customGraphics.Count > 0) {
            HasGraphics = true;
            UI.System.Q<DropdownField>("GraphicDropdown").choices = customGraphics;
            UI.System.Q<DropdownField>("GraphicDropdown").value = customGraphics[0];
        }
        else {
            HasGraphics = false;
        }
    }

    private List<string> GetCustomGraphics() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        if (!Directory.Exists(path + "/tokens")) {
            Directory.CreateDirectory(path + "/tokens");
        }
        if (!Directory.Exists(path + "/remote-tokens")) {
            Directory.CreateDirectory(path + "/remote-tokens");
        }

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
