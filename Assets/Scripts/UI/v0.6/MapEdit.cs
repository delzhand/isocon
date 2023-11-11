using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEdit
{
    public static bool Editing = false;
    private static string CurrentFile = "";

    public static void Setup()
    {
        UI.System.Q("FloatingControls").Q("EditMap").RegisterCallback<ClickEvent>(ToggleEditMode);

        VisualElement root = UI.System.Q("ToolsPanel");
        UI.ToggleDisplay(root, false);
        root.Q<Button>("Open").RegisterCallback<ClickEvent>(OpenOpenModal);
        root.Q<Button>("Save").RegisterCallback<ClickEvent>(OpenSaveModal);
    }

    private static void ToggleEditMode(ClickEvent evt) {
        Editing = !Editing;
        UI.ToggleDisplay("ToolsPanel", Editing);
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

        VisualElement searchField = SearchField.Create(GetAllMapFiles());
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
        Toast.Add($"File written to {fullPath.Replace("\\", "/")}");
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
}
