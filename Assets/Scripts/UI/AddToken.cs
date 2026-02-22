using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;

public class AddToken
{
    public static void OpenModal(ClickEvent evt)
    {
        Player.Self().SetOp("Adding a Token");
        Token.DeselectAll();
        Modal.Reset("Add Token");
        Modal.AddDropdownField("TokenType", "Token Type", "Basic", SystemTokenRegistry.GetAllSystems().ToArray(), (evt) =>
        {
            VisualElement v = UI.Modal.Q("Contents").Q("TypeData_0");
            if (v != null)
            {
                v.Clear();
                Modal.ResetPreferredButtons();
            }

            string type = UI.Modal.Q<DropdownField>("TokenType").value;
            SystemTokenRegistry.DoCallback($"{type}|AddTokenModal");
        });
        Modal.AddColumns("TypeData", 1);
        SystemTokenRegistry.DoCallback($"Basic|AddTokenModal");
        Modal.AddCloseCallback(CancelAddToken);
    }

    public static void OrderFields(string[] fieldNames)
    {
        foreach (string f in fieldNames)
        {
            Modal.MoveToColumn("TypeData_0", f);
        }
    }

    public static void FinalizeToken(string json)
    {
        Player.Self().CmdCreateSystemToken(json);
        Player.Self().ClearOp();
        Modal.Close();
    }

    public static void CancelAddToken(ClickEvent evt)
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
