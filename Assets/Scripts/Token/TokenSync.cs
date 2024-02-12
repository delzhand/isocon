using System;
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
        public int Width = 0;
        public int Height = 0;

        public bool Initialized { get => ChunkCount > 0; }

        public bool Complete { get => GetMissingChunks().Length == 0; }

        public float Percent { get => Mathf.RoundToInt((ChunkCount - GetMissingChunks().Length) / (float)ChunkCount * 100); }

        public int[] GetMissingChunks()
        {
            List<int> missingChunks = new();
            for (int i = 0; i < ChunkCount; i++)
            {
                if (Chunks[i] == null)
                {
                    missingChunks.Add(i);
                }
            }
            return missingChunks.ToArray();
        }
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
        try
        {
            bool syncing = IsSyncing();
            if (!syncing)
            {
                HudText.SetItem("syncStatus", "Synced", HudTextColor.Grey);
                return;
            }
            (int, int, int) syncInfo = GetOverallPercentage();
            int received = syncInfo.Item2;
            int total = syncInfo.Item3;
            int percent = syncInfo.Item1;
            HudText.SetItem("syncStatus", $"Syncing... {syncInfo.Item2}/{syncInfo.Item3} ({syncInfo.Item1}%)", HudTextColor.Blue);

            foreach (var syncImage in SyncImages.Values)
            {
                if (syncImage.Initialized)
                {
                    Player.Self().CmdRequestMissingChunks(syncImage.Meta.Hash, syncImage.GetMissingChunks());
                }
                else
                {
                    Player.Self().CmdRequestImageInfo(syncImage.Meta.Hash);
                }
            }
        }
        catch (Exception e)
        {
            Toast.AddError(e.Message);
        }

    }

    public static Texture2D LoadHashedImage(string hash)
    {
        if (SyncImages == null)
        {
            SyncImages = new();
        }
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string filename = $"{directory}/{hash}.png";
        if (File.Exists(filename))
        {
            byte[] imageData = File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            if (SyncImages.ContainsKey(hash))
            {
                SyncImages.Remove(hash);
            }

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
        return SyncImages.Count > 0;
    }

    private static (int, int, int) GetOverallPercentage()
    {
        int received = 0;
        int total = 0;
        foreach (var syncImage in SyncImages.Values)
        {
            if (syncImage.Initialized)
            {
                total += syncImage.ChunkCount;
                received += (syncImage.ChunkCount - syncImage.GetMissingChunks().Length);
            }
        }
        int percent = total > 0 ? Mathf.RoundToInt(received / (float)total) : 0;
        return (percent, received, total);
    }

    public static (int, int, int)? GetImageInfo(string hash)
    {
        FileLogger.Write($"Requesting chunk count for {TokenLibrary.TruncateHash(hash)}");
        Texture2D image = LoadHashedImage(hash);
        if (image == null)
        {
            FileLogger.Write($"This client does not have a graphic for {TokenLibrary.TruncateHash(hash)}");
            return null;
        }
        Color[] allColors = image.GetPixels();
        int chunkCount = Mathf.CeilToInt((float)allColors.Length / _chunkSize);
        FileLogger.Write($"Graphic {TokenLibrary.TruncateHash(hash)} located, returning chunk count {chunkCount}");
        return (chunkCount, image.width, image.height);
    }

    public static (int, Color[])? GetMissingChunk(string hash, int[] missingChunks)
    {
        FileLogger.Write($"Requesting one of {missingChunks.Length} chunk(s) for {TokenLibrary.TruncateHash(hash)}");
        Texture2D image = LoadHashedImage(hash);
        if (image == null)
        {
            FileLogger.Write($"This client does not have a graphic for {TokenLibrary.TruncateHash(hash)}");
            return null;
        }

        int i = missingChunks[UnityEngine.Random.Range(0, missingChunks.Length - 1)];
        Color[] allColors = image.GetPixels();
        int startIndex = i * _chunkSize;
        int remainingColors = Mathf.Min(_chunkSize, allColors.Length - startIndex);
        Color[] chunkColors = new Color[remainingColors];
        System.Array.Copy(allColors, startIndex, chunkColors, 0, remainingColors);
        return (i, chunkColors);
    }

    public static void SetImageInfo(string hash, int count, int width, int height)
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
            syncImage.Width = width;
            syncImage.Height = height;
            syncImage.Chunks = new ImageChunk[count];
        }

    }

    public static void SetMissingChunk(string hash, int index, Color[] chunk)
    {
        var syncImage = SyncImages[hash];
        if (syncImage == null || syncImage.Complete)
        {
            return;
        }
        syncImage.Chunks[index] = new ImageChunk()
        {
            Index = index,
            ColorChunk = chunk
        };
        int missingChunks = syncImage.GetMissingChunks().Length;
        int receivedChunks = syncImage.ChunkCount - missingChunks;
        if (SyncImages[hash].Complete)
        {
            AssembleImageFromChunks(hash);
            SyncImages.Remove(hash);
        }
    }



    private static void AssembleImageFromChunks(string hash)
    {
        var syncImage = SyncImages[hash];
        Texture2D receivedTexture = new(syncImage.Width, syncImage.Height);
        Color[] allColors = new Color[syncImage.Width * syncImage.Height];
        foreach (var imageChunk in syncImage.Chunks)
        {
            int startIndex = imageChunk.Index * _chunkSize;
            for (int i = 0; i < imageChunk.ColorChunk.Length; i++)
            {
                allColors[startIndex + i] = imageChunk.ColorChunk[i];
            }
        }
        receivedTexture.SetPixels(allColors);
        receivedTexture.Apply();

        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string filename = $"{directory}/{hash}.png";
        byte[] bytes = receivedTexture.EncodeToPNG();
        File.WriteAllBytes(filename, bytes);
    }
}