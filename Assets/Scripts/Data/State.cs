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

    public static string CurrentStateJson;

    public static void SaveState(string fileName) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        if (fileName.IndexOf(".json") < 0) {
            fileName += ".json";
        }
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        string fullPath = path + "/maps/" + fileName;
        File.WriteAllText(fullPath, json);
        Toast.Add("Map saved to " + fullPath);
    }

    public static void LoadState(string fileName) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string json = File.ReadAllText(path + "/maps/" + fileName);
        State state = JsonUtility.FromJson<State>(json);
        SetSceneFromState(state);
        Toast.Add("Map loaded.");
    }

    public static State GetStateFromScene() {
        List<string> blockStrings = new List<string>();
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blockStrings.Add(blocks[i].GetComponent<Block>().ToString());
        }
        State state = new State();
        state.Version = "v1";
        state.Color1 = ColorSidebar.ColorToHex(Environment.Color1);
        Debug.Log(state.Color1);
        state.Color2 = ColorSidebar.ColorToHex(Environment.Color2);
        state.Color3 = ColorSidebar.ColorToHex(Environment.Color3);
        state.Color4 = ColorSidebar.ColorToHex(Environment.Color4);
        state.Blocks = blockStrings.ToArray();
        return state;
    }

    public static void SetSceneFromState(State state) {
        TerrainController.DestroyAllBlocks();
        Environment.SetTileColors(ColorSidebar.FromHex(state.Color1), ColorSidebar.FromHex(state.Color2));
        Environment.SetBackgroundColors(ColorSidebar.FromHex(state.Color3), ColorSidebar.FromHex(state.Color4));
        foreach (string s in state.Blocks) {
            Block.FromString(state.Version, s);
        }
        TerrainController.ReorgNeeded = true;
    }

    public static void SetCurrentJson() {
        CurrentStateJson = JsonUtility.ToJson(State.GetStateFromScene());
    }
}
