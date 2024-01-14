using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public struct DiceTray
{
    public DiceRoll[] rolls;
    public int modifier;
    public string playerName;
    public string id;
    public DateTime time;
    public string op;
    public string description;

    public DiceTray(string playerName, DiceRoll[] rolls, int modifier, string op, string description) {
        this.playerName = playerName;
        this.rolls = rolls;
        this.modifier = modifier;
        this.op = op;
        this.description = description;
        time = DateTime.Now;
        id = Guid.NewGuid().ToString();
    }

    public DiceTray(string playerName, string roll, string op, string description) {
        this.playerName = playerName;
        List<DiceRoll> rolls = new();
        (int[],int) dice = ParseDiceSizes(roll);
        foreach (int die in dice.Item1) {
            rolls.Add(new DiceRoll(die));
        }
        this.rolls = rolls.ToArray();
        time = DateTime.Now;
        id = Guid.NewGuid().ToString();

        string[] split = roll.Split("+");
        string mod = split.Last<string>();
        modifier = dice.Item2;

        this.op = op;
        this.description = description;
    }

    private static (int[], int) ParseDiceSizes(string input)
    {
        List<int> diceSizes = new List<int>();

        // Define a regex pattern to match dice rolls like "2d20+3" or "3d8-2" or "1d6"
        string pattern = @"(\d+)d(\d+)([+\-]\d+)?";
        Match match = Regex.Match(input, pattern);

        // Check for invalid input
        if (!match.Success)
        {
            throw new ArgumentException("Invalid input format");
        }

        int dieSize = int.Parse(match.Groups[2].Value);
        int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        // Convert MatchCollection to List<Match> using LINQ
        for (int i = 0; i < int.Parse(match.Groups[1].Value); i++)
        {
            diceSizes.Add(dieSize);
        }

        return (diceSizes.ToArray(), modifier);
    }
}
