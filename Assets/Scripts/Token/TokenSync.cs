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
        public TokenData Data;

        public bool Complete { get => GetMissingChunks().Length == 0; }

        public float Percent { get => Mathf.RoundToInt((Meta.ChunkCount - GetMissingChunks().Length) / (float)Meta.ChunkCount * 100); }

        public int[] GetMissingChunks()
        {
            List<int> missingChunks = new();
            for (int i = 0; i < Meta.ChunkCount; i++)
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
    private static Dictionary<string, Color[]> ChunkCache;

    public static void Add(TokenMeta meta, TokenData data)
    {
        if (SyncImages == null)
        {
            SyncImages = new();
        }

        var syncImage = new SyncImage
        {
            Meta = meta,
            Data = data,
            Chunks = new ImageChunk[meta.ChunkCount]
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
            HudText.SetItem("syncStatus", $"Syncing... {syncInfo.Item2}/{syncInfo.Item3} ({syncInfo.Item1}%)", HudTextColor.Blue);

            foreach (var syncImage in SyncImages.Values)
            {
                Player.Self().CmdRequestMissingChunks(syncImage.Meta.Hash, syncImage.GetMissingChunks());
            }
        }
        catch (Exception e)
        {
            Toast.AddError(e.Message);
        }

    }

    public static Texture2D LoadHashedImage(string hash)
    {
        SyncImages ??= new();
        string directory = TokenLibrary.GetHashedImageDirectory();
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
            total += syncImage.Meta.ChunkCount;
            received += (syncImage.Meta.ChunkCount - syncImage.GetMissingChunks().Length);
        }
        int percent = total > 0 ? Mathf.RoundToInt(received / (float)total * 100) : 0;
        return (percent, received, total);
    }

    public static (int, Color[]) GetMissingChunk(string hash, int[] missingChunks)
    {
        int i = missingChunks[UnityEngine.Random.Range(0, missingChunks.Length - 1)];

        ChunkCache ??= new Dictionary<string, Color[]>();
        Color[] allColors = null;
        if (ChunkCache.ContainsKey(hash))
        {
            allColors = ChunkCache[hash];
        }
        else
        {
            Texture2D image = LoadHashedImage(hash);
            if (image == null)
            {
                return (-1, null);
            }
            allColors = image.GetPixels();
            ChunkCache[hash] = allColors;
        }

        int startIndex = i * _chunkSize;
        int remainingColors = Mathf.Min(_chunkSize, allColors.Length - startIndex);
        Color[] chunkColors = new Color[remainingColors];
        System.Array.Copy(allColors, startIndex, chunkColors, 0, remainingColors);
        return (i, chunkColors);
    }

    public static void SetMissingChunk(string hash, int index, Color[] chunk)
    {
        if (!SyncImages.ContainsKey(hash) || index < 0)
        {
            return;
        }

        var syncImage = SyncImages[hash];
        if (syncImage.Chunks[index] != null)
        {
            FileLogger.Write("wasted call");
        }
        syncImage.Chunks[index] = new ImageChunk()
        {
            Index = index,
            ColorChunk = chunk
        };
        if (SyncImages[hash].Complete)
        {
            AssembleImageFromChunks(hash, SyncImages[hash]);
            SyncImages.Remove(hash);
        }
        (int, int, int) syncInfo = GetOverallPercentage();
        HudText.SetItem("syncStatus", $"Syncing... {syncInfo.Item2}/{syncInfo.Item3} ({syncInfo.Item1}%)", HudTextColor.Blue);
    }

    private static void AssembleImageFromChunks(string hash, SyncImage syncImage)
    {
        FileLogger.Write($"File {TokenMeta.TruncateHash(hash)}");
        Texture2D receivedTexture = new(syncImage.Meta.Width, syncImage.Meta.Height);
        Color[] allColors = new Color[syncImage.Meta.Width * syncImage.Meta.Height];
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

        string directory = TokenLibrary.GetHashedImageDirectory();
        string filename = $"{directory}/{hash}.png";
        byte[] bytes = receivedTexture.EncodeToPNG();
        File.WriteAllBytes(filename, bytes);

        syncImage.Data.SetGraphic(receivedTexture);
    }
}