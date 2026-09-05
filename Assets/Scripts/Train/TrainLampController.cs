using UnityEngine;
using System.Collections.Generic;

public class TrainLampController : MonoBehaviour
{
    [SerializeField] Color color = Color.white;
    [SerializeField] Transform lampObject;
    [SerializeField] List<LightRenderController> lights = new();
    MeshRenderer meshRenderer;
    bool active = false;
    
    private void Start() {
        meshRenderer = (lampObject != null ? lampObject : transform).GetComponent<MeshRenderer>();

        SetState(false);
    }

    public bool IsActive() => active;

    public void SetState(bool newState) {
        active = newState;
        UpdateLamp();
    }

    private void UpdateLamp() {
        if (meshRenderer != null) meshRenderer.material.SetColor("_EmissionColor", active ? color : Color.black);

        foreach(LightRenderController light in lights) {
            light.SetShouldBeEnabled(active);
        }
    }
}
