using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class EnvironmentalToken : SystemToken
{
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public override void InitTokenPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("Data").Clear();
        panel.Q("ExtraInfo").Clear();
    }

    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Environmental tokens have no stats but are useful for interactive or indestructible objects.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Hex 1", "Hex 2", "Hex 3"));
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "NameField", "ShapeField", "ColorField"));
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
        EnvironmentalToken t = new()
        {
            System = "Environmental",
            Name = name,
            Shape = shape,
            Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };
        AddToken.FinalizeToken(t.Serialize());
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SystemTokenRegistry.RegisterSystem("Environmental");
        SystemTokenRegistry.RegisterInterfaceCallback("Environmental", DeserializeAsInterface);
        SystemTokenRegistry.RegisterSimpleCallback("Environmental|AddTokenModal", AddTokenModal);
    }

    public static ISystemToken DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<EnvironmentalToken>(json);
    }
}
