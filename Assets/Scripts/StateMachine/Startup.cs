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
    private static string _version = "0.9.0";
    private static string _latestVersion = "0.9.0";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void RunTasks()
    {
        SetVersionText();
        UI.SetScale();
        BlockRendering.Setup();
        DiceRoller.Setup();
        MapEdit.Setup();
        TokenLibraryModal.Setup();
        Autosaver.Setup();
        MemoryHacker.Setup();
        Tutorial.Setup();
        Viewport.Setup();
        BindUICallbacks();

        UI.SetBlocking(UI.System, StringUtility.CreateArray(@"BottomBar", "ToolsPanel", "ToolOptions", "LeftTokenPanel", "RightTokenPanel", "Backdrop", "TopRight"));
        Application.targetFrameRate = Preferences.Current.TargetFramerate;
    }

    public static void ReleaseNotes()
    {
        ReleaseNotesModal.OpenAtStartup(_version);
    }

    private static void BindUICallbacks()
    {
        UI.System.Q("DeployToggle").RegisterCallback<ClickEvent>((evt) =>
        {
            UI.ToggleActiveClass(UI.System.Q("BottomBar"));
        });
    }

    private static async void SetVersionText()
    {
        UI.System.Q<Button>("Version").RegisterCallback<ClickEvent>((evt) => ReleaseNotesModal.Open());
        await AsyncAwake();
        if (_version != _latestVersion)
        {
            UI.System.Q<Button>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Button>("Version").style.backgroundColor = ColorUtility.UIBlue;
        }
        else
        {
            UI.System.Q<Button>("Version").text = $"v{_version}";
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

}
