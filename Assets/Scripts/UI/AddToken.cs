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

        var dialog = Modal2.SetCurrentDialog("ShunDialog1");
        Modal2.SetCloseAction(() => CloseAddToken());
        var dialogContent = Modal2.Contents("ShunDialog1");
        dialogContent.Clear();

        Modal2.SetCloseAction(CloseAddToken);

        Modal2.AddDialogHeader("Add Actor");

        var token = Modal2.AddTokenField("Token", "Token");

        var actorType = Modal2.AddInlineComboboxField("ActorType", "Actor Type", null, ActorTypeRegistry.GetAllSystems());
        actorType.Q<ShunCombobox>().OnSelect += () =>
        {
            string type = dialogContent.Q<ShunCombobox>("ActorType").selectedValue;
            ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        };

        var typeContainer = new ShunContainer();
        typeContainer.name = "ActorTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        dialogContent.Add(typeContainer);

        var footer = Modal2.AddDialogFooter(() => dialog.Close());
        dialog.Open();
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
        Player.Self().CmdCreateActor(json);
        if (!UI.System.Q("BottomBar").ClassListContains("active"))
        {
            UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), true);
        }

        Modal2.Dialog("ShunDialog1").Close();
    }

    public static void CloseAddToken()
    {
        Player.Self().ClearOp();
        StateManager.Find().ChangeSubState(new NeutralState());
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
}
