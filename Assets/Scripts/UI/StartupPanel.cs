using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class StartupPanel : MonoBehaviour
{

    NetworkManager manager;

    void Awake()
    {
        UI.ToggleDisplay("StartupPanel", true);

        manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

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

        string system = PlayerPrefs.GetString("System", "Generic");
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
