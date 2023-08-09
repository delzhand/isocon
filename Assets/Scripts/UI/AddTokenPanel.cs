using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTokenPanel : MonoBehaviour
{
    void Awake() {
        UI.System.Q<Button>("AddTokenButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("AddTokenPanel");
        });

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        UI.System.Q<Label>("DataPath").text = "Graphics for tokens should be placed in " + path + "/tokens/";

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
