using UnityEngine;

public class RailDeadEnd : MonoBehaviour
{
    [SerializeField] bool beginIsDeadEnd = false;
    [SerializeField] bool endIsDeadEnd = false;
    [SerializeField] float deadEndThreshold = 5f;

    public bool BeginIsDeadEnd => beginIsDeadEnd;
    public bool EndIsDeadEnd => endIsDeadEnd;
    public float DeadEndThreshold => deadEndThreshold;
}
