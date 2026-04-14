using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private static Stack<IState> StateStack = new();

    public static bool ShowBorders; // @todo move this

    void Start()
    {
        // Startup.RunTasks();
        PushState(new LauncherState());
    }

    void Update()
    {
        foreach (IState state in StateStack)
        {
            state.UpdateState();
        }

        if (StateStack.Count > 0)
        {
            StateStack.Peek().HandleInput();
        }
    }

    public static void PushState(IState newState)
    {
        if (StateStack.Count > 0)
        {
            var currentState = StateStack.Peek();
            currentState.OnLoseFocus();
        }
        StateStack.Push(newState);
        // Debug.Log($"Added {newState.GetName()}");
        newState.OnEnter();
        HudText.SetItem("stack", StackString(), -100, HudTextColor.Red);
    }

    public static void PopState()
    {
        var currentState = StateStack.Peek();
        currentState.OnLoseFocus();
        currentState.OnExit();
        var oldState = StateStack.Pop();
        // Debug.Log($"Removed {oldState.GetName()}");
        var newState = StateStack.Peek();
        newState.OnEnter();
        HudText.SetItem("stack", StackString(), -100, HudTextColor.Red);
    }

    // public static bool ShowBorders()
    // {
    //     var state = StateStack.Peek();
    //     return state.GetType().Name == "MapEditingState";
    // }

    // public void ChangeState(IState newState)
    // {
    //     _current?.OnExit();
    //     _current = newState;
    //     _current.OnEnter(this);
    // }

    // public void ChangeSubState(IState newState)
    // {
    //     _substate?.OnExit();
    //     _substate = newState;
    //     _substate?.OnEnter(this);
    // }

    public static StateManager Find()
    {
        return GameObject.Find("AppState").GetComponent<StateManager>();
    }

    // public static void ToNeutral()
    // {
    //     Find().ChangeSubState(new NeutralState());
    // }

    public static string StackString()
    {
        List<string> states = new();
        foreach (IState _state in StateStack)
        {
            states.Add(_state.GetName());
        }
        states.Reverse();
        return string.Join(" | ", states);
    }

    public static bool IsModalState()
    {
        return StateStack.Peek().GetName().StartsWith("ModalState");
    }
}

public interface IState
{
    public void OnEnter();
    public void OnLoseFocus();
    public void OnExit();
    public void UpdateState();
    public string GetName();

    public void HandleInput();
}