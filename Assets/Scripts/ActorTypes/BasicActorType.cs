using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class BasicActorType : ActorType
{
    private readonly static string TypeName = "Basic";

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
        return JsonUtility.FromJson<BasicActorType>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int MaxHP;
    public int CurrentHP;
    #endregion

    #region Creation
    public static void AddActorModal()
    {
        Modal.AddTextField("NameField", "Actor Name", "Actor");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", ActorType.ShapeOptions());
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddIntField("MaxHPField", "Max HP", 100);
        Modal.AddPreferredButton("Create Actor", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddActor.OrderFields(StringUtility.CreateArray("NameField", "ShapeField", "ColorField", "MaxHPField"));
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
        BasicActorType t = new()
        {
            Type = TypeName,
            Name = name,
            MaxHP = maxHP,
            CurrentHP = maxHP,
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

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/SingleBar";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        // items.Add(new MenuItem("AddResource", "Add Resource", AddResourceClicked));
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.ActorCommand("ModHP"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, ActorData tokenData)
    {
        base.Command(command, tokenData);
        if (command.StartsWith("ModHP|"))
        {
            ModHP(command, tokenData);
        }
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        tokenData.OverheadElement.Q<ProgressBar>("HpBar").value = CurrentHP;
        tokenData.OverheadElement.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void UpdatePanel(ActorData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);
        VisualElement bar = panel.Q("Bars").Q("MainHPBar");
        bar.Q<ProgressBar>("HpBar").style.minWidth = 150;
        bar.Q<Label>("CHP").text = $"{CurrentHP}";
        bar.Q<Label>("MHP").text = $"/{MaxHP}";
        bar.Q<ProgressBar>("HpBar").value = CurrentHP;
        bar.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);

        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/SimpleHPBar");
        hpBar.name = "MainHPBar";
        panel.Q("Bars").Add(hpBar);
    }

    private void ModHP(string command, ActorData token)
    {
        int value = int.Parse(command.Split("|")[1]);
        if (value <= 0)
        {
            LoseHP(value, token);
        }
        else
        {
            GainHP(value, token);
        }
    }

    private void GainHP(int value, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        int diff = Math.Abs(value);
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

    private void LoseHP(int value, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        int diff = Math.Abs(value);
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

    private void UpdateGraphic(ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        token.SetDefeated(CurrentHP <= 0);
    }

}