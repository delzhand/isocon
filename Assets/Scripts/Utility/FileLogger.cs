using System;
using System.IO;
using UnityEngine;

public class FileLogger : MonoBehaviour
{
    public static void Write(string message)
    {
        string path = Preferences.Current.DataPath;
        if (!Directory.Exists(path + "/logs"))
        {
            Directory.CreateDirectory(path + "/logs");
        }
        string filename = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        message = "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;
        File.AppendAllText(path + "/logs/" + filename, message);
    }
}
