using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrainModel : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject headJoint, tailJoint;
    [SerializeField] Animator leftDoorsAnim, rightDoorsAnim;
    [SerializeField] HeadTrainModel headVagon, tailVagon;

    [Header("Train number management")]
    [Space(10f)]
    [SerializeField] int trainNumber = 2646;
    [SerializeField] TrainNumberGenerator trainNumberLeft, trainNumberRight;
    
    private bool leftDoorsOpened = false, rightDoorsOpened = false;

    [Header("Movement")]
    [SerializeField] private SplineContainer rail;
    [SerializeField] private bool invertRotation = false;

    private float frictionDeceleration = 0.025f;
    [SerializeField] private float currentSpeedMagnitude = 0f;
    [SerializeField] private Vector3 currentSpeed = Vector3.zero;

    [Header("Braking")]
    [SerializeField] private bool braking = false;
    [SerializeField] private bool releasing = false;
    [SerializeField] private bool compressorActive = false;
    private float brakingDeceleration = 1.8f;
    [SerializeField] private float brakeMagistralPressure = 0f;
    [SerializeField] private float naporMagistralPressure = 0f;
    [SerializeField] private float brakeCylindersPressure = 2f;

    [Header("Braking constants")]
    [SerializeField] private float brakeMagistralMaxPressure = 5.2f;
    [SerializeField] private float naporMagistralMaxPressure = 7.5f;
    [SerializeField] private float brakeCylindersMaxPressure = 2f;
    [SerializeField] private float cylindersPressureChangeSpeed = 1f;
    [SerializeField] private float magistralPressureChangeSpeed = .5f;
    [SerializeField] private float compressorPressureChargeSpeed = .125f;

    [Header("Braking system visualisation")]
    [SerializeField] private PressureGauge brakeMagistralGauge;
    [SerializeField] private PressureGauge naporMagistralGauge;
    [SerializeField] private PressureGauge brakeCylindersGauge;

    [Header("Indication lamps")]
    [SerializeField] private TrainIndicationLampController brakeLamp;
    [SerializeField] private TrainIndicationLampController doorLamp;

    [Header("Vagon lamps")]
    [SerializeField] private TrainLampController alarmLampController;
    [SerializeField] private TrainLampController regularLampController;

    bool regularLampsOn = false;

    private Spline currentSpline;

    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    public GameObject GetHeadJoint()
    {
        return headJoint;
    }

    public GameObject GetTailJoint()
    {
        return tailJoint;
    }

    public bool IsPoweredUp() => (headVagon != null ? headVagon.IsActive() : false) || (tailVagon != null ? tailVagon.IsActive() : false);

    public bool IsBraking() => braking;
    public void SetBraking(bool b) => braking = b;

    public bool IsReleasing() => releasing;
    public void SetReleasing(bool r) => releasing = r;

    public bool IsCompressorActive() => compressorActive;
    public void SetCompressorActive(bool a) => compressorActive = a; 

    public bool IsBraked() => brakeCylindersPressure > 0f;
    public float GetBrakeStrength() => brakeCylindersPressure / brakeCylindersMaxPressure;

    public bool IsInvertRotation() => invertRotation;

    private bool ShouldInvert()
    {
        bool headActive = headVagon.IsActive();
        bool tailActive = tailVagon.IsActive();
        if (headActive)
        {
            return tailVagon == this;
        }
        else if (tailActive)
        {
            return tailVagon != this;
        }
        return false;
    }

    public Animator GetLeftDoorsAnimator()
    {
        return ShouldInvert() ? rightDoorsAnim : leftDoorsAnim;
    }

    public Animator GetRightDoorsAnimator()
    {
        return ShouldInvert() ? leftDoorsAnim : rightDoorsAnim;
    }

    public bool LeftDoorsOpened()
    {
        return ShouldInvert() ? rightDoorsOpened : leftDoorsOpened;
    }

    public bool RightDoorsOpened()
    {
        return ShouldInvert() ? leftDoorsOpened : rightDoorsOpened;
    }

    public void SetLeftDoorsOpened(bool opened)
    {
        if (ShouldInvert())
            rightDoorsOpened = opened;
        else
            leftDoorsOpened = opened;

        GetLeftDoorsAnimator().SetBool("opened", LeftDoorsOpened());
    }

    public void SetRightDoorsOpened(bool opened)
    {
        if (ShouldInvert())
            leftDoorsOpened = opened;
        else
            rightDoorsOpened = opened;

        GetRightDoorsAnimator().SetBool("opened", RightDoorsOpened());
    }

    public int GetTrainNumber()
    {
        return trainNumber;
    }

    public void SetTrainNumber(int number)
    {
        if (0 < number && number < 100000)
        {
            trainNumber = number;

            trainNumberLeft.Number = trainNumber;
            trainNumberRight.Number = trainNumber;

            trainNumberLeft.GenerateNumber();
            trainNumberRight.GenerateNumber();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: ����� ������������ ����� ������ ({number})");
        }
    }

    public TrainNumberGenerator GetTrainNumberLeft()
    {
        return trainNumberLeft;
    }

    public TrainNumberGenerator GetTrainNumberRight()
    {
        return trainNumberRight;
    }

    public void HitJunction(SplineContainer newRail)
    {
        rail = newRail;
        currentSpline = rail.Splines[0];
    }

    public SplineContainer GetCurrentRail() => rail;

    public float GetCurrentSpeed()
    {
        return currentSpeedMagnitude;
    }

    public void SetCurrentSpeed(float newSpeed) {
        currentSpeedMagnitude = newSpeed;
    }

    public Vector3 GetCurrentSpeedVector() => currentSpeed;

    public void SetCurrentSpeedVector(Vector3 newVector) {
        currentSpeed = newVector;
    }

    public bool RegularLampsOn() => regularLampsOn;
    public void SetRegularLampsState(bool newState) => regularLampsOn = newState;

    private void Awake()
    {
        if (rail != null) currentSpline = rail.Splines[0];
        SetTrainNumber(trainNumber);
    }

    private void Update()
    {
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        brakeLamp.ChangeState(IsPoweredUp() && IsBraked());
        doorLamp.ChangeState(IsPoweredUp() && (leftDoorsOpened || rightDoorsOpened));
        UpdateLamps();
        UpdateBrakingSystem();
    }

    private void UpdateBrakingSystem()
    {
        float dt = Time.deltaTime;
        if (braking)
        {
            float bmPressureToUse = magistralPressureChangeSpeed * dt;
            float bmPressureUsed = Mathf.Min(bmPressureToUse, brakeMagistralPressure);
            float ratio = bmPressureUsed / bmPressureToUse;

            float cylindersPressureToReceive = cylindersPressureChangeSpeed * ratio * dt;
            float cylindersPressureReceived = Mathf.Min(cylindersPressureToReceive, brakeCylindersMaxPressure - brakeCylindersPressure);

            brakeMagistralPressure -= bmPressureUsed;
            brakeCylindersPressure += cylindersPressureReceived;
        }
        else if (releasing)
        {
            float cylindersPressureToRelease = cylindersPressureChangeSpeed * dt;
            float cylindersPressureReleased = Mathf.Min(cylindersPressureToRelease, brakeCylindersPressure);

            brakeCylindersPressure -= cylindersPressureReleased;

            if (naporMagistralPressure > brakeMagistralPressure && brakeMagistralPressure < brakeMagistralMaxPressure)
            {
                float meanMagistralPressure = (naporMagistralPressure + brakeMagistralPressure) / 2f;
                float nmPressureToUse = magistralPressureChangeSpeed * dt;
                float nmPressureUsed = Mathf.Min(nmPressureToUse, naporMagistralPressure - meanMagistralPressure);

                naporMagistralPressure -= nmPressureUsed;
                brakeMagistralPressure += nmPressureUsed;
            }
        }

        if (compressorActive)
        {
            float compressorChargeAmount = compressorPressureChargeSpeed * dt;

            float nmPressureCharged = Mathf.Min(compressorChargeAmount, naporMagistralMaxPressure - naporMagistralPressure);

            naporMagistralPressure += nmPressureCharged;
        }

        if (brakeCylindersGauge != null) brakeCylindersGauge.SetPressure(brakeCylindersPressure);
        if (brakeMagistralGauge != null) brakeMagistralGauge.SetPressure(brakeMagistralPressure);
        if (naporMagistralGauge != null) naporMagistralGauge.SetPressure(naporMagistralPressure);
    }

    private void UpdateLamps() {
        bool alarmLampState = IsPoweredUp();
        bool regularLampState = alarmLampState && regularLampsOn;

        if (alarmLampState != alarmLampController.IsActive()) alarmLampController.SetState(alarmLampState);
        if (regularLampState != regularLampController.IsActive()) regularLampController.SetState(regularLampState);
    }

    private void FixedUpdate()
    {
        // изменяем текущую скорость, учитывая трение и наклоны
        SetCurrentSpeed(Mathf.Sign(currentSpeed.magnitude) * Mathf.Clamp(Mathf.Abs(currentSpeed.magnitude) - frictionDeceleration * Time.fixedDeltaTime, 0f, 25f));
        SetCurrentSpeedVector(currentSpeed.normalized * currentSpeedMagnitude);

        // возвращаем вагон к ближайшей точке текущего сплайна с учетом его текущей скорости
        var native = new NativeSpline(currentSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position + currentSpeed * Time.fixedDeltaTime * (ShouldInvert() ? -1 : 1), out float3 nearest, out float t);

        rb.MovePosition(nearest);

        // поворачиваем вагон вдоль касательной к текущей точке сплайна
        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t)) * (invertRotation ? -1 : 1);
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        rb.MoveRotation(Quaternion.LookRotation(forward, up) * axisRemapRotation);

        // Vector3 engineForward = transform.forward;

        // if (invertRotation)
        // {
        //     engineForward *= -1;
        // }

        if (IsBraked() && Mathf.Abs(currentSpeedMagnitude) > 0f)
        {
            Vector3 newVelocity = currentSpeed;
            float newSpeed = Mathf.Sign(newVelocity.magnitude) * Mathf.Clamp(Mathf.Abs(newVelocity.magnitude) - brakingDeceleration * GetBrakeStrength() * Time.fixedDeltaTime, 0f, 25f);

            currentSpeed = newVelocity.normalized * newSpeed;
            currentSpeedMagnitude = newSpeed;
        }

        if (this is HeadTrainModel htm && htm.IsActive())
        {
            // currentSpeed = currentSpeedMagnitude * engineForward;
            foreach (TrainModel vagon in htm.chainedVagons)
            {
                if (vagon != this)
                {
                    vagon.SetCurrentSpeed(currentSpeedMagnitude);
                    Vector3 newSpeedVector = vagon.transform.forward * currentSpeedMagnitude;
                    vagon.SetCurrentSpeedVector(newSpeedVector);
                }
            }
        }
    }
}
