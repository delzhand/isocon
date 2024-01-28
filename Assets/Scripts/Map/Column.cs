using UnityEngine;

public class Column : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int Coordinate => new Vector2Int(X, Y);
    public Vector3Int Coordinate3 => new Vector3Int(X, Y, 0);

    public void Set(int x, int y)
    {
        X = x;
        Y = y;
    }

    void Update()
    {
        Vector3 v = new Vector3(X, 0, Y);
        if (TerrainController.GridType == "Hex")
        {
            v.z *= .866f;
            if (Y % 2 == 1)
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
