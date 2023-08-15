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

    private void CreateToken(ClickEvent evt) {
        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = CopyLocalImage(graphicField.value);
        string json = OnlineTokenDataRaw.ToJson(nameField.value, graphic);
        if (Player.IsOnline()) {
            Player.Self().CmdSpawnTokenData(json, null);
        }
        else {
            GameObject tokenObj = new();
            tokenObj.tag = "OfflineData";
            tokenObj.transform.position = new Vector3(3, .25f, 3);
            OfflineTokenData data = tokenObj.AddComponent<OfflineTokenData>();
            data.Json = json;
        }

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

    private Texture2D CopyLocalImage(string filename) {
        Texture2D graphic = TextureSender.LoadImageFromFile(filename, false);
        string hash = TextureSender.GetTextureHash(graphic);
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string remotefilepath = path + "/remote-tokens/" + hash + ".png";
        byte[] pngData = graphic.EncodeToPNG();
        File.WriteAllBytes(remotefilepath, pngData);
        return graphic;
    }
}
