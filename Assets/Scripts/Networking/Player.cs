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

                // If connecting as host, convert any offline data to online data
                GameObject[] objs = GameObject.FindGameObjectsWithTag("OfflineData");
                for (int i = 0; i < objs.Length; i++) {
                    Vector3 position = objs[i].transform.position;
                    OfflineTokenData otd = objs[i].GetComponent<OfflineTokenData>();
                    Destroy(otd.TokenObject);
                    Player.CreateTokenData(otd.Json, position);
                }

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

    public static bool IsOnline() {
        return Self() != null;
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

    #region Create Token
    public static void CreateTokenData(string json, Vector3 position) {
        if (Player.IsOnline()) {
            Player.Self().CmdCreateTokenData(json, position);
        }
        else {
            GameObject tokenObj = new("OfflineData");
            tokenObj.transform.parent = GameObject.Find("TokenData").transform;
            tokenObj.tag = "OfflineData";
            tokenObj.transform.position = position;
            OfflineTokenData data = tokenObj.AddComponent<OfflineTokenData>();
            data.Json = json;
        }
    }
    [Command]
    public void CmdCreateTokenData(string json, Vector3 position) {
        GameObject g = GameSystem.Current().GetDataPrefab();
        NetworkServer.Spawn(g); 
        RpcInitTokenData(g, json, position);
    }
    [ClientRpc]
    public void RpcInitTokenData(GameObject g, string json, Vector3 position) {
        g.transform.parent = GameObject.Find("TokenData").transform;
        g.transform.position = position;
        GameSystem.Current().TokenSetup(g, json);
        // OnlineTokenDataRaw raw = JsonUtility.FromJson<OnlineTokenDataRaw>(json);
        // OnlineTokenData onlineData = g.GetComponent<OnlineTokenData>();
        // onlineData.Name = raw.Name;
        // onlineData.CurrentHP = raw.CurrentHP;
        // onlineData.MaxHP = raw.MaxHP;
        // onlineData.GraphicHash = raw.GraphicHash;
    }
    #endregion


    #region Token Movement
    public static void MoveToken(Token token, Vector3 v){
        if (Player.IsOnline()) {
            Player.Self().CmdMoveToken(token.onlineDataObject, 1, v);
        }
        else {
            MoveLerp.Create(token.offlineDataObject, 1, v);
        }
    }
    [Command]
    public void CmdMoveToken(GameObject g, float d, Vector3 v) {
        RpcMoveToken(g, d, v);
    }
    [ClientRpc]
    public void RpcMoveToken(GameObject g, float d, Vector3 v) {
        DoMoveToken(g, d, v);
    }
    private void DoMoveToken(GameObject g, float d, Vector3 v) {
        MoveLerp.Create(g, d, v);
    }
    #endregion

    #region Session Init
    [Command]
    public void CmdRequestSession() {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        RpcDrawMap(json);
    }
    [ClientRpc]
    public void RpcDrawMap(string json) {
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Toast.Add("Map loaded.");
        TimedReorgHack.Add();
    }
    #endregion

    #region Images
    [Command]
    public void CmdSendTextureChunks(string hash, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height)
    {
        RpcReceiveTextureChunks(hash, chunkIndex, chunkTotal, chunkColors, width, height);
    }
    [ClientRpc]
    public void RpcReceiveTextureChunks(string hash, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height){
        TextureSender.Receive(hash, chunkIndex, chunkTotal, chunkColors, width, height);
    }
    #endregion
}
