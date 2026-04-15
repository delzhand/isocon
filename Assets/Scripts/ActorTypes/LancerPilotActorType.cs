using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using ShunUI;
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
        var contents = Modal2.Contents("PrimaryDialog");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();

        var name = Modal2.AddInlineTextField("Name", "Actor Name", "Actor");
        Modal2.MoveToContainer(name, typeContainer);
        var shape = Modal2.AddInlineComboboxField("Shape", "Shape", "Square 1x1", ActorType.ShapeOptions().ToList<string>());
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
        Modal2.ReadContext("PrimaryDialog");
        string token = Modal2.GetComboboxFieldValue("Token");
        if (token.Length == 0)
        {
            Toast.AddError("A token has not been selected");
            return;
        }
        string name = Modal2.GetTextFieldValue("Name");
        string shape = Modal2.GetComboboxFieldValue("Shape");
        string color = Modal2.GetComboboxFieldValue("Color");
        LancerPilotActorType t = new()
        {
            Type = TypeName,
            Name = name,
            MaxHP = 10,
            CurrentHP = 10,
            Speed = 4,
            Evade = 8,
            EDefense = 8,
        };
        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibraryModal.GetToken();
        a.Color = ColorUtility.GetCommonColor(color);
        a.Shape = shape;
        a.Position = Vector3.zero;
        a.Placed = false;
        a.ActorType = JsonUtility.ToJson(t);
        a.ActorTypeId = TypeName;
        string json = JsonUtility.ToJson(a);
        global::AddActorModal.FinalizeToken(json);
    }

    public override string Label()
    {
        return $"{Name}";
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/SingleBar";
    }

    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var changeValues = FindParent("Change Values", baseItems);

        changeValues.Children.Add(new MenuItem("Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
        changeValues.Children.Add(new MenuItem("Alter Core Stats", AlterStatModal));

        return baseItems;
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

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);

        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/SimpleHPBar");
        hpBar.name = "MainHPBar";
        panel.Q("Bars").Add(hpBar);
    }

    private void AlterStatModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Alter Core Stats");
        Modal2.AddInlineNumberNudgerField("MaxHP", "Max HP", MaxHP, 0, 50);
        Modal2.AddInlineNumberNudgerField("Armor", "Armor", Armor, 0, 50);
        Modal2.AddInlineNumberNudgerField("EDef", "E-Defense", EDefense, 0, 50);
        Modal2.AddInlineNumberNudgerField("Evade", "Evade", Evade, 0, 50);
        Modal2.AddInlineNumberNudgerField("Speed", "Speed", Speed, 0, 50);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            MaxHP = Modal2.GetNumberNudgerFieldValue("MaxHP");
            Armor = Modal2.GetNumberNudgerFieldValue("Armor");
            EDefense = Modal2.GetNumberNudgerFieldValue("EDef");
            Evade = Modal2.GetNumberNudgerFieldValue("Evade");
            Speed = Modal2.GetNumberNudgerFieldValue("Speed");
            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            this.InitPanel(Actor.GetSelected().Data, "LeftTokenPanel", true);
        });
    }

}
