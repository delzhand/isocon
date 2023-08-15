using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour
{
    public virtual string SystemName()
    {
        return "Generic System";
    }

    public static GameSystem Current() {
        return GameObject.Find("GameSystem").GetComponent<GameSystem>();
    }

    public virtual void Setup()
    {
    }

    public virtual void Teardown()
    {
    }

    public GameObject InitializeToken(string name, string imageSource, bool isLocal)
    {
        GameObject tokenDataObject = Instantiate(Resources.Load("Prefabs/GenericTokenData")) as GameObject;
        GenericTokenData gtd = tokenDataObject.GetComponent<GenericTokenData>();
        gtd.Name = name;
        if (isLocal) {
            gtd.LocalFilename = imageSource;
        }
        else {
            gtd.RemoteHash = imageSource;
        }
        gtd.MaxHP = 100;
        gtd.CurrentHP = gtd.MaxHP;
        gtd.Position = new Vector3(3, .25f, 3);

        return tokenDataObject;
    }
}
