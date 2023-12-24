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

    public override void InterpreterMethod(string name, object[] args)
    {
        Type classType = Type.GetType("GenericInterpreter");
        MethodInfo method = classType.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, args);
    }
    
    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
        }
    }

    public override void AddTokenModal()
    {
        base.AddTokenModal();
        Modal.AddDropdownField("SizeField", "Size", "1x1", new string[]{"1x1", "2x2", "3x3"});
        Modal.AddTextField("ExtraInfo", "Extra Info", "");
        Modal.AddIntField("HPField", "HP", 1);
    }

}

[Serializable]
public class GenericData {
    public int CurrentHP;
    public int MaxHP;
    public string ExtraInfo;
}

public class GenericInterpreter {

    public static void CreateToken() {
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

    public static void UpdateData(TokenData2 data) {
        GenericData mdata = JsonUtility.FromJson<GenericData>(data.SystemData);
        data.OverheadElement.Q<ProgressBar>("HpBar").value = mdata.CurrentHP;
        data.OverheadElement.Q<ProgressBar>("HpBar").highValue = mdata.MaxHP;        
    }

    public static void Change(string tokenId, string value) {
        TokenData2 data = TokenData2.Find(tokenId);
        Debug.Log($"GenericInterpreter change registered for {data.Name}: {value}");
    }

    public static void UpdateTokenPanel(string tokenId, string elementName) {
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

        UI.ToggleDisplay(panel.Q("Data"), false);
    }


}