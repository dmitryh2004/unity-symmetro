using UnityEngine;

public class SvetoforLight : MonoBehaviour
{
    Material material;
    bool active;
    Color lightColor;
    bool initialized = false;

    public void SetLightColor(Color color, bool forceActivate = false)
    {
        if (!initialized)
        {
            Debug.LogError($"{gameObject.name}: svetofor light not initialized!");
            return;
        }
        lightColor = color;
        SetActive(forceActivate || active);
    }

    public void Init(Color color, bool active)
    {
        material = GetComponent<Renderer>().material;
        initialized = true;
        SetLightColor(color);
        SetActive(active);
    }

    public void SetActive(bool active)
    {
        if (!initialized)
        {
            Debug.LogError($"{gameObject.name}: svetofor light not initialized!");
            return;
        }
        this.active = active;
        material.SetColor("_EmissionColor", this.active ? lightColor : Color.black);
    }
}
