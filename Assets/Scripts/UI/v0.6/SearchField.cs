using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SearchField
{
    public static VisualElement Create(string[] options) {

        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/SearchInput");
        VisualElement element = template.Instantiate();
        
        TextField input = element.Q<TextField>("SearchInput");
        VisualElement results = element.Q("SearchResults");

        for (int i = 0; i < options.Length; i++) {
            Label label = new Label(options[i]);
            label.RegisterCallback<ClickEvent>((evt) => {
                input.SetValueWithoutNotify(label.text);
                UI.ToggleDisplay(results, false);
            });
            label.RegisterCallback<MouseEnterEvent>((evt) => {
                label.AddToClassList("selected");
            });
            label.RegisterCallback<MouseLeaveEvent>((evt) => {
                label.RemoveFromClassList("selected");
            });
            results.Add(label);
        }

        input.RegisterCallback<FocusInEvent>((evt) => {
            UI.ToggleDisplay(results, true);
        });
        input.RegisterCallback<FocusOutEvent>((evt) => {
            UI.ToggleDisplay(results, false);
        });
        input.RegisterValueChangedCallback((evt) => {
            FilterElements(element);
        });

        return element;
    }

    private static void FilterElements(VisualElement root) {
        string input = root.Q<TextField>("SearchInput").value.ToLower();
        foreach (Label label in root.Q("SearchResults").Children()) {
            if (!label.text.ToLower().Contains(input) && input.Length > 0) {
                UI.ToggleDisplay(label, false);
            }
            else {
                UI.ToggleDisplay(label, true);
            }
        }
    }
}
