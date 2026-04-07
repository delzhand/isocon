using System;
using System.IO;
using UnityEngine;

[Serializable]
public class StoredPreferences
{
    public string DataPath;
    public string PlayerName;
    public string UIScale;
    public string WorldUIScale = "100%";
    public float TokenScale;
    public string TokenOutline;
    public string Grid;
    public float BlockBorderOpacity;
    public int PlayerCount;
    public string HostIP;
    public string TutorialsSeen;
    public string ReleaseNotesSeen;
    public bool SkipTutorials;
    public bool OverrideRules;
    public bool ShowHUD;
    public int TargetFramerate;
    public bool PanWithRight;
    public string MaleghastFile;
    public int AutosaveInterval = 300;
    public bool ShowIndicators;
    public string LastActorType;
}

public class Preferences
{
    private static StoredPreferences _current;
    public static StoredPreferences Current
    {
        get => _current;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        _current = new()
        {
            DataPath = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath),
            PlayerName = PlayerPrefs.GetString("PlayerName", "New Player"),
            UIScale = PlayerPrefs.GetString("UIScale", "100%"),
            WorldUIScale = PlayerPrefs.GetString("WorldUIScale", "100%"),
            TokenScale = PlayerPrefs.GetFloat("TokenScale", 1f),
            Grid = PlayerPrefs.GetString("Grid", "Square"),
            TokenOutline = PlayerPrefs.GetString("TokenOutline", "White"),
            PlayerCount = PlayerPrefs.GetInt("PlayerCount", 4),
            HostIP = PlayerPrefs.GetString("HostIP", ""),
            TutorialsSeen = PlayerPrefs.GetString("TutorialsSeen", ""),
            ReleaseNotesSeen = PlayerPrefs.GetString("ReleaseNotesSeen", ""),
            SkipTutorials = false,
            TargetFramerate = PlayerPrefs.GetInt("TargetFramerate", 30),
            ShowHUD = true,
            PanWithRight = false,
            MaleghastFile = PlayerPrefs.GetString("MaleghastFile", ""),
            AutosaveInterval = PlayerPrefs.GetInt("AutosaveInterval", 300),
            BlockBorderOpacity = PlayerPrefs.GetFloat("BlockBorderOpacity", 0),
            ShowIndicators = false,
            LastActorType = PlayerPrefs.GetString("LastActorType", ""),
        };

        string fileName = GetConfigFileName();
        // Load preferences from application directory if found
        if (File.Exists(fileName))
        {
            string json = File.ReadAllText(fileName);
            StoredPreferences loaded = JsonUtility.FromJson<StoredPreferences>(json);

            _current.DataPath = loaded.DataPath.Length > 0 ? loaded.DataPath : _current.DataPath;
            _current.PlayerName = loaded.PlayerName.Length > 0 ? loaded.PlayerName : _current.PlayerName;
            _current.UIScale = loaded.UIScale.Length > 0 ? loaded.UIScale : _current.UIScale;
            _current.WorldUIScale = loaded.WorldUIScale.Length > 0 ? loaded.WorldUIScale : _current.WorldUIScale;
            _current.TokenScale = loaded.TokenScale > 0 ? loaded.TokenScale : _current.TokenScale;
            _current.Grid = loaded.Grid.Length > 0 ? loaded.Grid : _current.Grid;
            _current.TokenOutline = loaded.TokenOutline.Length > 0 ? loaded.TokenOutline : _current.TokenOutline;
            _current.PlayerCount = loaded.PlayerCount > 0 ? loaded.PlayerCount : _current.PlayerCount;
            _current.HostIP = loaded.HostIP.Length > 0 ? loaded.HostIP : _current.HostIP;
            _current.ReleaseNotesSeen = loaded.ReleaseNotesSeen.Length > 0 ? loaded.ReleaseNotesSeen : _current.ReleaseNotesSeen;
            _current.SkipTutorials = loaded.SkipTutorials;
            _current.TargetFramerate = loaded.TargetFramerate > 0 ? loaded.TargetFramerate : _current.TargetFramerate;
            _current.MaleghastFile = loaded.MaleghastFile.Length > 0 ? loaded.MaleghastFile : _current.MaleghastFile;
            _current.AutosaveInterval = loaded.AutosaveInterval > 0 ? loaded.AutosaveInterval : _current.AutosaveInterval;
            _current.TutorialsSeen = loaded.TutorialsSeen.Length > 0 ? loaded.TutorialsSeen : _current.TutorialsSeen;
            _current.ShowHUD = loaded.ShowHUD;
            _current.BlockBorderOpacity = loaded.BlockBorderOpacity > 0 ? loaded.BlockBorderOpacity : _current.BlockBorderOpacity;
            _current.ShowIndicators = loaded.ShowIndicators;
            _current.PanWithRight = loaded.PanWithRight;
            _current.LastActorType = loaded.LastActorType?.Length > 0 ? loaded.LastActorType : _current.LastActorType;
        }

        DirectorySetup();
    }

    private static void DirectorySetup()
    {
        string[] directories = { "maleghast_data", "hashed_tokens", "maps", "tokens", "logs", "sessions" };
        for (int i = 0; i < directories.Length; i++)
        {
            string path = $"{_current.DataPath}/{directories[i]}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public static float GetUIScale()
    {
        string uiScale = Preferences.Current.UIScale;
        return float.Parse(uiScale.Replace("%", "")) / 100f;
    }

    public static float GetWorldUIScale()
    {
        string uiScale = Preferences.Current.WorldUIScale;
        return float.Parse(uiScale.Replace("%", "")) / 100f;
    }

    public static void SetTutorialsSeen(string value)
    {
        _current.TutorialsSeen = value;
        Save();
    }

    public static void SetReleaseNotesSeen(string value)
    {
        _current.ReleaseNotesSeen = value;
        Save();
    }

    public static string GetReleaseNotesSeen()
    {
        return (_current.ReleaseNotesSeen != null) ? _current.ReleaseNotesSeen : "";
    }

    public static void Save()
    {
        string fileName = GetConfigFileName();
        string json = JsonUtility.ToJson(_current);
        File.WriteAllText(fileName, json);
    }

    private static string GetConfigFileName()
    {
        string path = Application.persistentDataPath;
        string fileName = $"{path}/config.dat";
        return fileName;
    }
}