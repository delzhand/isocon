using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnityEngine;

public class LongPress : MonoBehaviour
{
    private float duration;
    private static string claim;

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
        if (GameObject.Find("Engine").GetComponent<LongPress>() == null) {
            LongPress longpress = GameObject.Find("Engine").AddComponent<LongPress>();
            LongPress.claim = claim;
            longpress.duration = .5f;
        }
    }

    public static void ClearAll() {
        GameObject.Find("Engine").GetComponents<LongPress>().ToList().ForEach((item) => {
            Destroy(item);
        });
    }
}
