using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System.Linq;

public class Launcher : MonoBehaviour
{
    public struct AppAttributes {
        public string LatestVersion;
    }

    private string _version = "0.6.0";
    private string _latestVersion = "0.6.0";
    private string _connectMode;
    private NetworkManager _manager;
    private bool _lastUpdateIsOnline = false;

    void Awake() {
        _manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Launcher", true);

        SetVersionText();
        SetCallbacks();

    }

    void Update()
    {
        UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
        UI.ToggleDisplay("Version", !NetworkClient.isConnected);

        CheckForDisconnect();
    }

    private async void SetVersionText() {
        await AsyncAwake();
        if (_version != _latestVersion) {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = Environment.FromHex("FF8B00");
        }
        else {
            UI.System.Q<Label>("Version").text = $"v{_version}";
        }
    }

    private void CheckForDisconnect() {
        // We just went offline, do cleanup
        if (_lastUpdateIsOnline != NetworkClient.isConnected && !NetworkClient.isConnected) {
            PlayerController.Disconnect();
        }
        _lastUpdateIsOnline = NetworkClient.isConnected;
    }

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
        _latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");
    }

    private void SetCallbacks() {

        // Exit selected
        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
            Application.Quit();            
        });

        // Solo mode selected
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            _connectMode = "solo";
            OpenConfigModal(evt);
        });

        // Host mode selected
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            _connectMode = "host";
            OpenConfigModal(evt);
        });

        // Client mode selected
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            _connectMode = "client";
            OpenConfigModal(evt);
        });      
    }

    private void OpenConfigModal(ClickEvent evt) {
        Modal.Reset("Configure Solo Mode");

        string name = PlayerPrefs.GetString("PlayerName", "New Player");
        TextField nameField = new TextField("Player Name");
        nameField.value = name;
        nameField.RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("PlayerName", evt.newValue);
        });
        Modal.AddContents(nameField);

        if (_connectMode == "solo" || _connectMode == "host") {
            string system = PlayerPrefs.GetString("System", "Generic");
            DropdownField systemField = new DropdownField("Game System");
            systemField.choices = new string[]{"Generic", "ICON 1.5", "Maleghast"}.ToList<string>();
            systemField.value = system; 
            systemField.RegisterValueChangedCallback<string>((evt) => {
                PlayerPrefs.SetString("System", evt.newValue);
            });            
            Modal.AddContents(systemField);
        }

        if (_connectMode == "host") {
            int maxPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
            IntegerField playerCount = new IntegerField("Player Count");
            playerCount.value = maxPlayers;
            playerCount.RegisterValueChangedCallback<int>((evt) => {
                PlayerPrefs.SetInt("PlayerCount", evt.newValue);
            });
            Modal.AddContents(playerCount);
        }

        if (_connectMode == "client") {
            string hostIP = PlayerPrefs.GetString("HostIP", "");
            TextField hostIPField = new TextField("Host IP");
            hostIPField.value = hostIP;
            hostIPField.RegisterValueChangedCallback<string>((evt) => {
                PlayerPrefs.SetString("HostIP", evt.newValue);
            });  
            Modal.AddContents(hostIPField);          
        }

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmConfig);
        confirm.AddToClassList("preferred");
        Modal.AddButton(confirm);

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);
        Modal.AddButton(cancel);
    }

    private void ConfirmConfig(ClickEvent evt) {
        switch (_connectMode) {
            case "solo":
                GameSystem.Set(PlayerPrefs.GetString("System", "Generic"));
                _manager.maxConnections = 1;
                _manager.StartHost();
                GetComponent<Tabletop>().ConnectAsSolo();
                break;
            case "host":
                GameSystem.Set(PlayerPrefs.GetString("System", "Generic"));
                _manager.maxConnections = PlayerPrefs.GetInt("PlayerCount", 4);
                _manager.StartHost();
                GetComponent<Tabletop>().ConnectAsHost();
                break;
            case "client":
                _manager.networkAddress = PlayerPrefs.GetString("HostIP", "");
                _manager.StartClient();
                GetComponent<Tabletop>().ConnectAsClient();
                break;
        }
        Modal.Close();
    }

    private void CloseModal(ClickEvent evt) {
        Modal.Close();
    }
}
