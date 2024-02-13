using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public class TokenMeta
{
    private static int _chunkSize = 512;

    public string Name = "";
    public int FPS = 0;
    public int Frames = 1;
    public string Hash;
    public bool Favorite;
    public int Width;
    public int Height;
    public int ChunkCount;

    public TokenMeta()
    {
    }

    public TokenMeta(Texture2D image, string filename)
    {
        if (image == null)
        {
            throw new Exception("Image must not be null");
        }
        if (filename == null || filename.Length == 0)
        {
            throw new Exception("Filename must not be empty");
        }
        Hash = GetHash(image);
        ChunkCount = Mathf.CeilToInt(image.GetPixels().Length / (float)_chunkSize);
        Name = filename.Split("\\").Last<string>();
        Width = image.width;
        Height = image.height;
    }

    private TokenMeta(TokenMeta original)
    {
        Name = original.Name;
        FPS = original.FPS;
        Frames = original.Frames;
        Hash = original.Hash;
        Favorite = original.Favorite;
        Width = original.Width;
        Height = original.Height;
        ChunkCount = original.ChunkCount;
    }

    public static TokenMeta Copy(TokenMeta meta)
    {
        return new TokenMeta(meta);
    }

    public static string GetHash(Texture2D texture)
    {
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

    public static string TruncateHash(string hash)
    {
        string firstThree = hash.Substring(0, 3);
        string lastThree = hash.Substring(hash.Length - 3);
        return $"{firstThree}...{lastThree}";
    }
}
