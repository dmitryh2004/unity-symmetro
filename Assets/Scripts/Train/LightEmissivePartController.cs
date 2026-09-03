using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LightPart {
    public Transform obj;
    public float emissionStrength;
}

public class LightEmissivePartController : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] bool _enabled = false;
    [SerializeField] List<LightPart> lightParts;
    [SerializeField] GameObject flaresParent;
    List<MeshRenderer> lightPartsMR = new();
    private void Awake()
    {
        foreach (var lightPart in lightParts) {
            lightPartsMR.Add(lightPart.obj.GetComponent<MeshRenderer>());
        }
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
        for (int i = 0; i < lightParts.Count; i++) 
        {
            float emissionStrength = lightParts[i].emissionStrength;
            MeshRenderer lightPartMR = lightPartsMR[i];
            if (_enabled == true)
            {
                lightPartMR.materials[0].SetColor("_EmissionColor", color * emissionStrength);
            }
            else
            {
                lightPartMR.materials[0].SetColor("_EmissionColor", Color.black);
            }
        }

        if (flaresParent != null)
            flaresParent.SetActive(_enabled);
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
