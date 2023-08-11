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
    public Texture2D Graphic;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    public void Setup()
    {
        MaxHP = 100;
        CurrentHP = MaxHP;
        Graphic = Resources.Load<Texture2D>("Textures/Chibis/bastion");
        Name = "Ada";
    }

    public void Teardown()
    {
    }
}
