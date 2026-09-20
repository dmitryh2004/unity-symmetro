using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactionLayer;
    private Interactable lastSeenInteractable = null;

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

        Debug.Log($"Trying to interact - origin: {ray.origin}, direction: {ray.direction}");

        if (Physics.Raycast(ray, out RaycastHit hit,
            10f,
            interactionLayer,
            QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"player: found {hit.collider.gameObject.name} at position = {hit.point}");
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                Debug.Log($"{hit.collider.gameObject.name} is interactable");
                if (hit.distance < interactable.GetInteractionRange())
                {
                    interactable.Interact(); // взаимодействуем по рейкасту
                    return;
                }
                else
                {
                    //Debug.Log($"{interactable.gameObject.name}: out of interaction range ({interactable.GetInteractionRange()})");
                }
            }
        }

        if (lastSeenInteractable != null) // если не получилось взаимодействовать через рейкаст, то пробуем взаимодействовать по ссылке на последний увиденный объект
            lastSeenInteractable.Interact();
    }

    private void UpdateInteractionHints()
    {
        if (cursorImage == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, interactionLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                lastSeenInteractable = interactable;
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
        lastSeenInteractable = null;
        cursorImage.color = defaultColor;
        if (hintText != null) hintText.text = string.Empty;
    }
}
