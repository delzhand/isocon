using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Keyboard : MonoBehaviour
{
    void Update()
    {
        if (Modal.IsOpen())
        {
            if (Input.GetKeyUp(KeyCode.Escape) && Modal.IsOpen())
            {
                Modal.Close();
            }

            if (Input.GetKeyUp(KeyCode.Return) && Modal.IsOpen())
            {
                Modal.Activate();
            }

            return;
        }


        if (Input.GetKeyUp(KeyCode.M))
        {
            MapEdit.ToggleEditMode(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            TerrainController.ToggleTerrainEffectMode(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            CameraControl.TogglePanMode();
        }

        if (Input.GetKeyUp(KeyCode.D) && Cursor.Mode == CursorMode.Default)
        {
            DiceRoller.ToggleVisible(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.I) && Cursor.Mode == CursorMode.Default)
        {
            Tabletop.ToggleInfo(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.S) && Cursor.Mode == CursorMode.Default)
        {
            Tabletop.ToggleSync(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.F) && Cursor.Mode == CursorMode.Default)
        {
            Config.OpenModal(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.A) && Cursor.Mode == CursorMode.Default)
        {
            AddToken.OpenModal(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.X) && Cursor.Mode == CursorMode.Default)
        {
            IsoConsole.OpenModal(new ClickEvent());
        }

    }
}
