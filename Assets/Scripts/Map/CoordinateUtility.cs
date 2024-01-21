using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CoordinateUtility
{

    static public Vector2Int[] GetRow(Vector2Int v2)
    {
        var coords = new Vector2Int[99];
        for(int x = 0; x < 99; x++)
        {
            coords[x] = new Vector2Int(x, v2.y);
        } 
        return coords;
    }

    static public Vector2Int[] GetColumn(Vector2Int v2)
    {
        var coords = new Vector2Int[99];
        for (int y = 0; y < 99; y++)
        {
            coords[y] = new Vector2Int(v2.x, y);
        }
        return coords;
    }

    static public Vector2Int[] OffsetCoordinates(Vector2Int center, Vector2Int[] offsets)
    {
        var coords = new Vector2Int[offsets.Length];
        for (int i = 0; i < coords.Length; i++)
        {
            coords[i] = center + offsets[i];
        }
        return coords;
    }

    static public Vector2Int[] GetCardinallyAdjacent(Vector2Int center)
    {
        return OffsetCoordinates(center, new Vector2Int[]{Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down });
    }
}
