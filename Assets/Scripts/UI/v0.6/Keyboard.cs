using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Keyboard : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Modal.Close();
        }

        if (Input.GetKeyUp(KeyCode.Return) && Modal.IsOpen()) {
            Modal.Activate();
        }

        if (Input.GetKeyUp(KeyCode.C)) {
            CameraControl.TogglePanMode();
        }

        if (Input.GetKeyUp(KeyCode.D) && Cursor.Mode == CursorMode.Default && !Modal.IsOpen()) {
            DiceRoller.ToggleVisible(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.M) && !Modal.IsOpen()) {
            MapEdit.ToggleEditMode(new ClickEvent());
        }
        
        if (Input.GetKeyUp(KeyCode.T) && !Modal.IsOpen()) {
            TerrainController.ToggleTerrainEffectMode(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.I) && Cursor.Mode == CursorMode.Default && !Modal.IsOpen()) {
            Tabletop.ToggleInfo(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.F) && Cursor.Mode == CursorMode.Default && !Modal.IsOpen()) {
            Config.OpenModal(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.A) && Cursor.Mode == CursorMode.Default && !Modal.IsOpen()) {
            AddToken.OpenModal(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.X) && Cursor.Mode == CursorMode.Default && !Modal.IsOpen()) {
            IsoConsole.OpenModal(new ClickEvent());
        }

    }
}
