using UnityEngine;

public class LightEmissivePartController : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] bool _enabled = false;
    [SerializeField] Transform lightPart;
    [SerializeField] GameObject flaresParent;
    MeshRenderer lightPartMR;
    private void Awake()
    {
        lightPartMR = lightPart.GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UpdateLight();
    }

    void ChangeState(bool newState)
    {
        _enabled = newState;
        Debug.Log(gameObject.name + ": active=" + _enabled);

        UpdateLight();
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

        flaresParent?.SetActive(_enabled);
    }

    public void Activate()
    {
        ChangeState(true);
    }

    public void Deactivate()
    {
        ChangeState(false);
    }

    public void Toggle()
    {
        ChangeState(!_enabled);
    }
}
