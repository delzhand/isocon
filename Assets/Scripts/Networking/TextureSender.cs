using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureSender : MonoBehaviour
{
    private static readonly int ChunkSize = 512;

    public string Hash;
    public Texture2D Image;
    public bool Complete = false;
    
    private Dictionary<int, Color[]> receivedChunks = new();

    private static List<string> openRequests = new();

    public static void SendToHost(Texture2D image)
    {
        Send(image, -1);
    }

    public static void SendToClient(Texture2D image, int connectionId) {
        Send(image, connectionId);
    }

    private static void Send(Texture2D image, int connectionId) {
        string hash = TextureSender.GetTextureHash(image);
        FileLogger.Write($"Sending image {TruncatedHash(hash)} to {connectionId}");
        Color[] allColors = image.GetPixels();
        int chunkCount = Mathf.CeilToInt((float)allColors.Length / ChunkSize);
        for (int i = 0; i < chunkCount; i++)
        {
            int startIndex = i * ChunkSize;
            int remainingColors = Mathf.Min(ChunkSize, allColors.Length - startIndex);
            Color[] chunkColors = new Color[remainingColors];
            System.Array.Copy(allColors, startIndex, chunkColors, 0, remainingColors);
            Player.Self().CmdSendTextureChunk(hash, connectionId, i, chunkCount, chunkColors, image.width, image.height);
        }
    }

    public static void Receive(string hash, int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height) {
        if (chunkIndex == 0) {
            FileLogger.Write($"Receiving first chunk of image {TruncatedHash(hash)}");
        }
        if (chunkIndex == chunkTotal - 1) {
            FileLogger.Write($"Receiving last chunk of image {TruncatedHash(hash)}");
        }

        GameObject receiver = GameObject.Find("TextureReceiver") ?? new GameObject("TextureReceiver");
        TextureSender[] receivers = receiver.GetComponents<TextureSender>();
        TextureSender match = null;
        for (int i = 0; i < receivers.Length; i++) {
            if (receivers[i].Hash == hash) {
                match = receivers[i];
                break;
            }
        }
        if (match == null) {
            match = receiver.AddComponent<TextureSender>();
            match.Hash = hash;
        }
        // No need to receive data
        if (!match.Complete) {
            match.Receive(chunkIndex, chunkTotal, chunkColors, width, height);
        }
    }

    private void Receive(int chunkIndex, int chunkTotal, Color[] chunkColors, int width, int height) {
        // Store the received chunkColors in a dictionary or list
        if (!receivedChunks.ContainsKey(chunkIndex))
        {
            receivedChunks.Add(chunkIndex, chunkColors);
        }

        // If all chunks have been received, reconstruct the complete texture
        if (receivedChunks.Count == chunkTotal)
        {
            // Combine all chunks to recreate the Texture2D on the server
            Color[] allColors = new Color[width * height];
            for (int i = 0; i < receivedChunks.Count; i++)
            {
                int startIndex = i * ChunkSize;
                receivedChunks[i].CopyTo(allColors, startIndex);
            }

            Texture2D receivedTexture = new(width, height);
            receivedTexture.SetPixels(allColors);
            receivedTexture.Apply();

            // Do something with the received texture
            Image = receivedTexture;
            Complete = true;

            // Save image to disk
            string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
            if (!Directory.Exists(path + "/remote-tokens")) {
              Directory.CreateDirectory(path + "/remote-tokens");
            }
            byte[] bytes = Image.EncodeToPNG();
            File.WriteAllBytes(path + "/remote-tokens/" + Hash + ".png", bytes);
        }        
    }

    public static string GetTextureHash(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null.");
            return null;
        }

        // Convert the texture data to a byte array
        byte[] textureData = texture.GetRawTextureData();

        // Compute the hash using SHA-256
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(textureData);

        // Convert the hash bytes to a hexadecimal string
        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static string TruncatedHash(string hash) {
        string firstThree = hash.Substring(0, 3);
        string lastThree = hash.Substring(hash.Length - 3);
        return $"{firstThree}...{lastThree}";
    }
    
    public static Texture2D LoadImageFromFile(string imageSource, bool isHash)
    {
        if (isHash) {
            return HandleRemote(imageSource);
        }
        else {
            return HandleLocal(imageSource);
        }
    }

    private static Texture2D HandleLocal(string filename) {
        FileLogger.Write($"Loading local file {filename}");

        // Load Image
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        path = $"{path}/tokens/{filename}";
        byte[] imageData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        // Copy to remote-images if necessary
        string hash = GetTextureHash(texture);
        path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        if (!Directory.Exists($"{path}/remote-tokens")) {
            Directory.CreateDirectory($"{path}/remote-tokens");
        }
        if (!File.Exists($"{path}/remote-tokens/{hash}.png")) {
            File.WriteAllBytes($"{path}/remote-tokens/{hash}.png", imageData);
        }

        // If not host, Send to host
        if (!Player.IsHost()) {
            SendToHost(texture);
        }
        
        return texture;
    }

    private static Texture2D HandleRemote(string hash) {
        FileLogger.Write($"Loading remote file {TruncatedHash(hash)}");

        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        path = $"{path}/remote-tokens/{hash}.png";
        if (File.Exists(path)) {
            openRequests.Remove(hash);
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            return texture;
        }

        // File doesn't exist and was already requested, just try again later
        if (openRequests.Contains(hash)) {
            FileLogger.Write($"Image not found at {path}, already requested");
            return null;
        }

        // Request server to resync items
        openRequests.Add(hash);
        FileLogger.Write($"Image not found at {path}, requesting from host");
        Player.Self().CmdRequestImage(hash);
        return null;

    }

    public static Texture2D CopyLocalImage(string filename) {
        Texture2D graphic = LoadImageFromFile(filename, false);
        string hash = GetTextureHash(graphic);
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string remotefilepath = path + "/remote-tokens/" + hash + ".png";
        byte[] pngData = graphic.EncodeToPNG();
        File.WriteAllBytes(remotefilepath, pngData);
        return graphic;
    }
}
