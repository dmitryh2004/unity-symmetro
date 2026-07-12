using UnityEngine;

public class LightRenderController : MonoBehaviour
{
    [SerializeField] Camera renderCamera;
    [SerializeField] Light lightComponent;
    [SerializeField] float range = 150f;
    [SerializeField] float updatePeriod = .5f;
    float cameraFovCos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lightComponent == null) lightComponent = GetComponent<Light>();
        if (renderCamera == null) renderCamera = Camera.main;
        cameraFovCos = Mathf.Cos(renderCamera.fieldOfView * Mathf.Deg2Rad);
        InvokeRepeating("UpdateLight", 0f, updatePeriod);
    }

    void UpdateLight()
    {
        cameraFovCos = Mathf.Cos(renderCamera.fieldOfView * Mathf.Deg2Rad); // update camera fov
        Vector3 cameraDirection = renderCamera.transform.forward;
        Vector3 directionToLight = transform.position - renderCamera.transform.position;

        bool disabledConditions = Vector3.Distance(transform.position, renderCamera.transform.position) > range;
        if (!disabledConditions)
        {
            if (Vector3.Dot(cameraDirection.normalized, directionToLight.normalized) < cameraFovCos)
            {

            }
        }
        lightComponent.enabled = !disabledConditions;
    }
}
