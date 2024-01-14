using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceRoller 
{
    private static Boolean visible;

    private static int d20count = 0;
    private static int d12count = 0;
    private static int d10count = 0;
    private static int d8count = 0;
    private static int d6count = 0;
    private static int d4count = 0;

    public static bool newItems = false;

    public static void Setup() {
        UI.ToggleDisplay("DiceRoller", false);
        VisualElement root = UI.System.Q("DiceRoller");

        root.Q("d20").RegisterCallback<ClickEvent>((evt) =>  {
            d20count++;
            root.Q("d20").Q<Label>("count").text = d20count.ToString();
            DieAdd("d20");
        });

        root.Q("d12").RegisterCallback<ClickEvent>((evt) =>  {
            d12count++;
            root.Q("d12").Q<Label>("count").text = d12count.ToString();
            DieAdd("d12");
        });

        root.Q("d10").RegisterCallback<ClickEvent>((evt) =>  {
            d10count++;
            root.Q("d10").Q<Label>("count").text = d10count.ToString();
            DieAdd("d10");
        });

        root.Q("d8").RegisterCallback<ClickEvent>((evt) =>  {
            d8count++;
            root.Q("d8").Q<Label>("count").text = d8count.ToString();
            DieAdd("d8");
        });

        root.Q("d6").RegisterCallback<ClickEvent>((evt) =>  {
            d6count++;
            root.Q("d6").Q<Label>("count").text = d6count.ToString();
            DieAdd("d6");
        });

        root.Q("d4").RegisterCallback<ClickEvent>((evt) =>  {
            d4count++;
            root.Q("d4").Q<Label>("count").text = d4count.ToString();
            DieAdd("d4");
        });

        root.Q<Button>("Reset").RegisterCallback<ClickEvent>((evt) =>  {
            reset();
        });

        root.Q<Button>("Total").RegisterCallback<ClickEvent>((evt) => {
            DieRoll("sum");
        });

        root.Q<Button>("Highest").RegisterCallback<ClickEvent>((evt) => {
            DieRoll("max");
        });

        UI.ToggleDisplay(root.Q("Total"), false);
        UI.ToggleDisplay(root.Q("Highest"), false);
    }

    private static void DieAdd(string die) {
        VisualElement root = UI.System.Q("DiceRoller");
        UI.ToggleDisplay(root.Q(die).Q<Label>("count"), true);
        UI.ToggleDisplay(root.Q("Total"), true);
        UI.ToggleDisplay(root.Q("Highest"), true);
    }

    private static void DieRoll(string func) {
        string rollString = GetRollString();
        Debug.Log($"{func}: {rollString}");
        Player.Self().CmdRequestDiceRoll(new DiceTray(Player.Self().Name, rollString));
        reset();
    }

    private static string GetRollString() {
        VisualElement root = UI.System.Q("DiceRoller");
        List<string> rolls = new();
        if (d20count > 0) {
            rolls.Add($"{d20count}d20");
        }
        if (d12count > 0) {
            rolls.Add($"{d12count}d12");
        }
        if (d10count > 0) {
            rolls.Add($"{d10count}d10");
        }
        if (d8count > 0) {
            rolls.Add($"{d8count}d8");
        }
        if (d6count > 0) {
            rolls.Add($"{d6count}d6");
        }
        if (d4count > 0) {
            rolls.Add($"{d4count}d4");
        }
        int mod = root.Q<IntegerField>("Number").value;
        if (mod != 0) {
            rolls.Add($"{mod}");
        }
        return String.Join("+", rolls.ToArray());
    }

    public static void ToggleVisible(ClickEvent evt) {
        visible = !visible;
        UI.ToggleDisplay("DiceRoller", visible);
        UI.ToggleActiveClass("Dice", visible);
    }

    private static void reset() {
        d20count = 0;
        d12count = 0;
        d10count = 0;
        d8count = 0;
        d6count = 0;
        d4count = 0;

        VisualElement root = UI.System.Q("DiceRoller");
        root.Q<IntegerField>("Number").value = 0;
        UI.ToggleDisplay(root.Q("d20").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("d12").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("d10").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("d8").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("d6").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("d4").Q<Label>("count"), false);
        UI.ToggleDisplay(root.Q("Total"), false);
        UI.ToggleDisplay(root.Q("Highest"), false);
    }

    public static void AddOutcome(DiceTray tray) {
        if (!visible) {
            UI.ToggleDisplay(UI.System.Q("Dice").Q("NewItems"), true);
        }
        VisualTreeAsset resultTemplate = Resources.Load<VisualTreeAsset>("UITemplates/DiceResult");
        VisualTreeAsset rollTemplate = Resources.Load<VisualTreeAsset>("UITemplates/Roll");

        VisualElement resultElement = resultTemplate.Instantiate();
        resultElement.style.display = DisplayStyle.Flex;
        resultElement.name = tray.id;

        int sum = tray.modifier;
        int highest = int.MinValue;
        int lowest = int.MaxValue;
        for (int i = 0; i < tray.rolls.Length; i++) {
            sum += tray.rolls[i].Rolled;
            highest = Math.Max(highest, tray.rolls[i].Rolled);
            lowest = Math.Min(lowest, tray.rolls[i].Rolled);

            VisualElement rollElement = rollTemplate.Instantiate();
            rollElement.Q<Label>("Value").text = $"{tray.rolls[i].Rolled}";
            rollElement.Q<Label>("Die").text = $"{tray.rolls[i].Die}";
            resultElement.Q("Rolls").Add(rollElement);
            if (i < tray.rolls.Length - 1) {
                VisualElement plusElement = new Label("+");
                plusElement.AddToClassList("dice-plus");
                resultElement.Q("Rolls").Add(plusElement);
            }
        }

        resultElement.Q<Label>("Sum").text = $"{sum}";
        resultElement.Q<Label>("Fns").text = $" (▲{highest} ▼{lowest} μ{Math.Floor(sum/(float)tray.rolls.Length)})";

        Toast.AddCustom(resultElement);

        // UI.System.Q("DiceLog").Q("Rolls").Add(resultElement);

        // DiceOutcome diceOutcome = GameObject.Find("UI").AddComponent<DiceOutcome>();
        // diceOutcome.Tray = tray;
    }


}
