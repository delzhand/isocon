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
}
