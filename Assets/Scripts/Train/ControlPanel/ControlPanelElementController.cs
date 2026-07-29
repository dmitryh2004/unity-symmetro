using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControlPanelElementController : Interactable
{
    [SerializeField] UnityEvent<bool> callback;
    [SerializeField] Animator anim;
    [SerializeField] List<IBoolCondition> changeStateConditions = new ();
    [SerializeField] List<IBoolCondition> invokeCallbackConditions = new ();
    [SerializeField] bool startState = false;
    bool currentState;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        currentState = startState;
    }

    private void Start()
    {
        InvokeCallback(currentState);
    }

    public void ChangeState(bool newState)
    {
        foreach (IBoolCondition condition in changeStateConditions)
        {
            if (condition == null) continue;
            if (!condition.Check()) return;
        }
        currentState = newState;
        if (anim != null)
            anim.SetBool("state", currentState);

        Debug.Log($"{gameObject.name}: changed state to {newState}");
        InvokeCallback(currentState);
    }

    public void ToggleState()
    {
        ChangeState(!currentState);
    }

    public bool GetCurrentState() => currentState;

    void InvokeCallback(bool var)
    {
        foreach (IBoolCondition condition in invokeCallbackConditions)
        {
            if (condition == null) continue;
            if (!condition.Check()) return;
        }
        Debug.Log($"{gameObject.name}: callback invoked");
        callback?.Invoke(var);
    }

    public override void Interact()
    {
        ToggleState();
    }
}
