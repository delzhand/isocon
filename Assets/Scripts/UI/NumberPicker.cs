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
        UI.NumberPicker.Q<Button>("DigitBack").RegisterCallback<ClickEvent>(RemoveDigit);
        UI.NumberPicker.Q<Button>("Exit").RegisterCallback<ClickEvent>((evt) =>
        {
            Close();
        });
    }

    public static void Open(EventCallback<ClickEvent> numberCommandCallback)
    {
        Callback = numberCommandCallback;
        UI.NumberPicker.Q("DigitEnter").RegisterCallback<ClickEvent>(numberCommandCallback);
        UI.ToggleDisplay("NumberPickerModal", true);
        UI.ToggleDisplay("Backdrop", true);
        NumberString = "0";
        UpdateValue();
        if (!initialized)
        {
            BindCallbacks();
        }
    }

    public static void Close()
    {
        UI.NumberPicker.Q("DigitEnter").UnregisterCallback<ClickEvent>(Callback);
        UI.ToggleDisplay("NumberPickerModal", false);
        UI.ToggleDisplay("Backdrop", false);
    }

    public static int GetNumber()
    {
        return int.Parse(NumberString);
    }

    public static void NumberCommand(string command)
    {
        SelectionMenu.Hide();
        NumberPicker.Open((evt) => NumberCommandCallback(evt, command));
    }

    private static void NumberCommandCallback(ClickEvent evt, string command)
    {
        int v = NumberPicker.GetNumber();
        NumberPicker.Close();
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"{command}|{v}");
    }

}