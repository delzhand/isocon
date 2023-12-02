using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LifeEditPanel : MonoBehaviour
{
    private static Icon_v1_5TokenData Data;

    // Start is called before the first frame update
    void Start()
    {
        UI.SetBlocking(UI.System, new string[]{"Icon1_5LifeEdit"});
        UI.System.Q<IntegerField>("v_VigDiff").RegisterValueChangedCallback<int>((evt) => {
            int hpDiff = UI.System.Q<IntegerField>("v_HpDiff").value;
            int vigDiff = UI.System.Q<IntegerField>("v_VigDiff").value;
            UI.System.Q<Label>("v_BulkDiff").text = $"{hpDiff + vigDiff}";
            UI.System.Q<Label>("v_NewBulk").text = $"({Data.Vigor + Data.CurrentHP - hpDiff - vigDiff})";
            UI.System.Q<Label>("v_NewVig").text = $"{Data.Vigor - vigDiff}";
        });
        UI.System.Q<IntegerField>("v_HpDiff").RegisterValueChangedCallback<int>((ext) => {
            int hpDiff = UI.System.Q<IntegerField>("v_HpDiff").value;
            int vigDiff = UI.System.Q<IntegerField>("v_VigDiff").value;
            UI.System.Q<Label>("v_BulkDiff").text = $"({hpDiff + vigDiff})";
            UI.System.Q<Label>("v_NewBulk").text = $"({Data.Vigor + Data.CurrentHP - hpDiff - vigDiff})";
            UI.System.Q<Label>("v_NewHp").text = $"{Data.CurrentHP - hpDiff}";
        });
        UI.System.Q<Button>("v_ApplyButton").RegisterCallback<ClickEvent>((evt) => {
            int hpDiff = UI.System.Q<IntegerField>("v_HpDiff").value;
            int vigDiff = UI.System.Q<IntegerField>("v_VigDiff").value;
            if (vigDiff != 0) {
                // Player.Self().CmdRequestTokenDataSetValue(Data, "Vigor", Data.Vigor - vigDiff);
            }
            if (hpDiff != 0) {
                if (vigDiff != 0) {
                    // To avoid overlapping popups, find a way to delay this by 2s
                    // Player.Self().CmdRequestTokenDataSetValue(Data, "CurrentHP", Data.CurrentHP - hpDiff);
                }
                else {
                    // Player.Self().CmdRequestTokenDataSetValue(Data, "CurrentHP", Data.CurrentHP - hpDiff);
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        // UI.ToggleDisplay("Icon1_5LifeEdit", TokenMenu.ActiveMenuItem == "AlterHP");
    }

    public static void Show(TokenData data) {
        SetPosition();
        Data = data as Icon_v1_5TokenData;
        SyncValues();
    }

    public static void SyncValues() {
        if (Data != null) {
            UI.System.Q<Label>("v_CurrentVig").text = $"{Data.Vigor}";
            UI.System.Q<Label>("v_NewVig").text = $"{Data.Vigor}";
            UI.System.Q<IntegerField>("v_VigDiff").value = 0;

            UI.System.Q<Label>("v_CurrentHp").text = $"{Data.CurrentHP}";
            UI.System.Q<Label>("v_NewHp").text = $"{Data.CurrentHP}";
            UI.System.Q<IntegerField>("v_HpDiff").value = 0;

            UI.System.Q<Label>("v_CurrentBulk").text = $"({Data.Vigor + Data.CurrentHP})";
            UI.System.Q<Label>("v_BulkDiff").text = $"({0})";
            UI.System.Q<Label>("v_NewBulk").text = $"({Data.Vigor + Data.CurrentHP})";
        }        
    }

    private static void SetPosition() {
        IResolvedStyle menuStyle = UI.System.Q("UnitMenu").resolvedStyle;
        UI.System.Q("Icon1_5LifeEdit").style.left = menuStyle.left + menuStyle.width + 4;
        UI.System.Q("Icon1_5LifeEdit").style.bottom = 80;
    }
}
