using System;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class EnvironmentalActorType : ActorType
{
    private readonly static string TypeName = "Environmental";

    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        ActorTypeRegistry.RegisterSystem($"{TypeName}");
        ActorTypeRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        ActorTypeRegistry.RegisterSimpleCallback($"{TypeName}|AddActorModal", AddActorModal);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static IActorType DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<EnvironmentalActorType>(json);
    }
    #endregion

    #region Stats
    public string Name;
    #endregion

    #region Creation
    public static void AddActorModal()
    {
        var contents = Modal2.Contents("ShunDialog1");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();


        var name = Modal2.AddInlineTextField("Name", "Actor Name", "Actor");
        Modal2.MoveToContainer(name, typeContainer);
        var shape = Modal2.AddInlineSelectField("Shape", "Shape", "Square 1x1", ActorType.ShapeOptions().ToList<string>());
        Modal2.MoveToContainer(shape, typeContainer);
        var color = Modal2.AddInlineComboboxField("Color", "Color", "Black", ColorUtility.CommonColors().ToList<string>());
        Modal2.MoveToContainer(color, typeContainer);

        var create = new ShunDialogClose();
        create.name = "CreateActor";
        create.text = "Create Actor";
        create.SetVariant(ButtonVariant.Primary);
        create.clicked += () => CreateClicked();
        contents.Q(className: "shun-dialog__footer").Add(create);
    }

    private static void CreateClicked()
    {
        string token = Modal2.GetComboboxFieldValue("ShunDialog1", "Token");
        if (token == null)
        {
            Toast.AddError("A token has not been selected");
            return;
        }
        string name = Modal2.GetTextFieldValue("ShunDialog1", "Name");
        string shape = Modal2.GetSelectFieldValue("ShunDialog1", "Shape");
        string color = Modal2.GetComboboxFieldValue("ShunDialog1", "Color");
        EnvironmentalActorType t = new()
        {
            Type = TypeName,
            Name = name,
        };
        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibrary.GetToken(token);
        a.Color = ColorUtility.GetCommonColor(color);
        a.Shape = shape;
        a.Position = Vector3.zero;
        a.Placed = false;
        a.ActorType = JsonUtility.ToJson(t);
        a.ActorTypeId = TypeName;
        string json = JsonUtility.ToJson(a);
        AddActor.FinalizeToken(json);
    }
    #endregion

    public override string Label()
    {
        return Name;
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);
    }

    public override void Command(string command, ActorData tokenData)
    {
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }
    }
}
