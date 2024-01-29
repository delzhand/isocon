using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _current;
    private IState _substate;

    public string Debug;

    // This LauncherState should be persisted because it tracks whether initial setup
    // has been done. Should that be tracked in this class instead?
    public LauncherState LauncherState = new();

    void Start()
    {
        Startup.RunTasks();
        ChangeState(LauncherState);
    }

    void Update()
    {
        Debug = $"{_current.GetType()}";
        if (_substate != null)
        {
            Debug = $"{_current.GetType()} ({_substate.GetType()})";
        }
        _current.UpdateState();
        _substate?.UpdateState();
    }

    public void ChangeState(IState newState)
    {
        _current?.OnExit();
        _current = newState;
        _current.OnEnter(this);
    }

    public void ChangeSubState(IState newState)
    {
        _substate?.OnExit();
        _substate = newState;
        _substate.OnEnter(this);
    }
}

public interface IState
{
    public void OnEnter(StateManager sm);
    public void OnExit();
    public void UpdateState();
}