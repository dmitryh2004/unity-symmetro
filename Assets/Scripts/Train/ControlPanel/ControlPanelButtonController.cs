using UnityEngine;
using UnityEngine.Events;

public class ControlPanelButton : Interactable
{
    [SerializeField] UnityEvent callback;

    public override void Interact()
    {
        Press();
    }

    public void Press()
    {
        InvokeCallback();
    }

    void InvokeCallback()
    {
        callback?.Invoke();
    }
}
