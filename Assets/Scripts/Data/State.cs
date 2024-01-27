using System;
using System.Collections.Generic;
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
    public string Title;

    public static State GetStateFromScene()
    {
        List<string> blockStrings = new List<string>();
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++)
        {
            blockStrings.Add(blocks[i].GetComponent<Block>().WriteOut());
        }
        State state = new State();
        state.Version = "v3";
        state.TileTops = ColorUtility.GetHex(Environment.TileTopColor);
        state.TileSides = ColorUtility.GetHex(Environment.TileSideColor);
        state.BgBottom = ColorUtility.GetHex(Environment.BgBottomColor);
        state.BgTop = ColorUtility.GetHex(Environment.BgTopColor);
        state.LightAngle = TerrainController.LightAngle;
        state.LightHeight = TerrainController.LightHeight;
        state.LightIntensity = TerrainController.LightIntensity;
        state.Blocks = blockStrings.ToArray();
        state.CreatorName = MapMeta.CreatorName;
        state.Description = MapMeta.Description;
        state.Objective = MapMeta.Objective;
        state.System = MapMeta.System;
        state.Title = MapMeta.Title;
        return state;
    }

    public static void SetSceneFromState(State state)
    {
        TerrainController.DestroyAllBlocks();
        Environment.SetTileColors(ColorUtility.GetColor(state.TileTops), ColorUtility.GetColor(state.TileSides));
        Environment.SetBackgroundColors(ColorUtility.GetColor(state.BgBottom), ColorUtility.GetColor(state.BgTop));
        foreach (string s in state.Blocks)
        {
            Block.ReadIn(state.Version, s);
        }
        TerrainController.LightAngle = state.LightAngle;
        TerrainController.LightHeight = state.LightHeight;
        TerrainController.LightIntensity = state.LightIntensity;
        TerrainController.UpdateLight();
        TerrainController.ReorgNeeded = true;
        TerrainController.MapDirty = false;
        BlockMesh.ToggleAllBorders(false);
        MapMeta.CreatorName = state.CreatorName;
        MapMeta.Description = state.Description;
        MapMeta.Objective = state.Objective;
        MapMeta.System = state.System;
        MapMeta.Title = state.Title;

        VisualElement optionsRoot = UI.System.Q("ToolOptions");
        optionsRoot.Q("DataOptions").Q<TextField>("MapTitle").value = MapMeta.Title;
        optionsRoot.Q("DataOptions").Q<TextField>("Description").value = MapMeta.Description;
        optionsRoot.Q("DataOptions").Q<TextField>("Objective").value = MapMeta.Objective;
        optionsRoot.Q("DataOptions").Q<TextField>("Author").value = MapMeta.CreatorName;
        optionsRoot.Q("DataOptions").Q<DropdownField>("System").value = MapMeta.System;
    }
}
