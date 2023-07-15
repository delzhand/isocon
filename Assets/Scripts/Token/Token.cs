using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public enum HoldState {
    None,
    Held,
    QuickHP,
    Edit,
}

public class Token : MonoBehaviour
{
    public bool InReserve = true;
    public int Size = 1;
    private HoldState state = HoldState.None;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        alignToCamera();
    }

    private void alignToCamera() {
        if (InReserve) {
            Transform camera = GameObject.Find("ReserveCamera").transform;
            transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 180, 0);
        }
        else {
            Transform camera = GameObject.Find("CameraOrigin").transform;
            transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
        }
    }

    public void CustomCutout(string filename) {
        StartCoroutine(LoadLocalFileIntoCutout(filename));
    }

    private IEnumerator LoadLocalFileIntoCutout(string filename) {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", texture);
            }
        }
    }

    public void PlaceAtBlock(Block block) {
        transform.position = block.transform.position + new Vector3(0, .25f, 0);
        InReserve = false;
        transform.Find("Base").gameObject.SetActive(true);

        ReserveSpot rs = ReserveSpot.GetReserveSpot(this);
        if (rs != null) {
            rs.Token = null;
        }
    }

    public void SetState(HoldState h) {
        state = h;
        switch (state) {
            case HoldState.None:
                ClearState();
                break;
            case HoldState.Held:
                Debug.Log("held mode");
                TokenController.SetHeld(this);
                TokenController.EnableFocusPane();
                GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 1);
                GetComponentInChildren<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 2f, 0, 2f));
                break;
            case HoldState.Edit:
                Debug.Log("edit mode");
                // TokenController.SetHeld(this);
                // GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 1);
                // GetComponentInChildren<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 0, 2f, 2f));
                break;
            case HoldState.QuickHP:
                Debug.Log("quick edit mode");
                TokenController.EnableQuickEdit();
                // TokenController.SetHeld(this);
                // GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 1);
                // GetComponentInChildren<MeshRenderer>().material.SetVector("_Color", new Vector4(0, 2f, 2f, 2f));
                break;
        }
    }

    public void AdvanceState() {
        int stateVal = (int)state;
        int newStateVal = (stateVal + 1) % Enum.GetValues(typeof(HoldState)).Length;
        SetState((HoldState)newStateVal);
    }

    public void ClearState() {
        state = HoldState.None;
        TokenController.DisableFocusPane();
        TokenController.DisableQuickEdit();
        GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 0);
    }

}
