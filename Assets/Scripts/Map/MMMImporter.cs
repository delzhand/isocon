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

        TerrainController.ResetTerrain(width, height, 0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x * width + y;
                int brush = int.Parse(decodedData[index].ToString());
                Build(x, y, brush);
            }
        }
    }

    private static void Build(int x, int y, int i)
    {
        List<string> blocks = new();
        switch (i)
        {
            case 0:
                // empty
                blocks.Add($"{x}|{y}|0|0|Solid|False||BigTileTop::#A0A0A0|ColorOnly::#000000");
                break;
            case 1:
                // adverse terrain
                blocks.Add($"{x}|{y}|0|0|Solid|False|Adverse Terrain::Wavy::White|BigTileTop::#A0A0A0|ColorOnly::#000000");
                break;
            case 2:
                // elevation
                blocks.Add($"{x}|{y}|0|0|Solid|False||BigTileTop::#A0A0A0|ColorOnly::#000000");
                blocks.Add($"{x}|{y}|1|0|Solid|True||BigTileTop::#FFFFFF|Brick3Side::#FFFFFF");
                break;
            case 3:
                // hazard
                blocks.Add($"{x}|{y}|0|0|Solid|False|Hazard::Spiky::White|BigTileTop::#A0A0A0|ColorOnly::#000000");
                break;
            case 4:
                // objective
                blocks.Add($"{x}|{y}|0|0|Solid|False|Objective::Border::Green|BigTileTop::#A0A0A0|ColorOnly::#000000");
                break;
            case 5:
                // special
                blocks.Add($"{x}|{y}|0|0|Solid|False|Special::Border::Blue|BigTileTop::#A0A0A0|ColorOnly::#000000");
                break;
            case 6:
                // stairs
                blocks.Add($"{x}|{y}|0|0|Solid|False||BigTileTop::#A0A0A0|ColorOnly::#000000");
                blocks.Add($"{x}|{y}|1|0|Steps|True||SmallTileTop::#DDDDDD|Brick3Side::#666666");
                break;
            case 7:
                // wall
                blocks.Add($"{x}|{y}|0|0|Solid|False||BigTileTop::#A0A0A0|ColorOnly::#000000");
                blocks.Add($"{x}|{y}|1|0|Solid|True||BigTileTop::#FFFFFF|Brick3Side::#FFFFFF");
                blocks.Add($"{x}|{y}|2|0|Solid|True|Wall::Blocked::Black|BigTileTop::#FFFFFF|Brick3Side::#FFFFFF");
                break;
        }
        foreach (string block in blocks)
        {
            Block.ReadIn("v3", block);
        }
        Environment.SetBackgroundColors(Color.black, ColorUtility.GetColor("333333"));
    }
}
