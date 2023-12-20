using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMesh: MonoBehaviour
{
    public static Mesh Hex;
    public static Dictionary<BlockShape, Mesh> Shapes = new();
    public static Dictionary<string, Material> SharedMaterials = new();

    public static bool IsSetup = false;

    public static void Setup() {
        IsSetup = true;

        Hex = Resources.Load<Mesh>("Models/Hex");

        Shapes.Add(BlockShape.Solid, Resources.Load<Mesh>("Models/Block"));
        Shapes.Add(BlockShape.Slope, Resources.Load<Mesh>("Models/Slope"));
        Shapes.Add(BlockShape.Steps, Resources.Load<Mesh>("Models/Steps"));
        Shapes.Add(BlockShape.Corner, Resources.Load<Mesh>("Models/Corner"));
        Shapes.Add(BlockShape.FlatCorner, Resources.Load<Mesh>("Models/FlatCorner"));
        Shapes.Add(BlockShape.Upslope, Resources.Load<Mesh>("Models/Upslope"));
        Shapes.Add(BlockShape.SlopeInt, Resources.Load<Mesh>("Models/SlopeIntCorner"));

        SharedMaterials.Add("side1", new Material(Resources.Load<Material>("Materials/Block/Checker/SideA")));
        SharedMaterials.Add("side2", new Material(Resources.Load<Material>("Materials/Block/Checker/SideB")));
        SharedMaterials.Add("top1", new Material(Resources.Load<Material>("Materials/Block/Checker/TopA")));
        SharedMaterials.Add("top2", new Material(Resources.Load<Material>("Materials/Block/Checker/TopB")));
        SharedMaterials.Add("highlighted", new Material(Resources.Load<Material>("Materials/Block/Highlighted")));

        SharedMaterials.Add("unfocused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        SharedMaterials.Add("focused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        SharedMaterials["focused"].SetInt("_Focused", 1);

        SharedMaterials.Add("selectfocused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        SharedMaterials["selectfocused"].SetInt("_Selected", 1);
        SharedMaterials["selectfocused"].SetInt("_Focused", 1);

        SharedMaterials.Add("selected", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        SharedMaterials["selected"].SetInt("_Selected", 1);

        foreach(string s in TextureMaterials()) {
            SharedMaterials.Add($"{s}", new Material(Resources.Load<Material>($"Materials/Block/Artistic/{s}")));
        }
    }

    public static void ToggleBorders(bool show) {
        foreach(string s in TextureMaterials()) {
            SharedMaterials[s].SetInt("_ShowOutline", show ? 1 : 0);
        }
        foreach(string s in StringUtility.Arr("side1", "side2", "top1", "top2")) {
            SharedMaterials[s].SetInt("_ShowOutline", show ? 1 : 0);
        }
        Block.ToggleBorders(show);
    }

    private static string[] TextureMaterials() {
        return StringUtility.Arr("Brick3Side", "Brick3Top", "SmallTileTop", "BigTileTop", "AcidSide","AcidTopFlow","AcidTopStill","Brick2Side","Brick2Top","BrickSide","BrickTop","DryGrassTop","GoldSide","GoldTop","GrassTop","LavaSide","LavaTopFlow","LavaTopStill","MetalSide","MetalTop","PoisonSide","PoisonTopFlow","PoisonTopStill","SandSide","SandTop","SnowSide","SnowTop","SoilSide","SoilTop","StoneSide","StoneTop","WaterSideFlow", "WaterSideStill","WaterTopFlow","WaterTopStill","Wood2Side","Wood2Top","WoodSide","WoodTop", "GrayBrickSide", "GrayBrickTop", "GrayMetalSide", "GrayMetalTop");
    }

    private static Material Instantiate(Material material)
    {
        throw new NotImplementedException();
    }

    public static int MaterialTopIndex(BlockShape shape) {
        if (TerrainController.GridType == "Hex") {
            return 1;
        }
        if (shape == BlockShape.Steps) {
            return 3; 
        }
        if (shape == BlockShape.Corner || shape == BlockShape.FlatCorner) {
            return 1;
        }
        return 0;
    }

    public static int MaterialSideIndex(BlockShape shape) {
        if (TerrainController.GridType == "Hex") {
            return 0;
        }
        if (shape == BlockShape.Steps) {
            return 1; 
        }
        if (shape == BlockShape.Corner || shape == BlockShape.FlatCorner) {
            return 0;
        }
        return 1;
    }

    public static int MaterialMarkerIndex(BlockShape shape) {
        return 2;
    }

    public static int MaterialFocusIndex(BlockShape shape) {
        if (shape == BlockShape.Steps) {
            return 0; 
        }
        return 3;
    }

    public static (string,string) StyleMaterials(string style) {
        switch (style) {
            case "Acid Flow":
                return ("AcidSide", "AcidTopFlow");
            case "Acid":
                return ("AcidSide", "AcidTopStill");
            case "Old Brick":
                return ("Brick2Side", "Brick2Top");
            case "Dry Grass":
                return ("SoilSide", "DryGrassTop");
            case "Grass":
                return ("SoilSide", "GrassTop");
            case "Lava Flow":
                return ("LavaSide", "LavaTopFlow");
            case "Lava":
                return ("LavaSide", "LavaTopStill");
            case "Poison Flow":
                return ("PoisonSide", "PoisonTopFlow");
            case "Poison":
                return ("PoisonSide", "PoisonTopStill");
            case "Water Flow":
                return ("WaterSideFlow", "WaterTopFlow");
            case "Water":
                return ("WaterSideStill", "WaterTopStill");
            case "Old Wood":
                return ("Wood2Side", "Wood2Top");
            case "Gray Metal":
                return ("GrayMetalSide", "GrayMetalTop");
            case "Gray Brick":
                return ("GrayBrickSide", "GrayBrickTop");
            case "Small Tile":
                return ("Brick3Side", "SmallTileTop");
            case "Big Tile":
                return ("Brick3Side", "BigTileTop");
            case "White Brick":
                return ("Brick3Side", "Brick3Top");
            default:
                return ($"{style}Side", $"{style}Top");
        }
    }
}
