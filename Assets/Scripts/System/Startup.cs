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
        string version = "0.7.1";
        string notes = @"<size=+3><b>IsoCON Version 0.7.1</b></size>

<size=+2><b>Features</b></size>
* Token Library - Maintain the tokens you plan to use in your games from inside Isocon and search it from the Add Token modal.
* Animated Tokens - Frames of animation should be laid out horizontally, FPS and number of frames can be configured in the token library.
* File Browser - tokens and maps can now be loaded from anywhere, not just the data directory.
* A config option to keep block borders outside of editing/dragging has been added.
* Homebrew support has been added - in the data directory you'll now find a folder called 'rules', and after launching this release it will contain a folder called latest.json. Copy this file and make your edits, and you can then select the new file in the config panel.

<size=+2><b>Improvements</b></size>
* Rebuilt token sync - speed and stability should be vastly improved
* Data HUD added that shows information including the sync status of other players
* The alt key can now be held when style painting blocks to quickly switch to the sample tool.
* Dragging a token into the side of a block now automatically move it to the topmost block.
* Eliminated hundreds of redundant function calls per frame.
* Screen transitions implemented when moving between launcher and tabletop.
* Camera can be rotated with right mouse button and dragged with middle mouse button. The top bar camera option now swaps this functionality.

<size=+2><b>Changes</b></size>
* Add Token button moved out of bottom bar, bottom bar hidden by default, shows now when the new Deploy button is clicked.
* Info/Sync top bar options removed

<size=+2><b>Fixes</b></size>
* Bugs with the dice roller have been resolved.
* 6x8 Maleghast Map Maker maps now import correctly.

<size=+2><b>Known Issues</b></size>
* The change to token placement means that multi-level maps can't have tokens on a lower level anymore.";

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
