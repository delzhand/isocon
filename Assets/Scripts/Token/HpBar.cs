using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HpBar : MonoBehaviour
{
    public VisualElement floater;
    public static bool SuppressFloaters;

    public int CHP;
    public int MHP;
    public int VIG;
    public int Wounds;
    public string Color;
    public bool Elite;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        toggleFloater();
        updateScreenPosition();
        updateData();
    }

    private void toggleFloater() {
        if (PlayerPrefs.GetInt("ShowFloaters", 1) != 0 && !SuppressFloaters) {
            if (floater == null) {
                createFloater();
            }
            floater.style.visibility = Visibility.Visible;
        }
        else {
            if (floater != null) {
                floater.style.visibility = Visibility.Hidden;
            }
        }
    }

    private void createFloater() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/TokenFloater");
        VisualElement instance = template.Instantiate();
        floater = instance.Q("Floater");
        floater.Q<VisualElement>("Color").AddToClassList(Color);
        floater.Q<VisualElement>("Elite").style.visibility = Elite ? Visibility.Visible : Visibility.Hidden;

        // UI.GameInfo.Add(floater);
    }

    public void DestroyFloater() {
        // UI.GameInfo.Remove(floater);
    }

    private void updateData() {
        floater.Q<ProgressBar>("HpBar").value = CHP;
        floater.Q<ProgressBar>("HpBar").highValue = MHP;
        floater.Q<ProgressBar>("VigorBar").value = VIG;
        floater.Q<ProgressBar>("VigorBar").highValue = MHP;
        if (VIG == 0) {
            floater.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Hidden;
        }
        else {
            floater.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Visible;
        }
        for (int i = 1; i <= 3; i++) {
            if (Wounds >= i) {
                floater.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
            }
            else {
                floater.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
            }
        }
    }

    private void updateScreenPosition() {
        // Vector3 worldPos = transform.Find("Offset/Avatar/Cutout/Cutout Quad/LabelAnchor").position;
        // Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        // if (floater.resolvedStyle.width != float.NaN) {
        //     Vector2 screenPos = new Vector2(
        //         Mathf.RoundToInt((viewportPos.x * UI.GameInfo.resolvedStyle.width)),
        //         Mathf.RoundToInt((1f - viewportPos.y) * UI.GameInfo.resolvedStyle.height)
        //     );
        //     Vector2 centerOffset = new Vector2(
        //         -floater.resolvedStyle.width/2f,
        //         0
        //     );
        //     Vector2 pos = screenPos + centerOffset;
        //     floater.style.left = pos.x;
        //     floater.style.top = pos.y;
        // }
    }


}
