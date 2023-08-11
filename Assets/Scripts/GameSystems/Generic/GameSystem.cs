using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
