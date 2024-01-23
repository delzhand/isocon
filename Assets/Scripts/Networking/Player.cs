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
            Name = PlayerPrefs.GetString("PlayerName", "New Player");

            if (GameObject.FindGameObjectsWithTag("Player").Length == 1) {
                Role = PlayerRole.GM;
            }

            if (NetworkServer.active && NetworkClient.active) {
                FileLogger.Write("Player is host");
                Host = true;
            }
            else {
                FileLogger.Write("Player is client");
                CmdRequestClientInit();
            }
        } 

        Toast.AddSimple(Name + " connected.");
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

    public void SetOp(string op) {
        GetComponent<CurrentOp>().Show(op);
    }

    public void ClearOp() {
        GetComponent<CurrentOp>().Hide();
    }

    #region Client Kickoff
    [Command]
    public void CmdRequestClientInit() {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested game system");
        string system = PlayerPrefs.GetString("System", "Generic");
        string systemVars = GameSystem.Current().GetSystemVars();
        TargetGameSystem(connectionToClient, system, systemVars);
        TargetGridType(connectionToClient, TerrainController.GridType);
    }
    [TargetRpc]
    public void TargetGameSystem(NetworkConnectionToClient target, string system, string systemVars) {
        FileLogger.Write($"Local game system set to {system}");
        GameSystem.Set(system);
        GameSystem.Current().SetSystemVars(systemVars);
    }
    [TargetRpc]
    public void TargetGridType(NetworkConnectionToClient target, string grid) {
        FileLogger.Write($"Local grid type set to {grid}");
        TerrainController.GridType = grid;
        CmdRequestMapSync();
    }

    #endregion

    #region Create Token
    [Command]
    public void CmdCreateToken(string system, string graphicHash, string name, int size, Color color, string systemData) {
        string id = Guid.NewGuid().ToString();
        GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/TokenData2"));
        TokenData data = g.GetComponent<TokenData>();
        data.Id = id;
        data.System = system;
        data.GraphicHash = graphicHash;
        data.Name = name;
        data.Size = size;
        data.Color = color;
        data.SystemData = systemData;
        NetworkServer.Spawn(g);
    }
    #endregion

    #region Delete Token
    [Command]
    public void CmdRequestDeleteToken(string tokenId) {
        TokenData data = TokenData.Find(tokenId);
        FileLogger.Write($"Client {connectionToClient.connectionId} requested to delete token {data.Name}");
        RpcDeleteToken(tokenId);
        data.Destroyed = true;
    }
    [ClientRpc]
    public void RpcDeleteToken(string tokenId) {
        TokenData data = TokenData.Find(tokenId);
        data.Delete();
        FileLogger.Write($"Token {data.Name} was deleted");
        Toast.AddSuccess($"{data.Name} deleted.");
    }
    #endregion 

    #region Token Movement
    [Command]
    public void CmdMoveToken(string tokenId, Vector3 v, bool immediate) {
        RpcDoMoveToken(tokenId, v, immediate);
    }

    [Command]
    public void CmdRequestPlaceToken(string tokenId, Vector3 v) {
        RpcPlaceToken(tokenId, true);
        RpcDoMoveToken(tokenId, v + new Vector3(0, 1f, 0), true);
        RpcDoMoveToken(tokenId, v, false);
    }
    [Command]
    public void CmdRequestRemoveToken(string tokenId) {
        RpcPlaceToken(tokenId, false);
        RpcDoMoveToken(tokenId, new Vector3(0, -10f, 0), true);
    }

    [ClientRpc]
    private void RpcDoMoveToken(string tokenId, Vector3 v, bool immediate) {
        TokenData data = TokenData.Find(tokenId);
        data.LastKnownPosition = v;
        if (immediate) {
            data.WorldObject.transform.position = v;
        }
        else {
            MoveLerp.Create(data.WorldObject, v);
        }
    }

    [ClientRpc]
    private void RpcPlaceToken(string tokenId, bool place) {
        TokenData data = TokenData.Find(tokenId);
        data.Place(place);
    }
    #endregion

    #region Token Status
    [Command]
    public void CmdRequestGameDataSetValue(string value) {
        RpcGameDataSetValue(value);
    }
    [ClientRpc]
    public void RpcGameDataSetValue(string value) {
        GameSystem.Current().GameDataSetValue(value);
    }
    [Command]
    public void CmdRequestTokenDataSetValue(string tokenId, string value) {
        RpcTokenDataSetValue(tokenId, value);
    }
    [ClientRpc]
    public void RpcTokenDataSetValue(string tokenId, string value) {
        GameSystem.Current().TokenDataSetValue(tokenId, value);
    }    
    #endregion

    #region MapChange
    [Command]
    public void CmdRequestMapSetValue(string[] blocks, string label, string value) {
        RpcMapSetValue(blocks, label, value);
    }
    [ClientRpc]
    public void RpcMapSetValue(string[] blocks, string label, string value) {
        FileLogger.Write($"{label}: {value}");
        if (label == "Effect") {
            for (int i = 0; i < blocks.Length; i++) {
                Block target = GameObject.Find(blocks[i]).GetComponent<Block>();
                target.EffectChange(value);
            }
        }
        else if (label == "RemoveEffect") {
            for (int i = 0; i < blocks.Length; i++) {
                Block target = GameObject.Find(blocks[i]).GetComponent<Block>();
                target.EffectRemove(value);
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
    [Command]
    public void CmdShareDiceRoll(string description, string result, string rolls, int die) {
        RpcDiceRoll(description, result, rolls, die);
    }
    [ClientRpc]
    public void RpcDiceRoll(DiceTray tray) {
        DiceRoller.AddOutcome(tray);
    }
    [ClientRpc]
    public void RpcDiceRoll(string description, string result, string rolls, int die) {
        DiceRoller.AddOutcome(description, result, rolls, die);
    }
    #endregion

    #region Session Init
    [Command]
    public void CmdMapSync() {
        FileLogger.Write("Map sent to all clients");
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        byte[] compressedJson = Compression.CompressString(json);
        // Debug.Log($"Original data size: {Encoding.UTF8.GetBytes(json).Length}, compressed data size: {compressedJson.Length}");
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
        Toast.AddSimple("Map synced.");
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
        Toast.AddSimple("Map synced.");
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
