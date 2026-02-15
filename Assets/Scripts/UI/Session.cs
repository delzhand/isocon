using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Session
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("Session");
        Modal.AddMarkup("SnapshotDesc", "Session snapshots capture the current status and position of all tokens, but not the map state.");
        Modal.AddFileField("SessionFile", "Session File", "", "sessions");
        Modal.AddButton("Save Session", SaveSession);
        Modal.AddButton("Load Session", LoadSession);
        Modal.AddButton("Cancel", CloseModal);
    }

    private static void LoadSession(ClickEvent evt)
    {
        string filename = UI.Modal.Q("SessionFile").Q<TextField>("File").value;
        GameSystem.Current().DeserializeSession(filename);
        Modal.Close();
    }

    private static void SaveSession(ClickEvent evt)
    {
        string filename = UI.Modal.Q("SessionFile").Q<TextField>("File").value;
        GameSystem.Current().SerializeSession(filename);
        Modal.Close();
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }
}
