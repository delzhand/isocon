using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown() {
        if (UI.ClicksSuspended) {
            return;
        }
        if (ModeController.Mode == ClickMode.Play) {
            Token t = GetComponentInParent<Token>();
            TokenController.TokenClick(t);
        }
    }
}
