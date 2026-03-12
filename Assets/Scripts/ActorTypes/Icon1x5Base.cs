using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public abstract class Icon1x5Base : ActorType
{
    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("Damage", "Damage HP/VIG", (evt) => { NumberPicker.TokenCommand("Damage", false); }));
        items.Add(new MenuItem("AttackRoll", "Attack Roll", AttackRollClicked));
        items.Add(new MenuItem("SaveRoll", "Save Roll", SaveRollClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    private void AttackRollClicked(ClickEvent evt)
    {
        Modal.Reset("Attack Roll");
        Modal.AddNumberNudgerField("PowerField", "Curse/Boon", 0, -20);
        Modal.AddPreferredButton("Roll", AttackRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        SelectionMenu.Hide();
    }

    private void SaveRollClicked(ClickEvent evt)
    {
        Modal.Reset("Save Roll");
        Modal.AddNumberNudgerField("PowerField", "Curse/Boon", 0, -20);
        Modal.AddPreferredButton("Roll", SaveRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        SelectionMenu.Hide();
    }

    private void AttackRoll(ClickEvent evt)
    {
        string name = Actor.GetSelected().Data.Name;
        string desc = $"{name}'s attack roll";
        BoonCurseRoll(desc);
        Modal.Close();
    }

    private void SaveRoll(ClickEvent evt)
    {
        string name = Actor.GetSelected().Data.Name;
        string desc = $"{name}'s save";
        BoonCurseRoll(desc);
        Modal.Close();
    }

    private void BoonCurseRoll(string desc)
    {
        int power = UI.Modal.Q<NumberNudger>("PowerField").value;
        int powerDice = Math.Abs(power);
        int x = 1 + Random.Range(0, 20);
        int y = 0;
        string plusMinus = "+";
        List<int> bcRolls = new();
        for (int i = 0; i < powerDice; i++)
        {
            int z = 1 + Random.Range(0, 6);
            bcRolls.Add(z);
            y = Math.Max(y, z);
        }
        if (power < 0)
        {
            y *= -1;
            plusMinus = "-";
        }
        string rolls = $"{x}";
        if (powerDice == 1)
        {
            rolls += $"{plusMinus}{Math.Abs(y)}";
        }
        else if (powerDice > 1)
        {
            string r = string.Join("|", bcRolls.ToArray());
            rolls += $"{plusMinus}max({r})";
        }
        Player.Self().CmdShareDiceRoll(desc, $"{x + y}", rolls, 20);
    }
}
