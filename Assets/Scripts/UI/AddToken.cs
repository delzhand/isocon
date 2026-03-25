using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine;
using ShunUI;

public class AddActor
{
    public static void OpenModal(ClickEvent evt)
    {
        Actor.Deselect();

        var dialog = ShunDialogHelper.SetCurrentDialog("ShunDialog1");
        ShunDialogHelper.SetCloseAction(() => CloseAddToken());
        var dialogContent = ShunDialogHelper.Contents("ShunDialog1");
        dialogContent.Clear();

        ShunDialogHelper.SetCloseAction(CloseAddToken);

        ShunDialogHelper.AddDialogHeader("Add Actor");

        var token = ShunDialogHelper.AddTokenField("Token", "Token");

        var actorType = ShunDialogHelper.AddInlineComboboxField("ActorType", "Actor Type", null, ActorTypeRegistry.GetAllSystems());
        actorType.Q<ShunCombobox>().OnSelect += () =>
        {
            string type = dialogContent.Q<ShunCombobox>("ActorType").selectedValue;
            ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        };

        var typeContainer = new ShunContainer();
        typeContainer.name = "ActorTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        dialogContent.Add(typeContainer);

        var footer = ShunDialogHelper.AddDialogFooter(() => dialog.Close());

        // var confirm = new ShunDialogClose();
        // confirm.SetVariant(ButtonVariant.Primary);
        // confirm.text = "Next";
        // confirm.clicked += () =>
        // {
        //     var results = ShunDialogHelper.Results("ShunDialog1");
        //     string type = results.Q<ShunCombobox>("ActorType").selectedValue;
        //     OpenTypeModal(type);
        // };
        // footer.Add(confirm);

        dialog.Open();

        // Player.Self().SetOp("Adding an Actor");
        // Actor.Deselect();
        // Modal.Reset("Add Actor");
        // Modal.AddTokenField("TokenSearchField");
        // Modal.AddDropdownField("ActorType", "Actor Type", "Basic", ActorTypeRegistry.GetAllSystems().ToArray(), (evt) =>
        // {
        //     VisualElement v = UI.Modal.Q("Contents").Q("TypeData_0");
        //     if (v != null)
        //     {
        //         v.Clear();
        //         Modal.ResetPreferredButtons();
        //     }

        //     string type = UI.Modal.Q<DropdownField>("ActorType").value;
        //     ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        // });
        // Modal.AddColumns("TypeData", 1);
        // ActorTypeRegistry.DoCallback($"Basic|AddActorModal");
        // Modal.AddCloseCallback(CancelAddToken);
    }

    // private static void OpenTypeModal(string actorType)
    // {
    //     ShunDialogHelper.Contents.Clear();
    //     ShunDialogHelper.AddDialogHeader($"Add {actorType}");

    // }


    public static void OrderFields(string[] fieldNames)
    {
        foreach (string f in fieldNames)
        {
            Modal.MoveToColumn("TypeData_0", f);
        }
    }

    public static void FinalizeToken(string json)
    {
        Player.Self().CmdCreateActor(json);
        if (!UI.System.Q("BottomBar").ClassListContains("active"))
        {
            UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), true);
        }

        ShunDialogHelper.Dialog("ShunDialog1").Close();
    }

    public static void CloseAddToken()
    {
        Debug.Log("CancelAddTOken");
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
