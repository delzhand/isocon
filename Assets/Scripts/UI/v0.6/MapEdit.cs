using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEdit
{
    private static bool MapDirty = false;
    private static string CurrentFile = "";
    private static List<string> EditOps = new List<string>();

    public static void Setup()
    {
        VisualElement root = UI.System.Q("ToolsPanel");
        UI.SetBlocking(UI.System, "ToolsPanel");
        UI.ToggleDisplay(root, false);
        root.Q<Button>("Open").RegisterCallback<ClickEvent>(OpenOpenModal);
        root.Q<Button>("Save").RegisterCallback<ClickEvent>(OpenSaveModal);
        root.Query<Button>(null, "tool-button").ForEach(RegisterButton);
        root.Query<Foldout>(null, "unity-foldout").ForEach(RegisterFoldout);
    }

    public static void ToggleEditMode(ClickEvent evt) {
        if (Cursor.Mode != ClickMode.Editing) {
            UI.ToggleDisplay("ToolsPanel", true);
            Block.DeselectAll();
            Cursor.Mode = ClickMode.Editing;
        }
        else {
            Cursor.Mode = ClickMode.Default;
            UI.ToggleDisplay("ToolsPanel", false);
            State.SetCurrentJson();
            Player.Self().CmdMapSync();
        }
    }

    private static void OpenSaveModal(ClickEvent evt) {
        Modal.Reset("Save Map");

        VisualElement modal = Modal.Find();

        TextField filenameField = new TextField();
        filenameField.name = "Filename";
        filenameField.label = "Filename";
        if (CurrentFile.Length > 0) {
            filenameField.value = CurrentFile;
        }
        filenameField.AddToClassList("no-margin");
        filenameField.style.minWidth = 400;
        modal.Q("Contents").Add(filenameField);

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmMapSave);
        confirm.AddToClassList("preferred");
        modal.Q("Buttons").Add(confirm);

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);
        modal.Q("Buttons").Add(cancel);
    }

    private static void OpenOpenModal(ClickEvent evt) {

        VisualElement searchField = SearchField.Create(GetAllMapFiles(), "Filename");
        searchField.name = "SearchField";

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmMapOpen);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.Reset("Open Map");
        Modal.AddContents(searchField);
        Modal.AddButton(confirm);
        Modal.AddButton(cancel);
    }
    
    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

    private static void ConfirmMapOpen(ClickEvent evt) {
        string value = Modal.Find().Q("SearchField").Q<TextField>("SearchInput").value;
        if (!MapDirty) {
            OpenFile();
            Modal.Close();
        }
        else {
            Modal.DoubleConfirm("Confirm Open", "You have unsaved changes. Discard?", OpenFile);
        }
    }

    private static void OpenFile() {
        string filename = Modal.Find().Q("SearchField").Q<TextField>("SearchInput").value;
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        MapSaver.StegLoad(fullPath);
    }

    private static void ConfirmMapSave(ClickEvent evt) {
        string value = Modal.Find().Q<TextField>("Filename").value;
        if (value.Length == 0) {
            Toast.Add("Not a valid filename.", ToastType.Error);
        }
        else {
            if (!value.EndsWith(".json")) {
                value += ".json";
                Modal.Find().Q<TextField>("Filename").value = value;
            }
            if (FileExists(value)) {
                Modal.DoubleConfirm("Confirm Overwrite", "A file with this name already exists. Overwrite?", WriteFile);
            }
            else {
                Modal.Close();
                WriteFile();
            }
        }
    }

    private static void WriteFile() {
        string filename = Modal.Find().Q<TextField>("Filename").value;
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        MapSaver.StegSave(fullPath);
    }

    private static bool FileExists(string filename) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        return File.Exists(fullPath);
    }

    private static void GetFilesRecursively(string basePath, string relativePath, List<string> fileList)
    {
        string[] files = Directory.GetFiles(basePath + relativePath);
        foreach (string file in files)
        {
            fileList.Add(relativePath + "/" + Path.GetFileName(file));
        }

        string[] directories = Directory.GetDirectories(basePath + relativePath);
        foreach (string directory in directories)
        {
            GetFilesRecursively(basePath, relativePath + "/" + Path.GetFileName(directory), fileList);
        }
    }

    public static string[] GetAllMapFiles()
    {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        List<string> mapFiles = new List<string>();

        if (!Directory.Exists(path + "/maps"))
        {
            Directory.CreateDirectory(path + "/maps");
        }

        GetFilesRecursively(path, "/maps", mapFiles);

        // Remove "/maps" from each string in the list
        for (int i = 0; i < mapFiles.Count; i++)
        {
            mapFiles[i] = mapFiles[i].Replace("/maps/", "");
        }

        return mapFiles.ToArray();
    }

    private static void RegisterButton(Button button) {
        button.clickable.clickedWithEventInfo += ButtonClick;
    }

    private static void ButtonClick(EventBase obj) {
        VisualElement root = UI.System.Q("ToolsPanel");
        root.Query<Button>(null, "tool-button").ForEach(DisableButton);
        Button button = (Button)obj.target;
        button.AddToClassList("active");
        EditOps.Clear();
        EditOps.Add(button.name);
    }

    private static void DisableButton(Button button) {
        button.RemoveFromClassList("active");
    }

    public static List<string> GetOps() {
        return EditOps;
    }

    private static void RegisterFoldout(Foldout foldout) {
        foldout.RegisterCallback<ClickEvent>((evt) => {
            if (foldout.value) {
                VisualElement root = UI.System.Q("ToolsPanel");
                root.Query<Foldout>(null, "unity-foldout").ForEach((otherFoldout) => {
                    if (otherFoldout != foldout) {
                        otherFoldout.value = false;
                    }
                });
            }
        });
    }

    public static string GetMarkerEffect() {
         return UI.System.Q("ToolsPanel").Q("EffectSearch").Q<TextField>("SearchInput").value;
    }
}
