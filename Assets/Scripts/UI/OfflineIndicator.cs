using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OfflineIndicator : MonoBehaviour
{
    void Update()
    {
        UI.ToggleDisplay("OfflineIndicator", !NetworkClient.isConnected);
        UI.ToggleDisplay("StartupPanel", !NetworkClient.isConnected);
        UI.ToggleDisplay("Frame", NetworkClient.isConnected);
        UI.ToggleDisplay("BottomBar", NetworkClient.isConnected);
    }
}
