using System;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PlayerRole
{
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
    public int PercentSynced = 100;

    [SyncVar]
    public bool Host = false;

    [SyncVar]
    public int MaxConnections = 1;

    void Start()
    {
        if (isLocalPlayer)
        {
            Name = Preferences.Current.PlayerName;
            if (NetworkServer.active && NetworkClient.active)
            {
                Host = true;
                Toast.AddSimple("Connected as host.");
                MaxConnections = GameObject.Find("NetworkController").GetComponent<NetworkManager>().maxConnections;
            }
            else
            {
                Toast.AddSimple("Connected as player.");
                CmdRequestClientInit();
            }
        }
    }

    public static Player Self()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject g in players)
        {
            if (g.GetComponent<Player>().isOwned)
            {
                return g.GetComponent<Player>();
            }
        }
        return null;
    }

    public void SetOp(string op)
    {
        GetComponent<CurrentOp>().Show(op);
    }

    public void ClearOp()
    {
        GetComponent<CurrentOp>().Hide();
    }

    #region Client Kickoff
    [Command]
    public void CmdRequestClientInit()
    {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested a system sync");
        string grid = TerrainController.GridType;
        BlockRendering.ToggleHex(grid == "Hex");
        GameSystem.Current().ClearTags();
        TargetClientInit(connectionToClient, grid);
    }

    [TargetRpc]
    public void TargetClientInit(NetworkConnectionToClient target, string grid)
    {
        TerrainController.GridType = grid;
        BlockRendering.ToggleHex(grid == "Hex");
        CmdRequestMapSync();
    }
    #endregion

    #region Create Actor
    [Command]
    public void CmdCreateActor(string json)
    {
        ActorPersistence ap = JsonUtility.FromJson<ActorPersistence>(json);
        GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/ActorData"));
        ActorData data = g.GetComponent<ActorData>();
        data.Id = Guid.NewGuid().ToString();
        data.Token = ap.Token;
        data.Name = ap.Name;
        data.Type = ap.ActorTypeId;
        data.TypeData = ap.ActorType;
        data.Shape = ap.Shape;
        data.Color = ap.Color;

        data.Placed = ap.Placed;
        data.LastKnownPosition = ap.Position;
        g.transform.localScale = ap.Placed ? Vector3.one : Vector3.zero;

        NetworkServer.Spawn(g);
    }
    #endregion

    #region Delete Actor
    [Command]
    public void CmdRequestDeleteActor(string actorId)
    {
        ActorData data = ActorData.Find(actorId);
        FileLogger.Write($"Client {connectionToClient.connectionId} requested to delete actor {data.Name}");
        RpcDeleteActor(actorId);
        data.Destroyed = true;
    }
    [Command]
    public void CmdRequestDeleteAllActors()
    {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested to delete all actors");
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("ActorData"))
        {
            ActorData data = g.GetComponent<ActorData>();
            if (data.Deletable && !data.Destroyed)
            {
                FileLogger.Write($"Client {connectionToClient.connectionId} requested to delete actor {data.Name} ({data.Id})");
                RpcDeleteActor(data.Id);
                data.Destroyed = true;
            }
        }
    }
    [ClientRpc]
    public void RpcDeleteActor(string actorId)
    {
        ActorData data = ActorData.Find(actorId);
        data.Delete();
        FileLogger.Write($"Actor {data.Name} was deleted");
        Toast.AddSuccess($"{data.Name} deleted.");
    }
    #endregion 

    #region Actor Movement
    [Command]
    public void CmdMoveActor(string actorId, Vector3 v, bool immediate)
    {
        RpcDoMoveActor(actorId, v, immediate);
    }

    [Command]
    public void CmdRequestPlaceActor(string actorId, Vector3 target)
    {
        RpcPlaceActor(actorId, true);
        Vector2 v = ActorData.Find(actorId).UnitBarElement.worldBound.center * Preferences.GetUIScale();
        Vector3 origin = Camera.main.ScreenToWorldPoint(new Vector3(v.x, Screen.height - v.y, 0));

        RpcDoMoveActor(actorId, origin, true);
        RpcDoMoveActor(actorId, target, false);
    }
    [Command]
    public void CmdRequestRemoveActor(string actorId)
    {
        RpcPlaceActor(actorId, false);
        RpcDoMoveActor(actorId, new Vector3(0, -10f, 0), true);
    }

    [ClientRpc]
    private void RpcDoMoveActor(string actorId, Vector3 v, bool immediate)
    {
        ActorData data = ActorData.Find(actorId);
        data.LastKnownPosition = v;
        if (immediate)
        {
            data.WorldObject.transform.position = v;
        }
        else
        {
            MoveLerp.Create(data.WorldObject, v);
        }
    }

    [ClientRpc]
    private void RpcPlaceActor(string actorId, bool place)
    {
        ActorData data = ActorData.Find(actorId);
        data.Place(place);
    }
    #endregion

    #region Actor Status
    [Command]
    public void CmdRequestGameSystemCommand(string value)
    {
        RpcGameSystemCommand(value);
    }
    [ClientRpc]
    public void RpcGameSystemCommand(string value)
    {
        GameSystem.Current().Command(value);
    }
    [Command]
    public void CmdRequestActorCommand(string actorId, string value)
    {
        RpcActorCommand(actorId, value);
    }
    [ClientRpc]
    public void RpcActorCommand(string actorId, string value)
    {
        ActorData.Command(actorId, value);
    }
    [Command]
    public void CmdRequestAllActorsCommand(string value)
    {
        RpcAllActorsCommand(value);
    }
    [ClientRpc]
    public void RpcAllActorsCommand(string value)
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("ActorData"))
        {
            ActorData data = g.GetComponent<ActorData>();
            ActorData.Command(data.Id, value);
        }
    }
    #endregion

    #region MapChange
    [Command]
    public void CmdRequestMapSetValue(string[] blocks, string label, string value)
    {
        RpcMapSetValue(blocks, label, value);
    }
    [ClientRpc]
    public void RpcMapSetValue(string[] blocks, string label, string value)
    {
        FileLogger.Write($"{label}: {value}");
        if (label == "AddEffect")
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                Block target = GameObject.Find(blocks[i]).GetComponent<Block>();
                target.AddMark(value);
            }
        }
        else if (label == "RemoveEffect")
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                Block target = GameObject.Find(blocks[i]).GetComponent<Block>();
                target.RemoveMark(value);
            }
        }

        TerrainController.SetInfo();
    }
    #endregion

    #region Dice Rolls
    [Command]
    public void CmdRequestDiceRoll(DiceTray tray)
    {
        for (int i = 0; i < tray.Rolls.Length; i++)
        {
            tray.Rolls[i].Rolled = 1 + Random.Range(0, tray.Rolls[i].Die);
        }
        RpcDiceRoll(tray);
    }
    [Command]
    public void CmdShareDiceRoll(string description, string result, string rolls, int die)
    {
        RpcDiceRoll(description, result, rolls, die);
    }
    [ClientRpc]
    public void RpcDiceRoll(DiceTray tray)
    {
        DiceRoller.AddOutcome(tray);
    }
    [ClientRpc]
    public void RpcDiceRoll(string description, string result, string rolls, int die)
    {
        DiceRoller.AddOutcome(description, result, rolls, die);
    }
    #endregion

    #region Session Init
    // Used by anyone after completing a map edit
    [Command]
    public void CmdMapSync(byte[] bytes)
    {
        FileLogger.Write("Map sent to all clients");
        RpcMapSync(bytes);
    }
    // Used by new players to request data from host
    [Command]
    public void CmdRequestMapSync()
    {
        FileLogger.Write($"Client {connectionToClient.connectionId} requested a map sync");
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        byte[] compressedJson = Compression.CompressString(json);
        TargetMapSync(connectionToClient, compressedJson);
    }
    [TargetRpc]
    public void TargetMapSync(NetworkConnectionToClient target, byte[] bytes)
    {
        FileLogger.Write($"Map received from host by this client");
        string json = Compression.DecompressString(bytes);
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        BlockRendering.ToggleSpacers(false);
        Toast.AddSimple("Map synced.");
    }

    [ClientRpc]
    public void RpcMapSync(byte[] bytes)
    {
        string json = Compression.DecompressString(bytes);
        FileLogger.Write($"Map received from host by everyone");
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        BlockRendering.ToggleSpacers(false);
        Toast.AddSimple("Map synced.");
    }
    #endregion

    #region TokenSync
    [Command]
    public void CmdRequestMissingChunks(string hash, int[] missingChunks)
    {
        RpcRequireMissingChunks(connectionToClient.connectionId, hash, missingChunks);
    }

    [ClientRpc]
    public void RpcRequireMissingChunks(int connectionId, string hash, int[] missingChunks)
    {
        TokenSync.StackRequest(connectionId, hash, missingChunks);
    }

    [Command]
    public void CmdDeliverMissingChunk(int targetConnection, string hash, int index, Byte[] chunk)
    {
        var connection = NetworkServer.connections[targetConnection];
        if (connection != null)
        {
            TargetDeliverMissingChunk(connection, hash, index, chunk);
        }
    }

    [TargetRpc]
    public void TargetDeliverMissingChunk(NetworkConnectionToClient target, string hash, int index, Byte[] chunk)
    {
        TokenSync.SetMissingChunk(hash, index, chunk);
    }

    #endregion
}
