using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceSidebar : MonoBehaviour
{
    public int d20count = 0;
    public int d12count = 0;
    public int d100count = 0;
    public int d10count = 0;
    public int d8count = 0;
    public int d6count = 0;
    public int d4count = 0;

    void Awake() {
        UI.System.Q<Button>("DiceToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("DiceSidebar");
        });

        UI.System.Q("d20").RegisterCallback<ClickEvent>((evt) =>  {
            d20count++;
        });

        UI.System.Q("d12").RegisterCallback<ClickEvent>((evt) =>  {
            d12count++;
        });

        UI.System.Q("d100").RegisterCallback<ClickEvent>((evt) =>  {
            d100count++;
        });

        UI.System.Q("d10").RegisterCallback<ClickEvent>((evt) =>  {
            d10count++;
        });

        UI.System.Q("d8").RegisterCallback<ClickEvent>((evt) =>  {
            d8count++;
        });

        UI.System.Q("d6").RegisterCallback<ClickEvent>((evt) =>  {
            d6count++;
        });

        UI.System.Q("d4").RegisterCallback<ClickEvent>((evt) =>  {
            d4count++;
        });

        UI.System.Q<Button>("RollButton").RegisterCallback<ClickEvent>((evt) =>  {
            List<DiceRoll> rolls = new();
            for (int i = 0; i < d20count; i++) {
                rolls.Add(new DiceRoll(20));
            }
            for (int i = 0; i < d12count; i++) {
                rolls.Add(new DiceRoll(12));
            }
            for (int i = 0; i < d100count; i++) {
                rolls.Add(new DiceRoll(100));
            }
            for (int i = 0; i < d10count; i++) {
                rolls.Add(new DiceRoll(10));
            }
            for (int i = 0; i < d8count; i++) {
                rolls.Add(new DiceRoll(8));
            }
            for (int i = 0; i < d6count; i++) {
                rolls.Add(new DiceRoll(6));
            }
            for (int i = 0; i < d4count; i++) {
                rolls.Add(new DiceRoll(4));
            }
            Player.Self().CmdRequestDiceRoll(rolls.ToArray());
            reset();
        });

        UI.System.Q<Button>("DiceResetButton").RegisterCallback<ClickEvent>((evt) =>  {
            reset();
        });
    }
    void Update() {
        UI.System.Q("d20").Q<Label>("count").text = d20count.ToString();
        UI.System.Q("d12").Q<Label>("count").text = d12count.ToString();
        UI.System.Q("d100").Q<Label>("count").text = d100count.ToString();
        UI.System.Q("d10").Q<Label>("count").text = d10count.ToString();
        UI.System.Q("d8").Q<Label>("count").text = d8count.ToString();
        UI.System.Q("d6").Q<Label>("count").text = d6count.ToString();
        UI.System.Q("d4").Q<Label>("count").text = d4count.ToString();
    }

    private void reset() {
        d20count = 0;
        d12count = 0;
        d100count = 0;
        d10count = 0;
        d8count = 0;
        d6count = 0;
        d4count = 0;
    }
}
