using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Keyboard : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Modal.Close();
        }

        if (Input.GetKeyUp(KeyCode.Return) && Modal.IsOpen()) {
            Modal.Activate();
        }
    }
}
