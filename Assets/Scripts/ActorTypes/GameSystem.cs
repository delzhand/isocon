using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Unity.VisualScripting;

[Serializable]
public class GameSystemData
{
    public List<GameSystemTag> Tags;
}

[Serializable]
public class GameSystemTag
{
    public string Name;
    public int Value;
    public int MaxValue;
    public Color Color;
    public string Type; // Simple, Clock, Value
}

public class GameSystem : MonoBehaviour
{
    public string SystemData;
    public List<GameSystemTag> Tags;

    public static GameSystem Current()
    {
        return GameObject.Find("GameSystem").GetComponent<GameSystem>();
    }

    public void Command(string value)
    {
        if (value.StartsWith("AddTag"))
        {
            string[] parts = value.Split("|");
            GameSystemTag tag = JsonUtility.FromJson<GameSystemTag>(parts[1]);
            Tags.Add(tag);
            UpdateTags();
        }
        if (value.StartsWith("IncrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], 1);
        }
        if (value.StartsWith("DecrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], -1);
        }
        if (value.StartsWith("RemoveTag"))
        {
            string[] parts = value.Split("|");
            RemoveTag(parts[1]);
        }
    }

    private void CounterTag(string name, int num)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        Tags[i].Value += num;
        if (Tags[i].MaxValue > 0)
        {
            Tags[i].Value = Math.Max(0, Math.Min(Tags[i].Value, Tags[i].MaxValue));
        }
        UpdateTags();
    }

    private void RemoveTag(string name)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        Tags.RemoveAt(i);
        UpdateTags();
    }

    private void UpdateTags()
    {
        UI.System.Q("TopRight").Q("Pills").Q("Wrapper").Clear();
        foreach (GameSystemTag tag in Tags)
        {
            VisualElement p = null;
            switch (tag.Type)
            {
                case "Simple":
                    p = Pill.InitStatic(tag.Name, tag.Name, tag.Color);
                    break;
                case "Number":
                    p = Pill.InitNumber(tag.Name, tag.Name, tag.Value, 0, tag.Color, false);
                    break;
                case "Clock":
                    p = Pill.InitNumber(tag.Name, tag.Name, tag.Value, tag.MaxValue, tag.Color, false);
                    break;
            }
            UI.System.Q("TopRight").Q("Pills").Q("Wrapper").Add(p);
        }
    }

    public void ClearTags()
    {
        Tags.Clear();
        UI.System.Q("TopRight").Q("Pills").Q("Wrapper").Clear();
    }
}