using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControlPanelButton : Interactable
{
    [SerializeField] UnityEvent callback;
    [SerializeField] Animator anim;
    [SerializeField] List<IBoolCondition> pressConditions = new ();
    [SerializeField] List<IBoolCondition> invokeCallbackConditions = new ();

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        Press();
    }

    public bool ArePressConditionsMet()
    {
        foreach (IBoolCondition condition in pressConditions)
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

    public void Press()
    {
        if (!ArePressConditionsMet()) return;

        InvokeCallback();
    }

    void InvokeCallback()
    {
        if (!AreInvokeCallbackConditionsMet()) return;

        callback?.Invoke();
    }
}
