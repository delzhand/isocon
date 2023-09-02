using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using System.Linq;
using IsoconUILibrary;
using System.Text;

public enum SelectedState {
    None,
    Selected,
    Moving,
    Placing,
}

public class TokenController : MonoBehaviour
{
    private static Token selected = null;
    public static SelectedState SelectedState = SelectedState.None;

    public static void TokenClick(Token token) {
        if (token == null) {
            selected = null;
            GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i].GetComponent<Token>().Deselect();
            }
        }
        else if (token == selected) {
            Deselect();
        }
        else {
            if (selected != null) {
                Deselect();
            }
            Select(token);
        }
    }

    public static void BlockClick(Block block) {
        if (selected != null) {
            selected.BlockClick(block);
            // ReserveController.Adjust();
        }
        else {
            CameraControl.GoToBlock(block);
            // UI.System.Q("TerrainInfo").style.display = DisplayStyle.Flex;
            block.SetTerrainInfo();
            Block.DeselectAll();
            block.Select();
        }
    }

    private static void Select(Token token) {
        selected = token;
        token.Select();
        UnitMenu.ShowMenu(token.onlineDataObject.GetComponent<TokenData>());
    }

    public static void Deselect() {
        if (selected) {
            selected.SetNeutral();
            UnitMenu.HideMenu();
            selected.Deselect();
            selected = null;
        }
    }
}
