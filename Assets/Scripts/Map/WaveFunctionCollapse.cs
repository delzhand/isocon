using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse
{

// nothing
// solid
// slope-N
// slope-E
// slope-S
// slope-W
// sci-NE
// sci-SE
// sci-SW
// sci-NW
// sco-NE
// sco-SE
// sco-SW
// sco-NW

// Positions
// 7 0 1
// 6 x 2
// 5 4 3

    // public static string[] AllowedNeighbors(string cell, string position) {
    //     switch (cell) {
    //         case "nothing":
    //             return AllowedNeighborsNothing(position);
    //     }
    // }

    // private static string[] AllowedNeighborsNothing(string position) {
    //     List<string> options = new();
    //     switch (position) {
    //         case ("n"):
    //             options.Add("slope-N");
    //             break;
    //         case ("e"):
    //             options.Add("slope-E");
    //             break;
    //         case ("s"):
    //             options.Add("slope-S");
    //             break;
    //         case ("w"):
    //             options.Add("slope-S");
    //             break;
    //     }
    //     return options.ToArray();
    // }

    // private static string[] AllowedNeighborsSolid(string position) {
    //     List<string> options = new();
    //     switch (position) {
    //         case ("n"):
    //             options.Add("slope-S");
    //             break;
    //         case ("e"):
    //             options.Add("slope-W");
    //             break;
    //         case ("s"):
    //             options.Add("slope-N");
    //             break;
    //         case ("w"):
    //             options.Add("slope-N");
    //             break;
    //     }
    //     return options.ToArray();        
    // }

    // private static string[] AllowedNeighborsSlopeN(string position) {
    //     List<string> options = new();
    //     switch (position) {
    //         case ("n"):
    //             options.Add("solid");
    //             break;
    //         case ("e"):
    //             options.Add("slope-N");
    //             options.Add("sci-NE");
    //             options.Add("sco-NW");
    //             break;
    //         case ("s"):
    //             options.Add("nothing");
    //             break;
    //         case ("w"):
    //             options.Add("slope-N");
    //             break;
    //     }
    //     return options.ToArray();        
    // }
}
