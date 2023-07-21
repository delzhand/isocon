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

    public List<string> Status;
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

    }

    void OnDestroy() {
        destroyOverhead();
    }

    // private void toggleOverhead() {
    //     if (!SuppressOverheads) {
    //         if (overhead == null) {
    //             createOverhead();
    //         }
    //         overhead.style.visibility = Visibility.Visible;
    //     }
    //     else {
    //         if (overhead != null) {
    //             overhead.style.visibility = Visibility.Hidden;
    //         }
    //     }        
    // }

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
            UI.FollowToken(this.GetComponent<Token>(), overhead, Camera.main, Vector2.zero);
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
}
