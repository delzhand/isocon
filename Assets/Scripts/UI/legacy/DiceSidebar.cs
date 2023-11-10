using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceSidebar : MonoBehaviour
{
    private int d20count = 0;
    private int d12count = 0;
    private int d100count = 0;
    private int d10count = 0;
    private int d8count = 0;
    private int d6count = 0;
    private int d4count = 0;

    public static bool newItems = false;

    void Awake() {
        UI.ToggleDisplay("DiceSidebar", false);

        UI.System.Q<Button>("DiceToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("DiceSidebar");
            newItems = false;
        });

        UI.System.Q("d20").RegisterCallback<ClickEvent>((evt) =>  {
            d20count++;
        });

        UI.System.Q("d12").RegisterCallback<ClickEvent>((evt) =>  {
            d12count++;
        });

        UI.System.Q("d100").RegisterCallback<ClickEvent>((evt) =>  {
            d100count++;
        });

        UI.System.Q("d10").RegisterCallback<ClickEvent>((evt) =>  {
            d10count++;
        });

        UI.System.Q("d8").RegisterCallback<ClickEvent>((evt) =>  {
            d8count++;
        });

        UI.System.Q("d6").RegisterCallback<ClickEvent>((evt) =>  {
            d6count++;
        });

        UI.System.Q("d4").RegisterCallback<ClickEvent>((evt) =>  {
            d4count++;
        });

        UI.System.Q<Button>("RollButton").RegisterCallback<ClickEvent>((evt) =>  {
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

        UI.System.Q<Button>("DiceResetButton").RegisterCallback<ClickEvent>((evt) =>  {
            reset();
        });
    }
    void Update() {
        UI.System.Q("d20").Q<Label>("count").text = d20count.ToString();
        UI.System.Q("d12").Q<Label>("count").text = d12count.ToString();
        UI.System.Q("d100").Q<Label>("count").text = d100count.ToString();
        UI.System.Q("d10").Q<Label>("count").text = d10count.ToString();
        UI.System.Q("d8").Q<Label>("count").text = d8count.ToString();
        UI.System.Q("d6").Q<Label>("count").text = d6count.ToString();
        UI.System.Q("d4").Q<Label>("count").text = d4count.ToString();

        UI.ToggleDisplay(UI.System.Q("DiceToggle").Q("NewItems"), newItems);
    }

    public static void AddOutcome(DiceTray tray) {
        newItems = true;
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

        DiceOutcome diceOutcome = GameObject.Find("UIObjects").AddComponent<DiceOutcome>();
        diceOutcome.Tray = tray;
    }

    private void reset() {
        d20count = 0;
        d12count = 0;
        d100count = 0;
        d10count = 0;
        d8count = 0;
        d6count = 0;
        d4count = 0;
    }
}
