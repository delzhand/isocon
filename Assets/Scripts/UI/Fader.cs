using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fader : MonoBehaviour
{

    public delegate void FaderCallback();

    private static VisualElement _element;
    private static float _duration;
    private static float _elapsed;
    private static FaderCallback _midpoint;
    private static FaderCallback _endpoint;

    void Update()
    {
        if (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            float percent = _elapsed / _duration;
            _element.style.opacity = Mathf.Sin(Mathf.PI * percent);
            if (percent >= .5)
            {
                _midpoint?.Invoke();
                _midpoint = null;
            }
            if (percent >= 1)
            {
                _endpoint?.Invoke();
                _endpoint = null;
                UI.ToggleDisplay(_element, false);
                Destroy(this);
            }
        }
    }

    public static void StartFade(Color color, float duration, FaderCallback midpoint = null, FaderCallback endpoint = null)
    {
        _element ??= UI.System.Q("Fader");
        _element.style.backgroundColor = color;
        _duration = duration;
        _elapsed = 0;
        _midpoint = midpoint;
        _endpoint = endpoint;
        UI.ToggleDisplay(_element, true);
        GameObject.Find("AppState").AddComponent<Fader>();
    }
}
