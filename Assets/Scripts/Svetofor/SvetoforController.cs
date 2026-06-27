using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SvetoforState
{
    public Color topLightColor = Color.red;
    public bool topLightActive;
    [Space]
    public Color bottomLightColor = Color.red;
    public bool bottomLightActive;
}

public class SvetoforController : MonoBehaviour
{
    [SerializeField] SvetoforLight topLight, bottomLight;
    [SerializeField] List<SvetoforState> states = new();

    private void Start()
    {
        SvetoforState startState = states[0];
        topLight.Init(startState.topLightColor, startState.topLightActive);
        bottomLight.Init(startState.bottomLightColor, startState.bottomLightActive);
    }
}
