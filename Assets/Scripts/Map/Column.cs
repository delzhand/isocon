using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    private int x;
    public int X {
        get { return x; }
        private set { x = value; }
    }

    private int y;
    public int Y {
        get { return y; }
        private set { y = value; }
    }

    public void Set(int x, int y) {
        this.x = x;
        this.y = y;
    }

    void Update() {
        Vector3 v = new Vector3(x, 0, y);
        if (TerrainController.GridType == "Hex") {
            v.z *= .866f;
            if (y%2 == 1) {
                v.x += .5f;
            }
        }
        transform.localPosition = v;
    }
}
