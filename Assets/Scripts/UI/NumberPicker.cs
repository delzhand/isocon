using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NumberPicker
{
    private static string NumberString = "";
    private static bool initialized = false;
    private static EventCallback<ClickEvent> Callback;
    private static bool negative = false;

    private static void AddDigit(ClickEvent evt, int i)
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
        UI.NumberPicker.Q<TextField>("ValueField").value = NumberString;
    }

    private static void RemoveDigit(ClickEvent evt)
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
        UI.NumberPicker.Q<TextField>("ValueField").value = NumberString;
    }

    private static void BindCallbacks()
    {
        for (int i = 0; i <= 9; i++)
        {
            UI.NumberPicker.Q<Button>($"Digit{i}").RegisterCallback<ClickEvent, int>(AddDigit, i);
        }

        UI.NumberPicker.Q("DigitAdd").RegisterCallback<ClickEvent>(SetPositive);
        UI.NumberPicker.Q("DigitSub").RegisterCallback<ClickEvent>(SetNegative);

        UI.NumberPicker.Q<Button>("DigitBack").RegisterCallback<ClickEvent>(RemoveDigit);
        UI.NumberPicker.Q<Button>("Exit").RegisterCallback<ClickEvent>((evt) =>
        {
            Close();
        });
    }

    public static void Open(bool allowNeg, EventCallback<ClickEvent> numberCommandCallback)
    {
        if (!initialized)
        {
            BindCallbacks();
        }
        Callback = numberCommandCallback;
        UI.NumberPicker.Q("DigitAdd").RegisterCallback<ClickEvent>(numberCommandCallback);
        UI.NumberPicker.Q("DigitSub").RegisterCallback<ClickEvent>(numberCommandCallback);
        UI.ToggleDisplay("DigitSub", allowNeg);
        UI.NumberPicker.Q("DigitAdd").Q<Label>().text = allowNeg ? "ADD" : "ENTER";
        UI.ToggleDisplay("NumberPickerModal", true);
        UI.ToggleDisplay("Backdrop", true);
        NumberString = "0";
        UpdateValue();
    }

    public static void Close()
    {
        UI.NumberPicker.Q("DigitAdd").UnregisterCallback<ClickEvent>(Callback);
        UI.NumberPicker.Q("DigitSub").UnregisterCallback<ClickEvent>(Callback);
        UI.ToggleDisplay("NumberPickerModal", false);
        UI.ToggleDisplay("Backdrop", false);
    }

    public static int GetNumber()
    {
        string value = UI.NumberPicker.Q<TextField>("ValueField").value;
        int ivalue = int.Parse(value);
        return negative ? -ivalue : ivalue;
    }

    private static void SetNegative(ClickEvent evt)
    {
        negative = true;
    }

    private static void SetPositive(ClickEvent evt)
    {
        negative = false;
    }

    public static void TokenCommand(string command, bool allowNeg = true)
    {
        SelectionMenu.Hide();
        NumberPicker.Open(allowNeg, (evt) => TokenCommandCallback(evt, command));
    }

    private static void TokenCommandCallback(ClickEvent evt, string command)
    {
        int v = NumberPicker.GetNumber();
        NumberPicker.Close();
        Player.Self().CmdRequestTokenDataCommand(Actor.GetSelected().Data.Id, $"{command}|{v}");
    }

    public static void AllTokensCommand(string command, bool allowNeg = true)
    {
        SelectionMenu.Hide();
        NumberPicker.Open(allowNeg, (evt) => AllTokensCommandCallback(evt, command));
    }

    private static void AllTokensCommandCallback(ClickEvent evt, string command)
    {
        int v = NumberPicker.GetNumber();
        NumberPicker.Close();
        Player.Self().CmdRequestAllTokenDataCommand($"{command}|{v}");
    }

}