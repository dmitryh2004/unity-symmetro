using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactionLayer;

[Header("UI Hints")]
    [SerializeField] private UnityEngine.UI.Image cursorImage; // Ссылка на Image точки на экране
    [SerializeField] private Color defaultColor = Color.white;  // Белый — нет объекта или далеко
    [SerializeField] private Color activeColor = Color.green;   // Зеленый — можно взаимодействовать
    [SerializeField] private Color disabledColor = Color.red;   // Красный — объект есть, но недоступен

[Header("UI Text Hints")]
    [SerializeField] private TMP_Text hintText;

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
            Debug.Log($"player: found {hit.collider.gameObject.name}");
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                Debug.Log($"{hit.collider.gameObject.name} is interactable");
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
        if (cursorImage == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, interactionLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                bool isRangeValid = hit.distance < interactable.GetInteractionRange();
                bool isActionPossible = interactable.CanInteract(); 

                if (isRangeValid && isActionPossible)
                {
                    cursorImage.color = activeColor;
                    // Очищаем или скрываем текст, так как объект доступен
                    if (hintText != null) hintText.text = interactable.GetInteractionHint(); 
                    return;
                }
                else if (isRangeValid)
                {
                    cursorImage.color = disabledColor;

                    // Выводим причину, если мы подошли достаточно близко
                    if (hintText != null && isRangeValid)
                    {
                        // Запрашиваем причину у самого объекта
                        hintText.text = interactable.GetUnavailableReason();
                    }
                    return;
                }
            }
        }

        // Если ни на что не смотрим — сбрасываем цвет и очищаем текст
        cursorImage.color = defaultColor;
        if (hintText != null) hintText.text = string.Empty;
    }
}
