using UnityEngine;

public abstract class IBoolCondition : MonoBehaviour
{
    [SerializeField] string lockedReasonText;

    public abstract bool Check();
    public string GetUnavailableReasonText() => lockedReasonText;
}
