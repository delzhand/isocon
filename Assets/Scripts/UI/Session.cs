using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Session
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("Session");
        Modal.AddMarkup("SnapshotDesc", "Saving a session snapshot will preserve all tokens including current status and position. It does not save the current map or state.");
        Modal.AddTextField("SnapshotFile", "Filename", "snapshot.json");
        Modal.AddContentButton("Snapshot", "Save Session Snapshot", SaveSnapshot);
        Modal.AddSeparator();

        Modal.AddMarkup("LoadSessionDesc", "Loading a session snapshot will delete all existing tokens and load the tokens from the file.");
        Modal.AddFileField("SessionFile", "Session File", "", "sessions");
        Modal.AddContentButton("SessionLoad", "Load Session", LoadSession);

        Modal.AddButton("Cancel", CloseModal);
    }

    private static void LoadSession(ClickEvent evt)
    {
        string filename = UI.Modal.Q("SessionFile").Q<TextField>("File").value;
        GameSystem.Current().DeserializeSession(filename);
        Modal.Close();
    }

    private static void SaveSnapshot(ClickEvent evt)
    {
        string filename = UI.Modal.Q<TextField>("SnapshotFile").value;
        GameSystem.Current().SerializeSession(filename);
        Modal.Close();
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }
}
