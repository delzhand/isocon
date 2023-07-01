using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Token : MonoBehaviour
{
    public static Token TokenHeld = null;

    public VisualElement container;
    public VisualElement tokenDisplay;

    public string ActorName = "Test";
    public string ActorJob = "Enochian";
    public string ActorClass = "wright";
    public bool IsEnemy = false;
    public int CHP = 24;
    public int MHP = 32;
    public int VIG = 8;
    public int Wounds = 1;
    public bool InReserve = true;

    // Start is called before the first frame update
    void Start()
    {
        container = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement;
        CreateLabelUI();
    }

    // Update is called once per frame
    void Update()
    {
        alignToCamera();
        updateScreenPosition();
        updateData();
    }

    private void CreateLabelUI()
    {
        VisualTreeAsset template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Templates/TokenDisplay.uxml");
        VisualElement instance = template.Instantiate();
        tokenDisplay = instance.Q("TokenDisplay");
        container.Add(tokenDisplay);

        // if (ActorName == "Gradis") {
        //     template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Templates/TokenPanel.uxml");
        //     instance = template.Instantiate();
        //     VisualElement tokenPanel = instance.Q("TokenPanel");
        //     container.Add(tokenPanel);
        //     tokenPanel.style.top = 100;
        //     tokenPanel.style.left = 30;
        // }

    }

    private void alignToCamera() {
        if (InReserve) {
            Transform camera = GameObject.Find("ReserveCamera").transform;
            transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 180, 0);
        }
        else {
            Transform camera = GameObject.Find("CameraOrigin").transform;
            transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
        }
    }

    void OnMouseDown()
    {
        switch (ModeController.GetMode()) {
            case Mode.View:
                Debug.Log(name + " clicked");
                TokenHeld = this;
                break;
        }
    }

    public void PlaceAtBlock(Block block) {
        transform.position = block.transform.position + new Vector3(0, .25f, 0);
        InReserve = false;
        TokenHeld = null;
    }

    private void updateData() {
        tokenDisplay.Q<Label>("Name").text = ActorName;
        tokenDisplay.Q<Label>("Job").text = ActorJob;
        tokenDisplay.Q<Label>("Job").AddToClassList(ActorClass);
        tokenDisplay.Q<Label>("CHP").text = CHP.ToString();
        tokenDisplay.Q<Label>("MHP").text = "/" + MHP.ToString();
        if (VIG > 0) {
            tokenDisplay.Q<Label>("VIG").text = "+" + VIG;
        }
        else {
            tokenDisplay.Q<Label>("VIG").text = "";
        }
        tokenDisplay.Q<ProgressBar>("HpBar").value = CHP;
        tokenDisplay.Q<ProgressBar>("HpBar").highValue = MHP;
        tokenDisplay.Q<ProgressBar>("VigorBar").value = VIG;
        tokenDisplay.Q<ProgressBar>("VigorBar").highValue = MHP;
        if (VIG == 0) {
            tokenDisplay.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Hidden;
        }
        else {
            tokenDisplay.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Visible;
        }
        for (int i = 1; i <= 3; i++) {
            if (Wounds >= i) {
                tokenDisplay.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
            }
            else {
                tokenDisplay.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
            }
        }
        
        if (IsEnemy) {
            tokenDisplay.AddToClassList("enemy");
        }
        else {
            tokenDisplay.RemoveFromClassList("enemy");
        }
    }

    private void updateScreenPosition() {
        if (tokenDisplay == null) {
            return;
        }
        Vector3 worldPos = transform.Find("Actor/Chibi/Cutout Quad/LabelAnchor").position;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        if (tokenDisplay.resolvedStyle.width != float.NaN) {
            Vector2 screenPos = new Vector2(
                Mathf.RoundToInt((viewportPos.x * container.resolvedStyle.width)),
                Mathf.RoundToInt((1f - viewportPos.y) * container.resolvedStyle.height)
            );
            Vector2 centerOffset = new Vector2(
                -tokenDisplay.resolvedStyle.width/2f,
                0
            );
            Vector2 pos = screenPos + centerOffset;
            tokenDisplay.style.left = pos.x;
            tokenDisplay.style.top = pos.y;
        }
    }

}
