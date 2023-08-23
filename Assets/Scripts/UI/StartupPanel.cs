using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class StartupPanel : MonoBehaviour
{

    NetworkManager manager;

    void Awake()
    {
        manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            manager.maxConnections = 1;
            manager.StartHost();
            UI.ToggleDisplay("StartupPanel", false);
            UI.ToggleDisplay("Frame", true);
            UI.ToggleDisplay("BottomBar", true);
        });

        string system = PlayerPrefs.GetString("System", "Generic");
        GameSystem.Set(system);
        UI.System.Q<DropdownField>("SystemField").value = system; 
        UI.System.Q<DropdownField>("SystemField").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("System", evt.newValue);
            GameSystem.Set(evt.newValue);
        });
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Disconnect() {
        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Frame", false);
        UI.ToggleDisplay("BottomBar", false);
    }
}
