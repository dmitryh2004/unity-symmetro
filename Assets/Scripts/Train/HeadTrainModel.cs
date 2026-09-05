using System.Collections.Generic;
using UnityEngine;

public class HeadTrainModel : TrainModel
{
    [Header("Head part management")]
    [SerializeField] bool active;
    [SerializeField] bool lightsEnabled;
    [SerializeField] List<LightRenderController> headLights = new ();
    [SerializeField] List<LightEmissivePartController> headLightControllers = new ();
    [SerializeField] List<LightEmissivePartController> upperHeadLightControllers = new ();

    [SerializeField] TrainEngine engine;
    [SerializeField] TrainLampController cabinLight;
    bool cabinLightEnabled = false;

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

    public void SetCabinLightEnabled(bool enabled)
    {
        cabinLightEnabled = enabled;
    }

    public void UpdateLights()
    {
        bool lightCondition = active && lightsEnabled;
        bool upperLightCondition = IsPoweredUp() && !active;
        foreach (LightRenderController light in headLights)
        {
            light.SetShouldBeEnabled(lightCondition);
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

        bool cabinLightEnabled = IsPoweredUp() && this.cabinLightEnabled;
        if (cabinLightEnabled != cabinLight.IsActive()) cabinLight.SetState(cabinLightEnabled);
    }
}
