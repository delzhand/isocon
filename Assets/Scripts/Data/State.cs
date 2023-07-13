using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class State
{
    public string Version;
    public Palette Palette;
    public Background Background;
    public string[] Blocks;

    public static string FullFilePath(string fileName) {
        return Application.persistentDataPath + "/maps/" + fileName;
    }

    public static void SaveState(string fileName) {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        string fullFileName = State.FullFilePath(fileName).Replace(".json", "") + ".json";
        File.WriteAllText(fullFileName, json);
        UI.SetHelpText("Map saved to " + fullFileName, HelpType.Success);
    }

    public static void LoadState(string fileName) {
        string fullFileName = State.FullFilePath(fileName).Replace(".json", "") + ".json";
        string json = File.ReadAllText(fullFileName);
        State state = JsonUtility.FromJson<State>(json);
        SetSceneFromState(state);
        UI.SetHelpText("Map loaded.", HelpType.Success);
    }

    public static State GetStateFromScene() {
        List<string> blockStrings = new List<string>();
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blockStrings.Add(blocks[i].GetComponent<Block>().ToString());
        }
        State state = new State();
        state.Version = "v1";
        state.Background = Environment.GetBackground();
        state.Palette = Environment.GetPalette();
        state.Blocks = blockStrings.ToArray();

        return state;
    }

    public static void SetSceneFromState(State state) {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            GameObject.Destroy(blocks[i]);
        }
        Environment.SetBackground(state.Background);
        Environment.SetPalette(state.Palette);
        foreach (string s in state.Blocks) {
            Block.FromString(state.Version, s);
        }
        TerrainController.UpdateIndicators();
    }
}
