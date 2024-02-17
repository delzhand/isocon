using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum HudTextColor
{
    Blue,
    Green,
    Yellow,
    Red,
    Grey
}

public class HudText
{
    private static Dictionary<string, (TextMeshPro, float)> _items = new();
    private static Dictionary<HudTextColor, Material> _materials = new();

    public static void SetItem(string id, string text, float weight, HudTextColor color)
    {
        if (_items.ContainsKey(id))
        {
            UpdateItem(id, text, color);
            Rebuild();
        }
        else
        {
            var gameObject = GameObject.Instantiate(Resources.Load("Prefabs/HudText") as GameObject);
            gameObject.transform.SetParent(GameObject.Find("HudCamera").transform);
            var textMeshPro = gameObject.GetComponent<TextMeshPro>();
            _items.Add(id, (textMeshPro, weight));
            UpdateItem(id, text, color);
            Rebuild();
        }
    }

    public static void RemoveItem(string id)
    {
        if (_items.ContainsKey(id))
        {
            GameObject.Destroy(_items[id].Item1.gameObject);
            _items.Remove(id);
            Rebuild();
        }
    }

    private static void UpdateItem(string id, string text, HudTextColor color)
    {
        _items[id].Item1.text = text;
        Material m = null;
        if (_materials.ContainsKey(color))
        {
            m = _materials[color];
        }
        else
        {
            m = Resources.Load<Material>($"Materials/Text/{color}HudText");
            _materials[color] = m;
        }
        List<Material> mats = new List<Material>();
        mats.Add(m);
        _items[id].Item1.GetComponent<MeshRenderer>().SetMaterials(mats);
    }

    private static void Rebuild()
    {
        float y = 5;
        var sortedValues = _items.Values.OrderBy(value => value.Item2);
        foreach (var item in sortedValues)
        {
            var textMeshPro = item.Item1;
            Vector3 position = new Vector3(-5, y, 10);
            textMeshPro.transform.localPosition = position;
            y -= .5f;
        }
    }
}
