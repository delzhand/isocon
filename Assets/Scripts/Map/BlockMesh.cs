using System.Collections.Generic;
using UnityEngine;

public class BlockMesh : MonoBehaviour
{
    public static Mesh Hex;
    public static Dictionary<BlockShape, Mesh> Shapes = new();
    private static Dictionary<string, Material> _sharedMaterials = new();
    private static Dictionary<string, Material> _customMaterials = new();
    public static bool IsSetup = false;

    public static void Setup()
    {
        IsSetup = true;

        Hex = Resources.Load<Mesh>("Models/Hex");

        Shapes.Add(BlockShape.Solid, Resources.Load<Mesh>("Models/Block"));
        Shapes.Add(BlockShape.Slope, Resources.Load<Mesh>("Models/Slope"));
        Shapes.Add(BlockShape.Steps, Resources.Load<Mesh>("Models/Steps"));
        Shapes.Add(BlockShape.Corner, Resources.Load<Mesh>("Models/Corner"));
        Shapes.Add(BlockShape.FlatCorner, Resources.Load<Mesh>("Models/FlatCorner"));
        Shapes.Add(BlockShape.Upslope, Resources.Load<Mesh>("Models/Upslope"));
        Shapes.Add(BlockShape.SlopeInt, Resources.Load<Mesh>("Models/SlopeIntCorner"));
        Shapes.Add(BlockShape.SlopeExt, Resources.Load<Mesh>("Models/SlopeExtCorner"));

        _sharedMaterials.Add("side1", new Material(Resources.Load<Material>("Materials/Block/Checker/SideA")));
        _sharedMaterials.Add("side2", new Material(Resources.Load<Material>("Materials/Block/Checker/SideB")));
        _sharedMaterials.Add("top1", new Material(Resources.Load<Material>("Materials/Block/Checker/TopA")));
        _sharedMaterials.Add("top2", new Material(Resources.Load<Material>("Materials/Block/Checker/TopB")));
        _sharedMaterials.Add("highlighted", new Material(Resources.Load<Material>("Materials/Block/Highlighted")));

        _sharedMaterials.Add("unfocused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        _sharedMaterials.Add("focused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        _sharedMaterials["focused"].SetInt("_Focused", 1);

        _sharedMaterials.Add("selectfocused", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        _sharedMaterials["selectfocused"].SetInt("_Selected", 1);
        _sharedMaterials["selectfocused"].SetInt("_Focused", 1);

        _sharedMaterials.Add("selected", new Material(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        _sharedMaterials["selected"].SetInt("_Selected", 1);
    }

    public static Material GetSharedMaterial(string key)
    {
        if (_sharedMaterials.ContainsKey(key))
        {
            return _sharedMaterials[key];
        }
        Debug.Log($"Material with key {key} not found");
        return null;
    }

    public static Material GetCustomMaterial(string key, bool top)
    {
        if (_customMaterials.ContainsKey(key))
        {
            return _customMaterials[key];
        }
        else
        {
            string texture = key.Split("::")[0];
            string tintHex = key.Split("::")[1];
            Color tint = ColorUtility.GetColor(tintHex);
            var material = new Material(Resources.Load<Material>($"Materials/Block/Artistic/{texture}"));
            material.SetColor("_Tint", tint);
            material.SetInt("_ShowOutline", Cursor.Mode == CursorMode.Editing ? 1 : 0);
            _customMaterials.Add(key, material);
            return material;
        }
    }

    public static void ToggleAllBorders(bool show)
    {
        foreach (string s in _customMaterials.Keys)
        {
            _customMaterials[s].SetInt("_ShowOutline", show ? 1 : 0);
        }
        foreach (string s in StringUtility.CreateArray("side1", "side2", "top1", "top2"))
        {
            _sharedMaterials[s].SetInt("_ShowOutline", show ? 1 : 0);
        }
    }

    public static int MaterialTopIndex(BlockShape shape)
    {
        if (TerrainController.GridType == "Hex")
        {
            return 1;
        }
        if (shape == BlockShape.Steps)
        {
            return 3;
        }
        if (shape == BlockShape.Corner || shape == BlockShape.FlatCorner)
        {
            return 1;
        }
        return 0;
    }

    public static int MaterialSideIndex(BlockShape shape)
    {
        if (TerrainController.GridType == "Hex")
        {
            return 0;
        }
        if (shape == BlockShape.Steps)
        {
            return 1;
        }
        if (shape == BlockShape.Corner || shape == BlockShape.FlatCorner)
        {
            return 0;
        }
        return 1;
    }

    public static int MaterialMarkerIndex(BlockShape shape)
    {
        return 2;
    }

    public static int MaterialFocusIndex(BlockShape shape)
    {
        if (shape == BlockShape.Steps)
        {
            return 0;
        }
        return 3;
    }

    public static string MaterialName(string styleName, bool top)
    {
        (string, string) topSidePair = TextureMap(styleName);
        return top ? topSidePair.Item2 : topSidePair.Item1;
    }

    private static (string, string) TextureMap(string style)
    {
        switch (style)
        {
            case "Color Only":
                return ("ColorOnly", "ColorOnly");
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

    public static string ReverseTextureMap(string texture)
    {
        switch (texture)
        {
            case "ColorOnly":
                return "Color Only";
            case "AcidTopFlow":
                return "Acid Flow";
            case "AcidSide":
            case "AcidTopStill":
                return "Acid";
            case "Brick2Side":
            case "Brick2Top":
                return "Old Brick";
            case "DryGrassTop":
                return "Dry Grass";
            case "GrassTop":
                return "Grass";
            case "LavaTopFlow":
                return "Lava Flow";
            case "LavaSide":
            case "LavaTopStill":
                return "Lava";
            case "PoisonTopFlow":
                return "Poison Flow";
            case "PoisonSide":
            case "PoisonTopStill":
                return "Poison";
            case "WaterSideFlow":
            case "WaterTopFlow":
                return "Water Flow";
            case "WaterSideStill":
            case "WaterTopStill":
                return "Water";
            case "Wood2Side":
            case "Wood2Top":
                return "Old Wood";
            case "GrayMetalSide":
            case "GrayMetalTop":
                return "Gray Metal";
            case "GrayBrickSide":
            case "GrayBrickTop":
                return "Gray Brick";
            case "Brick3Side":
            case "SmallTileTop":
                return "Small Tile";
            case "BigTileTop":
                return "Big Tile";
            case "Brick3Top":
                return "White Brick";
            default:
                // If the input doesn't match any known texture, return it as is.
                return texture.Replace("Top", "").Replace("Side", "");
        }
    }
}
