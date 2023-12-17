using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceOutcome : MonoBehaviour
{
    public DiceTray Tray;

    void Update()
    {
        VisualElement root = UI.System.Q(Tray.id);
        TimeSpan timeDifference = DateTime.Now - Tray.time;
        root.Q<Label>("MetaLabel").text = $"{Tray.playerName} {ConvertToTimeAgo(timeDifference)}";
        float percent = (float)(timeDifference.TotalSeconds/60f);
        root.Q("Result").style.backgroundColor = Color.Lerp(new Color(0, 84/255f, 152/255f), new Color(100/255f, 100/255f, 100/255f), percent);
    }

    public static string ConvertToTimeAgo(TimeSpan timeDifference)
    {
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
