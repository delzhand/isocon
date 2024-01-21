using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Launcher : MonoBehaviour
{
    public struct AppAttributes {
        public string LatestVersion;
    }

    private string _version = "0.6.4";
    private string _latestVersion = "0.6.4";
    private string _connectMode;
    private NetworkManager _manager;
    private bool _lastUpdateIsOnline = false;
    private bool _lastUpdateIsConnecting = false;

    void Awake() {
        _manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

        string uiScale = PlayerPrefs.GetString("UIScale", "100%");
        float value = float.Parse(uiScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = value;


        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Launcher", true);

        SetVersionText();
        SetCallbacks();

        // // Debug
        // UI.System.Q("Debug").RegisterCallback<ClickEvent>((evt) => {
        //     IsoConsole.OpenModal(evt);
        // });
    }

    void Update()
    {
        UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
        UI.ToggleDisplay("Version", !NetworkClient.isConnected);

        CheckForDisconnect();
        CheckForFailedConnect();

        bool isIdle = !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected;
        bool isConnecting = NetworkClient.active && !NetworkClient.isConnected;

        UI.ToggleDisplay("StartupOptions", isIdle);
        UI.ToggleDisplay("ConnectingMessage", isConnecting);

        if (!NetworkClient.isConnected) {
            foreach(Transform child in GameObject.Find("Tokens").transform) {
                DestroyImmediate(child.gameObject);
            }
            UI.System.Q("Worldspace").Clear();            
            UI.System.Q("UnitBar").Clear();
        }
    }

    private async void SetVersionText() {
        await AsyncAwake();
        if (_version != _latestVersion) {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = ColorUtility.UIBlue;
        }
        else {
            UI.System.Q<Label>("Version").text = $"v{_version}";
        }
    }

    private void CheckForDisconnect() {
        // We just went offline, do cleanup
        if (_lastUpdateIsOnline != NetworkClient.isConnected && !NetworkClient.isConnected) {
            PlayerController.Disconnect();
            MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
            Material launcherBg = Resources.Load<Material>($"Materials/SpecialBg");
            List<Material> bgmats = new();
            bgmats.Add(launcherBg);
            mr.SetMaterials(bgmats);
        }
        _lastUpdateIsOnline = NetworkClient.isConnected;
    }

    private void CheckForFailedConnect() {
        bool isIdle = !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected;
        bool isConnecting = NetworkClient.active && !NetworkClient.isConnected;

        if (isIdle && _lastUpdateIsConnecting) {
            Toast.AddError("Could not establish a connection.");
        }
        _lastUpdateIsConnecting = isConnecting;
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
        GameSystem.DataJson = RemoteConfigService.Instance.appConfig.GetJson("GameSystem");
    }

    private void SetCallbacks() {

        UI.System.Q<Button>("ClearCache").RegisterCallback<ClickEvent>((evt) => {
            Modal.DoubleConfirm("Clear Cache", "Clear cached settings? This will reset player preferences like name, last system used, and data directory.", () => {
                PlayerPrefs.DeleteAll();
            });
        });

        // Exit selected
        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
            Application.Quit();            
        });

        // Solo mode selected
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "solo";
            OpenConfigModal(evt);
        });

        // Host mode selected
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "host";
            OpenConfigModal(evt);
        });

        // Client mode selected
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "client";
            OpenConfigModal(evt);
        });      

        UI.System.Q<Button>("CancelConnecting").RegisterCallback<ClickEvent>((evt) => {
            GameObject.Find("NetworkController").GetComponent<NetworkManager>().StopClient();
            Toast.AddSimple("Connection attempt cancelled.");
        });
    }

    private void OpenConfigModal(ClickEvent evt) {
        Modal.Reset("Configure Solo Mode");

        string name = PlayerPrefs.GetString("PlayerName", "New Player");
        Modal.AddTextField("PlayerName", "Player Name", name, (evt) => {
            PlayerPrefs.SetString("PlayerName", evt.newValue);
        });

        if (_connectMode == "solo" || _connectMode == "host") {
            string system = PlayerPrefs.GetString("System", "Generic");
            Modal.AddDropdownField("GameSystem", "Game System", system, new string[]{"Generic", "ICON 1.5", "Maleghast"/*, "Lancer"*/}, (evt) => {
                PlayerPrefs.SetString("System", evt.newValue);
                ConfigModalEvaluateConditions();
            });

            string gridType = PlayerPrefs.GetString("Grid", "Square");
            Modal.AddDropdownField("GridType", "Grid Type", gridType, new string[]{"Square", "Hex"}, (evt) => {
                PlayerPrefs.SetString("Grid", evt.newValue);
            });
            Modal.AddMarkup("HexMessage", "Warning! Hex support is experimental. Some visual effects may not display correctly.");
        }

        if (_connectMode == "host") {
            int maxPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
            Modal.AddIntField("PlayerCount", "Max Player Count", maxPlayers, (evt) => {
                PlayerPrefs.SetInt("PlayerCount", evt.newValue);
            });
        }

        if (_connectMode == "client") {
            string hostIP = PlayerPrefs.GetString("HostIP", "");
            Modal.AddTextField("HostIP", "Host IP", hostIP, (evt) => {
                PlayerPrefs.SetString("HostIP", evt.newValue);
            });
        }

        Modal.AddPreferredButton("Confirm", ConfirmConfig);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        
        ConfigModalEvaluateConditions();
    }

    private void ConfirmConfig(ClickEvent evt) {
        TerrainController.GridType = DefaultGridType();

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

    private void ConfigModalEvaluateConditions() {
        if (UI.Modal.Q("GameSystem") != null) {
            bool grid = StringUtility.InList(UI.Modal.Q<DropdownField>("GameSystem").value, "Generic", "Lancer");
            UI.ToggleDisplay(UI.Modal.Q("GridType"), grid);
            UI.ToggleDisplay("HexMessage", grid);
        }
    }

    private string DefaultGridType() {
        switch (PlayerPrefs.GetString("System", "Generic")) {
            case "ICON 1.5":
            case "Maleghast":
                return "Square";
            case "Lancer":
                return "Hex";
            default:
                return PlayerPrefs.GetString("Grid", "Square");
        }
    }
}
