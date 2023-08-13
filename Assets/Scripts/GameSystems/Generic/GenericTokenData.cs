using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericTokenData : NetworkBehaviour, ITokenData
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    [SyncVar]
    public string ImageHash;

    public void Setup()
    {
    }

    public void Teardown()
    {
    }
}

[Serializable]
public class GenericTokenParams {
    public string Name;
    public string ImageHash;

    public GenericTokenParams(string name, string imageHash) {
        Name = name;
        ImageHash = imageHash;
    }
}