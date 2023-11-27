using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutout : MonoBehaviour
{
    public Token GetToken() {
        Transform t = transform;
        while (t.parent != null)
        {
            if (t.parent.tag == tag)
            {
                return t.GetComponent<Token>();
            }
            t = t.parent.transform;
        }
        throw new Exception("Could not find parent token.");
    }
}
