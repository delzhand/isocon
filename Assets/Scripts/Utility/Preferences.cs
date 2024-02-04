using System;
using System.IO;
using UnityEngine;

[Serializable]
public class StoredPreferences
{
    public string DataPath;
    public string PlayerName;
    public string System;
    public string UIScale;
    public float TokenScale;
    public string TokenOutline;
    public string Grid;
    public float BlockBorderOpacity;
    public int PlayerCount;
    public string HostIP;
    public string TutorialsSeen;
    public int SkipTutorials;
    public string RulesFile;
}

public class Preferences
{
    private static StoredPreferences _current;
    public static StoredPreferences Current
    {
        get => _current;
    }

    public static void Init()
    {
#if UNITY_WEBGL
        WebGLPrefs();
        return;
#endif

        string fileName = GetConfigFileName();
        // Load preferences from application directory if found
        if (File.Exists(fileName))
        {
            string json = File.ReadAllText(fileName);
            _current = JsonUtility.FromJson<StoredPreferences>(json);
        }
        // Otherwise set defaults from PlayerPrefs if they exist (legacy users) or static value
        else
        {
            _current = new()
            {
                DataPath = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath),
                PlayerName = PlayerPrefs.GetString("PlayerName", "New Player"),
                System = PlayerPrefs.GetString("System", "Generic"),
                UIScale = PlayerPrefs.GetString("UIScale", "100%"),
                TokenScale = PlayerPrefs.GetFloat("TokenScale", 1f),
                Grid = PlayerPrefs.GetString("Grid", "Square"),
                TokenOutline = PlayerPrefs.GetString("TokenOutline", "White"),
                PlayerCount = PlayerPrefs.GetInt("PlayerCount", 4),
                HostIP = PlayerPrefs.GetString("HostIP", ""),
                TutorialsSeen = PlayerPrefs.GetString("TutorialsSeen", ""),
                SkipTutorials = PlayerPrefs.GetInt("SkipTutorials", 0),
                RulesFile = PlayerPrefs.GetString("RulesFile", "lastest.json"),
            };
            Save();
        }
    }

    private static void WebGLPrefs()
    {
        _current = new()
        {
            DataPath = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath),
            PlayerName = PlayerPrefs.GetString("PlayerName", "New Player"),
            System = "",
            UIScale = PlayerPrefs.GetString("UIScale", "100%"),
            TokenScale = PlayerPrefs.GetFloat("TokenScale", 1f),
            Grid = "",
            TokenOutline = PlayerPrefs.GetString("TokenOutline", "White"),
            PlayerCount = 1,
            HostIP = PlayerPrefs.GetString("HostIP", ""),
            TutorialsSeen = PlayerPrefs.GetString("TutorialsSeen", ""),
            SkipTutorials = PlayerPrefs.GetInt("SkipTutorials", 0),
            RulesFile = "",
        };

    }

    public static void SetDataPath(string value)
    {
        PlayerPrefs.SetString("DataFolder", value);
        _current.DataPath = value;
        Save();
    }

    public static void SetUIScale(string value)
    {
        _current.UIScale = value;
        Save();
    }

    public static float GetUIScale()
    {
        string uiScale = Preferences.Current.UIScale;
        return float.Parse(uiScale.Replace("%", "")) / 100f;
    }

    public static void SetTokenScale(float value)
    {
        _current.TokenScale = value;
        Save();
    }

    public static void SetTokenOutline(string value)
    {
        _current.TokenOutline = value;
        Save();
    }

    public static void SetTutorialsSeen(string value)
    {
        _current.TutorialsSeen = value;
        Save();
    }

    public static void SetSkipTutorials(int value)
    {
        _current.SkipTutorials = value;
        Save();
    }

    public static void SetPlayerName(string value)
    {
        _current.PlayerName = value;
        Save();
    }

    public static void SetHostIP(string value)
    {
        _current.HostIP = value;
        Save();
    }

    public static void SetPlayerCount(int value)
    {
        _current.PlayerCount = value;
        Save();
    }

    public static void SetGrid(string value)
    {
        _current.Grid = value;
        Save();
    }

    public static void SetSystem(string value)
    {
        _current.System = value;
        Save();
    }

    public static void SetBlockBorderOpacity(float value)
    {
        _current.BlockBorderOpacity = value;
        Save();
    }

    public static void SetRulesFile(string value)
    {
        _current.RulesFile = value;
        Save();
    }

    private static void Save()
    {
        string fileName = GetConfigFileName();
        string json = JsonUtility.ToJson(_current);
        File.WriteAllText(fileName, json);
    }

    private static string GetConfigFileName()
    {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        if (Preferences.Current != null)
        {
            path = Preferences.Current.DataPath;
        }
        return $"{path}/config.dat";
    }
}