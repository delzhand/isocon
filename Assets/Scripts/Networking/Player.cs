using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
                FileLogger.Write("Player is host");
                Host = true;

                // If connecting as host, convert any offline data to online data
                GameObject[] objs = GameObject.FindGameObjectsWithTag("OfflineData");
                for (int i = 0; i < objs.Length; i++) {
                    Vector3 position = objs[i].transform.position;
                    OfflineTokenData otd = objs[i].GetComponent<OfflineTokenData>();
                    Destroy(otd.TokenObject);
                    Player.Self().CmdCreateTokenData(otd.Json, position);
                }
            }
            else {
                FileLogger.Write("Player is client");
            }
        } 

        // VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/ConnectedPlayer");
        // VisualElement instance = template.Instantiate();
        // UI.System.Q("PlayerList").Add(instance);
        // PlayerReference pRef = new GameObject(Name + " Reference").AddComponent<PlayerReference>();
        Toast.Add(Name + " connected.");
        // pRef.player = this;
        // pRef.visualElement = instance;

        if (isLocalPlayer) {
            // // Set up editing
            // instance.Q<Label>("PlayerName").RegisterCallback<ClickEvent>((evt) => {
            //     instance.Q<TextField>("PlayerNameEdit").value = Name;
            //     UI.ToggleDisplay(instance.Q("PlayerName"), false);
            //     UI.ToggleDisplay(instance.Q("PlayerNameEdit"), true);
            // });

            // // Change name
            // instance.Q<TextField>("PlayerNameEdit").RegisterCallback<BlurEvent>((evt) => {
            //     Name = instance.Q<TextField>("PlayerNameEdit").value;
            //     PlayerPrefs.SetString("PlayerName", Name);
            //     UI.ToggleDisplay(instance.Q("PlayerName"), true);
            //     UI.ToggleDisplay(instance.Q("PlayerNameEdit"), false);
            // });

            // Request session data from host
            CmdRequestMapSync();
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
    [Command]
    public void CmdCreateTokenData(string json, Vector3 position) {
        FileLogger.Write($"Client {connectionToClient.connectionId} created a token");
        GameObject g = GameSystem.Current().GetDataPrefab();
        NetworkServer.Spawn(g); 
        RpcInitTokenData(g, json, Guid.NewGuid().ToString(), position);
    }
    [ClientRpc]
    public void RpcInitTokenData(GameObject g, string json, string id, Vector3 position) {
        FileLogger.Write($"A token was initialized");
        g.transform.position = position;
        GameSystem.Current().TokenDataSetup(g, json, id);
    }
    #endregion

    #region Delete Token
    [Command]
    public void CmdRequestDeleteToken(TokenData data) {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested to delete token {data.Name}");
        RpcDeleteToken(data);
    }
    [ClientRpc]
    public void RpcDeleteToken(TokenData data) {
        FileLogger.Write($"Token {data.Name} was deleted");
        TokenData.DeleteById(data.Id);
        Toast.Add($"{data.Name} deleted.");
    }
    #endregion 

    #region Token Movement
    [Command]
    public void CmdMoveToken(GameObject dataObject, Vector3 v, bool immediate) {
        DoMoveToken(dataObject, v, immediate);
    }
    private void DoMoveToken(GameObject dataObject, Vector3 v, bool immediate) {
        dataObject.transform.localScale = Vector3.one;
        if (immediate) {
            dataObject.transform.position = v;
        }
        else {
            MoveLerp.Create(dataObject, v);
        }
    }
    #endregion

    #region Token Status
    [Command]
    public void CmdRequestPlaceToken(GameObject dataObject, Vector3 v) {
        dataObject.GetComponent<TokenData>().OnField = true;
        DoMoveToken(dataObject, v + new Vector3(0, 1f, 0), true);
        DoMoveToken(dataObject, v, false);
    }
    // [ClientRpc]
    // public void RpcPlaceToken(GameObject dataObject) {
    //     dataObject.GetComponent<TokenData>().OnField = true;
    // }
    [Command]
    public void CmdRequestGameDataSetValue(string label, int value) {
        RpcGameDataSetValue(label, value);
    }
    [ClientRpc]
    public void RpcGameDataSetValue(string label, int value) {
        GameSystem.Current().GameDataSetValue(label, value);
    }
    [Command]
    public void CmdRequestTokenDataSetValue(TokenData data, string label, int value) {
        RpcTokenDataSetValue(data, label, value);
    }
    [ClientRpc]
    public void RpcTokenDataSetValue(TokenData data, string label, int value) {
        GameSystem.Current().TokenDataSetValue(data, label, value);
    }
    [Command]
    public void CmdRequestTokenDataSetValue(TokenData data, string label, string value) {
        RpcTokenDataSetValue(data, label, value);
    }
    [ClientRpc]
    public void RpcTokenDataSetValue(TokenData data, string label, string value) {
        GameSystem.Current().TokenDataSetValue(data, label, value);
    }    
    #endregion

    #region MapChange
    [Command]
    public void CmdRequestMapSetValue(string[] blocks, string label, string value) {
        RpcMapSetValue(blocks, label, value);
    }
    [ClientRpc]
    public void RpcMapSetValue(string[] blocks, string label, string value) {
        if (label == "Effect") {
            for (int i = 0; i < blocks.Length; i++) {
                Block target = GameObject.Find(blocks[i]).GetComponent<Block>();
                target.EffectChange(value);
            }
        }
        TerrainController.SetInfo();
    }
    #endregion

    #region Dice Rolls
    [Command]
    public void CmdRequestDiceRoll(DiceTray tray) {
        for (int i = 0; i < tray.rolls.Length; i++) {
            tray.rolls[i].Rolled = 1 + Random.Range(0, tray.rolls[i].Die);
        }
        RpcDiceRoll(tray);
    }
    [ClientRpc]
    public void RpcDiceRoll(DiceTray tray) {
        DiceRoller.AddOutcome(tray);
    }
    #endregion

    #region Session Init
    [Command]
    public void CmdMapSync() {
        FileLogger.Write("Map sent to all clients");
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        byte[] compressedJson = Compression.CompressString(json);
        Debug.Log($"Original data size: {Encoding.UTF8.GetBytes(json).Length}, compressed data size: {compressedJson.Length}");
        RpcMapSync(compressedJson);
    }
    [Command]
    public void CmdRequestMapSync() {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested a map sync");
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        byte[] compressedJson = Compression.CompressString(json);
        TargetMapSync(connectionToClient, compressedJson);
    }
    [TargetRpc]
    public void TargetMapSync(NetworkConnectionToClient target, byte[] bytes) {
        FileLogger.Write($"Map received from host by this client");
        string json = Compression.DecompressString(bytes);
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Block.ToggleSpacers(false);
        Toast.Add("Map synced.");
    }

    [ClientRpc]
    public void RpcMapSync(byte[] bytes) {
        if (Player.IsGM()) {
            return; // GM already has current state
        }
        string json = Compression.DecompressString(bytes);
        FileLogger.Write($"Map received from host by everyone");
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Block.ToggleSpacers(false);
        Toast.Add("Map synced.");
    }
    #endregion

    #region Images
    [Command]
    public void CmdRequestImage(string hash) {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested image {TextureSender.TruncatedHash(hash)}");
        Texture2D graphic = TextureSender.LoadImageFromFile(hash, true);
        TextureSender.SendToClient(graphic, connectionToClient.connectionId);
    }

    [Command]
    public void CmdSendTextureChunk(string hash, int connectionId, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height)
    {
        // Send to host
        if (connectionId == -1) {
            TextureSender.Receive(hash, chunkIndex, chunkTotal, chunkColors, width, height);
        }

        // Send to that connection
        else {
            NetworkConnectionToClient targetClient = NetworkServer.connections[connectionId];
            TargetReceiveTextureChunk(targetClient, hash, chunkIndex, chunkTotal, chunkColors, width, height);
        }
    }

    [TargetRpc]
    public void TargetReceiveTextureChunk(NetworkConnectionToClient target, string hash, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height) {
        TextureSender.Receive(hash, chunkIndex, chunkTotal, chunkColors, width, height);
    }

    // [ClientRpc]
    // public void RpcReceiveTextureChunk(int connectionId, string hash, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height){
    //     FileLogger.Write($"Connection IDs: {GetCommaSeparatedKeys(NetworkServer.connections)}");
    //     NetworkConnection targetClient = NetworkServer.connections[connectionId];
    //     if (isLocalPlayer && connectionToClient == targetClient) {
    //         TextureSender.Receive(hash, chunkIndex, chunkTotal, chunkColors, width, height);
    //     }
    // }
    #endregion
}
