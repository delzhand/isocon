using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]

public class LancerPilotActorType : LancerBase
{
    private readonly static string TypeName = "Lancer Pilot";

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
        return JsonUtility.FromJson<LancerPilotActorType>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int MaxHP;
    public int CurrentHP;
    public int Armor;
    public int EDefense;
    public int Evade;
    public int Speed;
    #endregion
    public static void AddActorModal()
    {
        Modal.AddTextField("Name", "Name", "");
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddPreferredButton("Create Actor", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddActor.OrderFields(StringUtility.CreateArray("Name", "ColorField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("Name").value;
        string color = UI.Modal.Q<DropdownField>("ColorField").value;
        LancerPilotActorType t = new()
        {
            Type = TypeName,
            Name = name,
            MaxHP = 10,
            CurrentHP = 10,
            Speed = 4,
            Evade = 8,
            EDefense = 8,
            Shape = "Hex 1/2",
            Color = ColorUtility.GetCommonColor(color),
            Token = TokenLibrary.GetSelectedMeta()
        };
        AddActor.FinalizeToken(t.Serialize());
    }

    public override string Label()
    {
        return $"{Name}";
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/SingleBar";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("CoreStats", "Alter Stats", (evt) => { AlterStatModal(); }));
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.ActorCommand("ModHP"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        base.Command(command, tokenData);
        if (command.StartsWith("ModHP|"))
        {
            int original = CurrentHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentHP = Clamped(0, CurrentHP + changeValue, MaxHP);
            int diff = CurrentHP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(tokenData.GetActor(), $"/{plus}{diff}|_HP", Color.white);
            }
        }
        else if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }
        else if (command.StartsWith("UpdateStats"))
        {
            string json = command.Split("|")[1];
            LancerPilotActorType lmu = JsonUtility.FromJson<LancerPilotActorType>(json);
            MaxHP = lmu.MaxHP;
            Armor = lmu.Armor;
            EDefense = lmu.EDefense;
            Evade = lmu.Evade;
            Speed = lmu.Speed;
            PopoverText.Create(token, $"_STAT|_CHANGE", Color.white);
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

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);

        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/SimpleHPBar");
        hpBar.name = "MainHPBar";
        panel.Q("Bars").Add(hpBar);
    }

    private void AlterStatModal()
    {
        SelectionMenu.Hide();
        Modal.Reset("Alter Core Stats");
        Modal.AddNumberNudgerField("MaxHP", "Max HP", MaxHP, 0);
        Modal.AddNumberNudgerField("Armor", "Armor", Armor, 0);
        Modal.AddNumberNudgerField("EDef", "E-Defense", EDefense, 0);
        Modal.AddNumberNudgerField("Evade", "Evade", Evade, 0);
        Modal.AddNumberNudgerField("Speed", "Speed", Speed, 0);

        Modal.AddPreferredButton("Save", (evt) =>
        {
            MaxHP = UI.Modal.Q<NumberNudger>("MaxHP").value;
            Armor = UI.Modal.Q<NumberNudger>("Armor").value;
            EDefense = UI.Modal.Q<NumberNudger>("EDef").value;
            Evade = UI.Modal.Q<NumberNudger>("Evade").value;
            Speed = UI.Modal.Q<NumberNudger>("Speed").value;
            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            Modal.Close();
            this.InitPanel("LeftTokenPanel", true);
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

}
