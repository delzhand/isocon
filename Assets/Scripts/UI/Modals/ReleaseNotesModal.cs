using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReleaseNotesModal
{
    public static void Open(string version = null)
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader($"Release Notes");
        var scroll = Modal2.AddScrollArea("Scroll");

        var accordion = Modal2.AddAccordion("ReleaseNotes");

        var notes = getNotes();
        var orderedReleases = notes.Keys;
        var targetVersion = (version != null) ? version : orderedReleases.ElementAt(0);
        foreach (string v in orderedReleases)
        {
            bool isOpen = (v == targetVersion);
            Modal2.AddAccordionItem(accordion, $"v{v}", notes[v], isOpen);
        }

        Modal2.MoveToScrollArea(accordion, scroll);

        Modal2.AddDialogFooter("Close");
        Modal2.Open("Release Notes");
    }

    public static void OpenAtStartup(string version)
    {
        string seen = Preferences.GetReleaseNotesSeen();
        List<string> seenParts = seen.Split("|").ToList();
        if (seenParts.Contains(version))
        {
            return;
        }
        seenParts.Add(version);
        Preferences.SetReleaseNotesSeen(string.Join("|", seenParts.ToArray()));
        Open(version);
    }

    private static Dictionary<string, string> getNotes()
    {
        var notes = new Dictionary<string, string>();

        notes.Add("0.9.2", @"<size=+2><b>Fixes</b></size>
* Fixed a bug that prevented tokens from being added on new installs
* Token Library no longer automatically closes after adding a token
");

        notes.Add("0.9.1", @"<size=+2><b>Features</b></size>
* Black Mass generator for Maleghast to quickly generate a team
* System Setup tool for Maleghast to quickly create certain tabletop tags
* Custom Maleghast actor panels
* Overhead token display for Maleghast actors
* Global actor scale added to Preferences
* Upgrades can be configured for Maleghast units
<size=+2><b>Fixes</b></size>
* Map editing tools no longer 'stick' in alternate mode when alt-tabbing
");

        notes.Add("0.9.0", @"<size=+2><b>Features</b></size>
* Major UI Redesign
  > New token library
  > New main navigation
  > New right-click menus for actors and tiles
  > New Release Notes dialog, can be viewed by clicking version number on start screen
* Added Volvadani units to Maleghast base data
<size=+2><b>Improvements</b></size>
* Improved keyboard navigation between form elements
* Dialogs can now be closed by clicking anywhere outside of them
* Last-used actor type is remembered in Add Actor dialog
* File browser now uses dark theme
* Moving player operation text now shows lateral tiles moved (square grid only)
* Unit types with HP pips can modify health directly from the unit panel while selected
<size=+2><b>Fixes</b></size>
* Multiple click events no longer trigger off a single click in edit mode
<size=+2><b>Known Issues</b></size>
* Memory leak - as temporary mitigation a silent, automated task will run every five minutes to clear unused assets from memory
");

        notes.Add("0.8.2", @"<size=+2><b>Fixes</b></size>
* Adds an important missing context menu to Maleghast actors
* Maleghast actors can edit HP directly on the actor panel when selected");

        notes.Add("0.8.1", @"<size=+2><b>Fixes</b></size>
* Resolved an issue with config files from older versions breaking the main menu
* Resolved an issue wherein tile effects couldn't be applied");

        notes.Add("0.8.0", @"<size=+2><b>Features</b></size>
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
* Under certain circumstances, selecting a tile or dragging an actor to a tile will select a tile of lower elevation than intended");

        notes.Add("0.7.6", @"<size=+2><b>Features</b></size>
* Added support for Icon 2.0 Playtest game system
* Added a Lowest (disadvantage) option to the dice roller
* Updated documentation
* Custom GameSystems can now more easily make direct dice rolls");
        notes.Add("0.7.5", @"* Fixes a bug that caused status effects to appear on incorrect tokens. (ICON 1.5)
* Adds a framerate limiter to config. (thanks to McPalm)");
        notes.Add("0.7.4", @"* Tokens can now be deleted from the library.
* Missing image files in the hashed-tokens directory now throw an error instead of breaking.
* Unity version updated to 6.
* Unity splash screen removed.
* Minor UI tweaks.");
        notes.Add("0.7.3", "* A critical bug with client connections has been resolved");

        return notes;
    }
}
