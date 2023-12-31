using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DestinationRenderer : NetworkBehaviour
{
    [SyncVar]
    public string TokenId;

    [SyncVar]
    public Vector3 Target;

    [SyncVar]
    public bool Visible;

    [SyncVar]
    public bool ValidTarget;

    [SyncVar]
    public string Op;

    public static float Density = .25f;

    private LineRenderer Line;

    void Start() {
        Line = gameObject.AddComponent<LineRenderer>();
        Line.startColor = ColorUtility.UISelectYellow;
        Line.endColor = Color.white;
        Line.startWidth = .05f;
        Line.endWidth = .05f;
        Line.numCapVertices = 4;
        Line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // Line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        Line.material = Resources.Load<Material>("Materials/Line");
    }

    // Update is called once per frame
    void Update()
    {
        Line.positionCount = 0;
        if (!Visible || !ValidTarget) {
            return;
        }



        TokenData data = TokenData.Find(TokenId);
        Vector3 origin = data.WorldObject.transform.position + new Vector3(0, .25f, 0);
        List<Vector3> points = GenerateParabolaPoints(origin, Target, Density, 1f);
        Line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) {
            Line.SetPosition(i, points[i]);
        }
    }

    public void Init(string id, string op) {
        TokenId = id;
        Visible = true;
        Op = op;
    }

    public void Deinit() {
        TokenId = null;
        Visible = false;
        Op = null;
    }

    public void SetTarget(Vector3 v) {
        Target = v;
        ValidTarget = true;
    }

    public void UnsetTarget() {
        ValidTarget = false;
    }
    
    public List<Vector3> GenerateParabolaPoints(Vector3 origin, Vector3 target, float density, float parabolaHeight)
    {
        List<Vector3> points = new List<Vector3>();

        // Calculate the distance between origin and target
        float distance = Vector3.Distance(origin, target);
        if (distance < .9f) {
            return points;
        }

        // Calculate the number of points based on density
        int numPoints = Mathf.CeilToInt(distance / density);

        // Iterate through the points and calculate their positions along the parabola
        for (int i = 0; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            float x = Mathf.Lerp(origin.x, target.x, t);
            float y = ParabolaFunction(t, origin.y, target.y, parabolaHeight);
            float z = Mathf.Lerp(origin.z, target.z, t);

            points.Add(new Vector3(x, y, z));
        }

        return points;
    }

    // Parabola function to calculate y based on t (parameterized value)
    private float ParabolaFunction(float t, float startY, float endY, float amplitude)
    {
        float straightLine = Mathf.Lerp(startY, endY, t);
        float sineWave = amplitude * Mathf.Sin(Mathf.PI * t);

        return straightLine + sineWave;
    }
}
