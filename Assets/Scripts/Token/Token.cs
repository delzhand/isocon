using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirror;
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

    public GameObject offlineDataObject;
    public GameObject onlineDataObject;

    void Start() {
        // ApplyImage();
    }

    void Update()
    {
        alignToCamera();
    }

    // private void ApplyImage() {
    //     if (RemoteHash.Length > 0) {
    //         ImageSync.Find().Apply(this, RemoteHash);
    //     }
    //     else if (LocalFilename.Length > 0) {
    //         ImageSync.Find().ApplyLocal(this, LocalFilename);
    //     }        
    // }

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

    public void PlaceAtBlock(Block block) {
        Vector3 v = block.transform.position + new Vector3(0, .25f, 0);
        Player.MoveToken(this, v);
    }

    public void Select(bool deselectOthers = false) {
        if (deselectOthers) {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Token")) {
                g.GetComponent<Token>().Deselect();
            }
        }
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 1);
        // transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 1);
        // transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetVector("_Color", new Vector4(2f, 2f, 2f, 2f));
    }

    public void Deselect() {
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", 0);
        // transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", 0);
    }

    // private IEnumerator FindRemoteTokenImage() {
    //     Debug.Log("FindRemoteTokenImage");
    //     string imageHash = GameSystem.Current().GetTokenImageHash(gameObject);
    //     Debug.Log(imageHash);

    //     string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
    //     string filename = "file://" + path + "/remote-tokens/" + imageHash + ".png";
    //     Debug.Log(filename);
    //     using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename);
    //     yield return uwr.SendWebRequest();

    //     if (uwr.result != UnityWebRequest.Result.Success)
    //     {
    //         Toast.Add(uwr.error);
    //         Debug.Log(uwr.error);
    //     }
    //     else
    //     {
    //         Debug.Log("success");
    //         SetImage(DownloadHandlerTexture.GetContent(uwr));
    //     }

    // }
}
