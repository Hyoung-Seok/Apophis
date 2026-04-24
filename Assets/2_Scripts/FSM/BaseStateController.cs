using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseStateController : MonoBehaviour
{
    public IReadOnlyList<BaseState> BaseStates => states;
    [field: SerializeField] public BaseState MainState { get; protected set; }
    [field: SerializeField] public BaseState SubState { get; protected set; }
    [SerializeField] private List<BaseState> states = new();

    protected virtual void Awake()
    {
        foreach (var state in states)
        {
            if (state == null) continue;
            state.Init(this);
        }
    }

    public void ChangeMainState(BaseState state)
    {
        if (MainState != null)
        {
            MainState.OnStateExit();
        }
        
        MainState = state;
        MainState.OnStateEnter();
    }

    public void ChangeSubState(BaseState state)
    {
        if (SubState != null)
        {
            SubState.OnStateExit();
        }
        
        SubState = state;
        SubState.OnStateEnter();
    }

    public T GetState<T>() where T : BaseState
    {
        foreach (var state in states)
            if (state is T typed)
                return typed;

        return null;
    }
    
    private void Start()
    {
        MainState?.OnStateEnter();
        SubState?.OnStateEnter();
    }

    private void Update()
    {
        MainState?.OnUpdate();
        SubState?.OnUpdate();
    }
}
