using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShunUI;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenLibraryModal : MonoBehaviour
{
    private class TokenLibraryFile
    {
        public Token[] Tokens;
    }

    public delegate void LibraryCallback();
    public static Dictionary<string, Token> Tokens;
    public static Dictionary<string, (Token, VisualElement)> ElementMap;
    private static int LibraryItemSize = 100;
    private static bool AllowSelect = false;
    private static bool Editing = false;
    private static Token BackupMeta;
    private static string SelectedHash;
    private static LibraryCallback OnSelect;

    private static string EditingHash;
    private static VisualElement EditWrapper;
    private static VisualElement PickWrapper;
    private static VisualElement ScrollWrapper;

    public static void Setup()
    {
        Tokens = new();
        ElementMap = new();
    }

    void Update()
    {
        foreach ((Token, VisualElement) item in ElementMap?.Values)
        {
            var meta = item.Item1;
            var element = item.Item2;
            int currentFrameIndex = Mathf.FloorToInt(Time.time * meta.FPS) % meta.Frames;

            int offset = Mathf.RoundToInt(-100 * currentFrameIndex);
            element.Q("Sprite").style.left = Length.Percent(offset);
        }
    }

    public static List<string> Options()
    {
        List<string> options = new();
        ReadLibraryFile();
        foreach (string s in Tokens.Keys)
        {
            options.Add(Tokens[s].Name);
        }
        return options;
    }

    public static Token GetToken(string name)
    {
        ReadLibraryFile();
        foreach (string s in Tokens.Keys)
        {
            if (Tokens[s].Name == name)
            {
                return Tokens[s];
            }
        }
        return null;
    }

    public static void OpenDefault()
    {
        AllowSelect = false;
        Open("ShunDialog1");
    }

    public static void OpenSelect(LibraryCallback onSelect)
    {
        AllowSelect = true;
        OnSelect = onSelect;
        Open("ShunDialog2");
    }

    private static void Open(string dialogName)
    {
        SelectedHash = null;

        ReadLibraryFile();

        Modal2.SetCurrentDialog(dialogName);
        var dialogContents = Modal2.Contents(dialogName);
        dialogContents.Clear();

        Modal2.AddDialogHeader("Token Library");

        PickWrapper = new VisualElement();
        dialogContents.Add(PickWrapper);

        ScrollWrapper = Modal2.AddScrollArea("TokenLibraryModal");
        Modal2.MoveToContainer(ScrollWrapper, PickWrapper);

        EditWrapper = new ShunContainer();
        dialogContents.Add(EditWrapper);
        var editName = Modal2.AddTextField("EditName", "Token Name", null, "The name of the token");
        var editFrames = Modal2.AddIntField("EditFrames", "Frame Count", 1, "The number of animation frames, or 1 if not animated");
        var editFPS = Modal2.AddIntField("EditFPS", "FPS", 0, "How fast the animation plays, or 0 if not animated");
        Modal2.MoveToContainer(editName, EditWrapper);
        Modal2.MoveToContainer(editFrames, EditWrapper);
        Modal2.MoveToContainer(editFPS, EditWrapper);

        var saveChanges = new ShunButton();
        saveChanges.text = "Save Metadata";
        saveChanges.clicked += () =>
        {
            Tokens[EditingHash].Name = editName.Q<ShunInput>().value;
            Tokens[EditingHash].Frames = editFrames.Q<ShunIntInput>().value;
            Tokens[EditingHash].FPS = editFPS.Q<ShunIntInput>().value;
            WriteLibraryFile();
            UI.ToggleDisplay(EditWrapper, false);
            UI.ToggleDisplay(PickWrapper, true);
            SetupTokenDisplay(ElementMap[EditingHash].Item2, Tokens[EditingHash]);
        };
        EditWrapper.Add(saveChanges);

        UI.ToggleDisplay(EditWrapper, false);

        foreach (string s in Tokens.Keys)
        {
            Token token = Tokens[s];
            CreateTokenElement(token);
        }


        var footer = Modal2.AddDialogFooter("Close");
        Modal2.MoveToContainer(footer, PickWrapper);

        var add = new ShunButton();
        add.SetVariant(ButtonVariant.Secondary);
        add.text = "Add New Token";
        add.clicked += () => FileBrowserHelper.Open(ConfirmSelect, "", FileBrowserType.Tokens);
        footer.Add(add);

        Modal2.Open();
    }

    private static void UpdateFavorite(VisualElement item, Token token)
    {
        var fav = item.Q("Favorite").Q<Label>();
        fav.text = token.Favorite ? "❤" : "♡";
        fav.style.color = token.Favorite ? Color.orange : Color.white;
    }

    public static Token GetToken()
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

    public static void CreateTokenElement(Token token)
    {
        VisualElement item = UI.CreateFromTemplate("UI/TokenLibraryItem");
        Texture2D backgroundImage = TokenSync.LoadHashedFileAsTexture(token.Hash);
        item.Q("Sprite").style.backgroundImage = backgroundImage;
        item.Q<Label>("Name").text = token.Name;
        item.Q<Label>("Name").AddToClassList("shun-dialog__label");
        if (AllowSelect)
        {
            item.Q("TokenLibraryItem").AddToClassList("selectable");
            item.RegisterCallback<ClickEvent>((evt) =>
            {
                if (OnSelect != null)
                {
                    SelectedHash = token.Hash;
                    OnSelect.Invoke();
                    Modal2.Dialog("ShunDialog2").Close();
                }
            });
        }
        item.Q<Button>("Favorite").RegisterCallback<ClickEvent>((evt) =>
        {
            token.Favorite = !token.Favorite;
            UpdateFavorite(item, token);
            WriteLibraryFile();
            evt.StopPropagation();
        });
        UpdateFavorite(item, token);
        item.Q<Button>("Configure").RegisterCallback<ClickEvent>((evt) =>
        {
            UI.ToggleDisplay(EditWrapper, true);
            UI.ToggleDisplay(PickWrapper, false);
            EditingHash = token.Hash;
            EditWrapper.Q<ShunInput>("EditName").value = token.Name;
            EditWrapper.Q<ShunIntInput>("EditFrames").value = token.Frames;
            EditWrapper.Q<ShunIntInput>("EditFPS").value = token.FPS;
            evt.StopPropagation();
        });
        Modal2.MoveToScrollArea(item, ScrollWrapper);
        ElementMap[token.Hash] = (token, item);
        SetupTokenDisplay(item, token);
    }

    // public static void CancelButtonClicked(ClickEvent evt)
    // {
    //     if (Editing)
    //     {
    //         Editing = false;
    //         Tokens[SelectedHash].Name = BackupMeta.Name;
    //         Tokens[SelectedHash].Frames = BackupMeta.Frames;
    //         Tokens[SelectedHash].FPS = BackupMeta.FPS;
    //         Tokens[SelectedHash].Favorite = BackupMeta.Favorite;

    //         ElementMap[SelectedHash].Item2.Q<Label>("TokenLabel").text = Tokens[SelectedHash].Name;
    //         UI.System.Q("TokenLibraryModal").Q(SelectedHash).Add(ElementMap[SelectedHash].Item2);
    //         UpdateVisibility();
    //         UI.Redraw();
    //     }
    //     else
    //     {
    //         Close(evt);
    //     }

    //     if (SelectedHash != null)
    //     {
    //         ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
    //         SelectedHash = null;
    //     }
    // }

    // private static void EditButtonClicked(ClickEvent evt)
    // {
    //     Editing = true;
    //     UpdateVisibility();
    //     UI.Redraw();

    //     Token meta = Tokens[SelectedHash];
    //     BackupMeta = Token.Copy(meta);
    //     VisualElement root = UI.System.Q("TokenLibraryModal");
    //     root.Q<TextField>("NameField").value = meta.Name;
    //     root.Q<IntegerField>("FramesField").value = meta.Frames;
    //     root.Q<IntegerField>("FpsField").value = meta.FPS;
    //     root.Q<Toggle>("FavoriteField").value = meta.Favorite;
    //     root.Q("TokenPreview").style.width = LibraryItemSize;
    //     root.Q("TokenPreview").style.height = LibraryItemSize;
    //     root.Q("TokenPreview").Add(ElementMap[meta.Hash].Item2);
    // }

    // private static void DeleteButtonClicked(ClickEvent evt)
    // {
    //     Editing = false;
    //     UpdateVisibility();
    //     UI.Redraw();

    //     ElementMap.Remove(SelectedHash);
    //     Tokens.Remove(SelectedHash);
    //     UI.System.Q("TokenLibraryModal").Q(SelectedHash).RemoveFromHierarchy();
    //     Toast.AddSuccess($"Token deleted.");
    //     SelectedHash = null;
    //     WriteLibraryFile();
    // }

    // private static void SelectButtonClicked(ClickEvent evt)
    // {
    //     OnSelect?.Invoke();
    //     Close(evt);
    // }

    // private static void SaveButtonClicked(ClickEvent evt)
    // {
    //     // write file
    //     Editing = false;
    //     UpdateVisibility();
    //     UI.Redraw();
    //     WriteLibraryFile();

    //     UI.System.Q("TokenLibraryModal").Q(SelectedHash).Add(ElementMap[SelectedHash].Item2);
    //     ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
    //     SelectedHash = null;

    // }

    // private static void ChangeEditingValue()
    // {
    //     VisualElement root = UI.System.Q("TokenLibraryModal");
    //     Tokens[SelectedHash].Name = root.Q<TextField>("NameField").value;
    //     Tokens[SelectedHash].Frames = root.Q<IntegerField>("FramesField").value;
    //     Tokens[SelectedHash].FPS = root.Q<IntegerField>("FpsField").value;
    //     Tokens[SelectedHash].Favorite = root.Q<Toggle>("FavoriteField").value;
    //     ElementMap[SelectedHash].Item2.Q<Label>("TokenLabel").text = Tokens[SelectedHash].Name;
    //     UpdateAnimation(ElementMap[SelectedHash].Item2, Tokens[SelectedHash]);
    // }

    // private static void UpdateVisibility()
    // {
    //     VisualElement root = UI.System.Q("TokenLibraryModal");
    //     UI.ToggleDisplay(root.Q("TokenLibrary"), !Editing);
    //     UI.ToggleDisplay(root.Q("TokenMetaEdit"), Editing);

    //     UI.ToggleDisplay(root.Q("SelectButton"), AllowSelect && !Editing && SelectedHash != null);
    //     UI.ToggleDisplay(root.Q("DeleteButton"), Editing);
    //     UI.ToggleDisplay(root.Q("SaveButton"), Editing);
    //     UI.ToggleDisplay(root.Q("EditButton"), !Editing && SelectedHash != null);

    //     if (AllowSelect)
    //     {
    //         root.Q("EditButton").RemoveFromClassList("preferred");
    //     }
    //     else
    //     {
    //         root.Q("EditButton").AddToClassList("preferred");
    //     }
    // }

    public static void ConfirmSelect()
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
            var token = new Token(texture, filename, TokenSync.GetChunkCount(imageData.Length));
            File.WriteAllBytes($"{directory}/{token.Hash}.png", imageData);
            Tokens[token.Hash] = token;
            CreateTokenElement(token);
        }
        if (count > 0)
        {
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

    // private static void AddToUI(Token meta, VisualElement scrollArea)
    // {
    //     VisualElement wrapper = new();
    //     wrapper.name = meta.Hash;
    //     wrapper.AddToClassList("wrapper");
    //     wrapper.AddToClassList("token-library__item");
    //     wrapper.style.height = LibraryItemSize;
    //     wrapper.style.width = LibraryItemSize;

    //     VisualElement tokenDisplay = new();
    //     tokenDisplay.name = "Item";
    //     tokenDisplay.AddToClassList("item");

    //     VisualElement frame = new();
    //     frame.name = "Frame";
    //     frame.AddToClassList("frame");

    //     Label label = new();
    //     label.name = "TokenLabel";
    //     label.AddToClassList("panel-text");
    //     label.text = meta.Name;
    //     label.style.backgroundColor = new Color(0, 0, 0, .5f);

    //     VisualElement sprite = new();
    //     sprite.name = "Sprite";
    //     sprite.AddToClassList("sprite");
    //     Texture2D backgroundImage = TokenSync.LoadHashedFileAsTexture(meta.Hash);
    //     if (backgroundImage == null)
    //     {
    //         Toast.AddError($"Could not find library image {meta.Hash}.png in the hashed-tokens directory.");
    //     }
    //     sprite.style.backgroundImage = backgroundImage;

    //     frame.Add(sprite);
    //     tokenDisplay.Add(frame);
    //     tokenDisplay.Add(label);
    //     wrapper.Add(tokenDisplay);

    //     tokenDisplay.RegisterCallback<ClickEvent>((evt) =>
    //     {
    //         if (SelectedHash != null && SelectedHash != meta.Hash)
    //         {
    //             // Deselect other and select this
    //             ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
    //             SelectedHash = meta.Hash;
    //             ElementMap[SelectedHash].Item2.Q("Item").AddToClassList("selected");
    //         }
    //         else if (SelectedHash != null && SelectedHash == meta.Hash)
    //         {
    //             // Deselect this
    //             ElementMap[SelectedHash].Item2.Q("Item").RemoveFromClassList("selected");
    //             SelectedHash = null;
    //         }
    //         else
    //         {
    //             // Select this
    //             SelectedHash = meta.Hash;
    //             ElementMap[SelectedHash].Item2.Q("Item").AddToClassList("selected");
    //         }
    //         UpdateVisibility();
    //     });

    //     // UI.System.Q("TokenLibrary").Q("LibraryGrid").Add(wrapper);
    //     ShunDialogHelper.MoveToScrollArea(wrapper, scrollArea);

    //     ElementMap[meta.Hash] = (meta, tokenDisplay);
    //     UpdateAnimation(tokenDisplay, meta);
    // }

    private static void SetupTokenDisplay(VisualElement element, Token token)
    {
        Texture2D graphic = element.Q("Sprite").resolvedStyle.backgroundImage.texture;
        if (graphic == null)
        {
            return;
        }
        float aspectRatio = graphic.width / token.Frames / (float)graphic.height;
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
        element.Q("FrameContainer").style.width = LibraryItemSize;
        element.Q("FrameContainer").style.height = LibraryItemSize;
        element.Q("Frame").style.width = width;
        element.Q("Frame").style.height = height;
        element.Q("Sprite").style.width = width * token.Frames;
        element.Q("Sprite").style.height = height;
        element.Q<Label>("Name").text = token.Name;
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
        var sorted = from entry in tokenLibraryFile.Tokens orderby entry.Favorite descending, entry.Name ascending select entry;
        Tokens.Clear();
        foreach (var tokenMeta in sorted)
        {
            Tokens[tokenMeta.Hash] = tokenMeta;
        }
    }
}
