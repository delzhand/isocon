using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine;
using ShunUI;

public class AddActorModal
{
    public static void Open()
    {
        Actor.Deselect();

        Player.Self().SetOp("Adding Actor");

        Modal2.SetCurrentDialog("ShunDialog1");
        var contents = Modal2.Contents("ShunDialog1");
        contents.Clear();

        Modal2.AddDialogHeader("Add Actor");

        var token = Modal2.AddTokenField("Token", "Token");

        var actorType = Modal2.AddInlineComboboxField("ActorType", "Actor Type", null, ActorTypeRegistry.GetAllSystems());
        actorType.Q<ShunCombobox>().OnSelect += () =>
        {
            string type = contents.Q<ShunCombobox>("ActorType").selectedValue;
            ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        };

        var typeContainer = new ShunContainer();
        typeContainer.name = "ActorTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        contents.Add(typeContainer);

        var footer = Modal2.AddDialogFooter();
        Modal2.Open();
        Modal2.AddCloseAction(() =>
        {
            Player.Self().ClearOp();
        });
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
        Modal2.Dialog("ShunDialog1").Close();
        UI.ToggleActiveClass("BottomBar", true);
    }

    public static void CloseAddToken()
    {
        Player.Self().ClearOp();
        StateManager.PopState();
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
