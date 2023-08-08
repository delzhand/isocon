using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerRole {
    GM,
    Player,
    Observer
}

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public PlayerRole Role;

    [SyncVar]
    public bool Host = false;

    void Awake() {
    }

    void Start()
    {
        if (isLocalPlayer) {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 1) {
                Role = PlayerRole.GM;
            }
            Name = PlayerPrefs.GetString("PlayerName", "New Player");
            if (NetworkServer.active && NetworkClient.active) {
                Host = true;
            }
        } 

        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/ConnectedPlayer");
        VisualElement instance = template.Instantiate();
        UI.System.Q("PlayerList").Add(instance);
        PlayerReference pRef = new GameObject(Name + " Reference").AddComponent<PlayerReference>();
        pRef.player = this;
        pRef.visualElement = instance;

        if (isLocalPlayer) {
            // Set up editing
            instance.Q<Label>("PlayerName").RegisterCallback<ClickEvent>((evt) => {
                instance.Q<TextField>("PlayerNameEdit").value = Name;
                UI.ToggleDisplay(instance.Q("PlayerName"), false);
                UI.ToggleDisplay(instance.Q("PlayerNameEdit"), true);
            });

            // Change name
            instance.Q<TextField>("PlayerNameEdit").RegisterCallback<BlurEvent>((evt) => {
                Name = instance.Q<TextField>("PlayerNameEdit").value;
                PlayerPrefs.SetString("PlayerName", Name);
                UI.ToggleDisplay(instance.Q("PlayerName"), true);
                UI.ToggleDisplay(instance.Q("PlayerNameEdit"), false);
            });

            // Request session data from host
            CmdRequestSession();

        }
    }

    [Command]
    public void CmdRequestSession() {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        RpcDrawMap(json);
    }

    // [Command]
    // public void CmdRequestAddToken(string name, string job, string jclass) {
    //     GameObject newToken = Instantiate(Resources.Load("Prefabs/ProtoToken") as GameObject);
    //     NetworkServer.Spawn(newToken);
    //     ProtoToken p = newToken.GetComponent<ProtoToken>();
    //     p.Name = name;
    //     p.Job = job;
    //     p.JClass = jclass;
    // }

    public static bool IsHost() {
        return NetworkServer.active && NetworkClient.active;
    }

    public static bool IsGM() {
        Player p = Self();
        if (p) {
            return p.Role == PlayerRole.GM;
        }
        return false;
    }

    public static Player Self() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject g in players) {
            if (g.GetComponent<Player>().isOwned) {
                return g.GetComponent<Player>();
            }
        }
        return null;
    }

    [ClientRpc]
    public void RpcDrawMap(string json) {
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Toast.Add("Map loaded.");
        TimedReorgHack.Add();
    }
}
