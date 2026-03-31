// using System.Collections.Generic;
// using System.Drawing;
// using System.Linq;
// using Mirror;
// using ShunUI;
// using UnityEngine;
// using UnityEngine.UIElements;

// public class NeutralState : TabletopSubstate
// {
//     public static void Start()
//     {
//         StateManager.Find().ChangeSubState(new NeutralState());
//     }

//     public override void OnEnter(StateManager sm)
//     {
//         base.OnEnter(sm);
//     }

//     protected override void EnableInterface()
//     {
//         base.EnableInterface();
//         UI.ToggleDisplay(UI.System.Q("BottomBar"), true);
//         UI.ToggleDisplay(UI.System.Q("BottomRight"), true);
//     }

//     public override void UpdateState()
//     {
//         base.UpdateState();
//         ShowTokenPanels();
//         SelectionMenu.Update();
//         TileShare.Offsets();
//         Pointer.Point();
//         Autosaver.Tick();
//     }

//     protected override void HandleKeypresses()
//     {
//         base.HandleKeypresses();
//         if (DisallowShortcutKeys())
//         {
//             return;
//         }

//         if (Input.GetKeyUp(KeyCode.A))
//         {
//             AddActorModal.Open();
//             return;
//         }

//         if (Input.GetKeyUp(KeyCode.M))
//         {
//             MapEditingState.Start();
//             return;
//         }

//         // if (Input.GetKeyUp(KeyCode.T))
//         // {
//         //     GoToMarking(new ClickEvent());
//         // }

//         if (Input.GetKeyUp(KeyCode.F))
//         {
//             ConfigModal.Open();
//         }

//         if (Input.GetKeyUp(KeyCode.A))
//         {
//             AddActorModal.Open();
//         }

//         if (Input.GetKeyUp(KeyCode.X))
//         {
//             ShowConsole(new ClickEvent());
//         }

//         // if (Input.GetKeyUp(KeyCode.S))
//         // {
//         //     GoToSession(new ClickEvent());
//         // }

//         if (Input.GetKeyUp(KeyCode.V))
//         {
//             Viewport.FixView();
//         }
//     }

//     private void ShowTokenPanels()
//     {
//         Actor selected = Actor.GetSelected();
//         Actor focused = Actor.GetFocused();

//         if (focused && selected)
//         {
//             UI.ToggleActiveClass("LeftTokenPanel", true);
//             UI.ToggleActiveClass("RightTokenPanel", true);
//             if (Actor.RebuildPanels)
//             {
//                 selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
//                 focused.Data.GetActorType().InitPanel(focused.Data, "RightTokenPanel");
//                 Actor.RebuildPanels = false;
//             }
//             selected.Data.UpdateActorPanel("LeftTokenPanel");
//             focused.Data.UpdateActorPanel("RightTokenPanel");
//         }
//         else if (focused && !selected)
//         {
//             UI.ToggleActiveClass("LeftTokenPanel", true);
//             UI.ToggleActiveClass("RightTokenPanel", false);
//             if (Actor.RebuildPanels)
//             {
//                 focused.Data.GetActorType().InitPanel(focused.Data, "LeftTokenPanel");
//                 Actor.RebuildPanels = false;
//             }
//             focused.Data.UpdateActorPanel("LeftTokenPanel");
//         }
//         else if (selected && !focused)
//         {
//             UI.ToggleActiveClass("LeftTokenPanel", true);
//             UI.ToggleActiveClass("RightTokenPanel", false);
//             if (Actor.RebuildPanels)
//             {
//                 selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
//                 Actor.RebuildPanels = false;
//             }
//             selected.Data.UpdateActorPanel("LeftTokenPanel");
//         }
//         else
//         {
//             UI.ToggleActiveClass("LeftTokenPanel", false);
//             UI.ToggleActiveClass("RightTokenPanel", false);
//         }
//     }

//     #region Callbacks
//     protected override void BindCallbacks()
//     {
//         // UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
//         // UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToMarking);
//         // UI.TopBar.Q("AddActor").RegisterCallback<ClickEvent>(GoToAddToken);
//         // UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(GoToConfig);
//         // UI.TopBar.Q("Session").RegisterCallback<ClickEvent>(GoToSession);
//         // UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>(FixView);
//         // UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
//         // UI.System.Q("TopBarToggle").RegisterCallback<ClickEvent>(ToggleTopBar);
//         // UI.System.Q("DeployToggle").RegisterCallback<ClickEvent>(ToggleBottomBar);
//         // UI.System.Q("AddSystemTag").RegisterCallback<ClickEvent>(ShowSystemTagModal);
//         Dragger.LeftClickRelease += LeftClickRelease;
//         Dragger.RightClickRelease += RightClickRelease;
//         Dragger.LeftDragStart += LeftDragStart;
//         Dragger.LeftDragRelease += LeftDragRelease;

//     }

//     protected override void UnbindCallbacks()
//     {
//         //     UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
//         //     UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToMarking);
//         //     UI.TopBar.Q("AddActor").UnregisterCallback<ClickEvent>(GoToAddToken);
//         //     UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(GoToConfig);
//         //     UI.TopBar.Q("Session").UnregisterCallback<ClickEvent>(GoToSession);
//         //     UI.TopBar.Q("Dice").UnregisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
//         //     UI.System.Q("TopBarToggle").UnregisterCallback<ClickEvent>(ToggleTopBar);
//         //     UI.System.Q("DeployToggle").UnregisterCallback<ClickEvent>(ToggleBottomBar);
//         //     UI.System.Q("AddSystemTag").UnregisterCallback<ClickEvent>(ShowSystemTagModal);
//         Dragger.LeftClickRelease -= LeftClickRelease;
//         Dragger.RightClickRelease -= RightClickRelease;
//         Dragger.LeftDragStart -= LeftDragStart;
//         Dragger.LeftDragRelease -= LeftDragRelease;
//     }

//     private void LeftClickRelease()
//     {
//         Pointer.PickActor()?.ToggleSelect();
//     }

//     private void RightClickRelease()
//     {
//         Actor pickedActor = Pointer.PickActor(true);
//         if (pickedActor)
//         {
//             pickedActor.ToggleMenu();
//             return;
//         }
//         Block pickedBlock = Pointer.PickBlock();
//         if (pickedBlock)
//         {
//             pickedBlock.ToggleMenu();
//             return;
//         }
//     }

//     private void LeftDragStart()
//     {
//         Actor t = Pointer.PickActor();
//         t?.StartDragging();
//     }

//     private void LeftDragRelease()
//     {
//         Actor.StopDragging(Pointer.PickBlock(), Pointer.PickPoint());
//     }

//     private void ToggleTopBar(ClickEvent evt)
//     {
//         UI.ToggleActiveClass(UI.System.Q("TopBar"));
//     }

//     private void ToggleBottomBar(ClickEvent evt)
//     {
//         UI.ToggleActiveClass(UI.System.Q("BottomBar"));
//         UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), false);
//     }

//     private void ShowConsole(ClickEvent evt)
//     {
//         ConsoleModal.OpenModal(evt);
//     }



//     #endregion
// }