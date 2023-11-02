using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Maleghast : GameSystem
{
    public int TurnNumber = 1;

    public override string SystemName()
    {
        return "Maleghast 666";
    }

    public override string GetTokenData()
    {
        return MaleghastTokenDataRaw.ToJson();
    }

    public override void TokenDataSetValue(TokenData data, string label, int value)
    {
        (data as MaleghastTokenData).Change(label, value);
    }

    public override void TokenDataSetValue(TokenData data, string label, string value)
    {
        (data as MaleghastTokenData).Change(label, value);
    }

    public override void GameDataSetValue(string label, int value) {
        switch (label) {
            case "TurnNumber":
                TurnNumber = value;
                UI.System.Q<Label>("TurnNumber").text = $"Turn {TurnNumber}";
                foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                    MaleghastTokenData data = g.GetComponent<MaleghastTokenData>();
                    if (data.CheckCondition("TurnEnded")) {
                        data.Change("Status", "Turn Ended|neu");
                    }
                }
                break;
        }
    }

    public override Texture2D GetGraphic(string json) {
        MaleghastTokenDataRaw raw = JsonUtility.FromJson<MaleghastTokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json) {
        g.GetComponent<MaleghastTokenData>().TokenDataSetup(json);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/MaleghastTokenData"));
    }

    public override void Setup() {
        AddTokenSetup();
        SetTypeOptions("C.A.R.C.A.S.S.");
        UI.ToggleDisplay("Icon1_5TurnInfo", true);
        UI.System.Q<Button>("NewTurnButton").RegisterCallback<ClickEvent>((evt) => {
            Player.Self().CmdRequestGameDataSetValue("TurnNumber", TurnNumber+1);
        });
    }

    public override void Teardown() {
        AddTokenTeardown();
    }

    public override void UpdateSelectedTokenPanel(GameObject data)
    {
        data.GetComponent<MaleghastTokenData>().UpdateSelectedTokenPanel();
    }

    private void AddTokenSetup() {
        VisualElement root = UI.System.Q("AddTokenSystem").Q("Maleghast");
        UI.ToggleDisplay(root, true);
        root.Q<DropdownField>("HouseDropdown").RegisterValueChangedCallback(SetTypeOptions);
    }

    private void AddTokenTeardown() {
        foreach(VisualElement child in UI.System.Q("AddTokenSystem").Children()) {
            UI.ToggleDisplay(child, false);
        }
        VisualElement root = UI.System.Q("AddTokenSystem");
        root.Q<DropdownField>("HouseDropdown").UnregisterValueChangedCallback(SetTypeOptions);
    }

    private void SetTypeOptions(ChangeEvent<string> evt) {
        SetTypeOptions(evt.newValue);
    }

    private void SetTypeOptions(string house) {
        List<string> types = GetTypes(house);
        UI.System.Q<DropdownField>("TypeDropdown").choices = types;
        UI.System.Q<DropdownField>("TypeDropdown").value = types[0];
    }

    private List<string> GetTypes(string house) {
        return house switch
        {
            "C.A.R.C.A.S.S." => new string[] {"Gunwight/Thrall", "Enforcer/Scion", "Ammo Goblin/Freak", "Barrelform/Hunter", "AEGIS Weapon/Tyrant", "Operator/Necromancer"}.ToList(),
            "Goregrinders" => new string[] {"Warhead/Thrall", "Carnifex/Scion", "Painghoul/Freak", "Painwheel/Horror", "Berserker/Tyrant", "Warlord/Necromancer"}.ToList(),
            "Gargamox" => new string[] {"Scum/Thrall", "Rotten/Scion", "Leech/Freak", "Host/Hunter", "Slime/Horror", "Plaguelord/Necromancer"}.ToList(),
            "Deadsouls" => new string[] {"Sacrifice/Thrall", "Chosen/Scion", "Vizigheist/Horror", "Banshee/Hunter", "Bound Devil/Tyrant"}.ToList(),
            "Abhorrers" => new string[] {"Penitent/Scion", "Zealot/Horror", "Antipriest/Freak", "Inquisitor/Hunter", "Holy Body/Tyrant", "Exorcist/Necromancer"}.ToList(),
            "Igorri" => new string[] {"Stitch/Thrall", "Chop Doc/Scion", "Lycan/Horror", "Strigoi/Hunter", "Homonculus/Tyrant", "Chirurgeon/Necromancer"}.ToList(),
            _ => new string[] { }.ToList(),
        };
    }
}