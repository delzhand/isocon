using System;
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
        UI.SetBlocking(UI.System, StringUtility.CreateArray("SelectedTokenPanel", "FocusedTokenPanel"));

        Modal.Setup();
        MapEdit.Setup();
        SelectionMenu.Setup();
        DiceRoller.Setup();
        TopBarSetup();
        BottomBarSetup();
        TurnIndicatorSetup();
        // TerrainEffectSetup();

        UI.System.Q("TurnAdvance").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("turn advance");
        });
        UI.System.Q("TopRight").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("terrain info");
        });
        // UI.System.Q("ClearSelected").RegisterCallback<MouseEnterEvent>((evt) => {
        //     Tutorial.Init("clear selected");
        // });

    }

    void Update()
    {
        ImageSyncStatus();

        UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
        TileShare.Offsets();

        if (GameSystem.Current() != null)
        {
            Token selected = Token.GetSelected();
            GameSystem.Current().UpdateTokenPanel(selected != null ? selected.Data.Id : null, "SelectedTokenPanel");

            Token focused = Token.GetFocused();
            GameSystem.Current().UpdateTokenPanel(focused != null ? focused.Data.Id : null, "FocusedTokenPanel");

            UI.ToggleDisplay(UI.System.Q("SelectionMenu"), SelectionMenu.Visible);
            if (SelectionMenu.Visible)
            {
                if (SelectionMenu.FollowTransform != null)
                {
                    UI.FollowTransform(SelectionMenu.FollowTransform, UI.System.Q("SelectionMenu"), Camera.main, SelectionMenu.Offset);
                    UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, Length.Percent(-100)));
                }
                else
                {
                    UI.System.Q("SelectionMenu").style.top = 10;
                    UI.System.Q("SelectionMenu").style.left = 10;
                    UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, 0));
                }
            }

            // if (selected != null) {
            //     if (selected.Data.Placed) {
            //         UI.FollowToken(selected, UI.System.Q("SelectionMenu"), Camera.main, new Vector2(30, 50), true);
            //         UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, Length.Percent(-100)));
            //     }
            //     else {
            //         UI.System.Q("SelectionMenu").style.top = 10;
            //         UI.System.Q("SelectionMenu").style.left = 10;
            //         UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, 0));


            //         // Vector2 v = UI.System.Q("Frame").worldBound.center;
            //         // string uiScale = PlayerPrefs.GetString("UIScale", "100%");
            //         // float value = float.Parse(uiScale.Replace("%", "")) / 100f;
            //         // v *= value;
            //         // v.y = Screen.height - v.y;
            //         // UI.System.Q("SelectionMenu").style.left = v.x;
            //         // UI.System.Q("SelectionMenu").style.top = v.y;
            //         // UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(Length.Percent(0), Length.Percent(0)));
            //     }

            // }

        }

        UI.ToggleDisplay(UI.TopBar.Q("Config"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), Cursor.Mode != CursorMode.Editing);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), Cursor.Mode != CursorMode.Editing);

        // Map Meta
        UI.System.Q("InfoWindow").Q<Label>("MapTitle").text = MapMeta.Title;
        UI.System.Q("InfoWindow").Q<Label>("Description").text = MapMeta.Description;
        UI.System.Q("InfoWindow").Q<Label>("Objective").text = MapMeta.Objective;
        UI.System.Q("InfoWindow").Q<Label>("Author").text = MapMeta.CreatorName;
    }

    public void ConnectAsClient()
    {
        Tutorial.Init("tabletop");
        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = $"You are connected to {PlayerPrefs.GetString("HostIP", "an unknown address")} as a client.";
        IPFinder.ReplaceTokens(message);
    }

    public void ConnectAsHost()
    {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);

        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = "You are hosting on port <LocalIP> (local) and <GlobalIP> (global).<br><br>You must either have port forwarding for port 7777 TCP to your local IP with no active VPN, or use a 3rd party service like Ngrok or Hamachi.";
        IPFinder.ReplaceTokens(message);

        BlockMesh.ToggleBorders(false);
    }

    public void ConnectAsSolo()
    {
        Tutorial.Init("tabletop");
        TerrainController.InitializeTerrain(8, 8, 1);

        Label message = UI.System.Q<Label>("ConnectionMessage");
        message.text = "You are in solo mode. Other users cannot connect to this table.";
        IPFinder.ReplaceTokens(message);

        BlockMesh.ToggleBorders(false);
    }

    private void TopBarSetup()
    {
        UI.SetBlocking(UI.System, "TopBar");
        UI.TopBar.Q("Isocon").RegisterCallback<ClickEvent>(Tabletop.IsoconMenu);
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(MapEdit.ToggleEditMode);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(TerrainController.ToggleTerrainEffectMode);
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.HoverSetup(UI.TopBar.Q("Dice"));
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(Config.OpenModal);
        UI.HoverSetup(UI.TopBar.Q("Config"));
        UI.TopBar.Q("Info").RegisterCallback<ClickEvent>(ToggleInfo);
        UI.HoverSetup(UI.TopBar.Q("Info"));
        UI.TopBar.Q("Sync").RegisterCallback<ClickEvent>(ToggleSync);
        UI.HoverSetup(UI.TopBar.Q("Sync"));
    }

    private static bool showInfo = false;
    public static void ToggleInfo(ClickEvent evt)
    {
        showInfo = !showInfo;
        UI.ToggleDisplay("InfoWindow", showInfo);
        UI.ToggleActiveClass(UI.TopBar.Q("Info"), showInfo);

    }

    private static bool showSync = false;
    public static void ToggleSync(ClickEvent evt)
    {
        showSync = !showSync;
        UI.ToggleDisplay("SyncPanel", showSync);
        UI.ToggleActiveClass(UI.TopBar.Q("Sync"), showSync);
    }

    private void BottomBarSetup()
    {
        UI.SetBlocking(UI.System, "BottomBar");
        UI.System.Q("BottomBar").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("token bar");
        });

        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("add token");
        });

        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>((evt) =>
        {
            AddToken.OpenModal(evt);
        });
    }

    private void TurnIndicatorSetup()
    {
        UI.System.Q<Button>("TurnAdvance").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal.DoubleConfirm("Advance Turn", GameSystem.Current().TurnAdvanceMessage(), () =>
            {
                Player.Self().CmdRequestGameDataSetValue("IncrementTurn");
            });
        });
    }

    // private void TerrainEffectSetup() {
    //     VisualElement root = UI.System.Q("TopRight").Q("Effects");

    //     UI.HoverSetup(root.Q("ClearSelected"));
    //     root.Q("ClearSelected").RegisterCallback<ClickEvent>((evt) => {
    //         Block.DeselectAll();
    //     });

    //     UI.HoverSetup(root.Q("AddEffect"));
    //     root.Q("AddEffect").RegisterCallback<ClickEvent>((evt) => {
    //         AddTerrainEffect.OpenModal(evt);
    //     });

    //     UI.HoverSetup(root.Q("RemoveEffects"));
    //     root.Q("RemoveEffects").RegisterCallback<ClickEvent>((evt) => {
    //         AddTerrainEffect.ClearAll();
    //     });
    // }

    private static void IsoconMenu(ClickEvent evt)
    {
        if (NetworkClient.activeHost)
        {
            Modal.DoubleConfirm("Exit Tabletop", "You are hosting. <b>Disconnecting from the table will end the session!</b> Exit the tabletop and return to the Isocon Launcher?", Quit);
        }
        else
        {
            Modal.DoubleConfirm("Exit Tabletop", "Exit the tabletop and return to the Isocon Launcher?", Quit);
        }
    }

    private static void Quit()
    {
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

    private void ImageSyncStatus()
    {

    }
}
