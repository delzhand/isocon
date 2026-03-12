using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public abstract class Icon2x0Base : ActorType
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
        Modal.AddNumberNudgerField("PowerField", "Weakness/Power", 0, -20);
        Modal.AddPreferredButton("Roll", AttackRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private void SaveRollClicked(ClickEvent evt)
    {
        string name = Actor.GetSelected().Data.Name;
        DiceRoller.DirectDieRoll("sum", "1d6", $"{name}'s save roll");
        Actor.Deselect();
        Modal.Close();
    }

    private void AttackRoll(ClickEvent evt)
    {
        string name = Actor.GetSelected().Data.Name;
        int power = UI.Modal.Q<NumberNudger>("PowerField").value;
        string op = power > 0 ? "max" : "min";
        DiceRoller.DirectDieRoll(op, $"{Math.Abs(power) + 1}d10", $"{name}'s attack roll");
        Actor.Deselect();
        Modal.Close();
    }
}
