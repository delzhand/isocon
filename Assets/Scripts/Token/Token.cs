using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirror;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class Token : MonoBehaviour
{
    public int Size = 1;
    public Texture2D Image;

    public GameObject offlineDataObject;
    public GameObject onlineDataObject;

    public float ShareOffsetX;
    public float ShareOffsetY;

    public Token LastFocused;

    public bool Selected = false;
    public bool Focused = false;

    void Update()
    {
        alignToCamera();
        Offset();

        if (Focused && this != LastFocused) {
            Unfocus();
        }

    }

    private void alignToCamera() {
        Transform camera = GameObject.Find("CameraOrigin").transform;
        transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
    }

    private void Offset() {
        float x = ShareOffsetX;
        float y = ShareOffsetY;
        if (Size == 2) {
            x = 0;
            y = -.73f;
        }
        else if (Size == 3) {
            x = 0;
            y = 0;
        }
        transform.Find("Offset").transform.localPosition = new Vector3(x, .2f, y);
        transform.Find("Base").transform.localPosition = new Vector3(x, .2f,y);
    }

    public void SetImage(Texture2D image) {
        Image = image;
        float aspectRatio = Image.width/(float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
    }

    public void LeftClick() {
        if (Selected) {
            Deselect();
        }
        else {
            Select();
            // TokenMenu.ShowMenu(this.GetComponent<TokenData>());
        }
    }

    public void RightClick() {

    }

    public void Place(Block block) {
        Vector3 v = block.transform.position + new Vector3(0, .25f, 0);
        Player.Self().CmdRequestPlaceToken(onlineDataObject, v);
        // TokenMenu.DonePlacing();
        SetNeutral();
    }

    public void Move(Block block) {
        Vector3 v = block.transform.position + new Vector3(0, .25f, 0);
        Player.Self().CmdMoveToken(onlineDataObject, v, false);
    }

    public void Select() {
        DeselectAll();
        Selected = true;
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1); // worldspace token selected material
        UI.ToggleDisplay(onlineDataObject.GetComponent<TokenData>().Element.Q("Selected"), true); // selected indicator in unit bar
        UI.ToggleDisplay("SelectedTokenPanel", true); // selected token panel
        SetNeutral();
    }

    public void Deselect() {
        Selected = false;
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        UI.ToggleDisplay(onlineDataObject.GetComponent<TokenData>().Element.Q("Selected"), false);
        UI.ToggleDisplay("SelectedTokenPanel", false);
    }

    public static void DeselectAll() {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Token")) {
            g.GetComponent<Token>().Deselect();
        }
    }

    public static Token GetSelected() {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++) {
            if (tokens[i].GetComponent<Token>().Selected) {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public static GameObject GetSelectedData() {
        Token selected = GetSelected();
        if (selected != null && selected.onlineDataObject != null) {
            return selected.onlineDataObject;
        }
        return null;
    }

    public void Focus() {
        UnfocusAll();
        Focused = true;
    }

    public void Unfocus() {
        Focused = false;
    }

    public static Token GetFocused() {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++) {
            if (tokens[i].GetComponent<Token>().Focused) {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public static GameObject GetFocusedData() {
        Token focused = GetFocused();
        if (focused != null && focused.onlineDataObject != null) {
            return focused.onlineDataObject;
        }
        return null;
    }

    public static void UnfocusAll() {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++) {
            tokens[i].GetComponent<Token>().Unfocus();
        }        
    }

    public void SetPlacing() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
    }

    public void SetMoving() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 1);
    }

    public void SetNeutral() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
    }

    public void SetDefeated(bool defeated) {
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_Dead", defeated ? 1 : 0);
    }
    
}
