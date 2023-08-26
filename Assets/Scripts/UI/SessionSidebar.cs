using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionSidebar : MonoBehaviour
{
    public static string SessionFile;

    void Awake() {
        UI.System.Q<Button>("SessionToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("SessionSidebar");
        });  

        List<string> sessionFiles = GetSessionFiles();
        if (sessionFiles.Count == 0) {
            NewSession();
        }
        else if (sessionFiles.Count > 0) {
            UI.System.Q<DropdownField>("SessionField").choices = sessionFiles;
            SessionFile = PlayerPrefs.GetString("LastSession", "default");
            if (sessionFiles.Contains(SessionFile)) {
                UI.System.Q<DropdownField>("SessionField").value = SessionFile;
            }
        }

        UI.System.Q<DropdownField>("SessionField").RegisterValueChangedCallback<string>((evt) => {
            SessionFile = evt.newValue;
            PlayerPrefs.SetString("LastSession", SessionFile);
        });

        UI.System.Q<Button>("NewSessionButton").RegisterCallback<ClickEvent>((evt) => {
            NewSession();
        });
    }

    void Update() {
        if (!Player.IsHost()) {
            UI.ToggleDisplay("SessionToggle", false);
            UI.ToggleDisplay("SessionSidebar", false);
            return;
        }
        else {
            UI.ToggleDisplay("SessionToggle", true);
        }        
    }

    private List<string> GetSessionFiles() {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        
        if (!Directory.Exists(path + "/sessions")) {
            Directory.CreateDirectory(path + "/sessions");
        }

        List<string> sessionFiles = new List<string>{};
        DirectoryInfo info = new DirectoryInfo(path + "/sessions/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                sessionFiles.Add(fileInfo[i].Name);
            }        
        }
        return sessionFiles;
    }

    private void NewSession() {
        List<string> sessionFiles = GetSessionFiles();
        SessionFile = DateTime.Now.ToString("yyyy-MM-dd");
        string compositeFilename = SessionFile + ".json";
        if (sessionFiles.Contains(compositeFilename)) {
            int counter = 0;
            while (sessionFiles.Contains(compositeFilename)) {
                compositeFilename = SessionFile + "_" + ++counter;
            }
            SessionFile = SessionFile + "_" + counter;
        }
        SessionFile += ".json";
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        File.WriteAllText(path + "/sessions/" + SessionFile + ".json", "");
        sessionFiles = GetSessionFiles();
        UI.System.Q<DropdownField>("SessionField").choices = sessionFiles;
        UI.System.Q<DropdownField>("SessionField").value = SessionFile;
        PlayerPrefs.SetString("LastSession", SessionFile);

    }
}
