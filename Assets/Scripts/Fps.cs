using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class Fps : MonoBehaviour
{
    private float count;
    private Label fpscounter;
    private IEnumerator Start()
    {
        fpscounter = GameObject.Find("ModeUI").GetComponent<UIDocument>().rootVisualElement.Q<Label>("FPS");

        GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update() {
        fpscounter.text = "FPS: " + Mathf.Round(count);
    }
}