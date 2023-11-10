using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DiceTray
{
    public DiceRoll[] rolls;
    public string playerName;
    public string id;
    public DateTime time;

    public DiceTray(string playerName, DiceRoll[] rolls) {
        this.playerName = playerName;
        this.rolls = rolls;
        time = DateTime.Now;
        id = Guid.NewGuid().ToString();
    }
}
