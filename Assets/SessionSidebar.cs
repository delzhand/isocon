using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionSidebar : MonoBehaviour
{
    public static string SessionFile;

    private List<string> sessionFiles;

    void Awake() {
        UI.System.Q<Button>("SessionToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("SessionSidebar");
        });  

        sessionFiles = GetSessionFiles();
        if (sessionFiles.Count == 0) {
            NewSession();
        }
        if (sessionFiles.Count > 0) {
            UI.System.Q<DropdownField>("SessionField").choices = sessionFiles;
            SessionFile = PlayerPrefs.GetString("LastSession", "default");
            if (sessionFiles.Contains(SessionFile)) {
                UI.System.Q<DropdownField>("SessionField").value = SessionFile;
            }
        }


        UI.System.Q<Button>("NewSessionButton").RegisterCallback<ClickEvent>((evt) => {

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
        SessionFile = DateTime.Now.ToString("yyyy’-‘MM’-‘dd");
        string compositeFilename = SessionFile + ".json";
        if (sessionFiles.Contains(compositeFilename)) {
            int counter = 0;
            while (sessionFiles.Contains(compositeFilename)) {
                compositeFilename = SessionFile + "_" + ++counter;
            }
            SessionFile = SessionFile + "_" + counter;
        }
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        File.WriteAllText(path + "/sessions/" + SessionFile + ".json", "");
        UI.System.Q<DropdownField>("SessionField").value = SessionFile;
        GetSessionFiles();
    }
}
