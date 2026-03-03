using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

[Serializable]
public class GameSystemData
{
    public List<GameSystemTag> Tags;
}

[Serializable]
public class GameSystemTag
{
    public string Name;
    public int Value;
    public int MaxValue;
    public Color Color;
    public string Type; // Simple, Clock, Value
}

public class GameSystem : MonoBehaviour
{
    public string SystemData;
    public List<GameSystemTag> Tags;

    public static GameSystem Current()
    {
        return GameObject.Find("GameSystem").GetComponent<GameSystem>();
    }

    // public string GetSystemData()
    // {
    //     return SystemData;
    // }

    public void Command(string value)
    {
        if (value.StartsWith("AddTag"))
        {
            string[] parts = value.Split("|");
            GameSystemTag tag = JsonUtility.FromJson<GameSystemTag>(parts[1]);
            Tags.Add(tag);
            VisualElement p = null;
            switch (tag.Type)
            {
                case "Simple":
                    p = Pill.InitStatic(tag.Name, tag.Name, tag.Color);
                    break;
                case "Number":
                    p = Pill.InitNumber(tag.Name, tag.Name, tag.Value, tag.Color, false);
                    break;
            }
            UI.System.Q("TopRight").Q("Pills").Add(p);
        }
        if (value.StartsWith("IncrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], 1);
        }
        if (value.StartsWith("DecrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], -1);
        }
        if (value.StartsWith("RemoveTag"))
        {
            string[] parts = value.Split("|");
            RemoveTag(parts[1]);
        }
    }

    private void CounterTag(string name, int num)
    {
        Debug.Log(name);
        int i = Tags.FindIndex(a => a.Name == name);
        Debug.Log(i);
        Tags[i].Value += num;
    }

    private void RemoveTag(string name)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        Tags.RemoveAt(i);
    }


    // public virtual void Setup()
    // {
    //     HudText.SetItem("gameSystem", SystemName(), 1, HudTextColor.Blue);
    // }

    // public virtual void Teardown()
    // {
    // }

    // public virtual string SystemName()
    // {
    //     return null;
    // }

    // public virtual string GetSystemVars()
    // {
    //     return $"{RoundNumber}";
    // }

    // public virtual void SetSystemVars(string vars)
    // {
    //     RoundNumber = int.Parse(vars);
    //     UI.System.Q<Label>("TurnNumber").text = RoundNumber.ToString();
    // }


    // private static void ClickFlip(ClickEvent evt)
    // {
    //     Token.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
    //     Token.Deselect();
    // }

    // private static void ClickRemove(ClickEvent evt)
    // {
    //     Token.GetSelected().Remove();
    //     Token.Deselect();
    // }

    // private static void ClickDelete(ClickEvent evt)
    // {
    //     TokenData data = Token.GetSelected().Data;
    //     string name = data.Name.Length == 0 ? "this token" : data.Name;
    //     Modal.DoubleConfirm("Delete Token", $"Are you sure you want to delete {name}? This action cannot be undone.", () =>
    //     {
    //         Token.Deselect();
    //         Player.Self().CmdRequestDeleteToken(data.Id);
    //     });
    // }

    // private static void ClickClone(ClickEvent evt)
    // {
    //     TokenData data = Token.GetSelected().Data;
    //     string name = data.Name.Length == 0 ? "this token" : data.Name;
    //     Modal.DoubleConfirm("Clone Token", $"Are you sure you want to clone {name}?", () =>
    //     {
    //         Player.Self().CmdCreateToken(data.System, data.TokenMeta, data.Name, data.Size, data.Color, data.SystemData);
    //         Token.Deselect();
    //     });
    // }

    // private static void ClickEditName(ClickEvent evt)
    // {
    //     TokenData data = Token.GetSelected().Data;
    //     Modal.Reset("Edit Name");
    //     Modal.AddTextField("Name", "Name", data.Name);
    //     Modal.AddPreferredButton("Confirm", (evt) =>
    //     {
    //         string newName = UI.Modal.Q<TextField>("Name").value.Trim();
    //         Player.Self().CmdRequestTokenDataCommand(data.Id, $"Name|{newName}");
    //         Modal.Close();
    //         Token.Deselect();
    //     });
    //     Modal.AddButton("Cancel", Modal.CloseEvent);
    // }

    // private static void ClickResize(ClickEvent evt)
    // {
    //     Modal.Reset("Resize");
    //     Modal.AddDropdownField("SizeField", "Size", "1x1", new string[] { "1x1", "2x2", "3x3" });
    //     Modal.AddPreferredButton("Update Size", (evt) =>
    //     {
    //         string newSize = UI.Modal.Q<DropdownField>("SizeField").value;
    //         TokenData data = Token.GetSelected().Data;
    //         Player.Self().CmdRequestTokenDataCommand(data.Id, $"Size|{newSize}");
    //         Modal.Close();
    //         Token.Deselect();
    //     });
    //     Modal.AddButton("Cancel", Modal.CloseEvent);
    // }

    // private static void ClickEndTurn(ClickEvent evt)
    // {
    //     TokenData data = Token.GetSelected().Data;
    //     Player.Self().CmdRequestTokenDataCommand(data.Id, "EndTurn");
    //     Token.Deselect();
    // }

    // public virtual MenuItem[] GetTileMenuItems()
    // {
    //     List<MenuItem> items = new();
    //     return items.ToArray();
    // }

    // public virtual void GameDataSetValue(string value)
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual string[] GetEffectList()
    // {
    //     return new string[] { "Wavy", "Spiky", "Hand", "Skull", "Hole", "Blocked", "Corners" };
    // }

    // public virtual void CreateToken()
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual bool ValidateAddToken()
    // {
    //     if (!TokenLibrary.TokenSelected())
    //     {
    //         Toast.AddError("A token has not been selected.");
    //         return false;
    //     }
    //     return true;
    // }

    // public virtual void DeserializeToken(string json)
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual void SerializeToken(string tokenId)
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual void SerializeSession(string filename)
    // {
    //     throw new NotImplementedException();
    // }

    // public void WriteSessionToFile(string session, string filename)
    // {
    //     System.IO.File.WriteAllText(filename, session);
    //     Toast.AddSuccess($"Session saved to {filename}.");
    // }

    // public virtual void DeserializeSession(string filename)
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual void UpdateData(TokenData data)
    // {
    //     if (data.OverheadElement == null)
    //     {
    //         data.CreateOverheadElement();
    //     }
    // }

    // public virtual void TokenDataSetValue(string tokenId, string value)
    // {
    //     TokenData data = TokenData.Find(tokenId);
    //     Debug.Log(value);
    //     if (value == "EndTurn")
    //     {
    //         data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = ColorUtility.GetColor("#505050");
    //     }
    //     else if (value == "StartTurn")
    //     {
    //         data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = Color.white;
    //     }
    //     if (value.StartsWith("Name"))
    //     {
    //         data.Name = value.Split("|")[1];
    //     }
    //     if (value.StartsWith("Size"))
    //     {
    //         string sizeValue = value.Split("|")[1];
    //         string number = sizeValue.Substring(0, 1);
    //         data.Size = int.Parse(number);
    //         data.UpdateSize(data.Size);
    //     }
    // }

    //     public virtual void UpdateTokenPanel(string tokenId, string elementName)
    //     {
    //         throw new NotImplementedException();
    //     }

    //     public static void Set(string value)
    //     {
    //         GameSystem current = GameSystem.Current();
    //         if (current)
    //         {
    //             current.Teardown();
    //         }
    //         GameObject g = GameObject.Find("GameSystem");
    //         GameSystem system = g.GetComponent<GameSystem>();
    //         DestroyImmediate(system);

    //         switch (value)
    //         {
    //             case "Generic System":
    //                 system = g.AddComponent<Generic>();
    //                 break;
    //             case "ICON 1.5":
    //                 system = g.AddComponent<Icon_v1_5>();
    //                 break;
    //             case "ICON 2.0 Playtest":
    //                 system = g.AddComponent<Icon_v2_0>();
    //                 break;
    //             case "Maleghast":
    //                 system = g.AddComponent<Maleghast>();
    //                 break;
    //             case "Lancer":
    //                 system = g.AddComponent<Lancer>();
    //                 break;
    //         }
    //         Toast.AddSimple(system.SystemName() + " initialized.");
    //         system.Teardown();
    //         system.Setup();
    //     }
    // }

    // [Serializable]
    // public class GamesystemSessionChecker
    // {
    //     public string System;

    //     public static bool ValidateFile(string filename)
    //     {
    //         string session = File.ReadAllText(filename);
    //         GamesystemSessionChecker check = JsonUtility.FromJson<GamesystemSessionChecker>(session);
    //         string currentSystem = GameSystem.Current().SystemName();
    //         if (check.System != currentSystem)
    //         {
    //             Toast.AddError($"Session system '{check.System}' does not match current system '{currentSystem}'.");
    //             return false;
    //         }
    //         return true;
    //     }


}