using UnityEngine;

public class TrainIndicationLampController : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] bool _enabled = false;
    [SerializeField] Transform lightPart;
    MeshRenderer lightPartMR;
    private void Awake()
    {
        lightPartMR = lightPart.GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UpdateLight();
    }

    public void ChangeState(bool newState)
    {
        bool oldEnabled = _enabled;
        _enabled = newState;

        if (oldEnabled != newState)
        {
            Debug.Log(gameObject.name + ": active=" + _enabled);
            UpdateLight();
        }
    }

    void UpdateLight()
    {
        if (_enabled == true)
        {
            lightPartMR.materials[0].SetColor("_EmissionColor", color);
        }
        else
        {
            lightPartMR.materials[0].SetColor("_EmissionColor", Color.black);
        }
    }

    public bool IsEnabled() => _enabled;

    public void Toggle()
    {
        ChangeState(!_enabled);
    }
}
