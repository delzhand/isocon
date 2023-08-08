using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class MapSidebar : MonoBehaviour
{
    public static string MapFile;

    void Awake() {
        UI.System.Q<Button>("MapToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("MapSidebar");
        });  

        List<string> mapFiles = GetMapFiles();
        if (mapFiles.Count == 0) {
            UI.ToggleDisplay("LoadMapButton", false);
            UI.ToggleDisplay("MapField", false);
        }
        else {
            UI.System.Q<DropdownField>("MapField").choices = mapFiles;
        }

        UI.System.Q<Button>("LoadMapButton").RegisterCallback<ClickEvent>((evt) => {
            MapFile = UI.System.Q<DropdownField>("MapField").value;
            LoadMap();
            Debug.Log("load map " + MapFile);
        });

        UI.System.Q<Button>("NewMapButton").RegisterCallback<ClickEvent>((evt) =>  {
            MapFile = UI.System.Q<TextField>("NewMapField").value;
            Debug.Log("new map " + MapFile);
        });

        UI.System.Q<Button>("SaveMapButton").RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("save map " + MapFile);
        });
    }

    void Update() {
        if (!Player.IsGM()) {
            UI.ToggleDisplay("MapToggle", false);
            UI.ToggleDisplay("MapSidebar", false);
            return;
        }
        else {
            UI.ToggleDisplay("MapToggle", true);
        }        
    }

    private List<string> GetMapFiles() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        
        if (!Directory.Exists(path + "/maps")) {
            Directory.CreateDirectory(path + "/maps");
        }

        List<string> mapFiles = new List<string>{};
        DirectoryInfo info = new DirectoryInfo(path + "/maps/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                mapFiles.Add(fileInfo[i].Name);
            }        
        }
        return mapFiles;
    }

    private void LoadMap() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string json = File.ReadAllText(path + "/maps/" + MapFile);
        Player.Self().RpcDrawMap(json);
    }
}
