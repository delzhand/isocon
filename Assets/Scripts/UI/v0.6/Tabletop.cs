using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class Tabletop : MonoBehaviour
{
    void Start()
    {
        Modal.Setup();
        MapEdit.Setup();
    }



    void Update()
    {
        UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
    }

    public void ConnectAsClient() {

    }

    public void ConnectAsHost() {
        TerrainController.InitializeTerrain(8, 8, 1);
    }

}
