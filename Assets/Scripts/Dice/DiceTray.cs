using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public struct DiceTray
{
    public DiceRoll[] Rolls;
    public int Modifier;
    public string PlayerName;
    public string Id;
    public DateTime Time;
    public string Op;
    public string Description;

    public DiceTray(string playerName, string roll, string op, string description)
    {
        this.PlayerName = playerName;
        List<DiceRoll> rolls = new();
        (int[], int) dice = ParseDiceSizes(roll);
        foreach (int die in dice.Item1)
        {
            rolls.Add(new DiceRoll(die));
        }
        this.Rolls = rolls.ToArray();
        Time = DateTime.Now;
        Id = Guid.NewGuid().ToString();

        string[] split = roll.Split("+");
        string mod = split.Last<string>();
        Modifier = dice.Item2;

        this.Op = op;
        this.Description = description;
    }

    private static (int[], int) ParseDiceSizes(string input)
    {
        List<int> diceSizes = new List<int>();
        int extraValue = 0;

        // Use regular expression to match the dice pattern
        var matches = Regex.Matches(input, @"(\d+)d(\d+)|(\d+)");

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success && match.Groups[2].Success)
            {
                // Dice notation like "NdM"
                int count = int.Parse(match.Groups[1].Value);
                int size = int.Parse(match.Groups[2].Value);

                for (int i = 0; i < count; i++)
                {
                    diceSizes.Add(size);
                }
            }
            else if (match.Groups[3].Success)
            {
                // Single value not associated with a dice size
                extraValue += int.Parse(match.Groups[3].Value);
            }
        }

        return (diceSizes.ToArray(), extraValue);
    }
}
