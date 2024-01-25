using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Web;
using System.IO;
using System.Linq;
using LZStringCSharp;

public class MMMImporter
{
    public static void CreateFromURL(string url)
    {
        var uri = new Uri(url);
        int width = int.Parse(HttpUtility.ParseQueryString(uri.Query).Get("w"));
        int height = int.Parse(HttpUtility.ParseQueryString(uri.Query).Get("h"));
        string encodedData = HttpUtility.ParseQueryString(uri.Query).Get("m");
        string decodedData = LZString.DecompressFromEncodedURIComponent(encodedData);
        Debug.Log(decodedData);

        TerrainController.ResetTerrain(width, height, 0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * height + x;
                int brush = int.Parse(decodedData[index].ToString());
                Build(x, y, index);
            }
        }
    }

    private static string GetBrushName(int i) {
        string[] brushes = new string[]{"empty", "terrain", "elevation", "hazard", "objective", "special", "stairs", "wall"};
        return brushes[i];
    }

    private static void Build(int x, int y, int i)
    {
        Debug.Log($"{x} {y} {GetBrushName(i)}");
        List<string> blocks = new();
        switch (i)
        {
            case 0:
                blocks.Add($"{x}|{y}|0|0|Solid|False||False|||BigTileTop|Brick3Side");
                break;
            case 1:
                blocks.Add($"{x}|{y}|0|0|Solid|False|Adverse Terrain::Wavy::Black|False|||BigTileTop|Brick3Side");
                break;
            case 2:
                blocks.Add($"{x}|{y}|0|0|Solid|False||False|||BigTileTop|Brick3Side");
                blocks.Add($"{x}|{y}|1|0|Solid|False||False|||BigTileTop|Brick3Side");
                break;
            case 3:
                blocks.Add($"{x}|{y}|0|0|Solid|False|Hazard::Spiky::None|False|||BigTileTop|Brick3Side");
                break;
            case 4:
                blocks.Add($"{x}|{y}|0|0|Solid|False|Objective::Border::Green|False|||BigTileTop|Brick3Side");
                break;
            case 5:
                blocks.Add($"{x}|{y}|0|0|Solid|False|Special::Border::Blue|False|||BigTileTop|Brick3Side");
                break;
            case 6:
                blocks.Add($"{x}|{y}|0|0|Steps|False||False|||BigTileTop|Brick3Side");
                break;
            case 7:
                blocks.Add($"{x}|{y}|0|0|Solid|False|Wall::Blocked::Black|False|||BigTileTop|Brick3Side");
                blocks.Add($"{x}|{y}|1|0|Solid|False|Wall::Blocked::Black|False|||BigTileTop|Brick3Side");
                blocks.Add($"{x}|{y}|2|0|Solid|False|Wall::Blocked::Black|False|||BigTileTop|Brick3Side");
                break;
        }
        foreach(string block in blocks) {
            Block.ReadIn("v2", block);
        }
    }
}
