using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");

        (element.Q("CloneToken") as Button).RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("Clone " + Token.TokenHeld.name);
        });

        (element.Q("DeleteToken") as Button).RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("Delete " + Token.TokenHeld.name);
        });

        (element.Q("EditToken") as Button).RegisterCallback<ClickEvent>((evt) => {
            Debug.Log("Edit " + Token.TokenHeld.name);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
