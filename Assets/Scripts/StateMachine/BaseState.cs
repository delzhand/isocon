
using Unity.VisualScripting;

public abstract class BaseState : IState
{
    public virtual void OnEnter()
    {
    }

    public virtual void OnLoseFocus()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void UpdateState()
    {
    }

    public virtual void HandleInput()
    {
    }

    public virtual string GetName()
    {
        return this.GetType().Name;
    }
}