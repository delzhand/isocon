using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;

public class LongPress : MonoBehaviour
{
    private float duration;
    private string claim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) {
            MapEdit.LongPressResult(claim);
            Destroy(this);
        }
    }

    public static void Add(string claim) {
        LongPress longpress = GameObject.Find("Engine").AddComponent<LongPress>();
        longpress.claim = claim;
        longpress.duration = .5f;
    }
}
