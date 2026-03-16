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
    public int SkipTutorials;
    public bool OverrideRules;
    public bool ShowHUD;
    public int TargetFramerate;
    public bool DragPan;
    public string MaleghastFile;
    public int AutosaveInterval = 300;
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
            SkipTutorials = PlayerPrefs.GetInt("SkipTutorials", 0),
            TargetFramerate = PlayerPrefs.GetInt("TargetFramerate", 30),
            ShowHUD = true,
            DragPan = true,
            MaleghastFile = PlayerPrefs.GetString("MaleghastFile", ""),
            AutosaveInterval = PlayerPrefs.GetInt("AutosaveInterval", 300),
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
        }
    }

    public static void SetDataPath(string value)
    {
        PlayerPrefs.SetString("DataFolder", value);
        _current.DataPath = value;
        Save();
    }

    public static void SetShowHUD(bool value)
    {
        _current.ShowHUD = value;
        Save();
    }

    public static void SetDragPan(bool value)
    {
        _current.DragPan = value;
        Save();
    }


    public static void SetUIScale(string value)
    {
        _current.UIScale = value;
        Save();
    }

    public static void SetWorldUIScale(string value)
    {
        _current.WorldUIScale = value;
        Save();
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

    public static void SetReleaseNotesSeen(string value)
    {
        _current.ReleaseNotesSeen = value;
        Save();
    }

    public static string GetReleaseNotesSeen()
    {
        return (_current.ReleaseNotesSeen != null) ? _current.ReleaseNotesSeen : "";
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

    public static void SetBlockBorderOpacity(float value)
    {
        _current.BlockBorderOpacity = value;
        Save();
    }

    public static void SetTargetFramerate(int value)
    {
        _current.TargetFramerate = value;
        Save();
    }

    public static void SetMaleghastFile(string value)
    {
        _current.MaleghastFile = value;
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
        string path = Application.persistentDataPath;
        string fileName = $"{path}/config.dat";
        return fileName;
    }
}