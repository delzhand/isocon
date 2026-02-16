using System;
using System.Reflection;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Generic : GameSystem
{
    public override string SystemName()
    {
        return "Generic System";
    }

    public override void Setup()
    {
        base.Setup();
        SetupPanel("SelectedTokenPanel", true);
        SetupPanel("FocusedTokenPanel", false);
    }

    private void SetupPanel(string elementName, bool editable)
    {
        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UITemplates/GameSystem/SimpleHPBar");
        panel.Q("Data").Add(hpBar);
        panel.Q("Data").style.flexDirection = FlexDirection.Column;
        VisualElement resourceContainer = new VisualElement();
        resourceContainer.name = "Resources";
        panel.Q("Data").Add(resourceContainer);

        if (editable)
        {
            Button hpButton = new()
            {
                text = "Alter HP",
                name = "AlterHP"
            };
            hpButton.style.marginLeft = 6;
            hpButton.RegisterCallback<ClickEvent>(AlterHPModal);
            panel.Q("Data").Add(hpButton);
        }
    }

    public override void Teardown()
    {
        TeardownPanel("SelectedTokenPanel");
        TeardownPanel("FocusedTokenPanel");
    }

    private void TeardownPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("ExtraInfo").Clear();
        panel.Q("Data").Clear();
    }

    private void AlterHPModal(ClickEvent evt)
    {
        Modal.Reset("Alter HP");
        Modal.AddIntField("Number", "Value", 0);
        UI.Modal.Q("Number").AddToClassList("big-number");
        Modal.AddContentButton("ReduceHP", "Reduce HP", (evt) => AlterVitals("LoseHP"));
        Modal.AddContentButton("RecoverHP", "Recover HP", (evt) => AlterVitals("GainHP"));
        Modal.AddButton("Done", Modal.CloseEvent);
    }

    private static void AlterVitals(string cmd)
    {
        int val = UI.Modal.Q<IntegerField>("Number").value;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"{cmd}|{val}");
    }

    public override void GameDataSetValue(string value)
    {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn")
        {
            RoundNumber++;
            UI.System.Q<Label>("TurnNumber").text = RoundNumber.ToString();
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("TokenData"))
            {
                TokenData data = g.GetComponent<TokenData>();
                TokenDataSetValue(data.Id, "StartTurn");
            }
        }
    }

    public override void AddTokenModal()
    {
        base.AddTokenModal();
        Modal.AddDropdownField("SizeField", "Size", "1x1", new string[] { "1x1", "2x2", "3x3" });
        Modal.AddTextField("ExtraInfo", "Extra Info", "");
        Modal.AddIntField("HPField", "HP", 1);
        Modal.AddDropdownField("ColorField", "Color", "Gray", ColorUtility.CommonColors());
    }

    public override void CreateToken()
    {
        var tokenMeta = TokenLibrary.GetSelectedMeta();
        string name = UI.Modal.Q<TextField>("NameField").value;
        int size = int.Parse(UI.Modal.Q<DropdownField>("SizeField").value.Substring(0, 1));
        int hp = UI.Modal.Q<IntegerField>("HPField").value;
        string extraInfo = UI.Modal.Q<TextField>("ExtraInfo").value;
        string colorName = UI.Modal.Q<DropdownField>("ColorField").value;

        GenericData data = new()
        {
            CurrentHP = hp,
            MaxHP = hp,
            ExtraInfo = extraInfo,
            ColorName = colorName
        };

        Color color = ColorUtility.GetCommonColor(colorName);

        Player.Self().CmdCreateToken(SystemName(), tokenMeta, name, size, color, JsonUtility.ToJson(data));
    }

    public override MenuItem[] GetTokenMenuItems(TokenData data)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(data);

        List<MenuItem> items = new();
        items.Add(new MenuItem("AddResource", "Add Resource", AddResourceClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    private void AddResourceClicked(ClickEvent evt)
    {
        Modal.Reset("Add Resource");
        Modal.AddTextField("ResourceName", "Resource Name", "");
        Modal.AddIntField("ResourceValue", "Resource Initial Value", 0);
        Modal.AddPreferredButton("Add", AddResource);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        SelectionMenu.Hide();
    }

    private void AddResource(ClickEvent evt)
    {
        string resourceName = UI.Modal.Q<TextField>("ResourceName").value;
        int resourceValue = UI.Modal.Q<IntegerField>("ResourceValue").value;
        GenericTokenResource resource = new();
        resource.Name = resourceName;
        resource.Value = resourceValue;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"SetResource|{JsonUtility.ToJson(resource)}");
        Modal.Close();
    }

    public override void UpdateData(TokenData data)
    {
        base.UpdateData(data);
        GenericData mdata = JsonUtility.FromJson<GenericData>(data.SystemData);
        data.OverheadElement.Q<ProgressBar>("HpBar").value = mdata.CurrentHP;
        data.OverheadElement.Q<ProgressBar>("HpBar").highValue = mdata.MaxHP;
    }

    public override void TokenDataSetValue(string tokenId, string value)
    {
        base.TokenDataSetValue(tokenId, value);
        TokenData data = TokenData.Find(tokenId);
        Debug.Log($"GenericInterpreter change registered for {data.Name}: {value}");
        GenericData sysdata = JsonUtility.FromJson<GenericData>(data.SystemData);
        sysdata.Change(value, data.WorldObject.GetComponent<Token>(), data.Placed);
        data.SystemData = JsonUtility.ToJson(sysdata);
        data.NeedsRedraw = true;
    }

    public override void UpdateTokenPanel(string tokenId, string elementName)
    {
        TokenData data = TokenData.Find(tokenId);
        UI.ToggleActiveClass(elementName, data != null);
        if (!data)
        {
            return;
        }

        data.UpdateTokenPanel(elementName);
        GenericData sysdata = JsonUtility.FromJson<GenericData>(data.SystemData);

        VisualElement panel = UI.System.Q(elementName);

        panel.Q("ClassBackground").style.borderTopColor = data.Color;
        panel.Q("ClassBackground").style.borderRightColor = data.Color;
        panel.Q("ClassBackground").style.borderBottomColor = data.Color;
        panel.Q("ClassBackground").style.borderLeftColor = data.Color;

        panel.Q("ExtraInfo").Clear();
        Label l = new()
        {
            text = sysdata.ExtraInfo
        };
        panel.Q("ExtraInfo").Add(l);

        panel.Q<ProgressBar>("HpBar").style.minWidth = 150;
        panel.Q<Label>("CHP").text = $"{sysdata.CurrentHP}";
        panel.Q<Label>("MHP").text = $"/{sysdata.MaxHP}";
        panel.Q<ProgressBar>("HpBar").value = sysdata.CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = sysdata.MaxHP;

        if (data.NeedsRedraw)
        {
            data.NeedsRedraw = false;
            panel.Q("Resources").Clear();
            foreach (GenericTokenResource resource in sysdata.Resources)
            {
                VisualElement template = UI.CreateFromTemplate("UITemplates/GameSystem/ConditionTemplate");
                string label = $"{resource.Name} ({resource.Value})";
                template.Q<Label>("Name").text = label;
                if (elementName == "SelectedTokenPanel")
                {
                    template.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"IncrementResource|{resource.Name}");
                    });
                    template.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"DecrementResource|{resource.Name}");
                    });
                    template.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"LoseResource|{resource.Name}");
                    });
                    UI.ToggleDisplay(template.Q("Increment"), true);
                    UI.ToggleDisplay(template.Q("Decrement"), true);
                    UI.ToggleDisplay(template.Q("Remove"), true);
                }
                else
                {
                    UI.ToggleDisplay(template.Q("Increment"), false);
                    UI.ToggleDisplay(template.Q("Decrement"), false);
                    UI.ToggleDisplay(template.Q("Remove"), false);
                }
                template.Q("Wrapper").style.backgroundColor = Color.gray;
                panel.Q("Resources").Add(template);
            }
        }
    }

    private void DeserializeToken(GenericTokenPersistence tp)
    {
        Color color = Color.black;
        string data = JsonUtility.ToJson(tp.SystemData);
        Player.Self().CmdCreateTokenPlaced(SystemName(), tp.TokenMeta, tp.Name, tp.Size, color, data, tp.Position);
    }

    private GenericTokenPersistence PersistToken(string tokenId)
    {
        TokenData data = TokenData.Find(tokenId);
        GenericTokenPersistence p = new();
        p.Name = data.Name;
        p.SystemData = JsonUtility.FromJson<GenericData>(data.SystemData);
        p.TokenMeta = data.TokenMeta;
        p.Position = data.LastKnownPosition;
        p.Size = data.Size;
        return p;
    }

    public override void SerializeSession(string filename)
    {
        List<GenericTokenPersistence> tps = new();
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++)
        {
            GenericTokenPersistence tp = PersistToken(tokens[i].GetComponent<Token>().Data.Id);
            tps.Add(tp);
        }
        GenericSessionPersistence sp = new();
        sp.System = SystemName();
        sp.RoundNumber = RoundNumber;
        sp.Tokens = tps.ToArray();
        string session = JsonUtility.ToJson(sp);
        WriteSessionToFile(session, filename);
    }

    public override void DeserializeSession(string filename)
    {
        if (!GamesystemSessionChecker.ValidateFile(filename))
        {
            return;
        }

        // This runs immediately, locally, whereas the Cmd to delete all runs later async
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("TokenData"))
        {
            TokenData data = g.GetComponent<TokenData>();
            data.Deletable = true;
        }

        string session = System.IO.File.ReadAllText(filename);
        GenericSessionPersistence sp = JsonUtility.FromJson<GenericSessionPersistence>(session);
        Player.Self().CmdRequestDeleteAllTokens();
        RoundNumber = sp.RoundNumber;
        foreach (GenericTokenPersistence tp in sp.Tokens)
        {
            DeserializeToken(tp);
        }
        Player.Self().CmdRequestClientInit();
    }
}

[Serializable]
public class GenericTokenResource
{
    public string Name;
    public int Value;
}

[Serializable]
public class GenericData
{
    public int CurrentHP;
    public int MaxHP;
    public string ExtraInfo;
    public string ColorName;
    public GenericTokenResource[] Resources;

    public void Change(string value, Token token, bool placed)
    {
        if (value.StartsWith("GainHP"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP + diff > MaxHP)
            {
                diff = MaxHP - CurrentHP;
            }
            if (diff > 0)
            {
                CurrentHP += diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("LoseHP"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0)
            {
                diff = CurrentHP;
            }
            if (diff > 0)
            {
                CurrentHP -= diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("SetResource"))
        {
            string[] parts = value.Split("|");
            GenericTokenResource resource = JsonUtility.FromJson<GenericTokenResource>(parts[1]);
            SetResource(resource);
            PopoverText.Create(token, "Resource Set", Color.white);
        }
        if (value.StartsWith("IncrementResource"))
        {
            string[] parts = value.Split("|");
            CounterResource(parts[1], 1);
            if (placed)
            {
                PopoverText.Create(token, $"/+1|_{parts[1].ToUpper()}", Color.white);
            }
        }
        if (value.StartsWith("DecrementResource"))
        {
            string[] parts = value.Split("|");
            CounterResource(parts[1], -1);
            if (placed)
            {
                PopoverText.Create(token, $"/-1|_{parts[1].ToUpper()}", Color.white);
            }
        }
    }

    private void CounterResource(string name, int num)
    {
        List<GenericTokenResource> resourceList = Resources.ToList();
        foreach (GenericTokenResource resource in resourceList)
        {
            if (name == resource.Name)
            {
                resource.Value += num;
            }
        }
        Resources = resourceList.ToArray();
    }

    private void OnVitalChange(Token token)
    {
        token.SetDefeated(CurrentHP <= 0);
    }

    private void SetResource(GenericTokenResource resource)
    {
        List<GenericTokenResource> resourceList = Resources.ToList();
        resourceList.Add(resource);
        Resources = resourceList.ToArray();
    }
}

[Serializable]
public class GenericTokenPersistence
{
    public string Name;
    public TokenMeta TokenMeta;
    public GenericData SystemData;
    public string ColorName;
    public int Size;
    public Vector3 Position;
}

[Serializable]
public class GenericSessionPersistence
{
    public string System;
    public int RoundNumber;
    public GenericTokenPersistence[] Tokens;
}