using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class Session
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("Session");
        Modal.AddCloseCallback(BackToNeutral);

        if (NetworkClient.activeHost)
        {
            Modal.AddColumns("Tabs", 2);
            Modal.AddContentButton("LoadSettings", "Load", (evt) =>
            {
                AddLoadFields();
            });
            Modal.MoveToColumn("Tabs_0", "LoadSettings");
            Modal.AddContentButton("SaveSettings", "Save", (evt) =>
            {
                AddSaveFields();
            });
            Modal.MoveToColumn("Tabs_1", "SaveSettings");
            Modal.AddColumns("Fields", 1);
            UI.Modal.Q("Tabs").style.justifyContent = Justify.Center;
            AddLoadFields();
        }
        else
        {
            AddSaveFields();
        }
    }

    private static void ClearFields()
    {
        VisualElement v = UI.Modal.Q("Contents").Q("Fields_0");
        if (v != null)
        {
            v.Clear();
            Modal.ResetPreferredButtons();
        }
        Modal.AddButton("Cancel", CloseModal);
    }

    private static void AddLoadFields()
    {
        ClearFields();
        Modal.AddFileField("SessionFile", "Session File", "", "sessions", null);
        Modal.AddContentButton("LoadSession", "Load Session", LoadSession);
        Modal.MoveToColumn("Fields_0", "SessionFile");
        Modal.MoveToColumn("Fields_0", "LoadSession");
    }

    private static void AddSaveFields()
    {
        ClearFields();
        Modal.AddTextField("SaveFileName", "Filename", "latest.json");
        Modal.AddContentButton("SaveSession", "Save Session", SaveSession);
        if (NetworkClient.activeHost)
        {
            Modal.MoveToColumn("Fields_0", "SaveFileName");
            Modal.MoveToColumn("Fields_0", "SaveSession");
        }
    }

    private static void BackToNeutral(ClickEvent evt)
    {
        StateManager.Find().ChangeSubState(new NeutralState());
    }

    private static void LoadSession(ClickEvent evt)
    {
        string filename = UI.Modal.Q("SessionFile").Q<TextField>("File").value;
        DeserializeSession(filename);
        Modal.Close();
    }

    private static void SaveSession(ClickEvent evt)
    {
        string filename = UI.Modal.Q<TextField>("SaveFileName").value;
        SerializeSession(filename);
        Modal.Close();
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }

    public static void SerializeSession(string filename)
    {
        List<ActorPersistence> a = new();
        GameObject[] actors = GameObject.FindGameObjectsWithTag("ActorData");

        for (int i = 0; i < actors.Length; i++)
        {
            a.Add(actors[i].GetComponent<ActorData>().Persist());
        }

        SessionPersistence sp = new();
        sp.Actors = a.ToArray();
        sp.State = State.GetStateFromScene();
        sp.Tags = GameSystem.Current().Tags.ToArray();
        string session = JsonUtility.ToJson(sp);
        WriteSessionToFile(session, filename);
    }

    public static void WriteSessionToFile(string session, string filename)
    {
        string path = Preferences.Current.DataPath;
        if (!Directory.Exists(path + "/sessions"))
        {
            Directory.CreateDirectory(path + "/sessions");
        }

        File.WriteAllText($"{path}/sessions/{filename}", session);
        Toast.AddSuccess($"Session saved to {filename}.");
    }

    public static void DeserializeSession(string filename)
    {
        string session = File.ReadAllText(filename);
        SessionPersistence sp = JsonUtility.FromJson<SessionPersistence>(session);

        string json = "";
        // This runs immediately, locally, whereas the Cmd to delete all runs later async
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("ActorData"))
        {
            ActorData data = g.GetComponent<ActorData>();
            data.Deletable = true;
        }
        Player.Self().CmdRequestDeleteAllActors();
        foreach (ActorPersistence ap in sp.Actors)
        {
            // Reserialize for network transmission
            json = JsonUtility.ToJson(ap);
            Player.Self().CmdCreateActor(json);
        }

        json = JsonUtility.ToJson(sp.State);
        Actor.MoveAllActorsToOptimalBlock();
        Player.Self().CmdMapSync(Compression.CompressString(json));

        // Player.Self().CmdRequestClientInit();

        foreach (GameSystemTag gst in sp.Tags)
        {
            // Reserialize for network transmission
            json = JsonUtility.ToJson(gst);
            Player.Self().CmdRequestGameSystemCommand($"AddTag|{json}");
        }
    }
}

[Serializable]
public class SessionPersistence
{
    public GameSystemTag[] Tags;
    public ActorPersistence[] Actors;
    public State State;
}