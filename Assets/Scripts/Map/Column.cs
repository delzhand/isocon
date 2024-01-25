using UnityEngine;

public class Column : MonoBehaviour
{
    private int _x;
    public int X
    {
        get { return _x; }
        private set { _x = value; }
    }

    private int _y;
    public int Y
    {
        get { return _y; }
        private set { _y = value; }
    }

    public void Set(int x, int y)
    {
        this._x = x;
        this._y = y;
    }

    void Update()
    {
        Vector3 v = new(_x, 0, _y);
        if (TerrainController.GridType == "Hex")
        {
            v.z *= .866f;
            if (_y % 2 == 1)
            {
                v.x += .5f;
            }
        }
        transform.localPosition = v;
    }

    public Block GetTopBlock()
    {
        return transform.GetChild(transform.childCount - 1).GetComponent<Block>();
    }
}
