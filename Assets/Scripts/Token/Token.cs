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

public class Token : MonoBehaviour
{
    public bool InReserve = true;
    public int Size = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        alignToCamera();

        // if (TokenController.IsHeld(this)) {
        //     VisualElement edit = UI.GameInfo.Q("ModifyPane");
        //     HpBar hp = GetComponent<HpBar>();
        //     UnitState us = GetComponent<UnitState>();
        //     edit.Q<Label>("HPVal").text = hp.CHP.ToString();
        //     edit.Q<Label>("VigVal").text = hp.VIG.ToString();
        //     edit.Q<Label>("WoundsVal").text = hp.Wounds.ToString();
        //     edit.Q<Label>("ResVal").text = us.Resolve.ToString();
        //     edit.Q<Label>("GResVal").text = UnitState.GResolve.ToString();
        //     edit.Q<Label>("AthVal").text = us.Aether.ToString();
        //     edit.Q<Label>("VglVal").text = us.Vigilance.ToString();
        //     edit.Q<Label>("BlsVal").text = us.Blessings.ToString();
        // }
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

    public void Select() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 2f, 0, 2f));
    }

    public void Deselect() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
    }

    public void SetMoving() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 1);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 2f, 2f, 2f));
    }
}
