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
            TrainModel model = tjr.GetModel();
            linkedRailBlock.AddOccupier(model);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.TryGetComponent(out TrainJunctionRegister tjr)) return;
        if (output)
        {
            TrainModel model = tjr.GetModel();
            linkedRailBlock.RemoveOccupier(model);
        }
    }
}
