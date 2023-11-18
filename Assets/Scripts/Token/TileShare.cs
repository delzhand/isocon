using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileShare
{

    public static void Offsets() {
        GameObject[] tokenObjs = GameObject.FindGameObjectsWithTag("Token");
        foreach (GameObject tokenObj in tokenObjs) {
            SetOffset(tokenObj.GetComponent<Token>());
        }
    }

    private static void SetOffset(Token t) {
        List<Token> sharing = TokensNearby(t.transform.position);
        if (sharing.Count < 2) {
            // Nobody else at tile
            t.ShareOffsetX = 0;
            t.ShareOffsetY = 0;
        }
        else {
            // sharing.Add(t);
            float[,] offsets = {
                {0, -.33f}, {0, .33f},
                {-.33f, 0}, {.33f, 0},
            };
            for (int i = 0; i < sharing.Count; i++) {
                sharing[i].ShareOffsetX = offsets[i, 0];
                sharing[i].ShareOffsetY = offsets[i, 1];
            }
        }
    }

    private static List<Token> TokensNearby(Vector3 v) {
        List<Token> sharing = new();
        GameObject[] tokenObjects = GameObject.FindGameObjectsWithTag("Token");
        for(int i = 0; i < tokenObjects.Length; i++) {
            Token t = tokenObjects[i].GetComponent<Token>();
            float distance = Vector3.Distance(t.transform.localPosition, v);
            if (distance < .1f) {
                sharing.Add(t);
            }
        }
        return sharing;
    }

}
