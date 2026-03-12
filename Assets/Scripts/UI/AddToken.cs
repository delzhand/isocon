using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine;

public class AddToken
{
    public static void OpenModal(ClickEvent evt)
    {
        Player.Self().SetOp("Adding a Token");
        Actor.Deselect();
        Modal.Reset("Add Actor");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddDropdownField("ActorType", "Actor Type", "Basic", ActorTypeRegistry.GetAllSystems().ToArray(), (evt) =>
        {
            VisualElement v = UI.Modal.Q("Contents").Q("TypeData_0");
            if (v != null)
            {
                v.Clear();
                Modal.ResetPreferredButtons();
            }

            string type = UI.Modal.Q<DropdownField>("ActorType").value;
            ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        });
        Modal.AddColumns("TypeData", 1);
        ActorTypeRegistry.DoCallback($"Basic|AddActorModal");
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
        Player.Self().CmdCreateToken(json);
        Player.Self().ClearOp();
        Modal.Close();

        if (!UI.System.Q("BottomBar").ClassListContains("active"))
        {
            UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), true);
        }
    }

    public static void CancelAddToken(ClickEvent evt)
    {
        Player.Self().ClearOp();
        StateManager.Find().ChangeSubState(new NeutralState());
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
