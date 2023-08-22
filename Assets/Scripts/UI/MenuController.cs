using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{

    private bool open = false;

    // Start is called before the first frame update
    void Start()
    {
        registerCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"MainMenuToggle", "MainMenu", "ModalContainer"});

        UI.System.Q("MainMenuToggle").RegisterCallback<ClickEvent>((evt) => {
            if (open) {
                closeMenu();
            }
            else {
                openMenu();
            }
        });

        UI.System.Query<Button>(null, "menu-button").ForEach(registerButton);
        UI.System.Query<Button>(null, "modal-button").ForEach(registerButton);
        
        UI.System.Q<Label>("DataPath").text = "Custom tokens should be placed in " + Application.persistentDataPath + "/tokens/";

    }

    public void Clear() {
        closeMenu();
        disableModal();
    }

    private void openMenu() {
        open = true;
        UI.System.Q("MainMenu").AddToClassList("active");
    }

    private void closeMenu() {
        open = false;
        disableSubmenu();
        UI.System.Query<Button>(null, "menu-button").ForEach(disableButton);
        UI.System.Q("MainMenu").RemoveFromClassList("active");
    }

    private void registerButton(Button button) {
        button.clickable.clickedWithEventInfo += buttonClick;
    }

    private void buttonClick(EventBase obj) {
        UI.System.Query<Button>(null, "menu-button").ForEach(disableButton);

        Button button = (Button)obj.target;
        button.AddToClassList("active");
        switch (button.name) {
            case "FileButton":
                disableSubmenu();
                enableSubmenu("FileMenu");
                break;
            case "ConfigButton":
                disableSubmenu();
                enableSubmenu("ConfigMenu");
                break;
            case "OpenMapButton":
                GetComponent<DataController>().InitializeFileList();
                enableModal("LoadFileDialog");
                break;
            case "LoadFileCancelButton":
                disableModal();
                break;
            case "SaveMapButton":
                if (DataController.NeedFilename()) {
                    enableModal("SaveFileDialog");
                }
                else {
                    DataController.SaveMap();
                }
                break;
            case "SaveMapAsButton":
                enableModal("SaveFileDialog");
                break;
            case "SaveFileConfirmButton":
                DataController.currentFileName = UI.System.Q<TextField>("FilenameTextfield").value;
                DataController.SaveMap();
                break;
            case "SaveFileCancelButton":
                disableModal();
                break;
            case "ResetMapButton":
                DataController.currentFileName = null;
                TerrainController.ResetTerrain();
                closeMenu();
                break;
            case "ExitButton":
                Application.Quit();
                break;
        }
    }

    private void disableButton(Button button) {
        button.RemoveFromClassList("active");
    }

    private void enableSubmenu(string name) {
        UI.System.Q(name).style.display = DisplayStyle.Flex;
    }

    private void disableSubmenu(string name = null) {
        if (name == null) {
            string[] menus = new string[]{"FileMenu", "ConfigMenu"};
            foreach(string s in menus) {
                disableSubmenu(s);
            }
        }
        else {
            UI.System.Q(name).style.display = DisplayStyle.None;
        }

    }

    private void enableModal(string dialog) {
        // TokenState.SuppressOverheads = true;
        UI.System.Query(null, "dialog").ForEach(disableDialog);
        UI.System.Q("ModalContainer").AddToClassList("active");
        UI.System.Q(dialog).style.display = DisplayStyle.Flex;
    }

    private void disableModal() {
        // TokenState.SuppressOverheads = false;
        UI.System.Q("ModalContainer").RemoveFromClassList("active");
    }

    private void disableDialog(VisualElement dialog) {
        dialog.style.display = DisplayStyle.None;
    }

    public void EnableModal(string dialog) {
        enableModal(dialog);
    }

}