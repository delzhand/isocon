using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PopoverText : MonoBehaviour
{
    private static float Offset;
    private Token Token;
    private string Text;
    private List<Label> Elements;
    private float timer;
    private static float duration = .15f;

    public float spacing = 22f;
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < -1.5f) {
            for (int i = 0; i < Elements.Count; i++) {
                UI.System.Q("Worldspace").Remove(Elements[i]);
            }
            GameObject.Destroy(gameObject);
        }

        float totalWidth = spacing * (Elements.Count - 1);
        for (int i = 0; i < Elements.Count; i++) {
            float percentage = 1 - Mathf.Clamp((timer + (i/8f))/duration, 0, 1);
            float height = -20 - (Mathf.Sin(Mathf.PI * percentage) * 20);
            Elements[i].style.fontSize = Mathf.Lerp(2, 32, percentage);
            Vector2 offset = new Vector2(spacing * i - (totalWidth/2f), height);
            UI.FollowToken(Token, Elements[i], Camera.main, offset, true);
        }
    }

    public static void Create(Token token, string text, Color color) {
        GameObject g = new GameObject($"Popover {text}");
        PopoverText p = g.AddComponent<PopoverText>();
        p.timer = duration;
        p.Token = token;
        p.Elements = new();
        foreach (char character in text) {
            Label element = new Label(character.ToString());
            element.AddToClassList("centered");
            element.AddToClassList("popover");
            element.style.color = color;
            UI.System.Q("Worldspace").Add(element);
            p.Elements.Add(element);
        }
    }
}
