using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Toast : MonoBehaviour
{
    public string Message = "";
    private float duration = 3;
    private Label label;

    // Start is called before the first frame update
    void Start()
    {
        label = new Label();
        label.text = Message;     
            UI.System.Q("Toasts").Add(label);
        UI.System.Q("Toasts").style.display = DisplayStyle.Flex;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) {
            UI.System.Q("Toasts").Remove(label);
            if (UI.System.Q("Toasts").childCount == 0) {
                UI.System.Q("Toasts").style.display = DisplayStyle.None;
            }
            Destroy(this);
        }
    }
}
