using System;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceHistoryModal
{
    public static List<(string, DateTime)> history = new();

    public static void Open()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader($"Dice History");
        var scroll = Modal2.AddScrollArea("Scroll");

        for (int i = history.Count - 1; i >= 0; i--)
        {
            var label = new ShunLabel();
            label.text = @$"{history[i].Item1}
<size=-1><color=grey>{AsTimeAgo(history[i].Item2)}</color></size>";
            scroll.Add(label);
        }

        Modal2.AddDialogFooter("Close");
        Modal2.Open("Dice History");
    }

    public static string AsTimeAgo(DateTime dateTime)
    {
        TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);

        return timeSpan.TotalSeconds switch
        {
            <= 60 => $"{timeSpan.Seconds} seconds ago",

            _ => timeSpan.TotalMinutes switch
            {
                <= 1 => "about a minute ago",
                < 60 => $"about {timeSpan.Minutes} minutes ago",
                _ => timeSpan.TotalHours switch
                {
                    <= 1 => "about an hour ago",
                    < 24 => $"about {timeSpan.Hours} hours ago",
                    _ => timeSpan.TotalDays switch
                    {
                        <= 1 => "yesterday",
                        <= 30 => $"about {timeSpan.Days} days ago",

                        <= 60 => "about a month ago",
                        < 365 => $"about {timeSpan.Days / 30} months ago",

                        <= 365 * 2 => "about a year ago",
                        _ => $"about {timeSpan.Days / 365} years ago"
                    }
                }
            }
        };
    }

}
