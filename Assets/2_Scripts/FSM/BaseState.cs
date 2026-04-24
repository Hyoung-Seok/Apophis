using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    protected BaseStateController Controller;

    public virtual void Init(BaseStateController controller)
    {
        Controller = controller;
    }
    
    public abstract void OnStateEnter();
    public abstract void OnUpdate();
    public abstract void OnStateExit();
}
