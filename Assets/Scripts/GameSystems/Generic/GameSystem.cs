using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour, IGameSystem
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

    public virtual string GetTokenParams(string imageHash)
    {
        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        GenericTokenParams gtp = new(nameField.value, imageHash);
        return JsonUtility.ToJson(gtp);
    }

    public virtual string GetTokenImageHash(GameObject g) {
        return g.GetComponent<GenericTokenData>().ImageHash;
    }

    public void InitializeToken(GameObject g, string json)
    {
        GenericTokenData gtd = g.AddComponent<GenericTokenData>();
        GenericTokenParams gtp = JsonUtility.FromJson<GenericTokenParams>(json);
        gtd.Name = gtp.Name;
        gtd.MaxHP = 100;
        gtd.CurrentHP = gtd.MaxHP;
        gtd.ImageHash = gtp.ImageHash;
        Texture2D image = TextureSender.Locate(gtp.ImageHash);
        g.GetComponent<Token>().SetImage(image);
    }
}
