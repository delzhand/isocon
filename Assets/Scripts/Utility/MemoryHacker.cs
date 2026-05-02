using UnityEngine;

public class MemoryHacker : MonoBehaviour
{
    public static float Timer;
    private static float Interval = 300;

    public float _Timer;
    public static void Setup()
    {
        Timer = Interval;
    }

    void Update()
    {
        _Timer = Timer;
        Timer -= Time.deltaTime;

        if (Timer <= 0)
        {
            Resources.UnloadUnusedAssets();
            Timer += Interval;
        }
    }
}
