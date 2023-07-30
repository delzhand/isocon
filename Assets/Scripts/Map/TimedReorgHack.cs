using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedReorgHack : MonoBehaviour
{
    private float duration = .2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) {
            TerrainController.Reorg();
            Destroy(this);
        }
    }

    public static void Add() {
        TimedReorgHack trh = GameObject.Find("Engine").AddComponent<TimedReorgHack>();
    }
}
