using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SyncWatcher : MonoBehaviour
{
    // private static Dictionary<string, float> HashPercentages = new();
    // private static Dictionary<string, string> HashTokens = new();

    // // Update is called once per frame
    // void Update()
    // {
    //     VisualElement root = UI.TopBar.Q("SyncStatus");
    //     root.Clear();
    //     foreach (KeyValuePair<string, float> kvp in HashPercentages)
    //     {
    //         ProgressBar bar = new();
    //         bar.title = TextureSender.TruncateHash(kvp.Key);
    //         if (HashTokens.ContainsKey(kvp.Key))
    //         {
    //             bar.title = HashTokens[kvp.Key];
    //         }
    //         bar.value = kvp.Value * 100;
    //         root.Add(bar);
    //     }
    // }

    // public static void Receive(string hash, float percentage)
    // {
    //     if (!HashPercentages.ContainsKey(hash))
    //     {
    //         FindMatchingToken(hash);
    //     }
    //     HashPercentages[hash] = percentage;
    // }

    // private static void FindMatchingToken(string hash)
    // {
    //     foreach (var gameObject in GameObject.FindGameObjectsWithTag("Token"))
    //     {
    //         var tokenData = gameObject.GetComponent<Token>().Data;
    //         if (tokenData.GraphicHash == hash)
    //         {
    //             HashTokens[hash] = tokenData.Name;
    //         }
    //     }
    // }
}
