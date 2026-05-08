using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public abstract class Icon2x0Base : ActorType
{
    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var icon = new MenuItem("ICON 2.0", null);
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
        Modal2.AddInlineNumberNudgerField("Power", "Weakness/Power", 0, -10, 10);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Roll", AttackRoll);
        Modal2.Open("Attack Roll");
    }

    private void SaveRollClicked()
    {
        SaveRoll();
    }

    private void AttackRoll()
    {
        string name = Actor.GetSelected().Data.Name;
        int power = Modal2.GetNumberNudgerFieldValue("Power");
        string op = power > 0 ? "max" : "min";
        DiceRoller.DirectDieRoll(op, $"{Math.Abs(power) + 1}d10", $"{name}'s attack roll");
        Actor.Deselect();
    }

    private void SaveRoll()
    {
        SelectionMenu.Hide();
        string name = Actor.GetSelected().Data.Name;
        DiceRoller.DirectDieRoll("sum", "1d6", $"{name}'s save roll");
        Actor.Deselect();
    }
}
