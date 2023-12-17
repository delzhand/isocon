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
using System.Net.Http;
using System;

public class StartupPanel : MonoBehaviour
{

    // private string connectMode;
    
    // // [Serializable]
    // // private class IpifyResponse
    // // {
    // //     public string ip;
    // // }

    // // private static readonly string IpifyApiUrl = "https://api.ipify.org/?format=json";

    // public struct AppAttributes {
    //     public string LatestVersion;
    // }
    // string version = "0.6.0";
    // string latestVersion = "0.6.0";
    // // string latestMessage = "Could not retrieve startup messages.";

    // NetworkManager manager;

    // async Task InitializeRemoteConfigAsync() {
    //     await UnityServices.InitializeAsync();
    //     if (!AuthenticationService.Instance.IsSignedIn)
    //     {
    //         await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //     }
    // }

    // async Task AsyncAwake () {
    //     if (Utilities.CheckForInternetConnection()) 
    //     {
    //         await InitializeRemoteConfigAsync();
    //     }    
    //     RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
    //     await RemoteConfigService.Instance.FetchConfigsAsync(new AppAttributes(), new AppAttributes());

    //     // try
    //     // {
    //     //     using (HttpClient client = new HttpClient())
    //     //     {
    //     //         HttpResponseMessage response = await client.GetAsync(IpifyApiUrl);

    //     //         if (response.IsSuccessStatusCode)
    //     //         {
    //     //             string json = await response.Content.ReadAsStringAsync();
    //     //             Debug.Log(json);

    //     //             // Parse the JSON response to get the IP address
    //     //             IpifyResponse ipifyResponse = JsonUtility.FromJson<IpifyResponse>(json);
    //     //             Debug.Log(ipifyResponse);
    //     //             string ipAddress = ipifyResponse.ip;
    //     //             Debug.Log(ipAddress);
    //     //         }
    //     //         else
    //     //         {
    //     //             // Handle the error or log it
    //     //             Debug.Log($"Error: {response.StatusCode}");
    //     //         }
    //     //     }
    //     // }
    //     // catch (Exception ex)
    //     // {
    //     //     // Handle exceptions
    //     //     Debug.Log($"Exception: {ex.Message}");
    //     // }
    // }

    // void ApplyRemoteConfig (ConfigResponse configResponse) {
    //     switch (configResponse.requestOrigin) {
    //         case ConfigOrigin.Default:
    //             Debug.Log ("No settings loaded this session and no local cache file exists; using default values.");
    //             break;
    //         case ConfigOrigin.Cached:
    //             Debug.Log ("No settings loaded this session; using cached values from a previous session.");
    //             break;
    //         case ConfigOrigin.Remote:
    //             Debug.Log ("New settings loaded this session; update values accordingly.");
    //             break;
    //     }
    //     latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");

    //     // latestMessage = RemoteConfigService.Instance.appConfig.GetString("LatestMessage");
    //     // UI.System.Q<Label>("BetaMessage").text = $"{latestMessage}";
    // }


    

    // async void Awake()
    // {

    //     await AsyncAwake();
    //     if (version != latestVersion) {
    //         UI.System.Q<Label>("Version").text = $"v{version} (version {latestVersion} available)";
    //         UI.System.Q<Label>("Version").style.backgroundColor = Environment.FromHex("FF8B00");
    //     }
    //     else {
    //         UI.System.Q<Label>("Version").text = $"v{version}";
    //     }


    //     UI.ToggleDisplay("StartupPanel", true);
    //     UI.ToggleDisplay("ConnectionConfig", false);

    //     manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

    //     UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
    //         Application.Quit();            
    //     });

    //     UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
    //         UI.ToggleDisplay("StartupOptions", false);
    //         UI.ToggleDisplay("ConnectionConfig", true);
    //         UI.ToggleDisplay("SystemSelect", true);
    //         UI.ToggleDisplay("HostIP", false);
    //         UI.ToggleDisplay("PlayerCount", false);
    //         connectMode = "solo";

    //         // manager.maxConnections = 1;
    //         // manager.StartHost();
    //         // MapSidebar.GMStart();
    //     });

    //     UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
    //         UI.ToggleDisplay("StartupOptions", false);
    //         UI.ToggleDisplay("ConnectionConfig", true);
    //         UI.ToggleDisplay("SystemSelect", true);
    //         UI.ToggleDisplay("HostIP", false);
    //         UI.ToggleDisplay("PlayerCount", true);
    //         connectMode = "host";

    //         // manager.maxConnections = 8;
    //         // manager.StartHost();
    //         // MapSidebar.GMStart();
    //     });

    //     UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
    //         UI.ToggleDisplay("StartupOptions", false);
    //         UI.ToggleDisplay("ConnectionConfig", true);
    //         UI.ToggleDisplay("SystemSelect", false);
    //         UI.ToggleDisplay("HostIP", true);
    //         UI.ToggleDisplay("PlayerCount", false);
    //         connectMode = "client";

    //         // Toast.Add("Connecting to " + manager.networkAddress);
    //         // manager.StartClient();
    //         // MapSidebar.ClientStart();
    //     });

    //     string system = PlayerPrefs.GetString("System", "Generic");
    //     // GameSystem.Set(system);
    //     UI.System.Q<DropdownField>("SystemSelect").value = system; 
    //     UI.System.Q<DropdownField>("SystemSelect").RegisterValueChangedCallback<string>((evt) => {
    //         PlayerPrefs.SetString("System", evt.newValue);
    //     });

    //     int maxPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
    //     UI.System.Q<IntegerField>("PlayerCount").value = maxPlayers;
    //     UI.System.Q<IntegerField>("PlayerCount").RegisterValueChangedCallback<int>((evt) => {
    //         PlayerPrefs.SetInt("PlayerCount", evt.newValue);
    //     });

    //     string hostIP = PlayerPrefs.GetString("HostIP", "");
    //     UI.System.Q<TextField>("HostIP").value = hostIP;
    //     UI.System.Q<TextField>("HostIP").RegisterValueChangedCallback<string>((evt) => {
    //         PlayerPrefs.SetString("HostIP", evt.newValue);
    //     });

    //     string name = PlayerPrefs.GetString("PlayerName", "New Player");
    //     UI.System.Q<TextField>("PlayerName").value = name;
    //     UI.System.Q<TextField>("PlayerName").RegisterValueChangedCallback<string>((evt) => {
    //         PlayerPrefs.SetString("PlayerName", evt.newValue);
    //     });

    //     UI.System.Q("ConnectionConfig").Q("Cancel").RegisterCallback<ClickEvent>((evt) => {
    //         UI.ToggleDisplay("StartupOptions", true);
    //         UI.ToggleDisplay("ConnectionConfig", false);
    //     });

    //     UI.System.Q("ConnectionConfig").Q("Confirm").RegisterCallback<ClickEvent>((evt) => {
    //         UI.ToggleDisplay("StartupOptions", false);
    //         UI.ToggleDisplay("ConnectionConfig", false);
    //         UI.ToggleDisplay("Version", false);
    //         if (connectMode == "solo") {
    //             GameSystem.Set(UI.System.Q<DropdownField>("SystemSelect").value);
    //             manager.maxConnections = 1;
    //             manager.StartHost();
    //             TerrainController.InitializeTerrain(8, 8, 1);
    //         }
    //         if (connectMode == "host") {
    //             GameSystem.Set(UI.System.Q<DropdownField>("SystemSelect").value);
    //             manager.maxConnections = UI.System.Q<IntegerField>("PlayerCount").value;
    //             manager.StartHost();
    //             TerrainController.InitializeTerrain(8, 8, 1);
    //         }
    //         if (connectMode == "client") {
    //             manager.networkAddress = UI.System.Q<TextField>("HostIP").value;
    //             manager.StartClient();
    //         }
    //     });
    // }

    // // Start is called before the first frame update
    // void Start()
    // {
    // }

    // // public bool ServerActive;
    // // public bool ClientActive;
    // // public bool ClientConnected;
    // // // Update is called once per frame
    // // void Update()
    // // {
    // //     ServerActive = NetworkServer.active;
    // //     ClientActive = NetworkClient.active;
    // //     ClientConnected = NetworkClient.isConnected;

    // //     UI.ToggleDisplay("StartupOptions", !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected);

    // //     UI.ToggleDisplay("ConnectingMessage", NetworkClient.active && !NetworkClient.isConnected);
    // //     UI.System.Q<Label>("ConnectingMessage").text = "Connecting to " + manager.networkAddress + "...";
    // // }

    // private bool lastUpdateIsOnline = false;

    // void Update() {
    //     UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
    //     UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
        
    //     // We just went offline, do cleanup
    //     if (lastUpdateIsOnline != NetworkClient.isConnected && !NetworkClient.isConnected) {
    //         PlayerController.Disconnect();
    //     }
    //     lastUpdateIsOnline = NetworkClient.isConnected;

    // }

    // // public static void Disconnect() {
    // //     UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
    // //     UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);

    // //     UI.
    // //     UI.ToggleDisplay("StartupPanel", true);
    // //     UI.ToggleDisplay("Frame", false);
    // //     UI.ToggleDisplay("BottomBar", false);
    // // }
    // // this shouldn't be necessary since the update function is running in real time??


}
