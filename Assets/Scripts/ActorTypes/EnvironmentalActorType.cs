using System;
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
        Modal.AddTextField("NameField", "Actor Name", "Actor");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", ActorType.ShapeOptions());
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddPreferredButton("Create Actor", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddActor.OrderFields(StringUtility.CreateArray("NameField", "ShapeField", "ColorField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected.");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string shape = UI.Modal.Q<DropdownField>("ShapeField").value;
        string color = UI.Modal.Q<DropdownField>("ColorField").value;
        EnvironmentalActorType t = new()
        {
            Type = TypeName,
            Name = name,
        };
        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibrary.GetSelectedMeta();
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
