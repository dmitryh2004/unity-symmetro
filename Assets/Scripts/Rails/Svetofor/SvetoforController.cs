using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SvetoforState
{
    public Color topLightColor = Color.red;
    public bool topLightActive;
    [Space]
    public Color bottomLightColor = Color.red;
    public bool bottomLightActive;
    [Space]
    public bool bottomTextActive = false;
    public string bottomTextText;
}

public class SvetoforController : MonoBehaviour
{
    [SerializeField] SvetoforLight topLight, bottomLight;
    [SerializeField] TMP_Text bottomText;
    [SerializeField] List<SvetoforState> states = new();

    private void Start()
    {
        SetState(0);
    }

    private void ApplyState(int index)
    {
        SvetoforState state = states[index];

        topLight.Init(state.topLightColor, state.topLightActive);
        bottomLight.Init(state.bottomLightColor, state.bottomLightActive);
        bottomText.gameObject.SetActive(state.bottomTextActive);
        bottomText.SetText(state.bottomTextText);
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
