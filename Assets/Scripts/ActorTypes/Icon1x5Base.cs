using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public abstract class Icon1x5Base : ActorType
{
    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var icon = new MenuItem("ICON 1.5", null);
        baseItems.Add(icon);
        icon.Children.Add(new MenuItem("Damage HP/VIG", () => { NumberPicker.ActorCommand("Damage", false); }));
        icon.Children.Add(new MenuItem("Attack Roll", AttackRollClicked));
        icon.Children.Add(new MenuItem("Save Roll", SaveRollClicked));

        return baseItems;
    }

    private void AttackRollClicked()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Attack Roll");
        Modal2.AddInlineNumberNudgerField("Power", "Curse/Boon", 0, -10, 10);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Roll", AttackRoll);
        Modal2.Open("Attack Roll");
    }

    private void SaveRollClicked()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Attack Roll");
        Modal2.AddInlineNumberNudgerField("Power", "Curse/Boon", 0, -10, 10);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Roll", SaveRoll);
        Modal2.Open("Save Roll");
    }

    private void AttackRoll()
    {
        string name = Actor.GetSelected().Data.Name;
        string desc = $"{name}'s attack roll";
        int power = Modal2.GetNumberNudgerFieldValue("Power");
        BoonCurseRoll(power, desc);
    }

    private void SaveRoll()
    {
        string name = Actor.GetSelected().Data.Name;
        string desc = $"{name}'s save";
        int power = Modal2.GetNumberNudgerFieldValue("Power");
        BoonCurseRoll(power, desc);
    }

    private void BoonCurseRoll(int power, string desc)
    {
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
