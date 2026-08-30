using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected float interactionRange = 1f;

    public float GetInteractionRange() => interactionRange;

    public abstract void Interact();
}
