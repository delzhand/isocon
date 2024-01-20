using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class MapMeta
{
    public static string Title;
    public static string CreatorName;
    public static string Description;
    public static string Objective;
    public static string System;

    public static void Reset() {
        VisualElement optionsRoot = UI.System.Q("ToolOptions");
        optionsRoot.Q("DataOptions").Q<TextField>("MapTitle").value = "";
        optionsRoot.Q("DataOptions").Q<TextField>("Description").value = "";
        optionsRoot.Q("DataOptions").Q<TextField>("Objective").value = "";
        optionsRoot.Q("DataOptions").Q<TextField>("Author").value = "";
        optionsRoot.Q("DataOptions").Q<DropdownField>("System").value = "";

        MapMeta.Description = "";
        MapMeta.Title = "";
        MapMeta.CreatorName = "";
        MapMeta.Objective = "";
        MapMeta.System = "";        
    }
}
