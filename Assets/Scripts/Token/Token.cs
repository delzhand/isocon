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
    public bool OnField = false;
    public int Size = 1;
    public Texture2D Image;

    void Awake() {
        Debug.Log("awake");
        if (Image == null) {
            Debug.Log("find");
            StartCoroutine(FindRemoteTokenImage());
        }
    }

    // Update is called once per frame
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

    // public void CustomCutout(string filename) {
    //     StartCoroutine(LoadLocalFileIntoCutout(filename));
    // }

    // private IEnumerator LoadLocalFileIntoCutout(string filename) {
    //     using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename))
    //     {
    //         yield return uwr.SendWebRequest();

    //         if (uwr.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.Log(uwr.error);
    //         }
    //         else
    //         {
    //             // Get downloaded asset bundle
    //             Image = DownloadHandlerTexture.GetContent(uwr);
    //             float aspectRatio = Image.width/(float)Image.height;
    //             transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
    //             transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
    //         }
    //     }
    // }

    public void PlaceAtBlock(Block block) {
        Vector3 v = block.transform.position + new Vector3(0, .25f, 0);
        Player.Self().CmdRequestTokenMove(gameObject, v);
        // OnField = true;
        // transform.Find("Base").gameObject.SetActive(true);
    }

    public void Select() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 1);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 2f, 2f, 2f));
    }

    public void Deselect() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
    }

    private IEnumerator FindRemoteTokenImage() {
        Debug.Log("FindRemoteTokenImage");
        string imageHash = GameSystem.Current().GetTokenImageHash(gameObject);
        Debug.Log(imageHash);

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string filename = "file://" + path + "/remote-tokens/" + imageHash + ".png";
        Debug.Log(filename);
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Toast.Add(uwr.error);
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("success");
            SetImage(DownloadHandlerTexture.GetContent(uwr));
        }

    }
}
