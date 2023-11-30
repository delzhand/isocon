using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class AddToken
{
    public static void OpenModal(ClickEvent evt) {
        Modal.Reset("Add Token");

        VisualElement imageSearchField = SearchField.Create(GetImageOptions(), "Add Token");
        imageSearchField.name = "ImageSearchField";
        Modal.AddContents(imageSearchField);

        GameSystem.Current().AddTokenModal();

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmAddToken);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.AddButton(confirm);
        Modal.AddButton(cancel);
    }   

    private static void ConfirmAddToken(ClickEvent evt) {
        string json = GameSystem.Current().GetTokenDataRawJson();
        Debug.Log(json);
        Player.Self().CmdCreateTokenData(json);
        Modal.Close();
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

    private static bool FileExists(string filename) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/tokens/" + filename;
        return File.Exists(fullPath);
    }

    private static void GetFilesRecursively(string basePath, string relativePath, List<string> fileList)
    {
        string[] files = Directory.GetFiles(basePath + relativePath);
        foreach (string file in files)
        {
            if (file.EndsWith(".png")) {
                fileList.Add(relativePath + "/" + Path.GetFileName(file));
            }
        }

        string[] directories = Directory.GetDirectories(basePath + relativePath);
        foreach (string directory in directories)
        {
            GetFilesRecursively(basePath, relativePath + "/" + Path.GetFileName(directory), fileList);
        }
    }

    public static string[] GetImageOptions()
    {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        List<string> mapFiles = new List<string>();

        if (!Directory.Exists(path + "/tokens"))
        {
            Directory.CreateDirectory(path + "/tokens");
        }

        GetFilesRecursively(path, "/tokens", mapFiles);

        // Remove "/tokens" from each string in the list
        for (int i = 0; i < mapFiles.Count; i++)
        {
            mapFiles[i] = mapFiles[i].Replace("/tokens/", "");
        }

        return mapFiles.ToArray();
    }
}
