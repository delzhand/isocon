using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SimpleFileBrowser;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenLibrary : MonoBehaviour
{

    public delegate void LibraryCallback();

    private class TokenLibraryFile
    {
        public TokenMeta[] Tokens;
    }

    public static Dictionary<string, TokenMeta> Tokens;
    public static Dictionary<string, (TokenMeta, VisualElement)> ElementMap;

    private static int LibraryItemSize = 120;

    private static bool AllowSelect = false;
    private static bool Editing = false;
    private static TokenMeta BackupMeta;
    private static string SelectedHash;
    private static LibraryCallback OnSelect;

    public static void Setup()
    {
        Tokens = new();
        ElementMap = new();
        ReadLibraryFile();
        Bind();
    }

    void Update()
    {
        foreach ((TokenMeta, VisualElement) item in ElementMap.Values)
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
        root.Q("AddButton").RegisterCallback<ClickEvent>((evt) => FileBrowserHelper.OpenLoadTokenBrowser());
        root.Q("Exit").RegisterCallback<ClickEvent>(Close);
        root.Q("CancelButton").RegisterCallback<ClickEvent>(CancelButtonClicked);
        root.Q("EditButton").RegisterCallback<ClickEvent>(EditButtonClicked);
        root.Q("DeleteButton").RegisterCallback<ClickEvent>(DeleteButtonClicked);
        root.Q("SelectButton").RegisterCallback<ClickEvent>(SelectButtonClicked);
        root.Q("SaveButton").RegisterCallback<ClickEvent>(SaveButtonClicked);
        root.Q<TextField>("NameField").RegisterValueChangedCallback<string>((evt) => ChangeEditingValue());
        root.Q<IntegerField>("FramesField").RegisterValueChangedCallback<int>((evt) => ChangeEditingValue());
        root.Q<IntegerField>("FpsField").RegisterValueChangedCallback<int>((evt) => ChangeEditingValue());
        root.Q<Toggle>("FavoriteField").RegisterValueChangedCallback<bool>((evt) => ChangeEditingValue());
    }

    public static void ShowDefaultMode(ClickEvent evt)
    {
        AllowSelect = false;
        UI.ToggleDisplay("TokenLibraryModal", true);
        UpdateVisibility();
    }

    public static void ShowSelectMode(ClickEvent evt, LibraryCallback onSelect)
    {
        AllowSelect = true;
        OnSelect = onSelect;
        UI.ToggleDisplay("TokenLibraryModal", true);
        UpdateVisibility();
    }

    public static TokenMeta GetSelectedMeta()
    {
        return Tokens[SelectedHash];
    }

    public static bool TokenSelected()
    {
        return SelectedHash != null;
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
            Tokens[SelectedHash].Name = BackupMeta.Name;
            Tokens[SelectedHash].Frames = BackupMeta.Frames;
            Tokens[SelectedHash].FPS = BackupMeta.FPS;
            Tokens[SelectedHash].Favorite = BackupMeta.Favorite;

            ElementMap[SelectedHash].Item2.Q<Label>("TokenLabel").text = Tokens[SelectedHash].Name;
            UI.System.Q("TokenLibraryModal").Q(SelectedHash).Add(ElementMap[SelectedHash].Item2);
            UpdateVisibility();
            UI.Redraw();
        }
        else
        {
            Close(evt);
        }

        if (SelectedHash != null)
        {
            ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
            SelectedHash = null;
        }
    }

    private static void EditButtonClicked(ClickEvent evt)
    {
        Editing = true;
        UpdateVisibility();
        UI.Redraw();

        TokenMeta meta = Tokens[SelectedHash];
        BackupMeta = TokenMeta.Copy(meta);
        VisualElement root = UI.System.Q("TokenLibraryModal");
        root.Q<TextField>("NameField").value = meta.Name;
        root.Q<IntegerField>("FramesField").value = meta.Frames;
        root.Q<IntegerField>("FpsField").value = meta.FPS;
        root.Q<Toggle>("FavoriteField").value = meta.Favorite;
        root.Q("TokenPreview").style.width = LibraryItemSize;
        root.Q("TokenPreview").style.height = LibraryItemSize;
        root.Q("TokenPreview").Add(ElementMap[meta.Hash].Item2);
    }

    private static void DeleteButtonClicked(ClickEvent evt)
    {
        Editing = false;
        UpdateVisibility();
        UI.Redraw();

        ElementMap.Remove(SelectedHash);
        Tokens.Remove(SelectedHash);
        UI.System.Q("TokenLibraryModal").Q(SelectedHash).RemoveFromHierarchy();
        Toast.AddSuccess($"Token deleted.");
        SelectedHash = null;
        WriteLibraryFile();
    }

    private static void SelectButtonClicked(ClickEvent evt)
    {
        OnSelect?.Invoke();
        Close(evt);
    }

    private static void SaveButtonClicked(ClickEvent evt)
    {
        // write file
        Editing = false;
        UpdateVisibility();
        UI.Redraw();
        WriteLibraryFile();

        UI.System.Q("TokenLibraryModal").Q(SelectedHash).Add(ElementMap[SelectedHash].Item2);
        ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
        SelectedHash = null;

    }

    private static void ChangeEditingValue()
    {
        VisualElement root = UI.System.Q("TokenLibraryModal");
        Tokens[SelectedHash].Name = root.Q<TextField>("NameField").value;
        Tokens[SelectedHash].Frames = root.Q<IntegerField>("FramesField").value;
        Tokens[SelectedHash].FPS = root.Q<IntegerField>("FpsField").value;
        Tokens[SelectedHash].Favorite = root.Q<Toggle>("FavoriteField").value;
        ElementMap[SelectedHash].Item2.Q<Label>("TokenLabel").text = Tokens[SelectedHash].Name;
        UpdateAnimation(ElementMap[SelectedHash].Item2, Tokens[SelectedHash]);
    }

    private static void UpdateVisibility()
    {
        VisualElement root = UI.System.Q("TokenLibraryModal");
        UI.ToggleDisplay(root.Q("TokenLibrary"), !Editing);
        UI.ToggleDisplay(root.Q("TokenMetaEdit"), Editing);

        UI.ToggleDisplay(root.Q("SelectButton"), AllowSelect && !Editing && SelectedHash != null);
        UI.ToggleDisplay(root.Q("DeleteButton"), Editing);
        UI.ToggleDisplay(root.Q("SaveButton"), Editing);
        UI.ToggleDisplay(root.Q("EditButton"), !Editing && SelectedHash != null);

        if (AllowSelect)
        {
            root.Q("EditButton").RemoveFromClassList("preferred");
        }
        else
        {
            root.Q("EditButton").AddToClassList("preferred");
        }
    }

    public static void ConfirmSelect(ClickEvent evt)
    {
        int count = 0;
        string directory = GetHashedImageDirectory();
        foreach (string filename in FileBrowser.Result)
        {
            count++;
            byte[] imageData = File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            texture.filterMode = FilterMode.Point;
            var tokenMeta = new TokenMeta(texture, filename, TokenSync.GetChunkCount(imageData.Length));
            File.WriteAllBytes($"{directory}/{tokenMeta.Hash}.png", imageData);
            Tokens[tokenMeta.Hash] = tokenMeta;
            AddToUI(tokenMeta);
        }
        if (count > 0)
        {
            UI.Redraw();
            WriteLibraryFile();
            Toast.AddSuccess($"{count} tokens added to the library.");
        }
    }

    public static string GetHashedImageDirectory()
    {
        string directory = $"{Preferences.Current.DataPath}/hashed-tokens";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return directory;
    }

    private static void AddToUI(TokenMeta meta)
    {
        VisualElement wrapper = new();
        wrapper.name = meta.Hash;
        wrapper.AddToClassList("wrapper");
        wrapper.style.height = LibraryItemSize;
        wrapper.style.width = LibraryItemSize;

        VisualElement tokenDisplay = new();
        tokenDisplay.name = "Item";
        tokenDisplay.AddToClassList("item");

        VisualElement frame = new();
        frame.name = "Frame";
        frame.AddToClassList("frame");

        Label label = new();
        label.name = "TokenLabel";
        label.AddToClassList("panel-text");
        label.text = meta.Name;
        label.style.backgroundColor = new Color(0, 0, 0, .5f);

        VisualElement sprite = new();
        sprite.name = "Sprite";
        sprite.AddToClassList("sprite");
        Texture2D backgroundImage = TokenSync.LoadHashedFileAsTexture(meta.Hash);
        if (backgroundImage == null)
        {
            Toast.AddError($"Could not find library image {meta.Hash}.png in the hashed-tokens directory.");
        }
        sprite.style.backgroundImage = backgroundImage;

        frame.Add(sprite);
        tokenDisplay.Add(frame);
        tokenDisplay.Add(label);
        wrapper.Add(tokenDisplay);

        tokenDisplay.RegisterCallback<ClickEvent>((evt) =>
        {
            if (SelectedHash != null && SelectedHash != meta.Hash)
            {
                // Deselect other and select this
                ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
                SelectedHash = meta.Hash;
                ElementMap[SelectedHash].Item2.Q("Item").AddToClassList("selected");
            }
            else if (SelectedHash != null && SelectedHash == meta.Hash)
            {
                // Deselect this
                ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
                SelectedHash = null;
            }
            else
            {
                // Select this
                SelectedHash = meta.Hash;
                ElementMap[SelectedHash].Item2.Q("Item").AddToClassList("selected");
            }
            UpdateVisibility();
        });

        UI.System.Q("TokenLibrary").Q("LibraryGrid").Add(wrapper);

        ElementMap.Add(meta.Hash, (meta, tokenDisplay));
        UpdateAnimation(tokenDisplay, meta);
    }

    private static void UpdateAnimation(VisualElement element, TokenMeta meta)
    {
        Texture2D graphic = element.Q("Sprite").resolvedStyle.backgroundImage.texture;
        if (graphic == null)
        {
            return;
        }
        float aspectRatio = graphic.width / meta.Frames / (float)graphic.height;
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
        element.Q("Frame").style.width = width;
        element.Q("Frame").style.height = height;
        element.Q("Sprite").style.width = width * meta.Frames;
        UI.Redraw();
    }

    private static void WriteLibraryFile()
    {
        string directory = GetHashedImageDirectory();
        string fileName = $"{directory}/library.json";
        var tokenLibraryFile = new TokenLibraryFile();
        tokenLibraryFile.Tokens = Tokens.Values.ToArray();
        string json = JsonUtility.ToJson(tokenLibraryFile);
        File.WriteAllText(fileName, json);
    }

    private static void ReadLibraryFile()
    {
        string directory = GetHashedImageDirectory();
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
}
