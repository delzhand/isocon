using System;
using System.Reflection;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

public class Generic : GameSystem
{
    public override string SystemName()
    {
        return "Generic System";
    }

    public override void Setup() {
        SetupPanel("SelectedTokenPanel", true);
        SetupPanel("FocusedTokenPanel", false);
    }

    private void SetupPanel(string elementName, bool editable) {
        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UITemplates/GameSystem/SimpleHPBar");
        panel.Q("Data").Add(hpBar);
        if (editable) {
            Button hpButton = new(){
                text = "Alter HP",
                name = "AlterHP"
            };
            hpButton.style.marginLeft = 6;
            hpButton.RegisterCallback<ClickEvent>(AlterHPModal);
            panel.Q("Data").Add(hpButton);
        }
    }

    private void AlterHPModal(ClickEvent evt) {
        Modal.Reset("Alter HP");
        Modal.AddIntField("Number", "Value", 0);
        UI.Modal.Q("Number").AddToClassList("big-number");
        Modal.AddContentButton("ReduceHP", "Reduce HP", (evt) => AlterVitals("LoseHP"));
        Modal.AddContentButton("RecoverHP", "Recover HP", (evt) => AlterVitals("GainHP"));
        Modal.AddButton("Done", Modal.CloseEvent);
    }

    private static void AlterVitals(string cmd) {
        int val = UI.Modal.Q<IntegerField>("Number").value;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"{cmd}|{val}");
    }

    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                TokenData2 data = g.GetComponent<TokenData2>();
                TokenDataSetValue(data.Id, "StartTurn");
            }
        }
    }

    public override void AddTokenModal()
    {
        base.AddTokenModal();
        Modal.AddDropdownField("SizeField", "Size", "1x1", new string[]{"1x1", "2x2", "3x3"});
        Modal.AddTextField("ExtraInfo", "Extra Info", "");
        Modal.AddIntField("HPField", "HP", 1);
    }
   public override void CreateToken() {
        string name = UI.Modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        string graphicHash = TextureSender.GetTextureHash(graphic);
        int size = int.Parse(UI.Modal.Q<DropdownField>("SizeField").value.Substring(0, 1));
        int hp = UI.Modal.Q<IntegerField>("HPField").value;
        string extraInfo = UI.Modal.Q<TextField>("ExtraInfo").value;

        GenericData data = new(){
            CurrentHP = hp,
            MaxHP = hp,
            ExtraInfo = extraInfo
        };

        Player.Self().CmdCreateToken("Generic", graphicHash, name, size, Color.black, JsonUtility.ToJson(data));
    }

    public override void UpdateData(TokenData2 data) {
        GenericData mdata = JsonUtility.FromJson<GenericData>(data.SystemData);
        data.OverheadElement.Q<ProgressBar>("HpBar").value = mdata.CurrentHP;
        data.OverheadElement.Q<ProgressBar>("HpBar").highValue = mdata.MaxHP;        
    }

    public override void TokenDataSetValue(string tokenId, string value)
    {
        base.TokenDataSetValue(tokenId, value);
        TokenData2 data = TokenData2.Find(tokenId);
        Debug.Log($"GenericInterpreter change registered for {data.Name}: {value}");
        GenericData sysdata = JsonUtility.FromJson<GenericData>(data.SystemData);
        sysdata.Change(value, data.WorldObject.GetComponent<Token>(), data.Placed);
        data.SystemData = JsonUtility.ToJson(sysdata);
    }

    public override void UpdateTokenPanel(string tokenId, string elementName) {
        TokenData2 data = TokenData2.Find(tokenId);
        UI.ToggleActiveClass(elementName, data != null);
        if (!data) {
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
        panel.Q<Label>("CHP").text = $"{ sysdata.CurrentHP }";
        panel.Q<Label>("MHP").text = $"/{ sysdata.MaxHP }";
        panel.Q<ProgressBar>("HpBar").value = sysdata.CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = sysdata.MaxHP;
    }

}

[Serializable]
public class GenericData {
    public int CurrentHP;
    public int MaxHP;
    public string ExtraInfo;

    public void Change(string value, Token token, bool placed) {
        if (value.StartsWith("GainHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP + diff > MaxHP) {
                diff = MaxHP - CurrentHP;
            }
            if (diff > 0) {
                CurrentHP+=diff;
                if (placed) {
                    PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("LoseHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0) {
                diff = CurrentHP;
            }
            if (diff > 0) {
                CurrentHP-=diff;
                if (placed) {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }
    }

    private void OnVitalChange(Token token) {
        token.SetDefeated(CurrentHP <= 0);
    }
}
