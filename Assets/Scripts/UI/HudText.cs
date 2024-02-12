using System.Collections;
using System.Collections.Generic;
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
    private static Dictionary<string, TextMeshPro> _items = new();
    private static Dictionary<HudTextColor, Material> _materials = new();

    public static void SetItem(string id, string text, HudTextColor color)
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
            _items.Add(id, gameObject.GetComponent<TextMeshPro>());
            UpdateItem(id, text, color);
            Rebuild();
        }
    }

    public static void RemoveItem(string id)
    {
        if (_items.ContainsKey(id))
        {
            GameObject.Destroy(_items[id].gameObject);
            _items.Remove(id);
            Rebuild();
        }
    }

    private static void UpdateItem(string id, string text, HudTextColor color)
    {
        _items[id].text = text;
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
        _items[id].GetComponent<MeshRenderer>().SetMaterials(mats);
    }

    private static void Rebuild()
    {
        float y = 5;
        foreach (var textMeshPro in _items.Values)
        {
            Vector3 position = new Vector3(-5, y, 10);
            textMeshPro.transform.localPosition = position;
            y -= .5f;
        }
    }
}
