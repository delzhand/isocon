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

        // Debug
        UI.System.Q("Debug").RegisterCallback<ClickEvent>((evt) => {
            string json = "{\"Name\":\"Ada\",\"CurrentHP\":0,\"MaxHP\":100,\"GraphicHash\":\"df6ee698a739576676d5f99c113a61fecdd1f2a66cc3fd7fc1b8ac21f3ba4067\",\"Size\":1}";
            Player.Self().CmdCreateTokenData(json, new Vector3(3, .25f, 3));
            json = "{\"Name\":\"Graddes\",\"CurrentHP\":0,\"MaxHP\":100,\"GraphicHash\":\"82d39a85a409a2f54c4799049869001e216495926ae028bb531ec6cbce100b6b\",\"Size\":2}";
            Player.Self().CmdCreateTokenData(json, new Vector3(3, .25f, 3));
            json = "{\"Name\":\"Sae\",\"CurrentHP\":0,\"MaxHP\":100,\"GraphicHash\":\"7451fc67cb845c64f81d0918baeaf5829d7821790cf98b388b364d18a893e2fe\",\"Size\":1}";
            Player.Self().CmdCreateTokenData(json, new Vector3(3, .25f, 3));
        
            Toast.Add("Debug function 1 executed");
        });

        UI.System.Q("Debug2").RegisterCallback<ClickEvent>((evt) => {
            AuraManager am = GameObject.FindGameObjectWithTag("Token").AddComponent<AuraManager>();
            am.AddAura("Rampart", 2);
            Toast.Add("Debug function 2 executed");
        });
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
            UI.System.Q<Label>("Version").style.backgroundColor = Environment.FromHex("9C7A19");
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
    }

    private void OpenConfigModal(ClickEvent evt) {
        Modal.Reset("Configure Solo Mode");

        string name = PlayerPrefs.GetString("PlayerName", "New Player");
        Modal.AddTextField("PlayerName", "Player Name", name, (evt) => {
            PlayerPrefs.SetString("PlayerName", evt.newValue);
        });

        if (_connectMode == "solo" || _connectMode == "host") {
            string system = PlayerPrefs.GetString("System", "Generic");
            Modal.AddDropdownField("GameSystem", "Game System", system, new string[]{"Generic", "ICON 1.5", "Maleghast"}, (evt) => {
                PlayerPrefs.SetString("System", evt.newValue);
            });

            string gridType = PlayerPrefs.GetString("Grid", "Square");
            Modal.AddDropdownField("GridType", "Grid Type", gridType, new string[]{"Square"}, (evt) => {
                PlayerPrefs.SetString("Grid", evt.newValue);
            });
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
        string gridType = PlayerPrefs.GetString("Grid", "Square");
        TerrainController.GridType = gridType;

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
