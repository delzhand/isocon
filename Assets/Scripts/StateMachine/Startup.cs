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
        // Modal.Setup();
        BlockRendering.Setup();
        DiceRoller.Setup();
        MapEdit.Setup();
        TokenLibraryModal.Setup();
        Autosaver.Setup();
        Tutorial.Setup();
        Viewport.Setup();

        UI.SetBlocking(UI.System, StringUtility.CreateArray(@"SelectionMenu", "TopBar", "BottomBar", "ToolsPanel", "ToolOptions", "LeftTokenPanel", "RightTokenPanel", "Backdrop", "TopRight"));
        Application.targetFrameRate = Preferences.Current.TargetFramerate;

        // ReleaseNotes();
        MainMenuSetup();
        // BindUICallbacks();
    }

    public static void ReleaseNotes()
    {
        ReleaseNotesModal.OpenAtStartup(_version);
    }

    public static void MainMenuSetup()
    {
        var menu = UI.System.Q("TableMenu").Q<ShunMenuBar>();
        menu.variant = MenuBarVariant.Outline;
        menu.RegisterCallback<MouseEnterEvent>((evt) =>
        {
            menu.style.opacity = 1;
        });
        menu.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            menu.style.opacity = .25f;
        });

        menu.Query<ShunMenuBarMenu>().ForEach((item) =>
        {
            item.RemoveFromHierarchy();
        });

        var addMenu = menu.AddMenu("Add");
        addMenu.AddItem("Actor", AddActorModal.Open);
        addMenu.AddItem("Tag", SystemTagModal.Open);

        var sessionMenu = menu.AddMenu("Session");
        sessionMenu.AddItem("Save", SessionManager.Save);
        sessionMenu.AddItem("Load", SessionManager.Load);
        sessionMenu.AddItem("Exit", TabletopState.ConfirmReturnToLauncher);

        var mapMenu = menu.AddMenu("Map");
        mapMenu.AddItem("Edit");
        mapMenu.AddItem("Save");
        mapMenu.AddItem("Load");

        var viewMenu = menu.AddMenu("Config");
        viewMenu.AddItem("Dice Roller");
        viewMenu.AddItem("Tile Coords");
        viewMenu.AddItem("Top Down Camera");
        viewMenu.AddItem("True Iso Camera");
        viewMenu.AddItem("Preferences", ConfigModal.Open);
    }

    private static void BindUICallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>((evt) => StateManager.PushState(new MapEditingState()));
        UI.TopBar.Q("AddActor").RegisterCallback<ClickEvent>((evt) => AddActorModal.Open());
        UI.System.Q("AddTableTag").RegisterCallback<ClickEvent>((evt) => SystemTagModal.Open());
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
        // UI.System.Q("SessionWrapper").RegisterCallback<MouseEnterEvent>((evt) =>
        // {
        //     UI.ToggleDisplay(UI.TopBar.Q("SaveSession"), true);
        //     UI.ToggleDisplay(UI.TopBar.Q("LoadSession"), true);
        // });
        // UI.System.Q("SessionWrapper").RegisterCallback<MouseLeaveEvent>((evt) =>
        // {
        //     UI.ToggleDisplay(UI.TopBar.Q("SaveSession"), false);
        //     UI.ToggleDisplay(UI.TopBar.Q("LoadSession"), false);
        // });
        // UI.TopBar.Q("SaveSession").RegisterCallback<ClickEvent>((evt) => SessionManager.Save());
        // UI.TopBar.Q("LoadSession").RegisterCallback<ClickEvent>((evt) => SessionManager.Load());
        // UI.TopBar.Q("Isocon").RegisterCallback<ClickEvent>(TabletopState.ConfirmReturnToLauncher);
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
