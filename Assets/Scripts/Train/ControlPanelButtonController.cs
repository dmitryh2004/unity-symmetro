using UnityEngine;
using UnityEngine.Events;

public class ControlPanelButton : MonoBehaviour
{
    [SerializeField] UnityEvent callback;

    public void Press()
    {
        InvokeCallback();
    }

    void InvokeCallback()
    {
        callback?.Invoke();
    }
}
