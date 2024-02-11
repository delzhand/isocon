using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenSync
{
    private static readonly int _chunkSize = 512;

    private class SyncImage
    {
        public TokenMeta Meta;
        public ImageChunk[] Chunks;
        public int ChunkCount = 0;
    }

    private class ImageChunk
    {
        public int Index;
        public Color[] ColorChunk;
    }

    private static Dictionary<string, SyncImage> SyncImages;

    public static void Add(TokenMeta meta)
    {
        if (SyncImages == null)
        {
            SyncImages = new();
        }

        var syncImage = new SyncImage
        {
            Meta = meta
        };
        SyncImages.Add(meta.Hash, syncImage);
    }

    public static void SyncStep()
    {
        Debug.Log("Sync Step");
        bool syncing = IsSyncing();
        UI.ToggleDisplay("SyncIndicator", syncing);
        if (!syncing)
        {
            return;
        }

        foreach (var syncImage in SyncImages.Values)
        {
            Debug.Log(syncImage.Meta.Name);
        }
    }

    public static Texture2D LoadHashedImage(string hash)
    {
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string filename = $"{directory}/{hash}.png";
        if (File.Exists(filename))
        {
            byte[] imageData = File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
        return null;
    }

    public static bool IsSyncing()
    {
        if (SyncImages == null)
        {
            return false;
        }
        foreach (var syncImage in SyncImages.Values)
        {
            if (syncImage.ChunkCount == 0)
            {
                Player.Self().CmdRequestChunkCount(syncImage.Meta.Hash);
            }
        }
        return SyncImages.Count > 0;
    }

    public static int GetChunkCount(string hash)
    {
        FileLogger.Write($"Requesting chunk count for {TokenLibrary.TruncateHash(hash)}");
        Texture2D image = LoadHashedImage(hash);
        if (image == null)
        {
            return 0;
        }
        Color[] allColors = image.GetPixels();
        int chunkCount = Mathf.CeilToInt((float)allColors.Length / _chunkSize);
        return chunkCount;
    }

    public static void SetChunkCount(string hash, int count)
    {
        var syncImage = SyncImages[hash];
        if (syncImage == null)
        {
            return;
        }

        if (syncImage.ChunkCount != count)
        {
            FileLogger.Write($"Received chunk count {count} for {TokenLibrary.TruncateHash(hash)}");
            syncImage.ChunkCount = count;

        }

    }
}