using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SyncStatus : MonoBehaviour
{
    private static Dictionary<string, float> HashPercentages = new();


    // Update is called once per frame
    void Update()
    {
        VisualElement root = UI.TopBar.Q("SyncStatus");
        root.Clear();
        foreach(KeyValuePair<string, float> kvp in HashPercentages) {
            ProgressBar bar = new();
            bar.title = TextureSender.TruncatedHash(kvp.Key);
            bar.value = kvp.Value * 100;
            root.Add(bar);
        }
    }

    public static void Receive(string hash, float percentage) {
        HashPercentages[hash] = percentage;
    }
}
