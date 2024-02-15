using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenSync : MonoBehaviour
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

    private class SyncRequest
    {
        public int ConnectionId;
        public float TimeReceived;
        public string Hash;
        public int[] MissingChunks;
    }

    private static Dictionary<string, SyncImage> SyncImages;
    private static Dictionary<string, Color[]> ChunkCache;
    private static Stack<SyncRequest> Outbound;

    private float _interval = 0;
    private static float MaxInterval = .5f;
    private static int ChunksPerRequest = 30;

    void Start()
    {
        Outbound ??= new();
    }

    void Update()
    {
        HudText.SetItem("outboundCount", $"Outbound Count: {Outbound.Count}", HudTextColor.Red);
        if (Outbound.Count > 0 && _interval <= 0)
        {
            _interval = MaxInterval;
            SyncStepSend();
        }
        else
        {
            _interval -= Time.deltaTime;
        }
    }

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

    public static void SyncStepSend()
    {
        var syncRequest = Outbound.Pop();

        // purge if older than some duration

        int count = Math.Min(syncRequest.MissingChunks.Length, ChunksPerRequest);
        int[] missingChunks = syncRequest.MissingChunks;
        ShuffleArray(missingChunks);
        FileLogger.Write($"Filling {count} random chunks of {syncRequest.MissingChunks.Length}");
        for (int i = 0; i < count; i++)
        {
            (int, Color[]) chunkInfo = TokenSync.GetMissingChunk(syncRequest.Hash, missingChunks[i]);
            if (chunkInfo.Item1 > -1)
            {
                int index = chunkInfo.Item1;
                Color[] chunk = chunkInfo.Item2;
                Player.Self().CmdDeliverMissingChunk(syncRequest.ConnectionId, syncRequest.Hash, index, chunk);
            }
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

    public static void StackRequest(int connectionId, string hash, int[] missingChunks)
    {
        string directory = TokenLibrary.GetHashedImageDirectory();
        string filename = $"{directory}/{hash}.png";
        if (File.Exists(filename))
        {
            var syncRequest = new SyncRequest()
            {
                ConnectionId = connectionId,
                Hash = hash,
                MissingChunks = missingChunks,
                TimeReceived = Time.time
            };
            if (Outbound.Count == 0 || !Outbound.Peek().Equals(syncRequest))
            {
                Outbound.Push(syncRequest);
            }
        }

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

    public static (int, Color[]) GetMissingChunk(string hash, int chunkId)
    {
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

        int startIndex = chunkId * _chunkSize;
        int remainingColors = Mathf.Min(_chunkSize, allColors.Length - startIndex);
        Color[] chunkColors = new Color[remainingColors];
        System.Array.Copy(allColors, startIndex, chunkColors, 0, remainingColors);
        return (chunkId, chunkColors);
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

    private static void ShuffleArray(int[] array)
    {
        System.Random rng = new System.Random();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = rng.Next(i + 1);
            int temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}