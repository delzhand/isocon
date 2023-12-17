using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class IsoConsole
{
    public static void OpenModal(ClickEvent evt) {
        Modal.Reset("IsoConsole");
        Modal.AddTextField("Console", "Console", "");
        Modal.AddPreferredButton("Execute", ConsoleExecute);
        Modal.AddButton("Close", CloseModal);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

    private static void ConsoleExecute(ClickEvent evt) {
        string command = Modal.Find().Q<TextField>("Console").value;
        Modal.Find().Q<TextField>("Console").Focus();
        if (command == "MGSample") {
            string json = "{\"Name\":\"\",\"GraphicHash\":\"8b3627c36b26c32aaa2997d2ed422253e9f0e19f896978cd22514514545f3830\",\"Size\":0,\"HouseJob\":\"CARCASS/Gunwight\"}";
            json = "{\"Name\":\"\",\"GraphicHash\":\"8b3627c36b26c32aaa2997d2ed422253e9f0e19f896978cd22514514545f3830\",\"Size\":0,\"HouseJob\":\"CARCASS/Gunwight\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"4c08f3acb936735fe653143f2ead7901bff050a4866700c925da123f02d44d26\",\"Size\":0,\"HouseJob\":\"CARCASS/Enforcer\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"3b87c5c8d58f95efeafc0ccc9aa26ea27c62fe0c013945bae8395081be372892\",\"Size\":0,\"HouseJob\":\"CARCASS/Ammo Goblin\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"3b87c5c8d58f95efeafc0ccc9aa26ea27c62fe0c013945bae8395081be372892\",\"Size\":0,\"HouseJob\":\"CARCASS/Ammo Goblin\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"1863750677b8b4a95f23fd514d063bb64c5489d333553757b86dac69a8647c2d\",\"Size\":0,\"HouseJob\":\"CARCASS/EGIS Weapon\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Simon\",\"GraphicHash\":\"c6d4ec159945cd89ce03f532135b004d354dc6467f97fd5321eb2b07884f8acd\",\"Size\":0,\"HouseJob\":\"CARCASS/Operator\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"8cd93a4e7531967360021609aed93802c5965cd43cf9ae6babd9d51d0c194689\",\"Size\":0,\"HouseJob\":\"Goregrinders/Painghoul\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"8cd93a4e7531967360021609aed93802c5965cd43cf9ae6babd9d51d0c194689\",\"Size\":0,\"HouseJob\":\"Goregrinders/Painghoul\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"04620e2d88b75279048c70d47122571f875511486a7cbb8a4046f158422e9737\",\"Size\":0,\"HouseJob\":\"Goregrinders/Berserker\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"04620e2d88b75279048c70d47122571f875511486a7cbb8a4046f158422e9737\",\"Size\":0,\"HouseJob\":\"Goregrinders/Berserker\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"\",\"GraphicHash\":\"23315fbd145c38e918b4cd51513e3add44f1a22d745bae5345441216fcc267cd\",\"Size\":0,\"HouseJob\":\"Goregrinders/Painwheel\"}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Tethys\",\"GraphicHash\":\"ca6d8e3015a2f46d27b1e0f8b6a160652254cf4be8715131d145f0f5d46ca094\",\"Size\":0,\"HouseJob\":\"Goregrinders/Warlord\"}";
            Player.Self().CmdCreateTokenData(json);
        }
        if (command == "ICSample") {
            string json = "{\"Name\":\"Ada\",\"GraphicHash\":\"df6ee698a739576676d5f99c113a61fecdd1f2a66cc3fd7fc1b8ac21f3ba4067\",\"Size\":1,\"Class\":\"Stalwart\",\"Job\":\"Bastion\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Sae\",\"GraphicHash\":\"7451fc67cb845c64f81d0918baeaf5829d7821790cf98b388b364d18a893e2fe\",\"Size\":1,\"Class\":\"Mendicant\",\"Job\":\"Chanter\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Graddes\",\"GraphicHash\":\"82d39a85a409a2f54c4799049869001e216495926ae028bb531ec6cbce100b6b\",\"Size\":1,\"Class\":\"Wright\",\"Job\":\"Enochian\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Duvalla\",\"GraphicHash\":\"c7fc3457bc38f379578b41a1faaad501db00f06de38c21d7a13e6625b02708de\",\"Size\":1,\"Class\":\"Vagabond\",\"Job\":\"Shade\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Bungus\",\"GraphicHash\":\"7130a6a54a23159422fd5c5fda379cf59e5ed64f6a1534cf9ef318232561fcc3\",\"Size\":2,\"Class\":\"Heavy\",\"Job\":\"Halitoad\",\"Elite\":true,\"HPMultiplier\":2}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Kingpecker\",\"GraphicHash\":\"13883d5d1959df44b6f03973cfaad587056d00ae732e981d8050c24b1a4c1cad\",\"Size\":1,\"Class\":\"Skirmisher\",\"Job\":\"Skinner Shrike\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
        }
        if (command == "Aura") {
            AuraManager am = Token.GetSelected().AddComponent<AuraManager>();
            am.AddAura("Rampart", 2);
        }
        if (command == "FirstToken") {
            GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i].GetComponent<Token>().Select();
            }
        }
        if (command.StartsWith("SelectedToken|") || command.StartsWith("ST|")) {
            if (command.StartsWith("SelectedToken|")) {
                command = command.Substring("SelectedToken|".Length);
            } else if (command.StartsWith("ST|")) {
                command = command.Substring("ST|".Length);
            }
            TokenData data = Token.GetSelectedData().GetComponent<TokenData>();
            Player.Self().CmdRequestTokenDataSetValue(data, command);
            Toast.Add("Console command executed on selected token.");
        }
        if (command.StartsWith("FocusedToken|") || command.StartsWith("FT|")) {
            if (command.StartsWith("FocusedToken|")) {
                command = command.Substring("FocusedToken|".Length);
            } else if (command.StartsWith("FT|")) {
                command = command.Substring("FT|".Length);
            }
            TokenData data = Token.GetFocusedData().GetComponent<TokenData>();
            Player.Self().CmdRequestTokenDataSetValue(data, command);
            Toast.Add("Console command executed on focused token.");
        }
        if (command.StartsWith("GameSystem|") || command.StartsWith("SYS|")) {
            if (command.StartsWith("GameSystem|")) {
                command = command.Substring("GameSystem|".Length);
            } else if (command.StartsWith("SYS|")) {
                command = command.Substring("SYS|".Length);
            }
            Player.Self().CmdRequestGameDataSetValue(command);
            Toast.Add("Console command executed on current game system.");
        }
        if (command.StartsWith("MapArtDemo")) {
            MapSaver.StegLoad("C:\\Users\\delzh\\Projects\\isocon_data/maps/ArtDemo.png");
        }
        if (command.StartsWith("TileArtDemo")) {
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            for(int i = 0; i < blocks.Length; i++) {
                Block b = blocks[i].GetComponent<Block>();
                if (b.getY() == 0) {
                    b.ApplyTexture("DryGrass");
                }
                else if (b.getY() == 1) {
                    b.ApplyTexture("Grass");
                }
                else if (b.getY() == 2) {
                    b.ApplyTexture("Water");
                }
                else if (b.getY() == 3) {
                    b.ApplyTexture("Rock");
                }
                else if (b.getY() == 4) {
                    b.ApplyTexture("Stone");
                }
                else if (b.getY() == 5) {
                    b.ApplyTexture("Lava");
                }
            }
        }

        if (command.StartsWith("LavaDemo")) {
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            for(int i = 0; i < blocks.Length; i++) {
                Block b = blocks[i].GetComponent<Block>();
                b.ApplyTexture("Stone");
                if ((b.getY() == 3 || b.getY() == 4) && (b.getX() == 3 || b.getX() == 4)) {
                    b.ApplyTexture("Lava");
                }
            }
        }

    }


}
