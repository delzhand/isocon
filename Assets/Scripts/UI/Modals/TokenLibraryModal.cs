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
    private static string SelectedHash;
    private static LibraryCallback OnSelect;

    private static string EditingHash;
    private static VisualElement EditWrapper;
    private static VisualElement PickWrapper;
    private static VisualElement ScrollWrapper;

    private static string _dialogName;

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
        _dialogName = "PrimaryDialog";
        Open();
    }

    public static void OpenSelect(LibraryCallback onSelect)
    {
        AllowSelect = true;
        OnSelect = onSelect;
        _dialogName = "SecondaryDialog";
        Open();
    }

    private static void Open()
    {
        SelectedHash = null;

        ReadLibraryFile();

        Modal2.CreateContext(_dialogName);
        var dialogContents = Modal2.Contents(_dialogName);
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
        add.clicked += () =>
        {
            FileBrowserHelper.Open(ConfirmSelect, "", FileBrowserType.Tokens);
        };
        footer.Add(add);

        Modal2.Open("Token Library");
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
                    Modal2.Dialog("SecondaryDialog").Close();
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
        return directory;
    }

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
