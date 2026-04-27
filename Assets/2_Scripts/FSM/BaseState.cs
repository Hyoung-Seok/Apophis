using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    public abstract void Init(BaseStateController controller);
    public abstract void OnStateEnter();
    public abstract void OnUpdate();
    public abstract void OnStateExit();
}

public abstract class BaseState<TController> : BaseState
    where TController : BaseStateController
{
    protected TController Controller;

    public override void Init(BaseStateController controller)
    {
        Controller = (TController)controller;
    }
}
