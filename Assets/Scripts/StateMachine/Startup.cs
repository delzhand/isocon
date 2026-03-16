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
    private static string _version = "0.8.0";
    private static string _latestVersion = "0.8.0";

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
        Autosaver.Setup();
        Tutorial.Setup();

        UI.SetBlocking(UI.System, StringUtility.CreateArray(@"SelectionMenu", "TopBar", "BottomBar", "ToolsPanel", "ToolOptions", "LeftTokenPanel", "RightTokenPanel", "Backdrop", "NumberPickerModal", "TopRight"));
        Application.targetFrameRate = Preferences.Current.TargetFramerate;

        // Useful during development when editing UI
        UI.ToggleDisplay("Tabletop", false);

        ReleaseNotes();
    }

    private static void ReleaseNotes()
    {
        string notes = @$"<size=+3><b>IsoCON Version {_version}</b></size>

<size=+2><b>Features</b></size>
* Sessions can now be saved and loaded
* Session will autosave every 5 minutes or when exiting to launcher
* GameSystems replaced by Actor Types
* Multiple Actor Types can be active in the same session
* Actors can be customized with resources and stats
* Fixed view button added to top bar
* Tags and clocks can be added to sessions

<size=+2><b>Changes</b></size>
* Tokens renamed Actors
* Actor focus/selection behavior changed
* Actor size changed to shape, more hex options added
* Top bar and actor list can be hidden
* Certain actor shapes can be dragged to intersections to remain centered

<size=+2><b>New Actor Types</b></size>
* Environmental - a type with no stats
* Lancer Mech - a player type for LANCER
* Lancer Pilot - a player type for LANCER
* ICON 1.5 split into Player, Enemy, and Mob
* ICON 2.0 split into Player, Enemy, and Mob
* Generic renamed to Basic

<size=+2><b>Fixes</b></size>
* Shortcut keystrokes no longer trigger when modals are open
";

        string seen = Preferences.GetReleaseNotesSeen();
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(_version))
        {
            return;
        }

        Modal.Reset("Release Notes");
        Modal.AddLongMarkup("ReleaseNotes", notes);
        Modal.AddPreferredButton("Close", (evt) =>
        {
            seenParts.Add(_version);
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
