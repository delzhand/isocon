using System.Collections.Generic;
using UnityEngine;

public class TileShare
{
    public static void Offsets()
    {
        GameObject[] tokenObjs = GameObject.FindGameObjectsWithTag("Token");
        foreach (GameObject tokenObj in tokenObjs)
        {
            SetOffset(tokenObj.GetComponent<Token>());
        }
    }

    private static void SetOffset(Token t)
    {
        List<Token> sharing = GetNearbyTokens(t.transform.position);
        if (sharing.Count < 2)
        {
            // Nobody else at tile
            t.ShareOffsetX = 0;
            t.ShareOffsetY = 0;
        }
        else
        {
            // sharing.Add(t);
            float[,] offsets = {
                {0, -.33f}, {0, .33f},
                {-.33f, 0}, {.33f, 0},
            };
            for (int i = 0; i < sharing.Count; i++)
            {
                if (i < offsets.GetLength(0))
                {
                    sharing[i].ShareOffsetX = offsets[i, 0];
                    sharing[i].ShareOffsetY = offsets[i, 1];
                }
                else
                {
                    sharing[i].ShareOffsetX = 0;
                    sharing[i].ShareOffsetY = 0;
                }
            }
        }
    }

    public static List<Token> GetNearbyTokens(Vector3 v, float threshold = .1f)
    {
        List<Token> sharing = new();
        GameObject[] tokenObjects = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokenObjects.Length; i++)
        {
            Token t = tokenObjects[i].GetComponent<Token>();
            float distance = Vector3.Distance(t.transform.localPosition, v);
            if (distance < threshold)
            {
                sharing.Add(t);
            }
        }
        return sharing;
    }

}
