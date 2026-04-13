using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Setup()
    {
        var menu = UI.System.Q("TableMenu").Q<ShunMenuBar>();
        menu.variant = MenuBarVariant.Outline;

        // Clear defaults
        menu.Query<ShunMenuBarMenu>().ForEach((item) =>
        {
            item.RemoveFromHierarchy();
        });

        // Add real items
        var addMenu = menu.AddMenu("Add");
        addMenu.AddItem("Actor", AddActorModal.Open);
        addMenu.AddItem("Tag", SystemTagModal.Open);

        var sessionMenu = menu.AddMenu("Session");
        sessionMenu.AddItem("Save", SessionManager.Save);
        sessionMenu.AddItem("Load", SessionManager.Load);
        sessionMenu.AddItem("Exit", TabletopState.ConfirmReturnToLauncher);

        var mapMenu = menu.AddMenu("Map");
        mapMenu.AddItem("Edit", () => StateManager.PushState(new MapEditingState()));

        var viewMenu = menu.AddMenu("Config");
        viewMenu.AddItem("Dice Roller", DiceRoller.ToggleVisible);
        viewMenu.AddItem("Set View: Overhead", Viewport.FixViewOverhead);
        viewMenu.AddItem("Set View: Initial", Viewport.FixViewIso);
        viewMenu.AddItem("Preferences", ConfigModal.Open);
    }

    public static bool IsOpen
    {
        get
        {
            var menuBarOpen = false;
            UI.System.Query<ShunMenuBarMenu>().ForEach((item) =>
            {
                if (item.isOpen)
                {
                    menuBarOpen = true;
                }
            });
            return menuBarOpen;
        }
    }
}
