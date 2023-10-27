using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceOutcome : MonoBehaviour
{
    private DiceTray tray;
    public string id;

    void Awake() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        id = tray.id;
        VisualElement root = UI.System.Q(tray.id);
        root.Q<Label>("MetaLabel").text = $"{tray.playerName} {ConvertToTimeAgo(tray.time)}";
    }

    public static void Create(DiceTray tray) {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/DiceResult");
        VisualElement element = template.Instantiate();
        element.style.display = DisplayStyle.Flex;
        element.name = tray.id;
        UI.System.Q("DiceLog").Add(element);

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
