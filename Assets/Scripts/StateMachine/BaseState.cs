
using Unity.VisualScripting;

public abstract class BaseState : IState
{
    public StateManager SM;

    public virtual void OnEnter(StateManager sm)
    {
        SM = sm;
    }

    public virtual void OnExit()
    {
    }

    public virtual void UpdateState()
    {
    }

    public string TypeName()
    {
        return this.GetType().Name;
    }
}