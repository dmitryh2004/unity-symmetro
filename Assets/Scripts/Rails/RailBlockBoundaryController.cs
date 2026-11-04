using UnityEngine;

public class RailBlockBoundaryController : MonoBehaviour
{
    [SerializeField] RailBlockController linkedRailBlock;
    [SerializeField] bool input = false, output = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.TryGetComponent(out TrainJunctionRegister tjr)) return;
        if (input)
        {
            linkedRailBlock.AddOccupier(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.TryGetComponent(out TrainJunctionRegister tjr)) return;
        if (output)
        {
            linkedRailBlock.RemoveOccupier(other.transform);
        }
    }
}
