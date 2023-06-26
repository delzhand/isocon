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
    public static FileOp currentOp;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void InitializeFileList() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] fileInfo = info.GetFiles();
        List<string> items = new List<string>();
        for (int i = 0; i < fileInfo.Length; i++) {
            items.Add(fileInfo[i].Name);
        }        

        // The "makeItem" function is called when the
        // ListView needs more items to render.
        Func<VisualElement> makeItem = () => new Label();

        // As the user scrolls through the list, the ListView object
        // recycles elements created by the "makeItem" function,
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list).
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = items[i];

        // Provide the list view with an explict height for every row
        // so it can calculate how many items to actually display
        const int itemHeight = 16;

        // var listView = new ListView(items, itemHeight, makeItem, bindItem);

        // listView.selectionType = SelectionType.Multiple;

        // listView.onItemsChosen += objects => Debug.Log(objects);
        // listView.onSelectionChange += objects => Debug.Log(objects);

        // listView.style.flexGrow = 1.0f;

        ListView lv = modeUI.rootVisualElement.Q("FileList") as ListView;
        lv.makeItem = makeItem;
        lv.bindItem = bindItem;
        lv.fixedItemHeight = itemHeight;
        lv.itemsSource = items;
        lv.itemsChosen += OnItemsChosen;
        lv.selectionChanged += OnSelectionChanged;
    }

    public static void OnItemsChosen(IEnumerable<object> objects)
    {
        foreach (string s in objects) {
            currentFileName = s;
            State.LoadState(s);
            ModeController.CloseModal("LoadFileModal");
            return;
        }
    }

    // Custom event handler for selectionChanged
    public static void OnSelectionChanged(IEnumerable<object> objects)
    {
    }

}
