using System.Collections;
using System.Collections.Generic;
using IsoconUILibrary;
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
    public bool StackedDie;
    public string Stance = ""   ;

    public bool Elite;

    public List<string> Status = new List<string>();
    public string Mark = "";
    public string Hate = "";


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
            addStatus(panel, "Incapacitated", false);
        }
        else if (CurrentHP * 2 <= MaxHP) {
            statusCount++;
            addStatus(panel, "Bloodied", false);
        }
        for(int i = 0; i < Status.Count; i++) {
            statusCount++;
            addStatus(panel, Status[i], TokenController.IsPositive(Status[i]));
        }

        List<(string, int)> counters = new List<(string, int)>();
        counters.Add(("Aether", Aether));
        counters.Add(("Blessings", Blessings));
        counters.Add(("Vigilance", Vigilance));
        for(int i = 0; i < counters.Count; i++) {
            if (counters[i].Item2 > 0) {
                statusCount++;
                addStatus(panel, counters[i].Item1 + " " + counters[i].Item2, true);
            }
        }

        if (Stance.Length > 0) {
            statusCount++;
            addStatus(panel, "Stance " + Stance, true);
        }

        if (Mark.Length > 0) {
            statusCount++;
            addStatus(panel, "Marked by " + Mark, false);
        }

        if (Hate.Length > 0) {
            statusCount++;
            addStatus(panel, "Hatred of " + Hate, false);
        }

        if (statusCount == 0) {
            addStatus(panel, "Normal", true);
        }

        if (StackedDie) {
            addStatus(panel, "Stacked Die", true);
        }

        panel.Q<Label>("s_Defense").text = Defense.ToString();
        panel.Q<Label>("s_Damage").text = "D" + Damage.ToString();
        panel.Q<Label>("s_Fray").text = Fray.ToString();
        panel.Q<Label>("s_Range").text = Range.ToString();
        panel.Q<Label>("s_Speed").text = Speed.ToString();
        panel.Q<Label>("s_Dash").text = Dash.ToString();
    }

    public void SetStatus(string s) {
        if (!Status.Contains(s)) {
            Status.Add(s);
        }
    }

    public void DropStatus(string s) {
        Status.Remove(s);
    }

    private void addStatus(VisualElement v, string statusName, bool pos) {
        Label label = new Label(statusName);
        label.AddToClassList("status");
        label.AddToClassList(pos ? "pos" : "neg");
        v.Q("Statuses").Add(label);  
    }

    private void updateTokenEditPanel() {
        VisualElement panel = UI.System.Q("EditTokenPanel");

        panel.Q<Label>("e_CurrentHP").text = CurrentHP.ToString();
        panel.Q<SliderInt>("e_CurrentHPSlider").highValue = MaxHP;

        panel.Q<Label>("e_Vigor").text = Vigor.ToString();
        panel.Q<SliderInt>("e_VigorSlider").highValue = MaxHP;

        panel.Q<NumberNudger>("e_Resolve").value = Resolve;
        panel.Q<NumberNudger>("e_GResolve").value = GResolve;
    }

}
