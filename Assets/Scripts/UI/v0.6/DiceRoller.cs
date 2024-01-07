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
    private static int d100count = 0;
    private static int d10count = 0;
    private static int d8count = 0;
    private static int d6count = 0;
    private static int d4count = 0;

    public static bool newItems = false;

    public static void Setup() {
        VisualElement root = UI.System.Q("DiceRoller");

        root.Q("d20").RegisterCallback<ClickEvent>((evt) =>  {
            d20count++;
            root.Q("d20").Q<Label>("count").text = d20count.ToString();
        });

        root.Q("d12").RegisterCallback<ClickEvent>((evt) =>  {
            d12count++;
            root.Q("d12").Q<Label>("count").text = d12count.ToString();
        });

        root.Q("d100").RegisterCallback<ClickEvent>((evt) =>  {
            d100count++;
            root.Q("d100").Q<Label>("count").text = d100count.ToString();
        });

        root.Q("d10").RegisterCallback<ClickEvent>((evt) =>  {
            d10count++;
            root.Q("d10").Q<Label>("count").text = d10count.ToString();
        });

        root.Q("d8").RegisterCallback<ClickEvent>((evt) =>  {
            d8count++;
            root.Q("d8").Q<Label>("count").text = d8count.ToString();
        });

        root.Q("d6").RegisterCallback<ClickEvent>((evt) =>  {
            d6count++;
            root.Q("d6").Q<Label>("count").text = d6count.ToString();
        });

        root.Q("d4").RegisterCallback<ClickEvent>((evt) =>  {
            d4count++;
            root.Q("d4").Q<Label>("count").text = d4count.ToString();
        });

        root.Q<Button>("RollButton").RegisterCallback<ClickEvent>((evt) =>  {
            List<DiceRoll> rolls = new();
            for (int i = 0; i < d20count; i++) {
                rolls.Add(new DiceRoll(20));
            }
            for (int i = 0; i < d12count; i++) {
                rolls.Add(new DiceRoll(12));
            }
            for (int i = 0; i < d100count; i++) {
                rolls.Add(new DiceRoll(100));
            }
            for (int i = 0; i < d10count; i++) {
                rolls.Add(new DiceRoll(10));
            }
            for (int i = 0; i < d8count; i++) {
                rolls.Add(new DiceRoll(8));
            }
            for (int i = 0; i < d6count; i++) {
                rolls.Add(new DiceRoll(6));
            }
            for (int i = 0; i < d4count; i++) {
                rolls.Add(new DiceRoll(4));
            }
            DiceTray tray = new DiceTray(Player.Self().Name, rolls.ToArray());
            Player.Self().CmdRequestDiceRoll(tray);
            reset();
        });

        root.Q<Button>("DiceResetButton").RegisterCallback<ClickEvent>((evt) =>  {
            reset();
        });

    }

    public static void ToggleVisible(ClickEvent evt) {
        visible = !visible;
        UI.ToggleDisplay("DiceRoller", visible);
        UI.ToggleActiveClass("Dice", visible);
        UI.ToggleDisplay(UI.TopBar.Q("Dice").Q("NewItems"), false);
    }

    private static void reset() {
        d20count = 0;
        d12count = 0;
        d100count = 0;
        d10count = 0;
        d8count = 0;
        d6count = 0;
        d4count = 0;

        VisualElement root = UI.System.Q("DiceRoller");
        root.Q("d20").Q<Label>("count").text = d20count.ToString();
        root.Q("d12").Q<Label>("count").text = d12count.ToString();
        root.Q("d100").Q<Label>("count").text = d100count.ToString();
        root.Q("d10").Q<Label>("count").text = d10count.ToString();
        root.Q("d8").Q<Label>("count").text = d8count.ToString();
        root.Q("d6").Q<Label>("count").text = d6count.ToString();
        root.Q("d4").Q<Label>("count").text = d4count.ToString();
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

        int sum = 0;
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
        UI.System.Q("DiceLog").Add(resultElement);

        DiceOutcome diceOutcome = GameObject.Find("UI").AddComponent<DiceOutcome>();
        diceOutcome.Tray = tray;
    }


}
