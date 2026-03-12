using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class EnvironmentalActorType : ActorType
{
    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        ActorTypeRegistry.RegisterSystem("Environmental");
        ActorTypeRegistry.RegisterInterfaceCallback("Environmental", DeserializeAsInterface);
        ActorTypeRegistry.RegisterSimpleCallback("Environmental|AddActorModal", AddActorModal);
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
        Modal.AddMarkup("Description", "Environmental tokens have no stats but are useful for interactive or indestructible objects.");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", ActorType.ShapeOptions());
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "NameField", "ShapeField", "ColorField"));
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
            Type = "Environmental",
            Name = name,
            Shape = shape,
            Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };
        AddToken.FinalizeToken(t.Serialize());
    }
    #endregion

    public override string Label()
    {
        return Name;
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
    }

    public override void Command(string command, ActorData tokenData)
    {
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }
    }
}
