using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class Tabletop : MonoBehaviour
{
    void Start()
    {
        Modal.Setup();
        MapEdit.Setup();
        SelectionMenu.Setup();
        DiceRoller.Setup();
        TopBarSetup();
        BottomBarSetup();
        TurnIndicatorSetup();

        UI.System.Q("TerrainInfo").Q("AddEffectButton").RegisterCallback<ClickEvent>(AddTerrainEffect.OpenModal);

        UI.System.Q("TurnAdvance").RegisterCallback<MouseEnterEvent>((evt) => {
            Tutorial.Init("turn advance");
        });
        UI.System.Q("TopRight").RegisterCallback<MouseEnterEvent>((evt) => {
            Tutorial.Init("terrain info");
        });
        UI.System.Q("ClearSelected").RegisterCallback<MouseEnterEvent>((evt) => {
            Tutorial.Init("clear selected");
        });

    }

    void Update()
    {
        UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
        TileShare.Offsets();

        if (GameSystem.Current() != null) {
            Token selected = Token.GetSelected();
            GameSystem.Current().UpdateTokenPanel(selected != null ? selected.Data.Id : null, "SelectedTokenPanel");

            Token focused = Token.GetFocused();
            GameSystem.Current().UpdateTokenPanel(focused != null ? focused.Data.Id : null, "FocusedTokenPanel");

            if (selected != null) {
                if (selected.Data.Placed) {
                    UI.FollowToken(selected, UI.System.Q("SelectionMenu"), Camera.main, new Vector2(100, 0), true);
                    UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50)));
                }
                else {
                    UI.System.Q("SelectionMenu").style.top = 0;
                    UI.System.Q("SelectionMenu").style.left = 0;
                    UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, 0));
                }

            }
        }

        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), Cursor.Mode != CursorMode.Editing);

        // Update Player List
        NetworkManager netman = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        UI.System.Q("InfoWindow").Q<Label>("PlayerCount").text = $"{NetworkServer.connections.Count}/{netman.maxConnections}";
        VisualElement playerList = UI.System.Q("PlayerList");
        playerList.Clear();
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
            string name = g.GetComponent<Player>().Name;
            Label l = new(name);
            l.AddToClassList("playerlist-item");
            l.AddToClassList("no-margin");
            playerList.Add(l);
        }

        // Map Meta
        UI.System.Q("InfoWindow").Q<Label>("MapTitle").text = MapMeta.Title;
        UI.System.Q("InfoWindow").Q<Label>("Description").text = MapMeta.Description;
        UI.System.Q("InfoWindow").Q<Label>("Objective").text = MapMeta.Objective;
        UI.System.Q("InfoWindow").Q<Label>("Author").text = MapMeta.CreatorName;
    }

    public void ConnectAsClient() {
        Tutorial.Init("tabletop");
        UI.System.Q("FloatingControls").Q("Connection").Q<Label>("Message").text = "You are connected as a client.";

        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = $"You are connected to {PlayerPrefs.GetString("HostIP", "an uUnknown address")} as a client.";
        IPFinder.ReplaceTokens(message);
    }

    public void ConnectAsHost() {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);

        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = "You are hosting on port <LocalIP> (local) and <GlobalIP> (global).<br><br>You must either have port forwarding for port 7777 TCP to your local IP with no active VPN, or use a 3rd party service like Ngrok or Hamachi.";
        IPFinder.ReplaceTokens(message);

        BlockMesh.ToggleBorders(false);
    }

    public void ConnectAsSolo() {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);

        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = "You are in solo mode. Other users cannot connect to this table.";
        IPFinder.ReplaceTokens(message);

        BlockMesh.ToggleBorders(false);
    }

    private void TopBarSetup() {
        UI.SetBlocking(UI.System, "TopBar");
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(MapEdit.ToggleEditMode);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(Config.OpenModal);
        UI.TopBar.Q("Coordinates").RegisterCallback<ClickEvent>(TerrainController.ToggleIndicators);
        UI.TopBar.RegisterCallback<MouseEnterEvent>((evt) => {
            UI.TopBar.style.opacity = 1;
        });
        UI.TopBar.RegisterCallback<MouseLeaveEvent>((evt) => {
            UI.TopBar.style.opacity = 0;
        });

        UI.TopBar.Q("Isocon").RegisterCallback<ClickEvent>(Tabletop.IsoconMenu);

        UI.TopBar.Q("Info").RegisterCallback<MouseEnterEvent>((evt) => {
            UI.ToggleDisplay(UI.System.Q("InfoWindow"), true);
        });
        UI.TopBar.Q("Info").RegisterCallback<MouseLeaveEvent>((evt) => {
            UI.ToggleDisplay(UI.System.Q("InfoWindow"), false);
        });
    }
    
    private void BottomBarSetup() {
        UI.System.Q("BottomBar").RegisterCallback<MouseEnterEvent>((evt) => {
            Tutorial.Init("token bar");
        });

        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<MouseEnterEvent>((evt) => {
            Tutorial.Init("add token");
        });

        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>((evt) => {
            AddToken.OpenModal(evt);
        });
    }

    private void TurnIndicatorSetup() {
        UI.System.Q<Button>("TurnAdvance").RegisterCallback<ClickEvent>((evt) => {
            Modal.DoubleConfirm("Advance Turn", GameSystem.Current().TurnAdvanceMessage(), () => {
                Player.Self().CmdRequestGameDataSetValue("IncrementTurn");
            });
        });
         UI.System.Q("TerrainInfo").Q<Button>("ClearSelected").RegisterCallback<ClickEvent>((evt) => {
            Block.DeselectAll();
         });
    }

    private static void IsoconMenu(ClickEvent evt) {
        Modal.DoubleConfirm("Exit Tabletop", "Exit the tabletop and return to the main menu?", Quit);
    }

    private static void Quit() {
        NetworkManager manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            manager.StopHost();
            manager.StopClient();
        }
        else if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }
        PlayerController.Disconnect();
    }
}
