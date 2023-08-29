using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class OfflineIndicator : MonoBehaviour
{
    void Update()
    {
        UI.ToggleDisplay("OfflineIndicator", !NetworkClient.isConnected);
    }
}
