using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StatusEffect
{
    public string Name;
    public string Type;
    public string Color;
    public int Number;
    public bool Locked;

    public string Write() {
        return $"{Name},{Type},{Color},{Number},{Locked}";
    }

    public static StatusEffect Parse(string effect) {
        string[] split = effect.Split(",");
        return new StatusEffect(){
            Name = split[0],
            Type = split[1],
            Color = split[2],
            Number = int.Parse(split[3]),
            Locked = bool.Parse(split[4])
        };
    }


}
