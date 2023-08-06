using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerReference : MonoBehaviour
{
    public Player player;
    public VisualElement visualElement;

    void Update()
    {
        if (player == null) {
            visualElement.RemoveFromHierarchy();
            GameObject.Destroy(this.gameObject);
        }
        else {
            if (player.Role != PlayerRole.GM) {
                visualElement.Q("GMLabel").style.display = DisplayStyle.None;
            }
            visualElement.Q<Label>("PlayerName").text = player.Name;
            UI.ToggleDisplay(visualElement.Q<Label>("MeLabel"), player.isOwned);
            UI.ToggleDisplay(visualElement.Q<Label>("HostLabel"), player.Host);
        }
    }
}
