using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public static bool IsPointerOverUI(Vector2 screenPos) {
        GameObject uiObject = GameObject.Find("CameraUI");
        if (uiObject == null) {
            Debug.Log("fail");
            return false;
        }
        UIDocument ui = uiObject.GetComponent<UIDocument>();
        Vector2 pointerUiPos = new Vector2{ x = screenPos.x , y = Screen.height - screenPos.y };
        List<VisualElement> picked = new List<VisualElement>();
        ui.rootVisualElement.panel.PickAll( pointerUiPos , picked );
        foreach( var ve in picked )
        if( ve!=null )
        {
            Color32 bcol = ve.resolvedStyle.backgroundColor;
            if( bcol.a!=0 && ve.enabledInHierarchy )
                return true;
        }
        return false;        
    }
}
