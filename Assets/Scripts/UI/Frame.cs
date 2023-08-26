using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : MonoBehaviour
{
    void Start()
    {
        UI.ToggleDisplay("Frame", false);
        UI.SetBlocking(UI.System, new string[]{"BottomBar"});
    }

    void Update()
    {
        
    }
}
