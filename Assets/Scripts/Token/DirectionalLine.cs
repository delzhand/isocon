using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DirectionalLine : NetworkBehaviour
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

    private LineRenderer _line;

    void Start()
    {
        _line = gameObject.AddComponent<LineRenderer>();
        _line.startWidth = .05f;
        _line.endWidth = .05f;
        _line.numCapVertices = 0;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.material = Resources.Load<Material>("Materials/Line");
        _line.startWidth = .2f;
        _line.endWidth = .2f;
        _line.textureMode = LineTextureMode.Tile;
        _line.textureScale = new Vector2(5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        _line.positionCount = 0;
        if (!Visible || !ValidTarget)
        {
            return;
        }

        TokenData data = TokenData.Find(TokenId);
        // Don't show while token is moving
        if (data.WorldObject.GetComponent<MoveLerp>())
        {
            return;
        }

        float density = .25f;
        float amplitude = 1f;
        Color color = ColorUtility.UISelectYellow;
        _line.textureScale = new Vector2(5f, 1f);
        Vector3 originOffset = new Vector3(0, .25f, 0);
        Vector3 targetOffset = new Vector3(0, .25f, 0);

        if (Op == "Placing")
        {
            _line.textureScale = new Vector2(1f, 1f);
        }

        if (Op == "Attacking")
        {
            amplitude = 0f;
            color = Color.red;
            originOffset = new Vector3(0, .5f, 0);
            targetOffset = new Vector3(0, .5f, 0);
        }

        _line.colorGradient = GetGradient(color);

        Vector3 origin = data.WorldObject.transform.position + originOffset;
        if (!data.Placed)
        {
            Vector2 v = data.UnitBarElement.worldBound.center;
            string uiScale = Preferences.Current.UIScale;
            float value = float.Parse(uiScale.Replace("%", "")) / 100f;
            v *= value;
            origin = Camera.main.ScreenToWorldPoint(new Vector3(v.x, Screen.height - v.y, 0));
        }


        List<Vector3> points = GenerateParabolaPoints(origin, Target + targetOffset, density, amplitude);
        _line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            _line.SetPosition(i, points[i]);
        }
    }

    public void Init(string id, string op)
    {
        TokenId = id;
        Visible = true;
        Op = op;
    }

    public void Deinit()
    {
        TokenId = null;
        Visible = false;
        Op = null;
    }

    public void SetTarget(Vector3 v)
    {
        Target = v;
        ValidTarget = true;
    }

    public void UnsetTarget()
    {
        ValidTarget = false;
    }

    public List<Vector3> GenerateParabolaPoints(Vector3 origin, Vector3 target, float density, float parabolaHeight)
    {
        List<Vector3> points = new List<Vector3>();

        // Calculate the distance between origin and target
        float distance = Vector3.Distance(origin, target);
        if (distance < .9f)
        {
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

    private Gradient GetGradient(Color c)
    {
        var gradient = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(c, 0f);
        colors[1] = new GradientColorKey(c, 1f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[4];
        alphas[0] = new GradientAlphaKey(0f, 0f);
        alphas[1] = new GradientAlphaKey(1f, .33f);
        alphas[2] = new GradientAlphaKey(1f, .67f);
        alphas[3] = new GradientAlphaKey(0f, 1f);

        gradient.SetKeys(colors, alphas);

        return gradient;
    }
}
