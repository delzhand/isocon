using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class BasicToken : SystemToken
{
    public int MaxHP;
    public int CurrentHP;

    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public override string GetOverheadAsset()
    {
        return "UITemplates/GameSystem/SimpleOverhead";
    }

    public override MenuItem[] GetTokenMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(placed);

        List<MenuItem> items = new();
        // items.Add(new MenuItem("AddResource", "Add Resource", AddResourceClicked));
        items.Add(new MenuItem("GainHP", "Gain 3 HP", (evt) =>
        {
            Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "GainHP|3");
            SelectionMenu.Hide();
        }));
        items.Add(new MenuItem("LoseHP", "Lose 3 HP", (evt) =>
        {
            Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "LoseHP|3");
            SelectionMenu.Hide();
        }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void HandleCommand(string command, TokenData tokenData)
    {

        base.HandleCommand(command, tokenData);
        if (command.StartsWith("GainHP|"))
        {
            GainHP(command, tokenData);
        }
        if (command.StartsWith("LoseHP|"))
        {
            LoseHP(command, tokenData);
        }
    }

    private void GainHP(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (CurrentHP + diff > MaxHP)
        {
            diff = MaxHP - CurrentHP;
        }
        if (diff > 0)
        {
            CurrentHP += diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
            }
        }
        UpdateGraphic(tokenData);
    }

    private void LoseHP(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (CurrentHP - diff < 0)
        {
            diff = CurrentHP;
        }
        if (diff > 0)
        {
            CurrentHP -= diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
            }
        }
        UpdateGraphic(tokenData);
    }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(CurrentHP <= 0);
    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        tokenData.OverheadElement.Q<ProgressBar>("HpBar").value = CurrentHP;
        tokenData.OverheadElement.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void UpdateTokenPanel(TokenData tokenData, string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q<ProgressBar>("HpBar").style.minWidth = 150;
        panel.Q<Label>("CHP").text = $"{CurrentHP}";
        panel.Q<Label>("MHP").text = $"/{MaxHP}";
        panel.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void InitTokenPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("Data").Clear();
        panel.Q("ExtraInfo").Clear();
        VisualElement hpBar = UI.CreateFromTemplate("UITemplates/GameSystem/SimpleHPBar");
        panel.Q("Data").Add(hpBar);
        panel.Q("Data").style.flexDirection = FlexDirection.Column;
    }

    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Basic tokens have a primary HP stat by default, but custom resources can be assigned and tracked once created.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Hex 1", "Hex 2", "Hex 3"));
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddIntField("MaxHPField", "Max HP", 100);
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "NameField", "ShapeField", "ColorField", "MaxHPField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string shape = UI.Modal.Q<DropdownField>("ShapeField").value;
        int maxHP = UI.Modal.Q<IntegerField>("MaxHPField").value;
        string color = UI.Modal.Q<DropdownField>("ColorField").value;
        BasicToken t = new()
        {
            System = "Basic",
            Name = name,
            MaxHP = maxHP,
            CurrentHP = maxHP,
            Shape = shape,
            Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };
        AddToken.FinalizeToken(t.Serialize());
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SystemTokenRegistry.RegisterSystem("Basic");
        SystemTokenRegistry.RegisterInterfaceCallback("Basic", DeserializeAsInterface);
        SystemTokenRegistry.RegisterSimpleCallback("Basic|AddTokenModal", AddTokenModal);
    }

    public static ISystemToken DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<BasicToken>(json);
    }

}