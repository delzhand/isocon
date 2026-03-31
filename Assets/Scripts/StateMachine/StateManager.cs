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
            currentState.OnExit();
        }
        StateStack.Push(newState);
        newState.OnEnter();
        // Debug.Log(StackString());
    }

    public static void PopState()
    {
        var currentState = StateStack.Peek();
        currentState.OnExit();
        StateStack.Pop();
        var newState = StateStack.Peek();
        newState.OnEnter();
        // Debug.Log(StackString());
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
            states.Add(_state.TypeName());
        }
        states.Reverse();
        return string.Join(" | ", states);
    }

}

public interface IState
{
    public void OnEnter();
    public void OnExit();
    public void UpdateState();
    public string TypeName();

    public void HandleInput();
}