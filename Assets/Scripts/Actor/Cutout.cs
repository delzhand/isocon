using System;
using UnityEngine;

public class Cutout : MonoBehaviour
{
    public Actor GetActor()
    {
        Transform t = transform;
        while (t.parent != null)
        {
            if (t.parent.tag == "Actor")
            {
                return t.parent.GetComponent<Actor>();
            }
            t = t.parent.transform;
        }
        throw new Exception("Could not find parent actor.");
    }
}
