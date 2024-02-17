using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenSync : MonoBehaviour
{
    private static readonly int _chunkSize = 8 * 1024;

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
        public Byte[] ByteChunk;
    }

    private class SyncRequest
    {
        public int ConnectionId;
        public float TimeReceived;
        public string Hash;
        public int[] MissingChunks;
    }

    private static Dictionary<string, SyncImage> SyncImages;
    private static Dictionary<string, Byte[]> ChunkCache;
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
        if (Outbound.Count > 0 && _interval <= 0)
        {
            _interval = MaxInterval;
            SyncStepSend();
        }
        else
        {
            _interval -= Time.deltaTime;
        }

        if (Player.Self() != null)
        {
            Player.Self().PercentSynced = GetOverallPercentage().Item1;
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
                return;
            }
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
            (int, Byte[]) chunkInfo = TokenSync.GetMissingChunk(syncRequest.Hash, missingChunks[i]);
            if (chunkInfo.Item1 > -1)
            {
                int index = chunkInfo.Item1;
                Byte[] chunk = chunkInfo.Item2;
                Player.Self().CmdDeliverMissingChunk(syncRequest.ConnectionId, syncRequest.Hash, index, chunk);
            }
        }
    }

    public static Texture2D LoadHashedFileAsTexture(string hash)
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

    /// <summary>
    /// Provides information about all tokens in transit
    /// </summary>
    /// <returns>Item1 is percent, Item2 is received chunks, Item3 is total chunks</returns>
    private static (int, int, int) GetOverallPercentage()
    {
        SyncImages ??= new();
        int received = 0;
        int total = 0;
        foreach (var syncImage in SyncImages.Values)
        {
            total += syncImage.Meta.ChunkCount;
            received += (syncImage.Meta.ChunkCount - syncImage.GetMissingChunks().Length);
        }
        int percent = total > 0 ? Mathf.RoundToInt(received / (float)total * 100) : 100;
        return (percent, received, total);
    }

    public static int GetChunkCount(int byteLength)
    {
        return Mathf.CeilToInt(byteLength / (float)_chunkSize);
    }

    public static (int, Byte[]) GetMissingChunk(string hash, int chunkId)
    {
        ChunkCache ??= new Dictionary<string, Byte[]>();
        Byte[] allBytes = null;
        if (ChunkCache.ContainsKey(hash))
        {
            allBytes = ChunkCache[hash];
        }
        else
        {
            string directory = TokenLibrary.GetHashedImageDirectory();
            string filename = $"{directory}/{hash}.png";
            if (!File.Exists(filename))
            {
                return (-1, null);
            }
            allBytes = File.ReadAllBytes(filename);
            ChunkCache[hash] = allBytes;
        }

        int startIndex = chunkId * _chunkSize;
        int remainingBytes = Mathf.Min(_chunkSize, allBytes.Length - startIndex);
        Debug.Log(allBytes.Length / _chunkSize);
        Byte[] chunkBytes = new Byte[remainingBytes];
        System.Array.Copy(allBytes, startIndex, chunkBytes, 0, remainingBytes);
        return (chunkId, chunkBytes);
    }

    public static void SetMissingChunk(string hash, int index, Byte[] chunk)
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
            ByteChunk = chunk
        };
        if (SyncImages[hash].Complete)
        {
            AssembleImageFromChunks(hash, SyncImages[hash]);
            SyncImages.Remove(hash);
        }
        (int, int, int) syncInfo = GetOverallPercentage();
        if (syncInfo.Item1 < 100)
        {
            HudText.SetItem("syncStatus", $"{syncInfo.Item2 * _chunkSize}/{syncInfo.Item3 * _chunkSize} kb received", 11, HudTextColor.Blue);
        }
        else
        {
            HudText.RemoveItem("syncStatus");
        }
    }

    private static void AssembleImageFromChunks(string hash, SyncImage syncImage)
    {
        FileLogger.Write($"Assembly file {TokenMeta.TruncateHash(hash)}");
        Byte[] allBytes = new Byte[syncImage.Meta.ChunkCount * _chunkSize];
        foreach (var imageChunk in syncImage.Chunks)
        {
            int startIndex = imageChunk.Index * _chunkSize;
            for (int i = 0; i < imageChunk.ByteChunk.Length; i++)
            {
                allBytes[startIndex + i] = imageChunk.ByteChunk[i];
            }
        }

        string directory = TokenLibrary.GetHashedImageDirectory();
        string filename = $"{directory}/{hash}.png";
        File.WriteAllBytes(filename, allBytes);
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