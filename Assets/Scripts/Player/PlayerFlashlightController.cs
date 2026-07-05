using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlashlightController : MonoBehaviour
{
    [SerializeField] Light flashlight;
    private PlayerControls controls;
    [SerializeField] bool startActive = false;
    bool active;

    private void Awake()
    {
        controls = new PlayerControls();
        SetActive(startActive);
    }
    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void UpdateFlashlightState(InputAction.CallbackContext context)
    {
        if (context.performed) SetActive(!active);
    }

    public void SetActive(bool newActive)
    {
        active = newActive;
        UpdateFlashlight();
    }

    void UpdateFlashlight()
    {
        flashlight.enabled = active;
    }
}
