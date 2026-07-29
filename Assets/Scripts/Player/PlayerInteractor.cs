using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactionLayer;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }
    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryInteract();
    }

    private void Update()
    {
        UpdateInteractionHints();
    }

    private void TryInteract()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit,
            10f,
            interactionLayer,
            QueryTriggerInteraction.Ignore))
        {
            //Debug.Log($"player: found {hit.collider.gameObject.name}");
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                // Debug.Log($"{hit.collider.gameObject.name} is interactable");
                if (hit.distance < interactable.GetInteractionRange())
                {
                    interactable.Interact();
                }
                else
                {
                    //Debug.Log($"{interactable.gameObject.name}: out of interaction range ({interactable.GetInteractionRange()})");
                }
            }
        }
    }

    private void UpdateInteractionHints()
    {
        
    }
}
