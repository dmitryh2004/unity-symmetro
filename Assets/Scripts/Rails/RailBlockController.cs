using System.Collections.Generic;
using UnityEngine;

public class RailBlockController : MonoBehaviour
{
    [SerializeField] RailBlockController prev1, prev2, prev3, prev4;
    [SerializeField] RailBlockController next1, next2, next3, next4;
    [SerializeField] bool occupied = false;
    [SerializeField] List<Transform> occupiers = new();
    [SerializeField] List<SvetoforBlockController> linkedSvetofors = new();

    public bool IsOccupied() => occupied;
    public int GetNumberOfFreeNextBlocks()
    {
        if (IsOccupied()) return 0;
        if (next1 != null && next1.IsOccupied()) return 1;
        if (next2 != null && next2.IsOccupied()) return 2;
        if (next3 != null && next3.IsOccupied()) return 3;
        if (next4 != null && next4.IsOccupied()) return 4;
        return 4;
    }

    public void AddOccupier(Transform occupier)
    {
        if (!occupiers.Contains(occupier))
        {
            occupiers.Add(occupier);
            occupied = true;
            UpdateSvetofors();
        }
    }

    public void RemoveOccupier(Transform occupier)
    {
        if (occupiers.Contains(occupier))
        {
            occupiers.Remove(occupier);
        }
        occupied = occupiers.Count > 0;
        UpdateSvetofors();
        if (prev1 != null) prev1.UpdateSvetofors();
        if (prev2 != null) prev2.UpdateSvetofors();
        if (prev3 != null) prev3.UpdateSvetofors();
        if (prev4 != null) prev4.UpdateSvetofors();
    }

    public void UpdateSvetofors()
    {
        foreach (SvetoforBlockController svetofor in linkedSvetofors)
        {
            svetofor.SetState(GetNumberOfFreeNextBlocks());
        }
    }
}
