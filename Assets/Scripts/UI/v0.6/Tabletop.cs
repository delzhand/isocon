using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class Tabletop : MonoBehaviour
{
    void Start()
    {
        string[] strings = new string[]{
            "Stalwart", "Bastion", "Demon Slayer", "Knave", "Colossus",
            "Vagabond", "Shade", "Freelancer", "Fool", "Warden",
            "Mendicant", "Seer", "Chanter", "Sealer", "Harvester",
            "Wright", "Enochian", "Geomancer", "Spellblade", "Stormbender",
            "Heavy", "Warrior", "Soldier", "Impaler", "Greatsword", "Brute", "Knuckle", "Sentinel", "Crusher", "Berserker", "Sledge",
            "Skirmisher", "Pepperbox", "Hunter", "Fencer", "Assassin", "Hellion", "Skulk", "Shadow", "Arsonist",
            "Leader", "Errant", "Priest", "Commander", "Aburer", "Diviner", "Greenseer", "Judge", "Saint", "Cantrix",
            "Artillery", "Blaster", "Seismatist", "Storm Caller", "Rift Dancer", "Disruptor", "Chaos Wright", "Scourer", "Sapper", "Justicar", "Sniper", "Alchemist",
            "Legend", "Demolisher", "Nocturnal", "Master", "Razer",
            "Mob", "Mob",
            "Object", "Destructible",
        };

        for (int i = 0; i < strings.Length; i++) {
            Label label = new Label(strings[i]);
            label.RegisterCallback<ClickEvent>((evt) => {
                UI.System.Q<TextField>("SearchInput").SetValueWithoutNotify(label.text);
                UI.ToggleDisplay("SearchResults", false);
            });
            label.RegisterCallback<MouseEnterEvent>((evt) => {
                label.AddToClassList("selected");
            });
            label.RegisterCallback<MouseLeaveEvent>((evt) => {
                label.RemoveFromClassList("selected");
            });
            UI.System.Q("SearchResults").Add(label);
        }

        UI.System.Q<TextField>("SearchInput").RegisterCallback<FocusInEvent>((evt) => {
            UI.ToggleDisplay("SearchResults", true);
        });
        UI.System.Q<TextField>("SearchInput").RegisterCallback<FocusOutEvent>((evt) => {
            UI.ToggleDisplay("SearchResults", false);
        });
        UI.System.Q<TextField>("SearchInput").RegisterValueChangedCallback((evt) => {
            FilterElements();
        });
    }

    private void FilterElements() {
        string input = UI.System.Q<TextField>("SearchInput").value.ToLower();
        foreach (Label label in UI.System.Q("SearchResults").Children()) {
            if (!label.text.ToLower().Contains(input) && input.Length > 0) {
                UI.ToggleDisplay(label, false);
            }
            else {
                UI.ToggleDisplay(label, true);
            }
        }
    }

    void Update()
    {
        UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);
    }

    public void ConnectAsClient() {

    }

    public void ConnectAsHost() {
        TerrainController.InitializeTerrain(8, 8, 1);
    }

}
