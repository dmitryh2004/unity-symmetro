using System.Collections.Generic;
using UnityEngine;

public class HeadTrainModel : TrainModel
{
    [Header("Head part management")]
    [SerializeField] bool active;
    [SerializeField] bool lightsEnabled;
    [SerializeField] List<Light> headLights = new ();
    [SerializeField] List<LightEmissivePartController> headLightControllers = new ();
    [SerializeField] List<LightEmissivePartController> upperHeadLightControllers = new ();

    [SerializeField] TrainEngine engine;

    [Header("Head panel")]
    [SerializeField] SpeedController speedController;

    private void Start()
    {
        UpdateLights();
        engine.SetActive(active);
    }

    public bool IsActive() => active;
    public void SetActive(bool active)
    {
        this.active = active;
        engine.SetActive(active);
        UpdateLights();
    }

    public void SetLightsEnabled(bool lightsEnabled)
    {
        this.lightsEnabled = lightsEnabled;
        UpdateLights();
    }

    void UpdateLights()
    {
        bool lightCondition = active && lightsEnabled;
        bool upperLightCondition = !active;
        foreach (Light light in headLights)
        {
            light.gameObject.SetActive(lightCondition);
        }
        foreach (LightEmissivePartController hlc in headLightControllers)
        {
            if (lightCondition) hlc.Activate(); else hlc.Deactivate();
        }
        foreach (LightEmissivePartController hlc in upperHeadLightControllers)
        {
            if (upperLightCondition) hlc.Activate(); else hlc.Deactivate();
        }
    }

    override protected void UpdateState()
    {
        base.UpdateState();
        speedController.SetSpeedText(GetRigidbody().linearVelocity.magnitude);
    }
}
