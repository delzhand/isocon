using UnityEngine;
using UnityEngine.UIElements;

public class Pill
{
    public static VisualElement InitStatic(string name, string text, Color color)
    {
        VisualElement p = UI.CreateFromTemplate("UITemplates/GameSystem/Pill");
        p.name = name;
        p.Q<Label>("Name").text = text;
        p.Q("Pill").AddToClassList("static");
        p.Q("Pill").style.backgroundColor = color;
        p.Query(null, "roundbutton").ForEach((v) =>
        {
            v.style.display = DisplayStyle.None;
        });
        return p;
    }

    public static VisualElement InitNumber(string name, string text, int number, Color color, bool forToken)
    {
        VisualElement p = UI.CreateFromTemplate("UITemplates/GameSystem/Pill");
        p.name = name;
        p.Q<Label>("Name").text = $"{text}  {number}";
        p.Q("Pill").style.backgroundColor = color;
        p.Q("Decrement").style.color = color;
        p.Q("Increment").style.color = color;
        p.Q("Pill").style.backgroundColor = color;
        p.Q("Remove").style.color = color;
        if (forToken)
        {
            p.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"IncrementTag|{name}");
            });
            p.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"DecrementTag|{name}");
            });
            p.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"RemoveTag|{name}");
            });
        }
        else
        {
            p.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestGameSystemCommand($"IncrementTag|{name}");
            });
            p.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestGameSystemCommand($"DecrementTag|{name}");
            });
            p.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) =>
            {
                Player.Self().CmdRequestGameSystemCommand($"RemoveTag|{name}");
            });
        }
        return p;
    }

}
