using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceOutcome : MonoBehaviour
{
    private DiceTray tray;

    void Update()
    {
        VisualElement root = UI.System.Q(tray.id);
        root.Q<Label>("MetaLabel").text = $"{tray.playerName} {ConvertToTimeAgo(tray.time)}";
    }

    public static void Create(DiceTray tray) {
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
        diceOutcome.tray = tray;
    }

    public static string ConvertToTimeAgo(DateTime dateTime)
    {
        TimeSpan timeDifference = DateTime.Now - dateTime;

        if (timeDifference.TotalSeconds < 60) {
            return "just now";
        }
        // if (timeDifference.TotalSeconds < 60) {
        //     return "a few seconds ago";
        // }
        // if (timeDifference.TotalSeconds < 60)
        // {
        //     return $"{(int)timeDifference.TotalSeconds} seconds ago";
        // }
        else
        {
            int minutes = (int)timeDifference.TotalMinutes;
            if (minutes == 1)
            {
                return "1 minute ago";
            }
            else
            {
                return $"{minutes} minutes ago";
            }
        }
    }    
}
