using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SimpleFileBrowser;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenLibrary : MonoBehaviour
{
    private class TokenLibraryFile
    {
        public TokenMeta[] Tokens;
    }

    private class SyncImage
    {
        public TokenMeta Meta;
        public ImageChunk[] Chunks;
    }

    private class ImageChunk
    {
        public int Index;
        public Color[] ColorChunk;
    }

    public static Dictionary<string, TokenMeta> Tokens;
    public static List<(TokenMeta, VisualElement)> ElementMap;
    private static Dictionary<string, SyncImage> SyncImages;

    private static int LibraryItemSize = 120;

    private static bool AllowSelect = false;
    private static bool Editing = false; // if another state is ever necessary use an enum instead of more bools
    private static string SelectedHash;

    private static TokenLibrary Find()
    {
        return GameObject.Find("AppState").GetComponent<TokenLibrary>();
    }

    public static void Setup()
    {
        Tokens = new();
        ElementMap = new();
        SyncImages = new();
        Find().AddAddButtonToUI();
        ReadLibraryFile();
        Bind();
    }

    void Update()
    {
        foreach ((TokenMeta, VisualElement) item in ElementMap)
        {
            var meta = item.Item1;
            var element = item.Item2;
            int currentFrameIndex = Mathf.FloorToInt(Time.time * meta.FPS) % meta.Frames;
            int offset = Mathf.RoundToInt(-100 * currentFrameIndex);
            element.Q("Sprite").style.left = Length.Percent(offset);
        }
    }

    private static void Bind()
    {
        VisualElement root = UI.System.Q("TokenLibraryModal");
        root.Q("Exit").RegisterCallback<ClickEvent>(Close);
        root.Q("CancelButton").RegisterCallback<ClickEvent>(CancelButtonClicked);
        root.Q("EditButton").RegisterCallback<ClickEvent>(EditButtonClicked);
        root.Q("SelectButton").RegisterCallback<ClickEvent>(SelectButtonClicked);
        root.Q("SaveButton").RegisterCallback<ClickEvent>(SaveButtonClicked);
    }

    public static void ShowDefaultMode(ClickEvent evt)
    {
        AllowSelect = false;
        UI.ToggleDisplay("TokenLibraryModal", true);
        UpdateVisibility();
    }

    public static void ShowSelectMode(ClickEvent evt)
    {
        AllowSelect = true;
        UI.ToggleDisplay("TokenLibraryModal", true);
        UpdateVisibility();
    }

    public static void Close(ClickEvent evt)
    {
        UI.ToggleDisplay("TokenLibraryModal", false);
    }

    public static void CancelButtonClicked(ClickEvent evt)
    {
        if (Editing)
        {
            Editing = false;
            UpdateVisibility();
        }
        else
        {
            Close(evt);
        }
    }

    private static void EditButtonClicked(ClickEvent evt)
    {
        Editing = true;
        UpdateVisibility();

        TokenMeta meta = Tokens[SelectedHash];
        VisualElement root = UI.System.Q("TokenLibraryModal");
        root.Q<TextField>("NameField").value = meta.Name;
        root.Q<IntegerField>("FramesField").value = meta.Frames;
        root.Q<IntegerField>("FpsField").value = meta.FPS;
        root.Q<Toggle>("FavoriteField").value = meta.Favorite;

    }

    private static void SelectButtonClicked(ClickEvent evt)
    {
        // invoke???
    }

    private static void SaveButtonClicked(ClickEvent evt)
    {
        // write file
        Editing = false;
        UpdateVisibility();

        VisualElement root = UI.System.Q("TokenLibraryModal");
        Tokens[SelectedHash].Name = root.Q<TextField>("NameField").value;
        Tokens[SelectedHash].Frames = root.Q<IntegerField>("FramesField").value;
        Tokens[SelectedHash].FPS = root.Q<IntegerField>("FpsField").value;
        Tokens[SelectedHash].Favorite = root.Q<Toggle>("FavoriteField").value;
        WriteLibraryFile();
        foreach ((TokenMeta, VisualElement) item in ElementMap)
        {
            if (item.Item1.Hash == SelectedHash)
            {
                item.Item2.Q<Label>("TokenLabel").text = Tokens[SelectedHash].Name;
            }
        }
    }

    private static void UpdateVisibility()
    {
        VisualElement root = UI.System.Q("TokenLibraryModal");
        UI.ToggleDisplay(root.Q("TokenLibrary"), !Editing);
        UI.ToggleDisplay(root.Q("TokenMetaEdit"), Editing);

        UI.ToggleDisplay(root.Q("SelectButton"), AllowSelect && !Editing);
        UI.ToggleDisplay(root.Q("SaveButton"), Editing);
        UI.ToggleDisplay(root.Q("EditButton"), !Editing);
        UI.Redraw();
    }

    public static void Add(TokenMeta meta)
    {
        var syncImage = new SyncImage
        {
            Meta = meta
        };
        SyncImages.Add(meta.Hash, syncImage);
    }

    private void AddAddButtonToUI()
    {
        VisualElement tokenDisplay = new();
        tokenDisplay.AddToClassList("item");
        tokenDisplay.style.height = LibraryItemSize;
        tokenDisplay.style.width = LibraryItemSize;
        tokenDisplay.Add(new Label("Add"));

        tokenDisplay.RegisterCallback<ClickEvent>((evt) =>
        {
            FileBrowserHelper.OpenLoadTokenBrowser();
        });
        UI.System.Q("TokenLibrary").Q("LibraryGrid").Add(tokenDisplay);
    }

    public static void ConfirmSelect(ClickEvent evt)
    {
        int count = 0;
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        foreach (string filename in FileBrowser.Result)
        {
            count++;
            byte[] imageData = File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            string hash = GetTextureHash(texture);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes($"{directory}/{hash}.png", imageData);
            var tokenMeta = new TokenMeta
            {
                Hash = hash
            };
            tokenMeta.Name = filename.Split("\\").Last<string>();
            Tokens[tokenMeta.Hash] = tokenMeta;
            AddToUI(tokenMeta);
        }
        WriteLibraryFile();
        Toast.AddSuccess($"{count} tokens added to the library.");
    }

    private static Texture2D LoadHashedImage(string hash)
    {
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string filename = $"{directory}/{hash}.png";
        byte[] imageData = File.ReadAllBytes(filename);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    private static void AddToUI(TokenMeta meta)
    {
        Texture2D graphic = LoadHashedImage(meta.Hash);
        float aspectRatio = graphic.width / meta.Frames / (float)graphic.height;

        VisualElement tokenDisplay = new();
        tokenDisplay.AddToClassList("item");
        tokenDisplay.style.height = LibraryItemSize;
        tokenDisplay.style.width = LibraryItemSize;

        VisualElement frame = new();
        frame.AddToClassList("frame");
        int width = LibraryItemSize;
        int height = LibraryItemSize;
        if (aspectRatio >= 1)
        {
            height = Mathf.RoundToInt(LibraryItemSize / aspectRatio);
        }
        else
        {
            width = Mathf.RoundToInt(LibraryItemSize * aspectRatio);
        }
        frame.style.width = width;
        frame.style.height = height;

        Label label = new();
        label.name = "TokenLabel";
        label.AddToClassList("panel-text");
        label.text = meta.Name;
        label.style.backgroundColor = new Color(0, 0, 0, .5f);

        VisualElement sprite = new();
        sprite.name = "Sprite";
        sprite.AddToClassList("sprite");
        sprite.style.width = width * meta.Frames;
        sprite.style.backgroundImage = graphic;

        frame.Add(sprite);
        tokenDisplay.Add(frame);
        tokenDisplay.Add(label);

        tokenDisplay.RegisterCallback<ClickEvent>((evt) =>
        {
            foreach ((TokenMeta, VisualElement) item in ElementMap)
            {
                if (item.Item1.Hash == meta.Hash)
                {
                    SelectedHash = meta.Hash;
                    item.Item2.ToggleInClassList("selected");
                }
                else
                {
                    item.Item2.RemoveFromClassList("selected");
                }
            }
        });

        UI.System.Q("TokenLibrary").Q("LibraryGrid").Add(tokenDisplay);

        ElementMap.Add((meta, tokenDisplay));
    }

    private static void WriteLibraryFile()
    {
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string fileName = $"{directory}/library.json";
        var tokenLibraryFile = new TokenLibraryFile();
        tokenLibraryFile.Tokens = Tokens.Values.ToArray();
        string json = JsonUtility.ToJson(tokenLibraryFile);
        File.WriteAllText(fileName, json);
    }

    private static void ReadLibraryFile()
    {
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        string fileName = $"{directory}/library.json";
        if (!File.Exists(fileName))
        {
            return;
        }
        string json = File.ReadAllText(fileName);
        if (json.Length == 0)
        {
            return;
        }
        var tokenLibraryFile = JsonUtility.FromJson<TokenLibraryFile>(json);
        Tokens.Clear();
        foreach (var tokenMeta in tokenLibraryFile.Tokens)
        {
            Tokens[tokenMeta.Hash] = tokenMeta;
            AddToUI(tokenMeta);
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
}
