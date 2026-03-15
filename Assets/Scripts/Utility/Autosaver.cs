using UnityEngine;

public class Autosaver : MonoBehaviour
{
    public static float Timer;

    public float _Timer;
    public static void Setup()
    {
        Timer = Preferences.Current.AutosaveInterval;
    }

    void Update()
    {
        _Timer = Timer;
    }

    public static void Tick()
    {
        Timer -= Time.deltaTime;

        if (Timer <= 0)
        {
            Session.SerializeSession("autosave.json");
            Timer += Preferences.Current.AutosaveInterval;
        }

    }
}
