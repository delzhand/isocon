using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class MaleghastTokenDataRaw
{
    public string Name;
    public string House;
    public string UnitType;
    public int Size;
    public string GraphicHash;


    public static string ToJson() {
        MaleghastTokenDataRaw raw = new MaleghastTokenDataRaw();

        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        raw.Name = nameField.value;

        DropdownField houseField = UI.System.Q<DropdownField>("HouseDropdown");
        raw.House = houseField.value;

        DropdownField unitTypeField = UI.System.Q<DropdownField>("TypeDropdown");
        raw.UnitType = unitTypeField.value;

        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = TextureSender.CopyLocalImage(graphicField.value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);

        raw.Size = 1;
        if (raw.UnitType.Contains("Tyrant")) {
            raw.Size = 2;
        }

        return JsonUtility.ToJson(raw);
    }
}

public class MaleghastTokenData : TokenData
{
    [SyncVar]
    public string House;

    [SyncVar]
    public string UnitType;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int Soul;

    [SyncVar]
    public string Armor;

    public int MaxHP;

    public int Move;
    public int Defense;

    public int Size;

    public string ActAbilities;
    public string SoulAbilities;
    public string Upgrades;
    public string Traits;

    public int Strength;
    public int Vitality;
    public int Speed;
    public bool Loaded = true;

    public bool TurnEnded;

    void Update()
    {
        BaseUpdate();
    }

    public override bool NeedsSetup()
    {
        return MaxHP == 0;
    }

    public override void UpdateUIData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
        UI.ToggleDisplay(overhead.Q<ProgressBar>("VigorBar"), false);
        for (int i = 1; i <= 3; i++) {
            UI.ToggleDisplay(overhead.Q("Wound" + i), false);            
        }
    }

    public override void TokenDataSetup(string json, string id) {
        base.TokenDataSetup(json, id);
        DoTokenDataSetup();
        CurrentHP = MaxHP;
    }

    public override void DoTokenDataSetup() {
        MaleghastTokenDataRaw raw = JsonUtility.FromJson<MaleghastTokenDataRaw>(Json);
        Name = raw.Name;
        GraphicHash = raw.GraphicHash;
        House = raw.House;
        UnitType = raw.UnitType;
        Size = raw.Size;
        SetStats(raw.UnitType);
    }

    public override void CreateWorldToken() {
        base.CreateWorldToken();    
        Color c = ClassColor();
        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        m.SetColor("_Border", c);
        TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
    }

    public override void CreateUnitBarItem() {
        base.CreateUnitBarItem();
        Color c = ClassColor();
        Element.Q("ClassBackground").style.borderTopColor = c;
        Element.Q("ClassBackground").style.borderRightColor = c;
        Element.Q("ClassBackground").style.borderBottomColor = c;
        Element.Q("ClassBackground").style.borderLeftColor = c;
    }

    public override int GetSize()
    {
        return Size;
    }

    private Color ClassColor() {
        return House switch
        {
            "C.A.R.C.A.S.S." => Environment.FromHex("#f20dae"),
            "Goregrinders" => Environment.FromHex("#ff7829"),
            "Gargamox" => Environment.FromHex("#29ff3d"),
            "Deadsouls" => Environment.FromHex("#90ffef"),
            "Abhorrers" => Environment.FromHex("#ffc93b"),
            "Igorri" => Environment.FromHex("#a000ff"),
            _ => throw new Exception(),
        };
    }

    public void Change(string label, int value) {
        FileLogger.Write($"{Name} {label} set to {value}");
        int originValue;
        switch(label) {
            default:
                FileLogger.Write($"Invalid label '{label}' for int value change");
                throw new Exception($"Invalid label '{label}' for int value change");
        }

        if (originValue == value) {
            return;
        }
        // PopoverText.Create(TokenObject.GetComponent<Token>(), $"{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}{shortLabel}", ChangeColor(value, originValue));
        TokenEditPanel.SyncValues();
        LifeEditPanel.SyncValues();
        UpdateSelectedTokenPanel();
    }

    public void Change(string label, string value) {
        FileLogger.Write($"{Name} {label} set to {value}");
        switch(label) {
            case "Status":
                // cut out some conditions because this is only used for Turn Ended in maleghast
                if (TurnEnded) {
                    TurnEnded = false;
                    PopoverText.Create(TokenObject.GetComponent<Token>(), $"=TURN RESET", Color.white);
                    Element.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = Color.white;
                }
                else {
                    TurnEnded = true;
                    PopoverText.Create(TokenObject.GetComponent<Token>(), $"=TURN ENDED", Color.white);
                    Element.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = ColorSidebar.FromHex("#505050");
                }
                break;
            default:
                FileLogger.Write($"Invalid label '{label}' for string value change");
                throw new Exception($"Invalid label '{label}' for string value change");
        }

        TokenEditPanel.SyncValues();
        UpdateSelectedTokenPanel();
    }

    private void SetStats(string unitType) {
        switch (unitType) {
            case "Gunwight/Thrall":
                Move = 2;
                MaxHP = 2;
                Defense = 4;
                Armor = "";
                Traits = "Formation,Thrall";
                ActAbilities = "OL45,Baton";
                Upgrades = "Brace,Tactical Reload,Scavenge Ammo";
                break;
            case "Enforcer/Scion":
                Move = 3;
                MaxHP = 4;
                Defense = 3;
                Armor = "ARMOR";
                Traits = "Formation";
                ActAbilities = "Skull Crack,Flashbang,Shieldwall";
                Upgrades = "Shield Charge,Bulwark,Bone Dust Napalm";
                break;
            case "Ammo Goblin/Freak":
                Move = 3;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Formation";
                ActAbilities = "Regurgitate Ammo,Bone Shards,Destructive Glee";
                Upgrades = "Vomit Bullets,Napalm Injector,Hot Chamber";
                break;
            case "Barrelform/Hunter":
                Move = 2;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Formation";
                ActAbilities = "Snipe,Transform To Gun,Deathmark";
                Upgrades = "Claw Pitons,Extended Barrel,Caliber Up";
                break;
            case "Aegis Weapon/Tyrant":
                Move = 3;
                MaxHP = 6;
                Defense = 4;
                Armor = "ARMOR";
                Traits = "Formation,Siege Shield,Tyrant";
                ActAbilities = "Juggernaut,Mortar,Catechism Devil Cannon";
                Upgrades = "Fortify,Heavy Caliber Cannon,Gunner Pivot";
                break;
            case "Operator/Necromancer":
                Move = 4;
                MaxHP = 8;
                Defense = 4;
                Armor = "ARMOR";
                Traits = "Formation,Hot Clip";
                ActAbilities = "Akimbo";
                SoulAbilities = "Reload Slide,Bullet Time";
                break;
            case "Warhead/Thrall":
                Move = 4;
                MaxHP = 2;
                Defense = 3;
                Armor = "";
                Traits = "Blood Rage,Thrall";
                ActAbilities = "Charge,Cleave";
                Upgrades = "Bladed,Overclocked,Lobotomized";
                break;
            case "Carnifex/Scion":
                Move = 4;
                MaxHP = 4;
                Defense = 3;
                Armor = "";
                Traits = "Blood Rage";
                ActAbilities = "Rev,Chainsaw,Wild Slashes";
                Upgrades = "Heavy Swing,Rile,Bloody Teeth";
                break;
            case "Painghoul/Freak":
                Move = 4;
                MaxHP = 4;
                Defense = 3;
                Armor = "";
                Traits = "Blood Rage";
                ActAbilities = "Pain Frenzy,Cauterize,Meat Hook";
                Upgrades = "Stim Hook,Frenzy Hook,Adrenalize";
                break;
            case "Painwheel/Horror":
                Move = 5;
                MaxHP = 4;
                Defense = 3;
                Armor = "";
                Traits = "Blood Rage,Hellwheel";
                ActAbilities = "Exfoliate,Spin Out";
                Upgrades = "Barbed Wheel,Endless Screaming,Hurtle";
                break;
            case "Berserker/Tyrant":
                Move = 4;
                MaxHP = 6;
                Defense = 2;
                Armor = "";
                Traits = "Blood Rage,Steaming Rage,Tyrant";
                ActAbilities = "Pulverize,Building Rage,Rip and Tear";
                Upgrades = "Machineheart,Fuel Injectors,Bifurcate";
                break;
            case "Warlord/Necromancer":
                Move = 4;
                MaxHP = 10;
                Defense = 3;
                Armor = "";
                Traits = "Blood Rage,Fueled By Rage";
                ActAbilities = "Superheated Chainblade";
                SoulAbilities = "Ignite,Glory Kill";
                break;
            case "Scum/Thrall":
                Move = 3;
                MaxHP = 1;
                Defense = 4;
                Armor = "ARMOR";
                Traits = "Toxic Revenge,Thrall";
                ActAbilities = "Pseudopod,Shamble";
                Upgrades = "Bloat,Tentacle Whip,Aftermath";
                break;
            case "Rotten/Scion":
                Move = 4;
                MaxHP = 4;
                Defense = 3;
                Armor = "ARMOR";
                Traits = "Plaguebearer";
                ActAbilities = "Pustulate,Vomitous Mass,Rotblade";
                Upgrades = "Catalyze,Invigorating Viscera,Smog Shroud";
                break;
            case "Leech/Freak":
                Move = 4;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "";
                ActAbilities = "Suppurate,Evolve Strain,Swell with Corruption";
                Upgrades = "Massive Swell,Spreading Strain,Acid Blood";
                break;
            case "Host/Hunter":
                Move = 2;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Swarm Release";
                ActAbilities = "Propagate Swarm,Driving Vermin";
                Upgrades = "Swarm Feed,Toxic Avenger,Defiler";
                break;
            case "Slime/Horror":
                Move = 4;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Vile Rupture";
                ActAbilities = "Percolate,Surge";
                Upgrades = "Rotten Surge,Sticky Trail,The Gunk";
                break;
            case "Plaguelord/Necromancer":
                Move = 4;
                MaxHP = 10;
                Defense = 4;
                Armor = "";
                Traits = "Blessed with Filth";
                ActAbilities = "Virulence";
                SoulAbilities = "Infest,Slime Form";
                break;
            case "Sacrifice/Thrall":
                Move = 3;
                MaxHP = 2;
                Defense = 4;
                Armor = "";
                Traits = "Inverted Crucifix,Thrall";
                ActAbilities= "Beckon,Shudder";
                Upgrades = "Squirm,Dead Grasp,Impending Death";
                break;
            case "Chosen/Freak":
                Move = 4;
                MaxHP = 3;
                Defense = 5;
                Armor = "WARD";
                Traits = "Slither";
                ActAbilities = "Tombraiser,Kidnap,Serpent's Kiss";
                Upgrades = "Ivory Monument,Leap,Foul Monuments";
                break;
            case "Vizigheist/Horror":
                Move = 4;
                MaxHP = 3;
                Defense = 5;
                Armor = "WARD";
                Traits = "Teleport";
                ActAbilities = "Horrendous Shriek,Urgal Blade";
                Upgrades = "Terrorize,Soul Frost,Condemn";
                break;
            case "Banshee/Hunter":
                Move = 3;
                MaxHP = 3;
                Defense = 5;
                Armor = "WARD";
                Traits = "Soul Sight";
                ActAbilities = "Bale Scream,Tombstone";
                Upgrades = "Doom Bell,Freeze Soul,Siren";
                break;
            case "Bound Devil/Tyrant":
                Move = 3;
                MaxHP = 6;
                Defense = 3;
                Armor = "WARD";
                Traits = "Flight,Tyrant";
                ActAbilities= "Omnipresence,Beckon Lamb,Limb From Limb";
                Upgrades = "To The Slaughter,Death Toll,Strong Pact";
                break;
            case "Dark Priest/Necromancer":
                Move = 4;
                MaxHP = 8;
                Defense = 4;
                Armor = "WARD";
                Traits = "Dread Presence";
                ActAbilities = "Doom Blade";
                SoulAbilities = "Cyclopean Monolith,Soulfeed";
                break;
            case "Penitent/Scion":
                Move = 4;
                MaxHP = 3;
                Defense = 2;
                Armor = "SUPER";
                Traits = "Miracle";
                ActAbilities = "Mea Culpa, Holy Water, Excoriate";
                Upgrades = "Holy Blood, Eager, Taste the Lash";
                break;
            case "Zealot/Horror":
                Move = 4;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Miracle, Zealotry";
                ActAbilities = "Smite, Whirling Chain";
                Upgrades = "Punisher, Suffuse, Fiery Chain";
                break;
            case "Antipriest/Freak":
                Move = 3;
                MaxHP = 2;
                Defense = 4;
                Armor = "WARD";
                Traits = "Miracle";
                ActAbilities = "Gentleness, Delay Judgement, Blessed Censer";
                Upgrades = "Cleansing Prayer, Consecrate, Boiling Censer";
                break;
            case "Inquisitor/Hunter":
                Move = 3;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Miracle";
                ActAbilities = "Winch, Requiesce en Pace";
                Upgrades = "Focus, Heart Destroyer, Explosive Bolts";
                break;
            case "Holy Body/Tyrant":
                Move = 4;
                MaxHP = 3;
                Defense = 2;
                Armor = "SUPER";
                Traits = "Flight, Miraculous Flesh, Tyrant";
                ActAbilities = "Bolides, Indignation, Ablutions";
                Upgrades = "Scathe, Holy Font, Witness";
                break;
            case "Exorcist/Necromancer":
                Move = 4;
                MaxHP = 10;
                Defense = 4;
                Armor = "";
                Traits = "Holy Vessel";
                ActAbilities = "Starmetal Godsword, Absolution, Will of God";
                Upgrades = "";
                break;
            case "Stitch/Thrall":
                Move = 3;
                MaxHP = 2;
                Defense = 4;
                Armor = "";
                Traits = "Fall To Shambles";
                ActAbilities = "Unstable Mutation,Twisting Strike";
                Upgrades = "Genestealer, Spread Mutate, Warping Mutate";
                break;
            case "Chop Doc/Freak":
                Move = 4;
                MaxHP = 4;
                Defense = 4;
                Armor = "WARD";
                Traits = "Leftovers";
                ActAbilities = "Inject Mutagen, Purge, Marriage";
                Upgrades = "Absorb, Scour Flesh, Conjoin";
                break;
            case "Lycan/Horror":
                Move = 4;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Lope";
                ActAbilities = "Bloodgorger, Bloody Slashes";
                Upgrades = "The Hunger, Autophagia, Hunch";
                break;
            case "Strigoi/Hunter":
                Move = 3;
                MaxHP = 4;
                Defense = 4;
                Armor = "";
                Traits = "Flight";
                ActAbilities = "Regurgitate, Sin Eater";
                Upgrades = "Drown In Viscera, Rapid Adaptation, Cleansing Wash";
                break;
            case "Homonculus/Tyrant":
                Move = 4;
                MaxHP = 6;
                Defense = 2;
                Armor = "";
                Traits = "Warpflesh,Tyrant";
                ActAbilities = "Scult Flesh, Absorb, Flesh Whip";
                Upgrades = "Ball of Limbs, Meld, Form Carapace";
                break;
            case "Chiurgeon/Necromancer":
                Move = 4;
                MaxHP = 10;
                Defense = 4;
                Armor = "WARD";
                Traits = "Polyglot";
                ActAbilities = "Experimental Surgery, Wild Mutation, Sample Genome";
                Upgrades = "";
                break;
            default:
                throw new Exception($"UnitType '{unitType}' unsupported.");
        }
    }

    public void UpdateSelectedTokenPanel() {
        if (!TokenController.IsSelected(this)) {
            return;
        }

        VisualElement panel = UI.System.Q("SelectedTokenPanel");

        Color c = ClassColor();
        panel.Q("ClassBackground").style.borderTopColor = c;
        panel.Q("ClassBackground").style.borderRightColor = c;
        panel.Q("ClassBackground").style.borderBottomColor = c;
        panel.Q("ClassBackground").style.borderLeftColor = c;

        panel.Q("Portrait").style.backgroundImage = Graphic;

        panel.Q<Label>("CHP").text = CurrentHP.ToString();
        panel.Q<Label>("MHP").text = "/" + MaxHP.ToString();
        panel.Q<Label>("Name").text = Name;

        panel.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(panel.Q<ProgressBar>("VigorBar"), false);
        UI.ToggleDisplay(panel.Q<Label>("VIG"), false);
        UI.ToggleDisplay(panel.Q("ResolveWrapper"), false);
        UI.ToggleDisplay(panel.Q<Label>("Elite"), false);
        for (int i = 1; i <= 4; i++) {
            UI.ToggleDisplay(panel.Q("Wound" + i), false);
        }


        panel.Q<Label>("Job").text = UnitType.Replace("/", "\n");
        panel.Q<Label>("Job").style.backgroundColor = c;

        panel.Q("Statuses").Clear();
        int statusCount = 0;
        if (CurrentHP == 0) {
            statusCount++;
            addStatus(panel, "Corpse", "neg");
        }
        if (Strength >= 1) {
            statusCount++;
            addStatus(panel, $"Strength {Strength}", "pos");
        }
        else if (Strength <= -1) {
            statusCount++;
            addStatus(panel, $"Weak {-Strength}", "neg");
        }
        if (Speed >= 1) {
            statusCount++;
            addStatus(panel, $"Speed {Speed}", "pos");
        }
        else if (Speed <= -1) {
            statusCount++;
            addStatus(panel, $"Slow {-Speed}", "neg");
        }
        if (Vitality >= 1) {
            statusCount++;
            addStatus(panel, $"Vitality {Vitality}", "pos");
        }
        else if (Vitality <= -1) {
            statusCount++;
            addStatus(panel, $"Vulnerable {-Vitality}", "neg");
        }

        if (!Loaded) {
            statusCount++;
            addStatus(panel, "Reload", "neg");
        }

        // if armor

        UI.ToggleDisplay("StatusColumn", statusCount > 0);

        VisualElement stats = panel.Q("MaleghastStats");

        stats.Q<Label>("StatDef").text = Defense.ToString();
        stats.Q<Label>("StatMove").text = Move.ToString();
        stats.Q<Label>("StatArm").text = Armor.Length > 0 ? Armor : "None";
    }

    private void addStatus(VisualElement v, string statusName, string colorShorthand) {
        Color c = Color.white;
        if (colorShorthand == "pos") {
            c = ColorSidebar.FromHex("#74f774");
        }
        else if (colorShorthand == "neg") {
            c = ColorSidebar.FromHex("#f77474");
        }
        Label label = new Label(statusName);
        label.AddToClassList("no-margin");
        label.style.color = c;
        v.Q("Statuses").Add(label);  
    }

    public override bool CheckCondition(string label) {
        switch (label) {
            case "TurnEnded":
                return TurnEnded;
        }
        throw new Exception($"TokenData Condition '{label}' unsupported.");
    } 

}
