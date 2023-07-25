using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenState : MonoBehaviour
{
    public VisualElement overhead;
    public static bool SuppressOverheads;

    public string Color;
    public string Job;
    public string Class_;
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public int Wounds;
    public int Resolve;
    public static int GResolve;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    public int Aether;
    public int Vigilance;
    public int Blessings;

    public bool Elite;

    public List<string> Status = new List<string>();
    public List<string> OngoingStatus;
    public string Mark;
    public string Hate;


    // Start is called before the first frame update
    void Start()
    {
        createOverhead();
    }

    // Update is called once per frame
    void Update()
    {
        if (overhead != null) {
            updateScreenPosition();
            updateOverheadData();
        }
        if (TokenController.IsSelected(GetComponent<Token>())) {
            updateSelectedTokenPanel();
            updateTokenEditPanel();
        }
    }

    void OnDestroy() {
        destroyOverhead();
    }

    private void createOverhead() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/Overhead");
        VisualElement instance = template.Instantiate();
        overhead = instance.Q("Overhead");
        overhead.Q<VisualElement>("Color").AddToClassList(Color);
        overhead.Q<VisualElement>("Elite").style.visibility = Elite ? Visibility.Visible : Visibility.Hidden;
        UI.System.Add(overhead);
    }

    private void destroyOverhead() {
        if (overhead != null) {
            UI.System.Remove(overhead);
        }
    }

    private void updateScreenPosition() {
        if (SuppressOverheads) {
            overhead.style.display = DisplayStyle.None;
        }
        else {
            overhead.style.display = DisplayStyle.Flex;
            UI.FollowToken(this.GetComponent<Token>(), overhead, Camera.main, Vector2.zero, true);
        }
    }

    private void updateOverheadData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
        overhead.Q<ProgressBar>("VigorBar").value = Vigor;
        overhead.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        if (Vigor == 0) {
            overhead.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Hidden;
        }
        else {
            overhead.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Visible;
        }
        for (int i = 1; i <= 3; i++) {
            if (Wounds >= i) {
                overhead.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
            }
            else {
                overhead.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
            }
        }
    }

    private void updateSelectedTokenPanel() {
        VisualElement panel = UI.System.Q("SelectedTokenPanel");

        Texture2D image = GetComponent<Token>().Image;
        panel.Q("Portrait").style.backgroundImage = image;
        float height = 80;
        float width = 80;
        if (image.width > image.height) {
            height *= (image.height/(float)image.width);
        }
        else {
            width *= (image.width/(float)image.height);
        }
        panel.Q("Portrait").style.width = width;
        panel.Q("Portrait").style.height = height;

        panel.Q<Label>("CHP").text = CurrentHP.ToString();
        panel.Q<Label>("MHP").text = "/" + MaxHP.ToString();
        panel.Q<Label>("VIG").text = Vigor > 0 ? "+" + Vigor.ToString() : "";
        panel.Q<Label>("Name").text = name;

        panel.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = MaxHP;
        panel.Q<ProgressBar>("VigBar").value = Vigor;
        panel.Q<ProgressBar>("VigBar").highValue = MaxHP;
        if (Vigor == 0) {
            panel.Q<ProgressBar>("VigBar").style.visibility = Visibility.Hidden;
        }
        else {
            panel.Q<ProgressBar>("VigBar").style.visibility = Visibility.Visible;
        }
        for (int i = 1; i <= 3; i++) {
            if (Wounds >= i) {
                panel.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
            }
            else {
                panel.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
            }
        }

        if (!TokenController.IsFoe(Class_)) {
            panel.Q<ProgressBar>("PResolveBar").value = Resolve + GResolve;
            panel.Q<ProgressBar>("GResolveBar").value = GResolve;            
            panel.Q("ResolveWrapper").style.display = DisplayStyle.Flex;
        }
        else {
            panel.Q("ResolveWrapper").style.display = DisplayStyle.None;
        }

        if (Elite) {
            panel.Q<Label>("Elite").style.visibility = Visibility.Visible;
        }
        else {
            panel.Q<Label>("Elite").style.visibility = Visibility.Hidden;
        }

        panel.Q<Label>("Job").ClearClassList();
        panel.Q<Label>("Job").AddToClassList(Color);
        panel.Q<Label>("Job").AddToClassList("tag");
        panel.Q<Label>("Job").text = Job;

        panel.Q("Statuses").Clear();
        int statusCount = 0;
        if (CurrentHP == 0) {
            statusCount++;
            Label label = new Label("Incapacitated");
            label.AddToClassList("status");
            label.AddToClassList("neg");
            panel.Q("Statuses").Add(label);
        }
        else if (CurrentHP * 2 <= MaxHP) {
            statusCount++;
            Label label = new Label("Bloodied");
            label.AddToClassList("status");
            label.AddToClassList("neg");
            panel.Q("Statuses").Add(label);
        }
        for(int i = 0; i < Status.Count; i++) {
            statusCount++;
            Label label = new Label(Status[i]);
            label.AddToClassList("status");
            if (TokenController.IsPositive(Status[i])) {
                label.AddToClassList("pos");
            }
            else if (TokenController.IsNegative(Status[i])) {
                label.AddToClassList("neg");
            }
            panel.Q("Statuses").Add(label);
        }

        if (statusCount == 0) {
            Label label = new Label("Normal");
            label.AddToClassList("status");
            label.AddToClassList("pos");
            panel.Q("Statuses").Add(label);            
        }
    }

    private void updateTokenEditPanel() {
        VisualElement panel = UI.System.Q("EditTokenPanel");

        panel.Q<Label>("e_CurrentHP").text = CurrentHP.ToString();
        panel.Q<SliderInt>("e_CurrentHPSlider").highValue = MaxHP;

        panel.Q<Label>("e_Vigor").text = Vigor.ToString();
        panel.Q<SliderInt>("e_VigorSlider").highValue = MaxHP;
    }

}
