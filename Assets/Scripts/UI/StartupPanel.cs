using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;

public class StartupPanel : MonoBehaviour
{
    public struct AppAttributes {
        public string LatestVersion;
    }
    string version = "0.5.9";
    string latestVersion = "0.5.9";
    string latestMessage = "Could not retrieve startup messages.";

    NetworkManager manager;

    async Task InitializeRemoteConfigAsync() {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    async Task AsyncAwake () {
        if (Utilities.CheckForInternetConnection()) 
        {
            await InitializeRemoteConfigAsync();
        }    
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        await RemoteConfigService.Instance.FetchConfigsAsync(new AppAttributes(), new AppAttributes());
    }

    void ApplyRemoteConfig (ConfigResponse configResponse) {
        switch (configResponse.requestOrigin) {
            case ConfigOrigin.Default:
                Debug.Log ("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log ("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log ("New settings loaded this session; update values accordingly.");
                break;
        }
        latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");
        if (version != latestVersion) {
            UI.System.Q<Label>("Version").text = $"v{version} (version {latestVersion} available)";
        }

        latestMessage = RemoteConfigService.Instance.appConfig.GetString("LatestMessage");
        UI.System.Q<Label>("BetaMessage").text = $"{latestMessage}";
    }


    

    void Awake()
    {
        AsyncAwake();
        UI.System.Q<Label>("Version").text = $"v{version}";

        UI.ToggleDisplay("StartupPanel", true);

        manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("foo");
            Application.Quit();            
        });

        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            manager.maxConnections = 1;
            manager.StartHost();
            MapSidebar.GMStart();
        });

        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
            manager.maxConnections = 8;
            manager.StartHost();
            MapSidebar.GMStart();
        });

        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
            Toast.Add("Connecting to " + manager.networkAddress);
            manager.StartClient();
            MapSidebar.ClientStart();
        });
        UI.System.Q<TextField>("HostAddress").RegisterValueChangedCallback<string>((evt) => {
            manager.networkAddress = evt.newValue;
        });

        string system = PlayerPrefs.GetString("System", "ICON 1.5");
        GameSystem.Set(system);
        UI.System.Q<DropdownField>("SystemField").value = system; 
        UI.System.Q<DropdownField>("SystemField").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("System", evt.newValue);
            GameSystem.Set(evt.newValue);
        });
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public bool ServerActive;
    public bool ClientActive;
    public bool ClientConnected;
    // Update is called once per frame
    void Update()
    {
        ServerActive = NetworkServer.active;
        ClientActive = NetworkClient.active;
        ClientConnected = NetworkClient.isConnected;

        UI.ToggleDisplay("StartupOptions", !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected);

        UI.ToggleDisplay("ConnectingMessage", NetworkClient.active && !NetworkClient.isConnected);
        UI.System.Q<Label>("ConnectingMessage").text = "Connecting to " + manager.networkAddress + "...";
    }

    public static void Disconnect() {
        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Frame", false);
        UI.ToggleDisplay("BottomBar", false);
    }


}
