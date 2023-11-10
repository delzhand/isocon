using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OfflineIndicator : MonoBehaviour
{
    // private bool lastUpdateIsOnline = false;

    // void Update()
    // {
    //     UI.ToggleDisplay("OfflineIndicator", !NetworkClient.isConnected);
    //     UI.ToggleDisplay("StartupPanel", !NetworkClient.isConnected);
    //     UI.ToggleDisplay("Frame", NetworkClient.isConnected);
    //     UI.ToggleDisplay("BottomBar", NetworkClient.isConnected);
        
    //     // We just went offline, do cleanup
    //     if (lastUpdateIsOnline != NetworkClient.isConnected && !NetworkClient.isConnected) {
    //         PlayerController.Disconnect();
    //     }
    //     lastUpdateIsOnline = NetworkClient.isConnected;
    // }
}
