using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class AddToken
{
    public static void OpenModal(ClickEvent evt)
    {
        Player.Self().SetOp("Adding a Token");
        Token.DeselectAll();
        Modal.Reset("Add Token");
        string[] imageOptions = GetImageOptions();
        if (imageOptions.Length > 0)
        {
            Modal.AddSearchField("ImageSearchField", "Add Token", "", GetImageOptions());
            GameSystem.Current().AddTokenModal();
            Modal.AddPreferredButton("Confirm", ConfirmAddToken);
        }
        else
        {
            string path = Preferences.Current.DataPath;
            Modal.AddLabel($"No images were found. Token images must be added to {path}/tokens (this can be changed in configuration).", "error-message");
        }
        Modal.AddButton("Cancel", (evt) =>
        {
            Modal.Close();
        });
        Modal.AddCloseCallback(CancelAddToken);
    }

    private static void ConfirmAddToken(ClickEvent evt)
    {
        GameSystem.Current().CreateToken();
        Modal.Close();
        Player.Self().ClearOp();
    }

    private static void CancelAddToken(ClickEvent evt)
    {
        Player.Self().ClearOp();
    }

    private static bool FileExists(string filename)
    {
        string path = Preferences.Current.DataPath;
        string fullPath = path + "/tokens/" + filename;
        return File.Exists(fullPath);
    }

    private static void GetFilesRecursively(string basePath, string relativePath, List<string> fileList)
    {
        string[] files = Directory.GetFiles(basePath + relativePath);
        foreach (string file in files)
        {
            if (file.EndsWith(".png"))
            {
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
        string path = Preferences.Current.DataPath;
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
