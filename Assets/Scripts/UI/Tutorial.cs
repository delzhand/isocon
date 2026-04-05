using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class Tutorial
{
    public static void Setup()
    {
        UI.TopBar.Q("AddActor").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("add actor");
        });
        UI.TopBar.Q("Session").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("sessions");
        });
        UI.TopBar.Q("AddTableTag").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("system tag");
        });
    }

    public static void Init(string id)
    {
        if (Preferences.Current.SkipTutorials)
        {
            return;
        }

        string seen = Preferences.Current.TutorialsSeen;
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(id))
        {
            return;
        }

        seenParts.Add(id);
        Preferences.SetTutorialsSeen(string.Join("|", seenParts.ToArray()));

        (string, string) tutorial = GetTutorial(id);

        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader(tutorial.Item1);
        Modal2.AddLongMarkup(tutorial.Item2);
        var footer = Modal2.AddDialogFooter("Close");
        var skipAll = new ShunButton();
        skipAll.text = "Skip All Tutorials";
        skipAll.SetVariant(ButtonVariant.Outline);
        skipAll.clicked += () =>
        {
            Preferences.Current.SkipTutorials = true;
            Preferences.Save();
            Modal2.Close();
        };
        footer.Add(skipAll);
        Modal2.Open();
    }

    public static (string, string) GetTutorial(string id)
    {
        switch (id)
        {
            case "tabletop":
                return ("The Tabletop", "This is the tabletop screen. At the top of the screen are the main controls. At the bottom of the screen is the list of actors in the session.");
            case "add actor":
                return ("Adding an Actor", "Actors are not automatically added to the map. Drag an actor to the map to place it. Select an actor on the field and right click it for more options.");
            case "actor bar":
                return ("The Actor Bar", "Hovering over an actor in the actor bar will <i>focus</i> the actor, showing its information. Left clicking it will <i>select</i> it, allowing you to modify some values. Left click dragging will move it around the map. Right clicking will display a menu with additional options.");
            case "flip":
                return ("Flipping Actors", "Flipping actors is purely visual, but useful for screenshots.");
            case "edit mode":
                return ("Edit Mode", "Changes will be shared with other players on exiting edit mode.");
            case "sessions":
                return ("Sessions", "Saving a session will capture all actors, including their current status, the map, and any system tags/clocks. Loading a session will replace any existing actors, tags, and the current map. Sessions are autosaved every 5 minutes and when returning to the launcher.");
            case "subtools":
                return ("Subtools", "Tools with an arrow in the bottom right have alternate options that can be accessed by right clicking the icon.");
            case "camera modes":
                return ("Camera Modes", "By holding down the right mouse button, you can either drag or rotate the map. Clicking the button in the top bar will toggle between modes, or you can press C.");
            case "marking mode":
                return ("Terrain Effect Mode", "In Terrain Effect mode you can assign effects to tiles and mark them with visual effects. Left click tiles to select them, right click to open the tile menu.");
            case "web client":
                return ("Web Client", "The web version of Isocon can't use local storage, so you can't create actors. However, you can still interact with actors other players have created as well as edit the map and mark tiles.");
            case "style shortcut":
                return ("Style Shortcut", "With any of the style subtools selected, holding down the alt key will let you quickly sample block styles.");
            case "system tag":
                return ("Table Tags", "Table tags can be used to track numeric values like round number or group resources, and clocks display progress towards an event or objective.");
        }
        throw new System.Exception($"No such tutorial - {id}");
    }

}
