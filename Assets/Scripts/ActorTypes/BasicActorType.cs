using System;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
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
        var maxhp = Modal2.AddInlineIntField("MaxHP", "Max HP", 100);
        Modal2.MoveToContainer(maxhp, typeContainer);

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
        if (token == null)
        {
            Toast.AddError("A token has not been selected");
            return;
        }
        string name = Modal2.GetTextFieldValue("Name");
        string shape = Modal2.GetComboboxFieldValue("Shape");
        string color = Modal2.GetComboboxFieldValue("Color");
        int maxHP = Modal2.GetIntFieldValue("MaxHP");

        BasicActorType t = new()
        {
            Type = TypeName,
            Name = name,
            MaxHP = maxHP,
            CurrentHP = maxHP,
        };
        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibraryModal.GetToken(token);
        a.Color = ColorUtility.GetCommonColor(color);
        a.Shape = shape;
        a.Position = Vector3.zero;
        a.Placed = false;
        a.ActorType = JsonUtility.ToJson(t);
        a.ActorTypeId = TypeName;
        string json = JsonUtility.ToJson(a);
        global::AddActorModal.FinalizeToken(json);
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

    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var changeValues = FindParent("Change Values", baseItems);
        changeValues.Children.Add(new MenuItem("Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
        return baseItems;
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