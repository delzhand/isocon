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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
