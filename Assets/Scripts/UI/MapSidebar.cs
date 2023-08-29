using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class MapSidebar : MonoBehaviour
{
    public static string MapFile = "";
    private static List<string> editOps = new List<string>();

    private bool isLoading = false;
    private bool isSaving = false;

    void Awake() {
        UI.System.Q<Button>("MapToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("MapSidebar");
        });  

        Refresh();

        UI.System.Q<Button>("LoadMapButton").RegisterCallback<ClickEvent>((evt) => {
            Refresh();
            isLoading = true;
        });

        UI.System.Q<Button>("NewMapButton").RegisterCallback<ClickEvent>((evt) =>  {
            MapFile = "untitled.json";
            TerrainController.InitializeTerrain(8, 8, 1);
            Toast.Add("New map initialized.");
            UI.ToggleDisplay("MapSidebar", false);
            State.SetCurrentJson();
            Player.Self().CmdRequestMapSync();
        });

        UI.System.Q<Button>("ConfirmLoadButton").RegisterCallback<ClickEvent>((evt) =>  {
            MapFile = UI.System.Q<DropdownField>("MapDropdown").value;
            State.LoadState(MapFile);
            isLoading = false;
            UI.ToggleDisplay("MapSidebar", false);
            State.SetCurrentJson();
            Player.Self().CmdRequestMapSync();
        });


        UI.System.Q<Button>("SaveMapButton").RegisterCallback<ClickEvent>((evt) => {
            isSaving = true;
            UI.System.Q<TextField>("MapNameField").value = MapFile;
        });

        UI.System.Q<Button>("ConfirmSaveButton").RegisterCallback<ClickEvent>((evt) => {
            MapFile = UI.System.Q<TextField>("MapNameField").value;
            State.SaveState(MapFile);
            Toast.Add(MapFile + " saved.");
            isSaving = false;
        });

        UI.System.Q<Button>("CancelSaveButton").RegisterCallback<ClickEvent>((evt) => {
            isSaving = false;
        });

    }

    void Update() {
        UI.ToggleDisplay("SaveMapButton", MapFile.Length > 0);
        UI.ToggleDisplay("MapDefaultButtons", !isLoading && !isSaving);
        UI.ToggleDisplay("MapLoading", isLoading);
        UI.ToggleDisplay("MapSaving", isSaving);

        // UI.System.Q<Label>("DebugOutput").text = GameObject.FindGameObjectsWithTag("Block").Length.ToString();
    }

    public void Refresh() {
        List<string> mapFiles = GetMapFiles();
        if (mapFiles.Count == 0) {
            UI.ToggleDisplay("LoadMapButton", false);
            UI.ToggleDisplay("MapDropdown", false);
        }
        else {
            UI.System.Q<DropdownField>("MapDropdown").choices = mapFiles;
            UI.System.Q<DropdownField>("MapDropdown").value = mapFiles[0];
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
        Player.Self().RpcMapSync(json);
    }

}
