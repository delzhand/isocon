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
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                int index = x * width + y;
                int brush = int.Parse(decodedData[index].ToString());

                // Fix stair rotation aesthetics
                int rotation = 0;
                if (brush == 6) // steps
                {
                    int[] neighborIndices = new int[4];
                    neighborIndices[0] = index - width; // north
                    neighborIndices[1] = index + 1; // east
                    neighborIndices[2] = index + width; // south
                    neighborIndices[3] = index - 1; // west
                    for (int i = 0; i < 4; i++)
                    {
                        bool validWest = i != 3 || index % width != 0; // avoid finding elevation on last tile of previous row
                        bool validEast = i != 1 || index % width != width - 1; // avoid finding elevation on first tile of next row
                        if (validEast && validWest && neighborIndices[i] >= 0 && neighborIndices[i] < decodedData.Length)
                        {
                            int neighborBrush = int.Parse(decodedData[neighborIndices[i]].ToString());
                            if (neighborBrush == 2) // elevation
                            {
                                rotation = i;
                                continue;
                            }
                        }
                    }
                }

                Build(x, y, (rotation - 1) * 90, brush);
            }
        }
        Toast.AddSuccess("Map imported.");
    }

    private static void Build(int x, int y, int r, int i)
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
                blocks.Add($"{x}|{y}|0|90|Solid|False|Adverse Terrain::Wavy::White|BigTileTop::#A0A0A0|ColorOnly::#000000");
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
                blocks.Add($"{x}|{y}|1|{r}|Steps|True||SmallTileTop::#DDDDDD|Brick3Side::#666666");
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
