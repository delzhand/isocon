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
        TokenMenu.Setup();
        ConnectionSetup();
        BottomBarSetup();
        FloatingControlsSetup();
    }

    void Update()
    {
        UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
        TileShare.Offsets();
    }

    public void ConnectAsClient() {
        UI.System.Q("FloatingControls").Q("Connection").Q<Label>("Message").text = "You are connected as a client.";
        UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), true);
    }

    public void ConnectAsHost() {
        TerrainController.InitializeTerrain(8, 8, 1);
        Label message = UI.System.Q("FloatingControls").Q("Connection").Q<Label>("Message");
        message.text = "You are hosting. You need to have port forwarding for port 7777 TCP to your local IP. Other players will connect to your public IP, which appears to be <IP>. If you're unable to use port forwarding you can use a service like Ngrok or Hamachi.";
        IPFinder.GetPublic(message);

        UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), true);
    }

    public void ConnectAsSolo() {
        TerrainController.InitializeTerrain(8, 8, 1);
        UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), false);
    }

    private void ConnectionSetup() {
        UI.System.Q("FloatingControls").Q("Connection").RegisterCallback<MouseEnterEvent>((evt) =>  {
            UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection").Q("Panel"), true);
        });
        
        UI.System.Q("FloatingControls").Q("Connection").RegisterCallback<MouseLeaveEvent>((evt) =>  {
            UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection").Q("Panel"), false);
        });
    }

    private void FloatingControlsSetup() {
        UI.SetBlocking(UI.System, "FloatingControls");
    }

    private void BottomBarSetup() {
        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>((evt) => {
            AddToken.OpenModal(evt);
        });
    }

}
