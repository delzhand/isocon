using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _current;
    private IState _substate;
    public IState SubState { get => _substate; }

    public string Debug;
    public string Debug2;

    void Start()
    {
        Startup.RunTasks();
        ChangeState(new LauncherState());
    }

    void Update()
    {
        Debug2 = MapEdit.EditOp;
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
        _substate?.OnEnter(this);
    }
}

public interface IState
{
    public void OnEnter(StateManager sm);
    public void OnExit();
    public void UpdateState();
}