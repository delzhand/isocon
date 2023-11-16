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

public class TokenController : MonoBehaviour
{
    private static Token selected = null;

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

    // public static void BlockClick(Block block) {
    //     if (selected != null) {
    //         selected.BlockClick(block);
    //     }
    //     else {
    //         block.Select();
    //     }
    // }

    private static void Select(Token token) {
        selected = token;
        token.Select();
        TokenMenu.ShowMenu(token.onlineDataObject.GetComponent<TokenData>());
    }

    public static void Deselect() {
        if (selected) {
            selected.SetNeutral();
            TokenMenu.HideMenu();
            selected.Deselect();
            selected = null;
        }
    }

    public static bool IsSelected(TokenData data) {
        return data.TokenObject.GetComponent<Token>() == selected;
    }

    public static Token GetSelected() {
        return selected;
    }
    
}
