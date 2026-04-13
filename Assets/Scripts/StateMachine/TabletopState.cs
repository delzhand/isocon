using System.IO;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class TabletopState : BaseState
{
    private static ConnectMode _mode;

    public static void Activate(ConnectMode mode)
    {
        _mode = mode;
        StateManager.PushState(new TabletopState());
    }

    public override void OnEnter()
    {
        if (Player.Self().Host)
        {
            TerrainController.InitializeTerrain(8, 8, 1);
        }
        else
        {
            Player.Self().CmdRequestToast(null, $"{Player.Self().Name} connected");
        }
        SetConnectionMessage();
        EnableInterface();
        BindCallbacks();

        Tutorial.Init("tabletop");
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        UnbindCallbacks();
    }

    public override void OnExit()
    {
        base.OnExit();
        DisableInterface();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        CheckForDisconnect();
        Viewport.HandleInput();
        ShowTokenPanels();
        SelectionMenu.Update();
        TileShare.Offsets();
        Pointer.Point();
        Autosaver.Tick();
    }

    #region Interface
    private void EnableInterface()
    {
        UI.ToggleDisplay("Tabletop", true);
        UI.ToggleDisplay("DetailsHud", Preferences.Current.ShowHUD);
        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay("TableMenu", true);
        UI.ToggleDisplay(UI.System.Q("TopRight"), true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), true);
    }

    private void DisableInterface()
    {
        UI.ToggleDisplay("Tabletop", false);
    }
    #endregion

    #region Setup
    private void SetConnectionMessage()
    {
        string text = "";
        switch (_mode)
        {
            case ConnectMode.Client:
                text = $"Connected: {Preferences.Current.HostIP}";
                break;
            case ConnectMode.Host:
                text = "Hosting: <LocalIP>|<GlobalIP>";
                break;
            case ConnectMode.Solo:
                text = "Solo";
                break;

        }
        HudText.SetItem("connectionInfo", text, 1, HudTextColor.Blue);
        IPFinder.ReplaceTokens(text);
    }
    #endregion

    private void CheckForDisconnect()
    {
        if (NetworkClient.isConnected)
        {
            return;
        }

        if (StateManager.Find().GetComponent<Fader>() == null)
        {
            Fader.StartFade(Color.black, .5f, () =>
            {
                StateManager.PopState();
            });
        }
    }

    #region Callbacks
    private void BindCallbacks()
    {
        Dragger.RightDragStart += Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate += Viewport.UpdateRightDrag;
        Dragger.RightDragRelease += Viewport.EndRightDrag;

        Dragger.MiddleDragStart += Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate += Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease += Viewport.EndMiddleDrag;

        Dragger.LeftClickRelease += LeftClickRelease;
        Dragger.RightClickRelease += RightClickRelease;
        Dragger.LeftDragStart += LeftDragStart;
        Dragger.LeftDragRelease += LeftDragRelease;
    }

    private void UnbindCallbacks()
    {
        Dragger.RightDragStart -= Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate -= Viewport.UpdateRightDrag;
        Dragger.RightDragRelease -= Viewport.EndRightDrag;

        Dragger.MiddleDragStart -= Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate -= Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease -= Viewport.EndMiddleDrag;

        Dragger.LeftClickRelease -= LeftClickRelease;
        Dragger.RightClickRelease -= RightClickRelease;
        Dragger.LeftDragStart -= LeftDragStart;
        Dragger.LeftDragRelease -= LeftDragRelease;
    }

    public static void ConfirmReturnToLauncher()
    {
        SessionManager.SerializeSession($"{Preferences.Current.DataPath}/sessions/autosave.json");
        Modal2.CreateContext("PrimaryDialog");
        string message = "Exit the session and return to the IsoCON launcher?";
        if (NetworkClient.activeHost && _mode == ConnectMode.Host)
        {
            message = "You are hosting. <b>Disconnecting from the session will terminate all client connections.</b> Your session has been autosaved. Exit the session and return to the IsoCON launcher?";
        }
        Modal2.AddLongMarkup(message);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.Close();
            Quit();
        });

        Modal2.Open("Exit Session");
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

    private void LeftClickRelease()
    {
        Pointer.PickActor()?.ToggleSelect();
    }

    private void RightClickRelease()
    {
        Actor pickedActor = Pointer.PickActor(true);
        if (pickedActor)
        {
            pickedActor.ToggleMenu();
            return;
        }
        Block pickedBlock = Pointer.PickBlock();
        if (pickedBlock)
        {
            pickedBlock.ToggleMenu();
            return;
        }
    }

    private void LeftDragStart()
    {
        Actor t = Pointer.PickActor();
        t?.StartDragging();
    }

    private void LeftDragRelease()
    {
        Actor.StopDragging(Pointer.PickBlock(), Pointer.PickPoint());
    }

    #endregion

    public override void HandleInput()
    {
        // if (DisallowShortcutKeys())
        // {
        //     return;
        // }

        // if (Input.GetKeyUp(KeyCode.A))
        // {
        //     AddActorModal.Open();
        //     return;
        // }

        // if (Input.GetKeyUp(KeyCode.M))
        // {
        //     StateManager.PushState(new MapEditingState());
        //     return;
        // }

        // if (Input.GetKeyUp(KeyCode.T))
        // {
        //     GoToMarking(new ClickEvent());
        // }

        // if (Input.GetKeyUp(KeyCode.F))
        // {
        //     ConfigModal.Open();
        // }

        // if (Input.GetKeyUp(KeyCode.A))
        // {
        //     AddActorModal.Open();
        // }

        // if (Input.GetKeyUp(KeyCode.X))
        // {
        //     ShowConsole(new ClickEvent());
        // }

        // if (Input.GetKeyUp(KeyCode.S))
        // {
        //     GoToSession(new ClickEvent());
        // }

        // if (Input.GetKeyUp(KeyCode.V))
        // {
        //     Viewport.FixViewOverhead();
        //     return;
        // }
    }

    private void ShowTokenPanels()
    {
        Actor selected = Actor.GetSelected();
        Actor focused = Actor.GetFocused();

        if (focused && selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", true);
            if (Actor.RebuildPanels)
            {
                selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
                focused.Data.GetActorType().InitPanel(focused.Data, "RightTokenPanel");
                Actor.RebuildPanels = false;
            }
            selected.Data.UpdateActorPanel("LeftTokenPanel");
            focused.Data.UpdateActorPanel("RightTokenPanel");
        }
        else if (focused && !selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Actor.RebuildPanels)
            {
                focused.Data.GetActorType().InitPanel(focused.Data, "LeftTokenPanel");
                Actor.RebuildPanels = false;
            }
            focused.Data.UpdateActorPanel("LeftTokenPanel");
        }
        else if (selected && !focused)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Actor.RebuildPanels)
            {
                selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
                Actor.RebuildPanels = false;
            }
            selected.Data.UpdateActorPanel("LeftTokenPanel");
        }
        else
        {
            UI.ToggleActiveClass("LeftTokenPanel", false);
            UI.ToggleActiveClass("RightTokenPanel", false);
        }
    }

}