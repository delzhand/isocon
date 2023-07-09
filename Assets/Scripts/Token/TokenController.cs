using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenController : MonoBehaviour
{
    public static bool IsEditing;

    // Start is called before the first frame update
    void Start()
    {
        VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");

        element.Q<Button>("CloneToken").RegisterCallback<ClickEvent>((evt) => {
            GameObject newToken = GameObject.Instantiate(Token.TokenHeld.gameObject);
            newToken.name = Token.TokenHeld.name;
            ReserveSpot openReserve = ReserveSpot.LastReserveSpot();
            openReserve.PlaceAtReserveSpot(newToken.GetComponent<Token>());
        });

        element.Q<Button>("DeleteToken").RegisterCallback<ClickEvent>((evt) => {
            GameObject.Destroy(Token.TokenHeld);
            Reserve.Adjust();
        });

        element.Q<Button>("EditToken").RegisterCallback<ClickEvent>((evt) => {
            EditToggle();
        });

        InitCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        if (Token.TokenHeld) {
            VisualElement edit = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("ModifyPane");
            edit.Q<Label>("HPVal").text = Token.TokenHeld.GetComponent<HpBar>().CHP.ToString();
            edit.Q<Label>("VigVal").text = Token.TokenHeld.GetComponent<HpBar>().VIG.ToString();
            edit.Q<Label>("WoundsVal").text = Token.TokenHeld.GetComponent<HpBar>().Wounds.ToString();
            edit.Q<Label>("ResVal").text = Token.TokenHeld.GetComponent<UnitState>().Resolve.ToString();
            edit.Q<Label>("GResVal").text = UnitState.GResolve.ToString();
            edit.Q<Label>("AthVal").text = Token.TokenHeld.GetComponent<UnitState>().Aether.ToString();
            edit.Q<Label>("VglVal").text = Token.TokenHeld.GetComponent<UnitState>().Vigilance.ToString();
            edit.Q<Label>("BlsVal").text = Token.TokenHeld.GetComponent<UnitState>().Blessings.ToString();
        }
    }

    public static void InitCallbacks() {
        VisualElement edit = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("ModifyPane");

        edit.Q<TextField>("NameEdit").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.name = evt.newValue;
        });

        edit.Q<Button>("CHP-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<HpBar>().CHP++;
        });
        edit.Q<Button>("CHP-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<HpBar>().CHP--;
        });

        edit.Q<Button>("VIG-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<HpBar>().VIG++;
        });
        edit.Q<Button>("VIG-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<HpBar>().VIG--;
        });

        edit.Q<Button>("RES-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Resolve++;
        });
        edit.Q<Button>("RES-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Resolve--;
        });

        edit.Q<Button>("GRES-up").RegisterCallback<ClickEvent>((evt) => {
            UnitState.GResolve++;
        });
        edit.Q<Button>("GRES-down").RegisterCallback<ClickEvent>((evt) => {
            UnitState.GResolve--;
        });

        edit.Q<Button>("ATH-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Aether++;
        });
        edit.Q<Button>("ATH-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Aether--;
        });

        edit.Q<Button>("VGL-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Vigilance++;
        });
        edit.Q<Button>("VGL-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Vigilance--;
        });

        edit.Q<Button>("BLS-up").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Blessings++;
        });
        edit.Q<Button>("BLS-down").RegisterCallback<ClickEvent>((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Blessings--;
        });

        edit.Q<TextField>("HatredEdit").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Hatred = evt.newValue;
        });

        edit.Q<TextField>("MarkEdit").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Marked = evt.newValue;
        });

        edit.Q<Toggle>("SlashedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Slashed = evt.newValue;
        });
        edit.Q<Toggle>("BlindToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Blind = evt.newValue;
        });
        edit.Q<Toggle>("DazedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Dazed = evt.newValue;
        });
        edit.Q<Toggle>("PacifiedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Pacified = evt.newValue;
        });
        edit.Q<Toggle>("SealedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Sealed = evt.newValue;
        });
        edit.Q<Toggle>("ShatteredToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Shattered = evt.newValue;
        });
        edit.Q<Toggle>("StunnedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Stunned = evt.newValue;
        });
        edit.Q<Toggle>("WeakenedToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Weakened = evt.newValue;
        });
        edit.Q<Toggle>("VulnerableToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Vulnerable = evt.newValue;
        });
        
        edit.Q<Toggle>("CounterToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Counter = evt.newValue;
        });
        edit.Q<Toggle>("DefianceToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Defiance = evt.newValue;
        });
        edit.Q<Toggle>("DodgeToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Dodge = evt.newValue;
        });
        edit.Q<Toggle>("EvasionToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Evasion = evt.newValue;
        });
        edit.Q<Toggle>("FlyingToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Flying = evt.newValue;
        });
        edit.Q<Toggle>("PhasingToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Phasing = evt.newValue;
        });
        edit.Q<Toggle>("StealthToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Stealth = evt.newValue;
        });
        edit.Q<Toggle>("SturdyToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Sturdy = evt.newValue;
        });
        edit.Q<Toggle>("UnstoppableToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Unstoppable = evt.newValue;
        });
        edit.Q<Toggle>("RegenerationToggle").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Regeneration = evt.newValue;
        });

        edit.Q<TextField>("MarkEdit").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Marked = evt.newValue;
        });

        edit.Q<TextField>("HatredEdit").RegisterValueChangedCallback((evt) => {
            Token.TokenHeld.GetComponent<UnitState>().Hatred = evt.newValue;
        });

    }

    public static void EditToggle() {
        if (Token.TokenHeld == null) {
            return;
        }
        VisualElement edit = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("ModifyPane");
        VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");

        if (!IsEditing) {
            element.Q<Button>("EditToken").text = "DONE";
            element.Q<Button>("DeleteToken").style.display = DisplayStyle.None;
            element.Q<Button>("CloneToken").style.display = DisplayStyle.None;
            edit.style.display = DisplayStyle.Flex;
            GrabValues();
        }
        else {
            element.Q<Button>("EditToken").text = "EDIT";
            element.Q<Button>("DeleteToken").style.display = DisplayStyle.Flex;
            element.Q<Button>("CloneToken").style.display = DisplayStyle.Flex;
            edit.style.display = DisplayStyle.None;
        }

        IsEditing = !IsEditing;

    }

    public static void GrabValues() {
        VisualElement edit = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("ModifyPane");
        edit.Q<TextField>("NameEdit").value = Token.TokenHeld.name;

        edit.Q<Toggle>("SlashedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Slashed;
        edit.Q<Toggle>("BlindToggle").value = Token.TokenHeld.GetComponent<UnitState>().Blind;
        edit.Q<Toggle>("DazedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Dazed;
        edit.Q<Toggle>("PacifiedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Pacified;
        edit.Q<Toggle>("SealedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Sealed;
        edit.Q<Toggle>("ShatteredToggle").value = Token.TokenHeld.GetComponent<UnitState>().Shattered;
        edit.Q<Toggle>("StunnedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Stunned;
        edit.Q<Toggle>("WeakenedToggle").value = Token.TokenHeld.GetComponent<UnitState>().Weakened;
        edit.Q<Toggle>("VulnerableToggle").value = Token.TokenHeld.GetComponent<UnitState>().Vulnerable;
        
        edit.Q<Toggle>("CounterToggle").value = Token.TokenHeld.GetComponent<UnitState>().Counter;
        edit.Q<Toggle>("DefianceToggle").value = Token.TokenHeld.GetComponent<UnitState>().Defiance;
        edit.Q<Toggle>("DodgeToggle").value = Token.TokenHeld.GetComponent<UnitState>().Dodge;
        edit.Q<Toggle>("EvasionToggle").value = Token.TokenHeld.GetComponent<UnitState>().Evasion;
        edit.Q<Toggle>("FlyingToggle").value = Token.TokenHeld.GetComponent<UnitState>().Flying;
        edit.Q<Toggle>("PhasingToggle").value = Token.TokenHeld.GetComponent<UnitState>().Phasing;
        edit.Q<Toggle>("StealthToggle").value = Token.TokenHeld.GetComponent<UnitState>().Stealth;
        edit.Q<Toggle>("SturdyToggle").value = Token.TokenHeld.GetComponent<UnitState>().Sturdy;
        edit.Q<Toggle>("UnstoppableToggle").value = Token.TokenHeld.GetComponent<UnitState>().Unstoppable;
        edit.Q<Toggle>("RegenerationToggle").value = Token.TokenHeld.GetComponent<UnitState>().Regeneration;

        edit.Q<TextField>("HatredEdit").value = Token.TokenHeld.GetComponent<UnitState>().Hatred;
        edit.Q<TextField>("MarkEdit").value = Token.TokenHeld.GetComponent<UnitState>().Marked;
    }
}
