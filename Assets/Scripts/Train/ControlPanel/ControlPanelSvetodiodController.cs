using UnityEngine;

public class ControlPanelSvetodiodController : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] Light _light;
    Material material;
    bool active = false;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        if (_light == null) _light = GetComponent<Light>();
        if (_light != null)
        {
            _light.enabled = false;
            _light.color = color;
        }
        ChangeState(false);
    }

    public void ChangeState(bool state)
    {
        active = state;
        UpdateLights();
    }

    void UpdateLights()
    {
        _light.enabled = active;
        material.SetColor("_EmissionColor", active ? color : Color.black);
    }
}
