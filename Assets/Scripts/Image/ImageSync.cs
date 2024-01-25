using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ImageSync : MonoBehaviour
{
    private Dictionary<string, Texture2D> database = new();

    public static ImageSync Find()
    {
        return GameObject.Find("Engine").GetComponent<ImageSync>();
    }

    public Texture2D GetImage(string filename)
    {
        string path = Preferences.Current.DataPath;
        string filepath = "file://" + path + "/tokens/" + filename;
        return LoadImageFromFile(filepath);
    }

    public void ApplyLocal(Token t, string filename)
    {
        string path = Preferences.Current.DataPath;
        string filepath = "file://" + path + "/tokens/" + filename;
        StartCoroutine(LoadAndApply(t, filepath));
    }

    public void Apply(Token t, string hash)
    {
        if (database.ContainsKey(hash))
        {
            // The image file is known and loaded
            if (database[hash] != null)
            {
                t.SetImage(database[hash]);
            }
            else
            {
                // The image file is known but not yet loaded
                string path = Preferences.Current.DataPath;
                string filename = "file://" + path + "/remote-tokens/" + hash + ".png";
                StartCoroutine(LoadAndApply(t, filename));
            }
        }
        else
        {
            // The image is not known, retrieve from the host
            if (Player.IsOnline())
            {
                // Get from host
            }
            else
            {
                // Offline, can't get image
                throw new System.Exception("Cannot load image from remote host");
                // Toast.Add("Cannot load image from remote host.");
            }
        }
    }

    IEnumerator LoadAndApply(Token t, string filename)
    {
        IEnumerator loadTextureCoroutine = LoadTexture(filename);
        yield return loadTextureCoroutine;

        if (loadTextureCoroutine.Current != null && loadTextureCoroutine.Current is Texture2D)
        {
            Texture2D loadedImage = (Texture2D)loadTextureCoroutine.Current;
            string hash = TextureSender.GetTextureHash(loadedImage);
            if (database.ContainsKey(hash))
            {
                database[hash] = loadedImage;
            }
            else
            {
                database.Add(hash, loadedImage);
            }
            t.SetImage(loadedImage);
        }
    }

    public Texture2D LoadImageFromFile(string filePath)
    {
        // Read the image file data
        byte[] imageData = File.ReadAllBytes(filePath);

        // Create a new Texture2D
        Texture2D texture = new Texture2D(2, 2); // Initialize with placeholder values

        // Load image data into the Texture2D
        if (texture.LoadImage(imageData))
        {
            return texture;
        }
        else
        {
            Debug.LogError("Failed to load image into Texture2D.");
            return null;
        }
    }


    private IEnumerator LoadTexture(string filename)
    {
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename);
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Texture2D image = DownloadHandlerTexture.GetContent(uwr);
            yield return image; // Return the loaded texture
        }
    }
}
