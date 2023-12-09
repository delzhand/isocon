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
        if (command == "Ada") {
            string json = "{\"Name\":\"Ada\",\"GraphicHash\":\"df6ee698a739576676d5f99c113a61fecdd1f2a66cc3fd7fc1b8ac21f3ba4067\",\"Size\":1,\"Class\":\"Stalwart\",\"Job\":\"Bastion\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Sae\",\"GraphicHash\":\"7451fc67cb845c64f81d0918baeaf5829d7821790cf98b388b364d18a893e2fe\",\"Size\":1,\"Class\":\"Mendicant\",\"Job\":\"Chanter\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Graddes\",\"GraphicHash\":\"82d39a85a409a2f54c4799049869001e216495926ae028bb531ec6cbce100b6b\",\"Size\":2,\"Class\":\"Wright\",\"Job\":\"Enochian\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
            json = "{\"Name\":\"Duvalla\",\"GraphicHash\":\"c7fc3457bc38f379578b41a1faaad501db00f06de38c21d7a13e6625b02708de\",\"Size\":1,\"Class\":\"Vagabond\",\"Job\":\"Shade\",\"Elite\":false,\"HPMultiplier\":1}";
            Player.Self().CmdCreateTokenData(json);
        }
        if (command == "SampleParty") {
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
                    b.ApplyStyle("DryGrass");
                }
                else if (b.getY() == 1) {
                    b.ApplyStyle("Grass");
                }
                else if (b.getY() == 2) {
                    b.ApplyStyle("Water");
                }
                else if (b.getY() == 3) {
                    b.ApplyStyle("Rock");
                }
                else if (b.getY() == 4) {
                    b.ApplyStyle("Stone");
                }
                else if (b.getY() == 5) {
                    b.ApplyStyle("Lava");
                }
            }
        }

        if (command.StartsWith("LavaDemo")) {
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            for(int i = 0; i < blocks.Length; i++) {
                Block b = blocks[i].GetComponent<Block>();
                b.ApplyStyle("Stone");
                if ((b.getY() == 3 || b.getY() == 4) && (b.getX() == 3 || b.getX() == 4)) {
                    b.ApplyStyle("Lava");
                }
            }
        }

    }


}
