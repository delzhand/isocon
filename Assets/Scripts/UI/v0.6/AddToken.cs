using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class AddToken
{
    public static void OpenModal(ClickEvent evt) {
        Token.DeselectAll();
        Modal.Reset("Add Token");
        Modal.AddSearchField("ImageSearchField", "Add Token", "", GetImageOptions());
        GameSystem.Current().AddTokenModal();
        Modal.AddPreferredButton("Confirm", ConfirmAddToken);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }   

    private static void ConfirmAddToken(ClickEvent evt) {
        string json = GameSystem.Current().GetTokenDataRawJson();
        Debug.Log(json);
        Player.Self().CmdCreateTokenData(json);
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
