using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactable : MonoBehaviour
{
    // Ссылка на ваше действие взаимодействия. 
    // Можно перенести её в базовый класс Interactable, чтобы не дублировать.
    [SerializeField] protected PlayerInput playerInput; 
    [SerializeField] protected float interactionRange = 1f;

    public float GetInteractionRange() => interactionRange;

    public abstract void Interact();

    void Start() 
    {
        Init();
    }

    protected virtual void Init() 
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public virtual bool CanInteract()
    {
        return true; // По умолчанию со всеми объектами можно взаимодействовать
    }

    public virtual string GetUnavailableReason()
    {
        return "Взаимодействие невозможно"; // Текст по умолчанию
    }

    public virtual string GetInteractionHint() 
    {
        // Используем новый метод и здесь для текста по умолчанию
        return $"[{GetInteractKeyDisplayString()}] - взаимодействовать"; 
    }

    /// <summary>
    /// Возвращает строковое отображение текущей клавиши взаимодействия из активной Action Map.
    /// </summary>
    protected string GetInteractKeyDisplayString()
    {
        if (playerInput != null && playerInput.currentActionMap != null)
        {
            // Ищем действие "Interact" в текущей активной карте
            InputAction interactAction = playerInput.currentActionMap.FindAction("Interact");

            if (interactAction != null)
            {
                // Возвращает актуальную клавишу (например: "E", "Space", "X" на геймпаде)
                return interactAction.GetBindingDisplayString();
            }
        }

        return "E"; // Запасной вариант по умолчанию, если ввод не найден
    }
}
