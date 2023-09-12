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

    void Update()
    {
        alignToCamera();
    }

    private void alignToCamera() {
        Transform camera = GameObject.Find("CameraOrigin").transform;
        transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
    }

    public void SetImage(Texture2D image) {
        Image = image;
        float aspectRatio = Image.width/(float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
    }

    public void BlockClick(Block block) {
        Vector3 v = block.transform.position + new Vector3(0, .25f, 0);
        switch (UnitMenu.ActiveMenuItem) {
            case "Placing":
                Player.Self().CmdRequestPlaceToken(onlineDataObject, v);
                UnitMenu.DonePlacing();
                SetNeutral();
                break;
            case "Moving":
                Player.Self().CmdMoveToken(onlineDataObject, v, false);
                break;
        }
    }

    public void Select(bool deselectOthers = false) {
        if (deselectOthers) {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Token")) {
                g.GetComponent<Token>().Deselect();
            }
        }
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1); // worldspace token selected material
        UI.ToggleDisplay(onlineDataObject.GetComponent<TokenData>().Element.Q("Selected"), true); // selected indicator in unit bar
        UI.ToggleDisplay("SelectedTokenPanel", true); // selected token panel
        GameSystem.Current().UpdateSelectedTokenPanel(onlineDataObject);
        SetNeutral();
    }

    public void Deselect() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        UI.ToggleDisplay(onlineDataObject.GetComponent<TokenData>().Element.Q("Selected"), false);
        UI.ToggleDisplay("SelectedTokenPanel", false);
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
