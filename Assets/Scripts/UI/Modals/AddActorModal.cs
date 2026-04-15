using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine;
using ShunUI;
using Unity.VisualScripting;

public class AddActorModal
{
    public static void Open()
    {
        Actor.Deselect();

        Player.Self().SetOp("Adding Actor");

        Modal2.CreateContext("PrimaryDialog");
        var contents = Modal2.Contents("PrimaryDialog");
        contents.Clear();

        Modal2.AddDialogHeader("Add Actor");

        var token = Modal2.AddTokenField("Token", "Token");

        string lastActorType = Preferences.Current.LastActorType;
        var actorType = Modal2.AddInlineComboboxField("ActorType", "Actor Type", lastActorType, ActorTypeRegistry.GetAllSystems());
        actorType.Q<ShunCombobox>().OnSelect += () =>
        {
            string type = contents.Q<ShunCombobox>("ActorType").selectedValue;
            Preferences.Current.LastActorType = type;
            Preferences.Save();
            ActorTypeRegistry.DoCallback($"{type}|AddActorModal");
        };

        var typeContainer = new ShunContainer();
        typeContainer.name = "ActorTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        contents.Add(typeContainer);

        var footer = Modal2.AddDialogFooter(cancelAction: () =>
        {
            Modal2.Close("PrimaryDialog");
        });
        Modal2.Open("Add Actor");
        Modal2.AddCloseAction(() =>
        {
            Player.Self().ClearOp();
        });

        if (lastActorType.Length > 0)
        {
            ActorTypeRegistry.DoCallback($"{lastActorType}|AddActorModal");
        }
    }

    public static void FinalizeToken(string json)
    {
        Player.Self().CmdCreateActor(json);
        Modal2.Dialog("PrimaryDialog").Close();
        UI.ToggleActiveClass("BottomBar", true);
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
