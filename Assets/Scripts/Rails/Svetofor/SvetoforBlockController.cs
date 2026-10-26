using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SvetoforBlockState
{
    public int upperSvetoforState = 0,
        lowerSvetoforState = 0;
}
public class SvetoforBlockController : MonoBehaviour
{
    [SerializeField] SvetoforController upperSvetofor, lowerSvetofor;

    [SerializeField] List<SvetoforBlockState> states = new ();

    private void Start()
    {
        SetState(0);
    }

    private void ApplyState(int index)
    {
        SvetoforBlockState state = states[index];

        upperSvetofor.SetState(state.upperSvetoforState);
        lowerSvetofor.SetState(state.lowerSvetoforState);
    }

    public void SetState(int index)
    {
        if (index < 0 || index >= states.Count)
        {
            Debug.LogWarning($"{gameObject.name}: Attempted to set state = {index} (available states - 0-{states.Count - 1})");
        }
        ApplyState(index);
    }
}
