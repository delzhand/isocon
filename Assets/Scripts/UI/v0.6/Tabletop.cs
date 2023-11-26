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
        DiceRoller.Setup();
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
        VisualElement root = UI.System.Q("FloatingControls");
        UI.SetBlocking(UI.System, "FloatingControls");
        UI.HoverSetup(root.Q("EditMap"));
        UI.HoverSetup(root.Q("Config"));
        UI.HoverSetup(root.Q("RotateCCW"));
        UI.HoverSetup(root.Q("RotateCW"));
        UI.HoverSetup(root.Q("Connection"));
        UI.HoverSetup(root.Q("FixedView"));
        UI.HoverSetup(root.Q("Indicators"));
        UI.HoverSetup(root.Q("Dice"));

        root.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        root.Q("EditMap").RegisterCallback<ClickEvent>(MapEdit.ToggleEditMode);
        root.Q("Config").RegisterCallback<ClickEvent>(Config.OpenModal);
        root.Q("Indicators").RegisterCallback<ClickEvent>((evt) => {
            bool val = !TerrainController.Indicators;
            TerrainController.Indicators = val;
            if (val) {
                root.Q("Indicators").AddToClassList("active");
            }
            else {
                root.Q("Indicators").RemoveFromClassList("active");
            }
        });
    }

    private void BottomBarSetup() {
        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>((evt) => {
            AddToken.OpenModal(evt);
        });
    }

}
