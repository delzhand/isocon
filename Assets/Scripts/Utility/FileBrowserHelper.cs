using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum FileBrowserType
{
    Sessions,
    Rules,
    Tokens,
    Maps,
    Maleghast
}

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

    public static void Open(EventCallback<ClickEvent> onSelect, string fieldName, FileBrowserType type, bool saveOp = false)
    {
        string filterName = "Sessions";
        string filterExt = ".json";
        string title = "Select a Session File";
        string directory = "sessions";
        bool multiple = false;
        if (type == FileBrowserType.Maleghast)
        {
            filterName = "MaleghastData";
            filterExt = ".json";
            title = "Select a Maleghast Data File";
            directory = "maleghast_data";
            multiple = false;
        }
        else if (type == FileBrowserType.Tokens)
        {
            filterName = "Images";
            filterExt = ".png";
            title = "Add Tokens to Library";
            directory = "tokens";
            multiple = true;
        }
        else if (type == FileBrowserType.Maps)
        {
            filterName = "Maps";
            filterExt = ".json";
            title = "Select a Map File";
            directory = "maps";
            multiple = false;
        }
        FieldOrigin = fieldName;
        FileBrowser.SetFilters(true, new FileBrowser.Filter(filterName, filterExt));
        FileBrowser.SetDefaultFilter(filterExt);
        Find().StartCoroutine(Find().ShowDialogCoroutine(saveOp, FileBrowser.PickMode.Files, multiple, $"{Preferences.Current.DataPath}/{directory}", null, title, saveOp ? "Save" : "Select", onSelect, null));
    }

    // public static void OpenLoadRulesBrowser(EventCallback<ClickEvent> onSelect, string fieldName)

    // {
    //     FieldOrigin = fieldName;
    //     Find().LoadRulesBrowser(onSelect);
    // }

    // private void LoadRulesBrowser(EventCallback<ClickEvent> onSelect)
    // {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter("Rules", ".json"));
    //     FileBrowser.SetDefaultFilter(".json");
    //     StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/ruledata", null, "Select a Rule File", "Select", onSelect, null));
    // }

    // public static void OpenLoadSessionsBrowser(EventCallback<ClickEvent> onSelect, string fieldName)

    // {
    //     FieldOrigin = fieldName;
    //     Find().LoadSessionsBrowser(onSelect);
    // }

    // private void LoadSessionsBrowser(EventCallback<ClickEvent> onSelect)
    // {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter("Sessions", ".json"));
    //     FileBrowser.SetDefaultFilter(".json");
    //     StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/sessions", null, "Select a Session File", "Select", onSelect, null));
    // }


    // public static void OpenLoadTokenBrowser()
    // {
    //     Find().LoadTokenBrowser();
    // }

    // private void LoadTokenBrowser()
    // {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
    //     FileBrowser.SetDefaultFilter(".png");
    //     StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, true, $"{Preferences.Current.DataPath}/tokens", null, "Add Tokens to Library", "Select", TokenLibrary.ConfirmSelect, null));
    // }

    // public static void OpenSaveMapBrowser(string fileName)
    // {
    //     Find().SaveMapBrowser(fileName);
    // }

    // private void SaveMapBrowser(string fileName)
    // {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
    //     FileBrowser.SetDefaultFilter(".json");
    //     StartCoroutine(ShowDialogCoroutine(true, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/maps", fileName, "Save Map", "Save", MapEdit.ConfirmMapSave, null));
    // }

    // public static void OpenLoadMapBrowser()
    // {
    //     Find().LoadMapBrowser();
    // }

    // private void LoadMapBrowser()
    // {
    //     FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
    //     FileBrowser.SetDefaultFilter(".json");
    //     StartCoroutine(ShowDialogCoroutine(false, FileBrowser.PickMode.Files, false, $"{Preferences.Current.DataPath}/maps", null, "Load Map", "Load", MapEdit.ConfirmMapOpen, null));
    // }

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
    }
}
