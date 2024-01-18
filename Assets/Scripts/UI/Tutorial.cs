using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tutorial
{
    public static void Init(string id) {
        // Debug.Log($"Tutorial check: {id}");
        // PlayerPrefs.SetString("TutorialsSeen", "");

        int skip = PlayerPrefs.GetInt("SkipTutorials", 0);
        if (skip == 1) {
            return;
        }

        string seen = PlayerPrefs.GetString("TutorialsSeen", "");
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(id)) {
            return;
        }

        seenParts.Add(id);
        PlayerPrefs.SetString("TutorialsSeen", string.Join("|", seenParts.ToArray()));
        
        (string,string) tutorial = GetTutorial(id);
        Modal.Reset(tutorial.Item1);
        Modal.AddMarkup("TutorialText", tutorial.Item2);
        Modal.AddPreferredButton("Close", Modal.CloseEvent);
        Modal.AddButton("Skip All Tutorials", (evt) => {
            PlayerPrefs.SetInt("SkipTutorials", 1);
        });
    }

    public static void SkipTutorials() {
        PlayerPrefs.SetInt("SkipTutorials", 1);
    }

    public static (string,string) GetTutorial(string id) {
        switch (id) {
            case ("tabletop"):
                return ("The Tabletop", "This is the tabletop screen. Mouse over the icon at the top center to reveal camera and configuration controls. At the bottom of the screen is the token bar, where you can add or select tokens.");
            case ("add token"):
                return ("Adding a Token", "Tokens are not automatically added to the map. To add a token to the map, left click it in the token bar, then left click a tile on the map.");
            case ("token bar"):
                return ("The Token Bar", "Hovering over a token in the token bar will <i>focus</i> the token, show its information in the bottom right. Left clicking it will <i>select</i> it and allow you to move it to or around the field. Right clicking will display a menu with additional options.");
            case ("flip"):
                return ("Flipping Tokens", "Flipping tokens is purely visual, but useful for screenshots.");
            case ("end edit"):
                return ("Edit Mode", "Changes will be shared with other players on exiting edit mode.");
            case ("subtools"):
                return ("Subtools", "Tools with an arrow in the bottom right have alternate options that can be accessed by right clicking the icon.");
            case ("turn advance"):
                return ("Round Advance", "Advancing a round will always reset all ended turns, but may do other things depending on the game system. For example, in ICON, it will increase Party Resolve.");
            case ("terrain info"):
                return ("Terrain Info", "Hovering over a tile will display its coordinates, elevation, and terrain effects. Clicking on a single tile will select it, allowing you to apply or clear terrain effects. Selecting multiple tiles will allow you to apply terrain effects to all tiles.");
            case ("clear selected"):
                return ("Clear Selected", "This button will deselect all currently selected tiles.");
            case "camera modes":
                return ("Camera Modes", "By holding down the right mouse button, you can either drag or rotate the map. Clicking the button in the top bar will toggle between modes, or you can press C.");
        }
        throw new System.Exception("No such tutorial");
    }

}
