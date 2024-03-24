using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class Startup
{
    private static string _version = "0.7.1";
    private static string _latestVersion = "0.7.1";

    public static void RunTasks()
    {
        Preferences.Init();
        SetVersionText();
        UI.SetScale();
        Modal.Setup();
        BlockRendering.Setup();
        DiceRoller.Setup();
        MapEdit.Setup();
        TokenLibrary.Setup();

        UI.SetBlocking(UI.System, StringUtility.CreateArray(@"SelectionMenu", "TopBar", "BottomBar", "ToolsPanel", "ToolOptions", "SelectedTokenPanel", "FocusedTokenPanel", "Backdrop"));

        // Useful during development when editing UI
        UI.ToggleDisplay("Tabletop", false);

        ReleaseNotes();
    }

    private static void ReleaseNotes()
    {
        string version = "0.7.2";
        string notes = @"<size=+3><b>IsoCON Version 0.7.2</b></size>

<size=+2><b>Improvements</b></size>
* Homebrew rule selection has been moved to the launcher

<size=+2><b>Fixes</b></size>
* Turn Advance button now functions correctly
* Configured outline color is applied to new tokens
* A bug preventing maps over a certain filesize from being shared has been resolved.
* The Resolve/Party Resolve bar in ICON 1.5 token display has been fixed.
* Long patch notes are now scrollable and shouldn't overflow the screen at certain resolution/scales";

        string seen = Preferences.GetReleaseNotesSeen();
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(version))
        {
            return;
        }

        Modal.Reset("Release Notes");
        Modal.AddLongMarkup("ReleaseNotes", notes);
        Modal.AddPreferredButton("Close", (evt) =>
        {
            seenParts.Add(version);
            Preferences.SetReleaseNotesSeen(string.Join("|", seenParts.ToArray()));
            Modal.Close();
        });

    }

    private static async void SetVersionText()
    {
#if UNITY_WEBGL
        UI.System.Q<Label>("Version").text = $"v{_version}";
        return;
#endif

        await AsyncAwake();
        if (_version != _latestVersion)
        {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = ColorUtility.UIBlue;
        }
        else
        {
            UI.System.Q<Label>("Version").text = $"v{_version}";
        }
    }

    private static async Task AsyncAwake()
    {
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        await RemoteConfigService.Instance.FetchConfigsAsync(new AppAttributes(), new AppAttributes());
    }

    private static async Task InitializeRemoteConfigAsync()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private static void ApplyRemoteConfig(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                FileLogger.Write("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                FileLogger.Write("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                FileLogger.Write("New settings loaded this session; update values accordingly.");
                break;
        }
        _latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");
        string latestData = RemoteConfigService.Instance.appConfig.GetJson("GameSystem");

        string path = Preferences.Current.DataPath;
        if (!Directory.Exists($"{path}/ruledata"))
        {
            Directory.CreateDirectory($"{path}/ruledata");
        }
        string fileName = "latest.json";
        System.IO.File.WriteAllText($"{path}/ruledata/{fileName}", latestData);
    }

    public struct AppAttributes
    {
        public string LatestVersion;
    }

    public static string[] GetArguments()
    {
#if (UNITY_WEBGL) && !UNITY_EDITOR
            if (Application.absoluteURL.Contains("isocon.app"))
            {
                string parameters = Application.absoluteURL.Substring(Application.absoluteURL.IndexOf("?")+1);
                return parameters.Split(new char[] { '&', '=' });
            }
#endif
        return new string[] { };
    }

}
