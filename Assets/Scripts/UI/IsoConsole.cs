using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class IsoConsole
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("IsoConsole");
        Modal.AddTextField("Console", "Console", "");
        Modal.AddPreferredButton("Execute", ConsoleExecute);
        Modal.AddButton("Close", CloseModal);
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }

    private static void ConsoleExecute(ClickEvent evt)
    {
        string command = UI.Modal.Q<TextField>("Console").value;
        UI.Modal.Q<TextField>("Console").Focus();
        // if (command.StartsWith("SelectedToken|") || command.StartsWith("ST|")) {
        //     if (command.StartsWith("SelectedToken|")) {
        //         command = command.Substring("SelectedToken|".Length);
        //     } else if (command.StartsWith("ST|")) {
        //         command = command.Substring("ST|".Length);
        //     }
        //     TokenData data = Token.GetSelectedData().GetComponent<TokenData>();
        //     Player.Self().CmdRequestTokenDataSetValue(data, command);
        //     Toast.Add("Console command executed on selected token.");
        // }
        // if (command.StartsWith("FocusedToken|") || command.StartsWith("FT|")) {
        //     if (command.StartsWith("FocusedToken|")) {
        //         command = command.Substring("FocusedToken|".Length);
        //     } else if (command.StartsWith("FT|")) {
        //         command = command.Substring("FT|".Length);
        //     }
        //     TokenData data = Token.GetFocusedData().GetComponent<TokenData>();
        //     Player.Self().CmdRequestTokenDataSetValue(data, command);
        //     Toast.Add("Console command executed on focused token.");
        // }
        // if (command.StartsWith("GameSystem|") || command.StartsWith("SYS|")) {
        //     if (command.StartsWith("GameSystem|")) {
        //         command = command.Substring("GameSystem|".Length);
        //     } else if (command.StartsWith("SYS|")) {
        //         command = command.Substring("SYS|".Length);
        //     }
        //     Player.Self().CmdRequestGameDataSetValue(command);
        //     Toast.Add("Console command executed on current game system.");
        // }
    }


}
