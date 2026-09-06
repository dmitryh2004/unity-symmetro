using UnityEngine;

public class ControlPanelSvetodiodController : MonoBehaviour
{
    [SerializeField] Color color;
    Material material;
    bool active = false;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    private void Start() {
        ChangeState(false);
    }

    public void ChangeState(bool state)
    {
        active = state;
        UpdateLights();
    }

    void UpdateLights()
    {
        material.SetColor("_EmissionColor", active ? color : Color.black);
    }
}
