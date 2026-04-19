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
            SessionManager.SerializeSession($"{Preferences.Current.DataPath}/sessions/autosave.json", false);
            Toast.Add($"Autosaving...", "", ShunUI.ToastVariant.Success);
            Timer += Preferences.Current.AutosaveInterval;
        }

    }

    public static void Immediate()
    {
        SessionManager.SerializeSession($"{Preferences.Current.DataPath}/sessions/autosave.json", false);
        Toast.Add($"Autosaving...", "", ShunUI.ToastVariant.Success);
        Timer = Preferences.Current.AutosaveInterval;
    }
}
