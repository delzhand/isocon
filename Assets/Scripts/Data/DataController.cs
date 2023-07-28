using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;

public enum FileOp {
    Save,
    Load
}

public class DataController : MonoBehaviour
{
    public static string currentFileName;
    public static FileOp CurrentOp;
    
    // Start is called before the first frame update
    void Start()
    {
        // registerCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool NeedFilename() {
        return DataController.currentFileName == null || DataController.currentFileName.Length == 0;
    }

    public static void SaveMap() {
        DataController.CurrentOp = FileOp.Save;
        State.SaveState(DataController.currentFileName);
        UI.SetHelpText("Map saved", HelpType.Success);
    }

    private void SaveMapCopy(ClickEvent evt) {
        DataController.CurrentOp = FileOp.Save;
        GetComponent<ModeController>().ActivateElementByName("FilenameModal");
    }

    private void LoadMap(ClickEvent evt) {
        InitializeFileList();
        GetComponent<ModeController>().ActivateElementByName("LoadFileModal");
    }

    private void ResetMap(ClickEvent evt) {
        DataController.currentFileName = null;
        TerrainController.ResetTerrain();
        GetComponent<MenuController>().Clear();
    }

    public void FilenameConfirm(ClickEvent evt) {    
        string value = UI.System.Q<TextField>("Filename").value;
        if (value.Length > 0) {
            DataController.currentFileName = value;
            State.SaveState(value);
            GetComponent<ModeController>().DeactivateByName("FilenameModal");
        }
        else {
            UI.SetHelpText("Filename cannot be empty.", HelpType.Error);
        }
    }

    public void FilenameCancel(ClickEvent evt) {    
        GetComponent<ModeController>().DeactivateByName("FilenameModal");
    }

    public void InitializeFileList() {
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/maps/");
        FileInfo[] fileInfo = info.GetFiles();
        List<string> items = new List<string>();
        for (int i = 0; i < fileInfo.Length; i++) {
            items.Add(fileInfo[i].Name);
        }        
        Func<VisualElement> makeItem = () => new Label();
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = items[i];
        const int itemHeight = 16;
        ListView lv = UI.System.Q("FileSelectListView") as ListView;
        lv.makeItem = makeItem;
        lv.bindItem = bindItem;
        lv.fixedItemHeight = itemHeight;
        lv.itemsSource = items;
        lv.itemsChosen += OnItemsChosen;
        lv.selectionChanged += OnSelectionChanged;
    }

    public void OnItemsChosen(IEnumerable<object> objects)
    {
        foreach (string s in objects) {
            currentFileName = s;
            State.LoadState(s);
            GetComponent<MenuController>().Clear();
            return;
        }
    }

    // Custom event handler for selectionChanged
    public void OnSelectionChanged(IEnumerable<object> objects)
    {
    }

}
