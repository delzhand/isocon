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
        // ConnectionSetup();
        // FloatingControlsSetup();
        // SystemMenuSetup();

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
        // UI.ToggleDisplay(UI.TopBar.Q("Info"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Rotate"), CameraControl.Overhead == false);
        UI.ToggleDisplay(UI.TopBar.Q("Tilt"), CameraControl.Overhead == false);
    }

    public void ConnectAsClient() {
        Tutorial.Init("tabletop");
        UI.System.Q("FloatingControls").Q("Connection").Q<Label>("Message").text = "You are connected as a client.";
        UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), true);
    }

    public void ConnectAsHost() {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);
        // Label message = UI.System.Q("FloatingControls").Q("Connection").Q<Label>("Message");
        // message.text = "You are hosting. You need to have port forwarding for port 7777 TCP to your local IP. Other players will connect to your public IP, which appears to be <IP>. If you're unable to use port forwarding you can use a service like Ngrok or Hamachi.";
        // IPFinder.GetPublic(message);

        // UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), true);
        BlockMesh.ToggleBorders(false);
    }

    public void ConnectAsSolo() {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);
        // UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection"), false);
        BlockMesh.ToggleBorders(false);
    }

    private void ConnectionSetup() {
        // UI.System.Q("FloatingControls").Q("Connection").RegisterCallback<MouseEnterEvent>((evt) =>  {
        //     UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection").Q("Panel"), true);
        // });
        
        // UI.System.Q("FloatingControls").Q("Connection").RegisterCallback<MouseLeaveEvent>((evt) =>  {
        //     UI.ToggleDisplay(UI.System.Q("FloatingControls").Q("Connection").Q("Panel"), false);
        // });
    }

    // private void SystemMenuSetup() {
    //     VisualElement root = UI.System.Q("SystemMenu");

    //     // root.Q()

    //     UI.HoverSetup(root.Q("EditMap"));
    //     UI.HoverSetup(root.Q("Config"));
    //     UI.HoverSetup(root.Q("Dice"));

    //     root.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
    //     root.Q("EditMap").RegisterCallback<ClickEvent>(MapEdit.ToggleEditMode);
    //     root.Q("Config").RegisterCallback<ClickEvent>(Config.OpenModal);

    // }

    // private void FloatingControlsSetup() {
    //     VisualElement root = UI.System.Q("FloatingControls");
    //     VisualElement locator = UI.System.Q("FControlLocator");

    //     root.style.opacity = 0f;
    //     locator.style.opacity = 1f;
    //     root.RegisterCallback<MouseEnterEvent>((evt) => {
    //         root.style.opacity = 1f;
    //         locator.style.opacity = 0f;
    //     });
    //     root.RegisterCallback<MouseLeaveEvent>((evt) => {
    //         root.style.opacity = 0f;
    //         locator.style.opacity = 1f;
    //     });
    //     UI.SetBlocking(UI.System, "FloatingControls");
    //     UI.SetBlocking(UI.System, "SelectedTokenPanel");
    //     UI.SetBlocking(UI.System, "FocusedTokenPanel");
    //     UI.HoverSetup(root.Q("RotateCCW"));
    //     UI.HoverSetup(root.Q("RotateCW"));
    //     // UI.HoverSetup(root.Q("Connection"));
    //     UI.HoverSetup(root.Q("FixedView"));
    //     UI.HoverSetup(root.Q("Indicators"));

    //     UI.System.Q("FloatingControls").RegisterCallback<MouseEnterEvent>((evt) => {
    //         Tutorial.Init("edit mode");
    //     });

    //     root.Q("Indicators").RegisterCallback<ClickEvent>((evt) => {
    //         bool val = !TerrainController.Indicators;
    //         TerrainController.Indicators = val;
    //         if (val) {
    //             root.Q("Indicators").AddToClassList("active");
    //         }
    //         else {
    //             root.Q("Indicators").RemoveFromClassList("active");
    //         }
    //     });
    // }

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
