using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileLogger : MonoBehaviour
{
    public static void Write(string message) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        if (!Directory.Exists(path + "/logs")) {
            Directory.CreateDirectory(path + "/logs");
        }
        string filename = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        message = "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;
        File.AppendAllText(path + "/logs/" + filename, message);
    }
}
