using UnityEngine;

public class StateManager : MonoBehaviour
{
    private IState _current;
    private IState _substate;
    public IState SubState { get => _substate; }

    public static bool ShowBorders;

    public string TempString;

    void Start()
    {
        Startup.RunTasks();
        ChangeState(new LauncherState());
    }

    void Update()
    {
        TempString = $"{MapEdit.EditOp} {MapEdit.StyleOp}";
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

    public static StateManager Find()
    {
        return GameObject.Find("AppState").GetComponent<StateManager>();
    }
}

public interface IState
{
    public void OnEnter(StateManager sm);
    public void OnExit();
    public void UpdateState();
}