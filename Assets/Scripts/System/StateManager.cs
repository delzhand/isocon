using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _current;

    // This LauncherState should be persisted because it tracks whether initial setup
    // has been done. Should that be tracked in this class instead?
    public LauncherState LauncherState = new();

    void Start()
    {
        LauncherState.OnEnter(this);
    }

    void Update()
    {
        _current.UpdateState();
    }

    public void ChangeState(IState newState)
    {
        _current.OnExit();
        _current = newState;
        _current.OnEnter(this);
    }

    public void SetConnectionMode(ConnectMode mode)
    {

    }
}

public interface IState
{
    public void OnEnter(StateManager sm);
    public void OnExit();
    public void UpdateState();
}