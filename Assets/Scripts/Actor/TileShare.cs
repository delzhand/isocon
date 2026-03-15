using System.Collections.Generic;
using UnityEngine;

public class TileShare
{
    public static void Offsets()
    {
        GameObject[] actorObjs = GameObject.FindGameObjectsWithTag("Actor");
        foreach (GameObject actorObj in actorObjs)
        {
            SetOffset(actorObj.GetComponent<Actor>());
        }
    }

    private static void SetOffset(Actor t)
    {
        List<Actor> sharing = GetNearbyActors(t.transform.position);
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

    public static List<Actor> GetNearbyActors(Vector3 v, float threshold = .1f)
    {
        List<Actor> sharing = new();
        GameObject[] actorObjs = GameObject.FindGameObjectsWithTag("Actor");
        for (int i = 0; i < actorObjs.Length; i++)
        {
            Actor t = actorObjs[i].GetComponent<Actor>();
            float distance = Vector3.Distance(t.transform.localPosition, v);
            if (distance < threshold)
            {
                sharing.Add(t);
            }
        }
        return sharing;
    }

}
