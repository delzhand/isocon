using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitState : MonoBehaviour
{

    public bool Focused = false;

    public bool Foe;
    public string Color;
    public string Job;
    public int Resolve;
    public static int GResolve;
    public int Damage;
    public int Fray;
    public int Range;
    public int Defense;
    public int Speed;
    public int Dash;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Focused) {
            // updateScreenPosition();
            updateData();
        }
    }

    private void updateData() {
        HpBar hp = GetComponent<HpBar>();
        VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");
        element.Q<Label>("CHP").text = hp.CHP.ToString();
        element.Q<Label>("MHP").text = "/" + hp.MHP.ToString();
        element.Q<Label>("VIG").text = hp.VIG > 0 ? "+" + hp.VIG.ToString() : "";
        element.Q<Label>("Name").text = name;

        element.Q<ProgressBar>("HpBar").value = hp.CHP;
        element.Q<ProgressBar>("HpBar").highValue = hp.MHP;
        element.Q<ProgressBar>("VigBar").value = hp.VIG;
        element.Q<ProgressBar>("VigBar").highValue = hp.MHP;
        if (hp.VIG == 0) {
            element.Q<ProgressBar>("VigBar").style.visibility = Visibility.Hidden;
        }
        else {
            element.Q<ProgressBar>("VigBar").style.visibility = Visibility.Visible;
        }
        // for (int i = 1; i <= 3; i++) {
        //     if (hp.Wounds >= i) {
        //         element.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
        //     }
        //     else {
        //         element.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
        //     }
        // }

        if (!Foe) {
            element.Q<ProgressBar>("PResolveBar").value = Resolve + GResolve;
            element.Q<ProgressBar>("GResolveBar").value = GResolve;            
            element.Q("ResolveWrapper").style.display = DisplayStyle.Flex;
        }
        else {
            element.Q("ResolveWrapper").style.display = DisplayStyle.None;
        }

        if (hp.Elite) {
            element.Q<Label>("Elite").style.visibility = Visibility.Visible;
        }
        else {
            element.Q<Label>("Elite").style.visibility = Visibility.Hidden;
        }

        element.Q<Label>("Tag").ClearClassList();
        element.Q<Label>("Tag").AddToClassList(hp.Color);
        element.Q<Label>("Tag").text = Job;

        element.Q<Label>("DMG").text = "D" + Damage;
        element.Q<Label>("FRAY").text = Fray.ToString();
        element.Q<Label>("RNG").text = Range.ToString();
        element.Q<Label>("SPD").text = Speed + "/" + Dash;
        element.Q<Label>("DEF").text = Defense.ToString();

    }

    private void updateScreenPosition() {
        VisualElement container = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement;
        VisualElement element = container.Q("FocusToken");
        
        Vector3 worldPos = transform.Find("Offset/Avatar/Cutout/Cutout Quad/LabelAnchor").position;        
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector2 screenPos = new Vector2(
            Mathf.RoundToInt((viewportPos.x * container.resolvedStyle.width)),
            Mathf.RoundToInt((1f - viewportPos.y) * container.resolvedStyle.height)
        );
        Vector2 pos = screenPos;
        element.style.left = pos.x;
        element.style.top = pos.y;
    }

}
