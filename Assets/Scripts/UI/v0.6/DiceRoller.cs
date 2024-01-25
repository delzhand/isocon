using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    public static void Setup()
    {
        UI.ToggleDisplay("DiceRoller", false);
        VisualElement root = UI.System.Q("DiceRoller");

        root.Q("d20").RegisterCallback<ClickEvent>((evt) =>
        {
            d20count++;
            root.Q("d20").Q<Label>("count").text = d20count.ToString();
            DieAdd("d20");
        });

        root.Q("d12").RegisterCallback<ClickEvent>((evt) =>
        {
            d12count++;
            root.Q("d12").Q<Label>("count").text = d12count.ToString();
            DieAdd("d12");
        });

        root.Q("d10").RegisterCallback<ClickEvent>((evt) =>
        {
            d10count++;
            root.Q("d10").Q<Label>("count").text = d10count.ToString();
            DieAdd("d10");
        });

        root.Q("d8").RegisterCallback<ClickEvent>((evt) =>
        {
            d8count++;
            root.Q("d8").Q<Label>("count").text = d8count.ToString();
            DieAdd("d8");
        });

        root.Q("d6").RegisterCallback<ClickEvent>((evt) =>
        {
            d6count++;
            root.Q("d6").Q<Label>("count").text = d6count.ToString();
            DieAdd("d6");
        });

        root.Q("d4").RegisterCallback<ClickEvent>((evt) =>
        {
            d4count++;
            root.Q("d4").Q<Label>("count").text = d4count.ToString();
            DieAdd("d4");
        });

        root.Q<Button>("Reset").RegisterCallback<ClickEvent>((evt) =>
        {
            reset();
        });

        root.Q<Button>("Total").RegisterCallback<ClickEvent>((evt) =>
        {
            DieRoll("sum");
        });

        root.Q<Button>("Highest").RegisterCallback<ClickEvent>((evt) =>
        {
            DieRoll("max");
        });

        UI.ToggleDisplay(root.Q("Total"), false);
        UI.ToggleDisplay(root.Q("Highest"), false);
    }

    private static void DieAdd(string die)
    {
        VisualElement root = UI.System.Q("DiceRoller");
        UI.ToggleDisplay(root.Q(die).Q<Label>("count"), true);
        UI.ToggleDisplay(root.Q("Total"), true);
        UI.ToggleDisplay(root.Q("Highest"), true);
    }

    private static void DieRoll(string op)
    {
        string rollString = GetRollString();
        Player.Self().CmdRequestDiceRoll(new DiceTray(Player.Self().Name, rollString, op, null));
        reset();
    }

    private static string GetRollString()
    {
        VisualElement root = UI.System.Q("DiceRoller");
        List<string> rolls = new();
        if (d20count > 0)
        {
            rolls.Add($"{d20count}d20");
        }
        if (d12count > 0)
        {
            rolls.Add($"{d12count}d12");
        }
        if (d10count > 0)
        {
            rolls.Add($"{d10count}d10");
        }
        if (d8count > 0)
        {
            rolls.Add($"{d8count}d8");
        }
        if (d6count > 0)
        {
            rolls.Add($"{d6count}d6");
        }
        if (d4count > 0)
        {
            rolls.Add($"{d4count}d4");
        }
        string rollString = String.Join("+", rolls.ToArray());

        int mod = root.Q<IntegerField>("Number").value;
        if (mod < 0)
        {
            rollString += $"{mod}";
        }
        else if (mod > 0)
        {
            rollString += $"+{mod}";
        }
        return rollString;
    }

    public static void ToggleVisible(ClickEvent evt)
    {
        visible = !visible;
        UI.ToggleDisplay("DiceRoller", visible);
        UI.ToggleActiveClass("Dice", visible);
    }

    private static void reset()
    {
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

    public static void AddOutcome(DiceTray tray)
    {
        VisualTreeAsset resultTemplate = Resources.Load<VisualTreeAsset>("UITemplates/DiceResult");
        VisualElement resultElement = resultTemplate.Instantiate();

        List<int> rolls = new();
        int sum = 0;
        int max = int.MinValue;
        int min = int.MaxValue;
        int largestDie = 2;
        for (int i = 0; i < tray.Rolls.Length; i++)
        {
            largestDie = math.max(largestDie, tray.Rolls[i].Die);
            max = Math.Max(max, tray.Rolls[i].Rolled);
            min = Math.Min(min, tray.Rolls[i].Rolled);
            sum += tray.Rolls[i].Rolled;
            rolls.Add(tray.Rolls[i].Rolled);
        }

        if (tray.Description != null)
        {
            resultElement.Q<Label>("Label").text = $"{tray.Description} ({tray.PlayerName})";
        }
        else
        {
            resultElement.Q<Label>("Label").text = tray.PlayerName;
        }

        switch (largestDie)
        {
            case 4:
            case 6:
            case 8:
            case 10:
            case 12:
            case 20:
                resultElement.Q("Icon").style.backgroundImage = Resources.Load<Texture2D>($"Textures/die_{largestDie}");
                break;
            default:
                UI.ToggleDisplay(resultElement.Q("Icon"), false);
                break;
        }

        string modString = "";
        if (tray.Modifier < 0)
        {
            modString += $"{tray.Modifier}";
        }
        if (tray.Modifier > 0)
        {
            modString += $"+{tray.Modifier}";
        }

        switch (tray.Op)
        {
            case "sum":
                resultElement.Q<Label>("Result").text = $"{sum + tray.Modifier}";
                resultElement.Q<Label>("Rolls").text = $"{string.Join("+", rolls.ToArray())}{modString}";
                break;
            case "max":
                resultElement.Q<Label>("Result").text = $"{max + tray.Modifier}";
                resultElement.Q<Label>("Rolls").text = $"({string.Join(", ", rolls.ToArray())}){modString}";
                break;
            default:
                resultElement.Q<Label>("Result").text = $"({string.Join(", ", rolls.ToArray())}){modString}";
                UI.ToggleDisplay(resultElement.Q<Label>("Rolls"), false);
                break;
        }

        // resultElement.Q<Label>("Sum").text = $"{sum}";
        // resultElement.Q<Label>("Fns").text = $" (▲{highest} ▼{lowest} μ{Math.Floor(sum/(float)tray.rolls.Length)})";

        Toast.AddCustom(resultElement, 15);

        // UI.System.Q("DiceLog").Q("Rolls").Add(resultElement);

        // DiceOutcome diceOutcome = GameObject.Find("UI").AddComponent<DiceOutcome>();
        // diceOutcome.Tray = tray;
    }

    public static void AddOutcome(string description, string result, string rolls, int die)
    {
        VisualTreeAsset resultTemplate = Resources.Load<VisualTreeAsset>("UITemplates/DiceResult");
        VisualElement resultElement = resultTemplate.Instantiate();
        resultElement.Q<Label>("Label").text = description;
        resultElement.Q("Icon").style.backgroundImage = Resources.Load<Texture2D>($"Textures/die_{die}");
        resultElement.Q<Label>("Result").text = $"{result}";
        resultElement.Q<Label>("Rolls").text = $"{rolls}";
        Toast.AddCustom(resultElement, 15);
    }


}
