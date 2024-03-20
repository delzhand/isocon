using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FileBrowserHelper : MonoBehaviour
{
    public static string[] FileNames;
    public static string FieldOrigin;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private static FileBrowserHelper Find()
    {
        return GameObject.Find("AppState").GetComponent<FileBrowserHelper>();
    }

    public static void OpenLoadRulesBrowser(EventCallback<ClickEvent> onSelect, string fieldName)

    {
        FieldOrigin = fieldName;
        Find().LoadRulesBrowser(onSelect);
    }

    private void LoadRulesBrowser(EventCallback<ClickEvent> onSelect)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Rules", ".json"));
        FileBrowser.SetDefaultFilter(".json");
        StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/tokens", null, "Select a Rule File", "Select", onSelect, null));
    }

    public static void OpenLoadTokenBrowser()
    {
        Find().LoadTokenBrowser();
    }

    private void LoadTokenBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
        FileBrowser.SetDefaultFilter(".png");
        StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, true, $"{Preferences.Current.DataPath}/tokens", null, "Add Tokens to Library", "Select", TokenLibrary.ConfirmSelect, null));
    }

    public static void OpenSaveMapBrowser(string fileName)
    {
        Find().SaveMapBrowser(fileName);
    }

    private void SaveMapBrowser(string fileName)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
        FileBrowser.SetDefaultFilter(".json");
        StartCoroutine(ShowDialogCoroutine(true, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/maps", fileName, "Save Map", "Save", MapEdit.ConfirmMapSave, null));
    }

    public static void OpenLoadMapBrowser()
    {
        Find().LoadMapBrowser();
    }

    private void LoadMapBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
        FileBrowser.SetDefaultFilter(".json");
        StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/maps", null, "Load Map", "Load", MapEdit.ConfirmMapOpen, null));
    }

    IEnumerator ShowDialogCoroutine(bool saveOp, FileBrowser.PickMode pickMode, bool multiple, string dir, string file, string title, string confirmLabel, EventCallback<ClickEvent> success, EventCallback<ClickEvent> cancel)
    {
        if (saveOp)
        {
            yield return FileBrowser.WaitForSaveDialog(pickMode, multiple, dir, file, title, confirmLabel);
        }
        else
        {
            yield return FileBrowser.WaitForLoadDialog(pickMode, multiple, dir, file, title, confirmLabel);
        }

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        if (FileBrowser.Success)
        {
            success?.Invoke(new ClickEvent());
        }
        else
        {
            cancel?.Invoke(new ClickEvent());
        }

        // if (FileBrowser.Success)
        // {
        //     FileNames = FileBrowser.Result;
        //     // // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
        //     // for (int i = 0; i < FileBrowser.Result.Length; i++)
        //     // {
        //     //     MapEdit.FileBrowserFileName = FileBrowser.Result[i];
        //     // }

        //     // Read the bytes of the first file via FileBrowserHelpers
        //     // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
        //     byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

        //     // Or, copy the first file to persistentDataPath
        //     string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
        //     FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        // }
    }
}
