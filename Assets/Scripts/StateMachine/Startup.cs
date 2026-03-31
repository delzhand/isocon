using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using ShunUI;
using ShunUI.Primitives;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class Startup
{
    private static string _version = "0.8.2";
    private static string _latestVersion = "0.8.2";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void RunTasks()
    {
        Preferences.Init();
        SetVersionText();
        UI.SetScale();
        Modal.Setup();
        BlockRendering.Setup();
        DiceRoller.Setup();
        MapEdit.Setup();
        TokenLibraryModal.Setup();
        Autosaver.Setup();
        Tutorial.Setup();
        Viewport.Setup();

        UI.SetBlocking(UI.System, StringUtility.CreateArray(@"SelectionMenu", "TopBar", "BottomBar", "ToolsPanel", "ToolOptions", "LeftTokenPanel", "RightTokenPanel", "Backdrop", "NumberPickerModal", "TopRight"));
        Application.targetFrameRate = Preferences.Current.TargetFramerate;

        ReleaseNotes();
        BindUICallbacks();
    }

    private static void ReleaseNotes()
    {
        string seen = Preferences.GetReleaseNotesSeen();
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(_version))
        {
            return;
        }
        ReleaseNotesModal.Open(_version);
    }

    private static void BindUICallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>((evt) =>
        {
            // StateManager.Find().ChangeSubState(new MapEditingState());
        });
        UI.TopBar.Q("AddActor").RegisterCallback<ClickEvent>((evt) => AddActorModal.Open());
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>((evt) => ConfigModal.Open());
        UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>((evt) => Viewport.FixView());
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>((evt) => DiceRoller.ToggleVisible());
        UI.System.Q("TopBarToggle").RegisterCallback<ClickEvent>((evt) =>
        {
            UI.ToggleActiveClass(UI.System.Q("TopBar"));
        });
        UI.System.Q("DeployToggle").RegisterCallback<ClickEvent>((evt) =>
        {
            UI.ToggleActiveClass(UI.System.Q("BottomBar"));
        });
        UI.System.Q("AddSystemTag").RegisterCallback<ClickEvent>((evt) => SystemTagModal.Open());
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
