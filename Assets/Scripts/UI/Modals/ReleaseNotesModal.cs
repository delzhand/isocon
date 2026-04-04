using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReleaseNotesModal
{
    private static readonly string notes = @$"<size=+2><b>Features</b></size>
* Sessions can now be saved and loaded
* Session will autosave every 5 minutes or when exiting to launcher
* GameSystems replaced by Actor Types
* Multiple Actor Types can be active in the same session
* Actors can be customized with resources and stats
* Fixed view button added to top bar
* Tags and clocks can be added to sessions

<size=+2><b>Changes</b></size>
* Tokens renamed Actors
* Actor focus/selection behavior changed
* Actor size changed to shape, more hex options added
* Top bar and actor list can be hidden
* Hellminth units added to Maleghast data
* Maleghast actors can now alter their core stats
* Actor colors now show as a border on their base shadow

<size=+2><b>New Actor Types</b></size>
* Environmental - a type with no stats
* Lancer Mech - a player type for LANCER
* Lancer Pilot - a player type for LANCER
* ICON 1.5 split into Player, Enemy, and Mob
* ICON 2.0 split into Player, Enemy, and Mob
* Generic renamed to Basic

<size=+2><b>Fixes</b></size>
* Shortcut keystrokes no longer trigger when modals are open
* Certain large actor shapes can be dragged to intersections to remain centered

<size=+2><b>Known Issues</b></size>
* Custom cursor prevents window resize handles from being shown, though resizing still works
* Under certain circumstances, selecting a tile or dragging an actor to a tile will select a tile of lower elevation than intended
";

    public static void Open(string version)
    {
        Modal2.CreateContext("ShunDialog1");
        Modal2.AddDialogHeader($"Release Notes for IsoCON v{version}");
        var scroll = Modal2.AddScrollArea("Scroll");
        var notesField = Modal2.AddLongMarkup(notes);
        Modal2.MoveToScrollArea(notesField, scroll);
        Modal2.AddDialogFooter("Close", () =>
        {
            string seen = Preferences.GetReleaseNotesSeen();
            List<string> seenParts = seen.Split("|").ToList();
            seenParts.Add(version);
            Preferences.SetReleaseNotesSeen(string.Join("|", seenParts.ToArray()));
            Modal2.Close();
        });
        Modal2.Open();
    }
}
