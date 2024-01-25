using System;
using UnityEngine;

public class Cutout : MonoBehaviour
{
    public Token GetToken()
    {
        Transform t = transform;
        while (t.parent != null)
        {
            if (t.parent.tag == "Token")
            {
                return t.parent.GetComponent<Token>();
            }
            t = t.parent.transform;
        }
        throw new Exception("Could not find parent token.");
    }
}
