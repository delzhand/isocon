using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Icon2x0MobActorType : Icon2x0Base
{
    private readonly static string TypeName = "Icon 2.0 Mob";

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
        return JsonUtility.FromJson<Icon2x0MobActorType>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int Hits;
    public int Vigor;
    public int Move;
    public int Defense;
    #endregion

    #region Creation
    public static void AddActorModal()
    {
        Modal.AddTextField("NameField", "Actor Name", "Actor");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", ActorType.ShapeOptions());

        Modal.AddPreferredButton("Create Actor", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        AddActor.OrderFields(StringUtility.CreateArray("NameField", "ShapeField"));
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

        Icon2x0MobActorType t = new()
        {
            Type = TypeName,
            Name = name,
            Hits = 2,
            Move = 4,
            Defense = 4,
            Vigor = 0,
        };

        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibrary.GetSelectedMeta();
        a.Color = ColorUtility.GetCommonColor("gray");
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
        return "UI/TableTop/Overheads/PipCounter";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("ModVig", "Modify VIG", (evt) => { NumberPicker.ActorCommand("ModVIG"); }));
        if (Hits < 2)
        {
            items.Add(new MenuItem("RestoreHit", "Restore Hit", (evt) =>
            {
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, "RestoreHit");
                SelectionMenu.Hide();
            }));
        }

        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        base.Command(command, tokenData);
        if (command.StartsWith("Damage"))
        {
            int diff = Math.Abs(int.Parse(command.Split("|")[1]));
            if (diff < Vigor)
            {
                Vigor -= diff;
                PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
            }
            else if (diff > 0)
            {
                Vigor = 0;
                Hits--;
                PopoverText.Create(token, $"/-1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command == "RestoreHit")
        {
            if (Hits < 2)
            {
                Hits += 1;
                PopoverText.Create(token, $"/+1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command.StartsWith("ModVIG"))
        {
            int original = Vigor;
            int changeValue = int.Parse(command.Split("|")[1]);
            Vigor = Clamped(0, Vigor + changeValue, 6);
            int diff = Vigor - original;
            diff = Math.Min(diff, 6);
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        if (Vigor > 0)
        {
            o.Q<Label>("Pips").text = MobHPString();
        }
        else
        {
            o.Q<Label>("Pips").text = SymbolString("■", Hits, 2);
        }

        UI.ToggleDisplay(o, Hits > 0 && tokenData.Placed);
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        Label l = new();
        l.name = "MainHPLabel";
        l.text = MobHPString();
        l.style.color = Color.red;
        l.style.unityTextOutlineColor = Color.white;
        l.style.unityTextOutlineWidth = 1;
        l.style.fontSize = 26;
        panel.Q("Bars").Add(l);

        VisualElement s3 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s3.Q<Label>("Label").text = "MOVE";
        s3.Q<Label>("Value").text = $"{Move}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", "Mob", ColorUtility.GetCommonColor("gray")));
    }

    private string MobHPString()
    {
        string x = "■";
        StringBuilder sb = new();
        for (int i = 0; i < Hits; i++)
        {
            sb.Append(x);
        }
        sb.Append("<color=#25E1F2>");
        for (int i = 0; i < Vigor; i++)
        {
            sb.Append(x);
        }
        sb.Append("</color>");
        return sb.ToString();
    }

    public override void UpdatePanel(ActorData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        Label mainHPLabel = panel.Q<Label>("MainHPLabel");
        mainHPLabel.text = MobHPString();
    }

    private void UpdateGraphic(ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        token.SetDefeated(Hits <= 0);
    }
}