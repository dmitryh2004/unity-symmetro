using UnityEngine;
using UnityEngine.Events;

public class ControlPanelElementController : MonoBehaviour
{
    [SerializeField] UnityEvent<bool> callback;
    [SerializeField] bool startState = false;
    bool currentState;

    private void Awake()
    {
        currentState = startState;
    }

    private void Start()
    {
        InvokeCallback(currentState);
    }

    public void ChangeState(bool newState)
    {
        currentState = newState;
        InvokeCallback(currentState);
    }

    public void ToggleState()
    {
        ChangeState(!currentState);
    }

    void InvokeCallback(bool var)
    {
        callback?.Invoke(var);
    }
}
