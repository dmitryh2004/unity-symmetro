using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ControlPanelButtonColors
{
    White = 0,
    Red,
    Yellow,
    Green,
    Blue,
    Purple
}
public class ControlPanelElementController : Interactable
{
    [SerializeField] UnityEvent<bool> callback;
    [SerializeField] Animator anim;
    [SerializeField] ControlPanelButtonColors color = ControlPanelButtonColors.White;
    [SerializeField] List<IBoolCondition> changeStateConditions = new ();
    [SerializeField] List<IBoolCondition> invokeCallbackConditions = new ();
    [SerializeField] bool startState = false;
    bool currentState;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        currentState = startState;

        if (anim != null)
        {
            if (CompareTag("ColoredButton"))
                anim.SetInteger("ButtonColor", (int)color);
        }
    }

    private void Start()
    {
        InvokeCallback(currentState);
    }

    public bool AreChangeStateConditionsMet()
    {
        foreach (IBoolCondition condition in changeStateConditions)
        {
            if (condition == null) continue;
            if (!condition.Check()) return false;
        }
        return true;
    }

    public bool AreInvokeCallbackConditionsMet()
    {
        foreach (IBoolCondition condition in invokeCallbackConditions)
        {
            if (condition == null) continue;
            if (!condition.Check()) return false;
        }
        return true;
    }

    public void ChangeState(bool newState)
    {
        if (!AreChangeStateConditionsMet()) return;

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
        if (!AreInvokeCallbackConditionsMet()) return;

        Debug.Log($"{gameObject.name}: callback invoked");
        callback?.Invoke(var);
    }

    public override void Interact()
    {
        ToggleState();
    }
}
