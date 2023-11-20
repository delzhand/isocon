using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Aura {
    public string Name;
    public int Size;
    public Aura(string name, int size) {
        Name = name;
        Size = size;
    }
}

public class AuraManager : MonoBehaviour
{
    private List<GameObject> _indicators = new List<GameObject>();
    private List<Aura> _auras = new List<Aura>();
    private int _lastSize;

    public void AddAura(string name, int size) {
        _auras.Add(new Aura(name, size));
    }

    void Update() {
        int currentSize = 0;
        for (int i = 0; i < _auras.Count; i++) {
            currentSize = math.max(currentSize, _auras[i].Size);
        }

        if (_lastSize != currentSize) {
            Redraw(currentSize);
            _lastSize = currentSize;
        }
    }

    private void Redraw(int size) {
        foreach(GameObject g in _indicators) {
            GameObject.Destroy(g);
        }
        _indicators.Clear();

        Vector2 origin = new Vector2(transform.position.x, transform.position.z);

        Vector2Int[] affected = AffectedCoords(size, new Vector2Int((int)origin.x, (int)origin.y));
        foreach (Vector2Int v in affected) {
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/AuraProjector"));
            g.transform.parent = transform;
            g.transform.position = new Vector3(v.x, 10, v.y);
            _indicators.Add(g);
        }
        
    }

    private Vector2Int[] AffectedCoords(int distance, Vector2Int origin)
    {
        // Ensure the distance is non-negative
        distance = Mathf.Max(0, distance);

        // Create a list to store the coordinates
        List<Vector2Int> coordinatesList = new List<Vector2Int>();

        // Iterate through all possible coordinates within the specified Manhattan distance
        for (int x = -distance; x <= distance; x++)
        {
            for (int y = -distance; y <= distance; y++)
            {
                // Calculate Manhattan distance
                int currentDistance = Mathf.Abs(x) + Mathf.Abs(y);

                // Check if the current coordinate is within the specified distance
                if (currentDistance <= distance)
                {
                    // Add the coordinate to the list
                    Vector2Int coordinate = new Vector2Int(origin.x + x, origin.y + y);
                    coordinatesList.Add(coordinate);
                }
            }
        }

        // Convert the list to an array and return it
        return coordinatesList.ToArray();
    }
    
}
