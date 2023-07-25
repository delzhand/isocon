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

    public Texture2D Image;

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
                Image = DownloadHandlerTexture.GetContent(uwr);
                float aspectRatio = Image.width/(float)Image.height;
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
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
