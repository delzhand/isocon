using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionManager
{
    public static void Save()
    {
        FileBrowserHelper.Open(SaveSession, "", FileBrowserType.Sessions, true);
    }

    public static void Load()
    {
        FileBrowserHelper.Open(LoadSession, "", FileBrowserType.Sessions, false);
    }

    public static void SaveSession()
    {
        string filename = FileBrowser.Result.First<string>();
        SerializeSession(filename);
    }

    private static void LoadSession()
    {
        string filename = FileBrowser.Result.First<string>();
        DeserializeSession(filename);
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
        File.WriteAllText(filename, session);
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

        Player.Self().CmdRequestGameSystemCommand($"ClearTags");
        foreach (GameSystemTag gst in sp.Tags)
        {
            // Reserialize for network transmission
            json = JsonUtility.ToJson(gst);
            Player.Self().CmdRequestGameSystemCommand($"AddTag|{json}");
        }
    }

    public static void LauncherMap()
    {
        string path = Preferences.Current.DataPath;
        string filename = $"{path}/sessions/autosave.json";
        if (File.Exists(filename))
        {
            string session = File.ReadAllText(filename);
            SessionPersistence sp = JsonUtility.FromJson<SessionPersistence>(session);
            State.SetSceneFromState(sp.State);
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