using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericTokenData : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    [SyncVar]
    public Vector3 Position;

    [SyncVar]
    public string RemoteHash;

    [SyncVar]
    public string LocalFilename;

    [SyncVar]
    public bool NetworkSpawned;

    public GameObject Token;


    
}

public class OfflineGenericTokenData : MonoBehaviour
{
    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public Vector3 Position;
    public string RemoteHash;
    public string LocalFilename;
    public bool NetworkSpawned;
}

[Serializable]
public class GenericTokenParams {
    public string Name;

    public GenericTokenParams(string name) {
        Name = name;
    }
}