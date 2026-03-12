using UnityEngine;

public class MoveLerp : MonoBehaviour
{
    public float Duration;
    public float Timer;
    public Vector3 Origin;
    public Vector3 Destination;

    // Start is called before the first frame update
    void Start()
    {
        Timer = Duration;
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;

        float percentage = 1 - (Timer / Duration);

        // Ground translation
        Vector3 lerpPos = Vector3.Lerp(Origin, Destination, percentage);

        // Arc
        float height = Mathf.Sin(Mathf.PI * percentage) * 1;
        lerpPos.y += height;

        gameObject.transform.position = lerpPos;

        if (Timer <= 0)
        {
            gameObject.transform.position = Destination;
            Destroy(this);
        }
    }

    public static void Create(GameObject g, Vector3 destination)
    {
        float distance = Vector3.Distance(destination, g.transform.position);
        if (distance < .01f)
        {
            return;
        }
        MoveLerp ml = g.AddComponent<MoveLerp>();
        ml.Origin = g.transform.position;
        ml.Destination = destination;
        ml.Duration = .35f;
    }
}
