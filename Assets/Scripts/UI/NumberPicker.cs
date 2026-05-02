using System;
using System.Collections;
using System.Collections.Generic;
using ShunUI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NumberPicker
{
    private static string NumberString = "";
    private static EventCallback<ClickEvent> Callback;
    private static bool negative = false;

    private static void AddDigit(int i)
    {
        string s = $"{i}";
        if (NumberString.Length == 1 && NumberString == "0")
        {
            NumberString = s;
        }
        else
        {
            NumberString += s;
        }
        var context = Modal2.Contents("PrimaryDialog");
        context.Q<ShunInput>("Value").value = NumberString;
    }

    private static void RemoveDigit()
    {
        if (NumberString.Length > 0)
        {
            NumberString = NumberString.Substring(0, NumberString.Length - 1);
        }
        if (NumberString.Length == 0)
        {
            NumberString = "0";
        }
        UpdateValue();
    }

    private static void UpdateValue()
    {
        var context = Modal2.Contents("PrimaryDialog");
        context.Q<ShunInput>("Value").value = NumberString;
    }

    public static void Open(bool allowNeg, Action numberCommand)
    {
        Modal2.CreateContext("PrimaryDialog");
        var contents = Modal2.Contents("PrimaryDialog");

        var digitWrapper = new VisualElement();
        digitWrapper.style.flexDirection = FlexDirection.Row;
        digitWrapper.style.flexWrap = Wrap.Wrap;
        digitWrapper.style.justifyContent = Justify.SpaceBetween;
        digitWrapper.style.marginTop = -12;
        contents.Add(digitWrapper);

        var controlWrapper = new VisualElement();
        controlWrapper.style.flexDirection = FlexDirection.Row;
        controlWrapper.style.justifyContent = Justify.SpaceBetween;
        contents.Add(controlWrapper);

        // Why this doesn't work I'll never know
        // int[] numbers = new int[] { 7, 8, 9, 4, 5, 6, 1, 2, 3, 0 };

        List<int> numbers = new();
        numbers.Add(7);
        numbers.Add(8);
        numbers.Add(9);

        numbers.Add(4);
        numbers.Add(5);
        numbers.Add(6);

        numbers.Add(1);
        numbers.Add(2);
        numbers.Add(3);

        numbers.Add(0);

        for (int i = 0; i < numbers.Count; i++)
        {
            int num = numbers[i];
            var digitButton = new ShunButton($"{num}", ButtonVariant.Outline);
            SetButtonStyle(digitButton);
            digitButton.clicked += () =>
            {
                AddDigit(num);
            };
            digitWrapper.Add(digitButton);
        }

        var backspace = new ShunButton("◀", ButtonVariant.Outline);
        backspace.clicked += () =>
        {
            RemoveDigit();
        };
        SetButtonStyle(backspace);
        backspace.style.flexBasis = Length.Percent(65);
        digitWrapper.Add(backspace);

        var addButton = new ShunButton("Add", ButtonVariant.Primary);
        SetButtonStyle(addButton);
        addButton.clicked += () =>
        {
            SetPositive();
            numberCommand();
        };
        controlWrapper.Add(addButton);

        var valueInput = new ShunInput();
        valueInput.name = "Value";
        valueInput.style.marginBottom = 0;
        valueInput.style.marginTop = 12;
        valueInput.style.marginLeft = 0;
        valueInput.style.marginRight = 0;
        valueInput.Q<TextElement>().style.fontSize = 20;
        valueInput.style.flexBasis = Length.Percent(30);
        controlWrapper.Add(valueInput);

        if (allowNeg)
        {
            var subButton = new ShunButton("Sub", ButtonVariant.Primary);
            SetButtonStyle(subButton);
            subButton.clicked += () =>
            {
                SetNegative();
                numberCommand();
            };
            controlWrapper.Add(subButton);
        }
        else
        {
            addButton.style.flexBasis = Length.Percent(65);
            addButton.text = "Enter";
        }

        NumberString = "0";
        UpdateValue();

        Modal2.Open("Number Picker");
    }

    private static void SetButtonStyle(VisualElement v)
    {
        v.style.flexBasis = Length.Percent(30);
        v.style.paddingBottom = 20;
        v.style.paddingTop = 20;
        v.style.marginTop = 12;
        v.style.fontSize = 20;
    }

    public static int GetNumber()
    {
        Modal2.ReadContext("PrimaryDialog");
        string value = Modal2.GetTextFieldValue("Value");
        int ivalue = int.Parse(value);
        return negative ? -ivalue : ivalue;
    }

    private static void SetNegative()
    {
        negative = true;
    }

    private static void SetPositive()
    {
        negative = false;
    }

    public static void ActorCommand(string command, bool allowNeg = true)
    {
        SelectionMenu.Hide();
        NumberPicker.Open(allowNeg, () => TokenCommandCallback(command));
    }

    private static void TokenCommandCallback(string command)
    {
        int v = NumberPicker.GetNumber();
        Modal2.Close();
        Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"{command}|{v}");
    }

    public static void AllTokensCommand(string command, bool allowNeg = true)
    {
        SelectionMenu.Hide();
        NumberPicker.Open(allowNeg, () => AllTokensCommandCallback(command));
    }

    private static void AllTokensCommandCallback(string command)
    {
        int v = NumberPicker.GetNumber();
        Modal2.Close();
        Player.Self().CmdRequestAllActorsCommand($"{command}|{v}");
    }

}