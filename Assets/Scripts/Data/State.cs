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
    public string TileTops;
    public string TileSides;
    public string BgBottom;
    public string BgTop;
    public float LightIntensity;
    public float LightAngle;
    public float LightHeight;
    public string[] Blocks;
    public string CreatorName;
    public string Description;
    public string Objective;
    public string System;

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
            blockStrings.Add(blocks[i].GetComponent<Block>().WriteOut());
        }
        State state = new State();
        state.Version = "v2";
        state.TileTops = ColorUtility.ColorToHex(Environment.TileTopColor);
        state.TileSides = ColorUtility.ColorToHex(Environment.TileSideColor);
        state.BgBottom = ColorUtility.ColorToHex(Environment.BgBottomColor);
        state.BgTop = ColorUtility.ColorToHex(Environment.BgTopColor);
        state.LightAngle = TerrainController.LightAngle;
        state.LightHeight = TerrainController.LightHeight;
        state.LightIntensity = TerrainController.LightIntensity;
        state.Blocks = blockStrings.ToArray();
        state.CreatorName = MapMeta.CreatorName;
        state.Description = MapMeta.Description;
        state.Objective = MapMeta.Objective;
        state.System = MapMeta.System;
        return state;
    }

    public static void SetSceneFromState(State state) {
        TerrainController.DestroyAllBlocks();
        Environment.SetTileColors(ColorUtility.ColorFromHex(state.TileTops), ColorUtility.ColorFromHex(state.TileSides));
        Environment.SetBackgroundColors(ColorUtility.ColorFromHex(state.BgBottom), ColorUtility.ColorFromHex(state.BgTop));
        foreach (string s in state.Blocks) {
            Block.ReadIn(state.Version, s);
        }
        TerrainController.LightAngle = state.LightAngle;
        TerrainController.LightHeight = state.LightHeight;
        TerrainController.LightIntensity = state.LightIntensity;
        TerrainController.UpdateLight();
        TerrainController.ReorgNeeded = true;
        TerrainController.MapDirty = false;
        MapMeta.CreatorName = state.CreatorName;
        MapMeta.Description = state.Description;
        MapMeta.Objective = state.Description;
        MapMeta.System = state.System;        

        VisualElement optionsRoot = UI.System.Q("ToolOptions");
        optionsRoot.Q("DataOptions").Q<TextField>("Description").value = MapMeta.Description;
        optionsRoot.Q("DataOptions").Q<TextField>("Objective").value = MapMeta.Objective;
        optionsRoot.Q("DataOptions").Q<TextField>("Author").value = MapMeta.CreatorName;
        optionsRoot.Q("DataOptions").Q<DropdownField>("System").value = MapMeta.System;
    }

    public static void SetCurrentJson() {
        CurrentStateJson = JsonUtility.ToJson(State.GetStateFromScene());
    }
}
