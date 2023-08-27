using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class State
{
    public string Version;
    public Palette Palette; // Deprecated
    public BackgroundGradient Background; // Deprecated

    public string Color1; // Tile tops
    public string Color2; // Tile sides
    public string Color3; // Background bottom
    public string Color4; // Background Top

    public string[] Blocks;

    public static string FullFilePath(string fileName) {
        return Application.persistentDataPath + "/maps/" + fileName;
    }

    public static void SaveState(string fileName) {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        Debug.Log(json);
        string fullFileName = State.FullFilePath(fileName).Replace(".json", "") + ".json";
        File.WriteAllText(fullFileName, json);
        Toast.Add("Map saved to " + fullFileName);
    }

    public static void LoadState(string fileName) {
        string fullFileName = State.FullFilePath(fileName).Replace(".json", "") + ".json";
        string json = File.ReadAllText(fullFileName);
        State state = JsonUtility.FromJson<State>(json);
        SetSceneFromState(state);
        Toast.Add("Map loaded.");
        TimedReorgHack.Add();
    }

    public static State GetStateFromScene() {
        List<string> blockStrings = new List<string>();
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blockStrings.Add(blocks[i].GetComponent<Block>().ToString());
        }
        State state = new State();
        state.Version = "v1";
        state.Color1 = ColorSidebar.ColorToHex(UI.System.Q("Color1").resolvedStyle.backgroundColor);
        state.Color2 = ColorSidebar.ColorToHex(UI.System.Q("Color2").resolvedStyle.backgroundColor);
        state.Color3 = ColorSidebar.ColorToHex(UI.System.Q("Color3").resolvedStyle.backgroundColor);
        state.Color4 = ColorSidebar.ColorToHex(UI.System.Q("Color4").resolvedStyle.backgroundColor);
        state.Blocks = blockStrings.ToArray();
        return state;
    }

    public static void SetSceneFromState(State state) {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            GameObject.Destroy(blocks[i]);
        }
        Environment.SetTileColors(ColorSidebar.FromHex(state.Color1), ColorSidebar.FromHex(state.Color2));
        Environment.SetBackgroundColors(ColorSidebar.FromHex(state.Color3), ColorSidebar.FromHex(state.Color4));
        foreach (string s in state.Blocks) {
            Block.FromString(state.Version, s);
        }
        TerrainController.Reorg();
    }
}
