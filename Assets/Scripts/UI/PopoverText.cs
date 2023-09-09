using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PopoverText : MonoBehaviour
{
    private Token Token;
    private VisualElement Element;
    private float timer;
    private static float duration = .15f;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < -1.5f) {
            UI.System.Q("Worldspace").Remove(Element);
            GameObject.Destroy(gameObject);
        }

        for (int i = 0; i < Element.childCount; i++) {
            VisualElement child = Element.Children().ToArray()[i];
            float percentage = 1 - Mathf.Clamp((timer + (i/8f))/duration, 0, 1);
            float distance = Mathf.Sin(Mathf.PI * percentage) * 20;
            child.style.left = distance * .5f;
            child.style.bottom = distance;
            child.style.scale = new Scale(Vector2.Lerp(Vector2.zero, Vector2.one, percentage));
        }

        UI.FollowToken(Token, Element, Camera.main, new Vector2(0, -18), true);
    }

    public static void Create(Token token, string text, Color color) {
        // String is split on |
        // A leading / means large, sub-split characters
        // A leading = means medium text
        // A leading _ means small text

        GameObject g = new GameObject($"Popover {text}");
        PopoverText p = g.AddComponent<PopoverText>();
        p.timer = duration;
        p.Token = token;
        p.Element = new VisualElement();
        p.Element.AddToClassList("centered");
        p.Element.AddToClassList("popover");
        p.Element.style.color = color;
        UI.System.Q("Worldspace").Add(p.Element);

        string[] parts = text.Split("|");
        foreach (string part in parts) {
            switch (part[0]) {
                case '_':
                case '=':
                    Label partLabel = new();
                    partLabel.text = part[1..];
                    partLabel.style.color = color;
                    p.Element.Add(partLabel);
                    if (part[0] == '_') partLabel.AddToClassList("small");
                    if (part[0] == '=') partLabel.AddToClassList("medium");
                    break; 
                case '/':
                    foreach (char c in part[1..]) {
                        Label cLabel = new();
                        cLabel.text = $"{c}";
                        cLabel.style.color = color;
                        cLabel.AddToClassList("large");
                        p.Element.Add(cLabel);
                    }
                    break;
            }
        }
    }
}
