using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Shows NetworkManager controls in a GUI at runtime.</summary>
[DisallowMultipleComponent]
[AddComponentMenu("Network/Connection HUD")]
[RequireComponent(typeof(NetworkManager))]
public class ConnectionHUD : MonoBehaviour
{
    NetworkManager manager;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();

        UI.System.Q<TextField>("JoinAddress").RegisterValueChangedCallback<string>((evt) => {
            manager.networkAddress = evt.newValue;
            UI.System.Q<Label>("ConnectingMessage").text = "Connecting to " + evt.newValue + "...";
        });
        
        UI.System.Q<Button>("HostButton").RegisterCallback<ClickEvent>((evt) => {
            manager.StartHost();
        });

        UI.System.Q<Button>("JoinButton").RegisterCallback<ClickEvent>((evt) => {
            manager.StartClient();
        });

        UI.System.Q<Button>("CancelConnecting").RegisterCallback<ClickEvent>((evt) => {
            manager.StopClient();
        });

        UI.System.Q<Button>("DisconnectButton").RegisterCallback<ClickEvent>((evt) => {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                manager.StopHost();
                manager.StopClient();
            }
            else if (NetworkClient.isConnected)
            {
                manager.StopClient();
            }
        });

        UI.System.Q<Button>("ConnectPanelClose").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleHidden("ConnectPanel");
        });

        UI.System.Q<Button>("ConnectionToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleHidden("ConnectPanel");
        });
    }

    void Update() {
        if (NetworkServer.active && NetworkClient.active)
        {
            UI.System.Q<Label>("ConnectionStatus").text = $"<b>Host</b>: running via {Transport.active}";
        }
        else if (NetworkServer.active)
        {
            UI.System.Q<Label>("ConnectionStatus").text = $"<b>Server</b>: running via {Transport.active}";
        }
        else if (NetworkClient.isConnected)
        {
            UI.System.Q<Label>("ConnectionStatus").text = $"<b>Client</b>: connected to {manager.networkAddress} via {Transport.active}";
        }
        else {
            UI.System.Q<Label>("ConnectionStatus").text = "Not connected";
        } 

        if (!NetworkClient.isConnected && !NetworkServer.active) {
            if (!NetworkClient.active) {
                UI.ToggleDisplay("JoinSection", true);
                UI.ToggleDisplay("Connection", false);
                UI.ToggleDisplay("ConnectingSection", false);
            }
            else {
                UI.ToggleDisplay("JoinSection", false);
                UI.ToggleDisplay("Connection", false);
                UI.ToggleDisplay("ConnectingSection", true);
            }
        }
        else {
            UI.ToggleDisplay("JoinSection", false);
            UI.ToggleDisplay("Connection", true);
            UI.ToggleDisplay("ConnectingSection", false);
        }  
    }
}
