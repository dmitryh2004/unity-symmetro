using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrainModel : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] GameObject headJoint, tailJoint;
    [SerializeField] Animator leftDoorsAnim, rightDoorsAnim;
    [SerializeField] HeadTrainModel headVagon, tailVagon;

    [Header("Train number management")]
    [Space(10f)]
    [SerializeField] int trainNumber = 2646;
    [SerializeField] TrainNumberGenerator trainNumberLeft, trainNumberRight;
    
    private bool leftDoorsOpened = false, rightDoorsOpened = false;

    [Header("Movement")]
    [SerializeField] protected SplineContainer rail;
    [SerializeField] protected bool invertRotation = false;
    protected float splineDirectionSign = 1f;
    [SerializeField] protected float vagonSpacing = 19.25f;
    [SerializeField] protected float currentSplineDistance = 0f;
    [SerializeField] protected float currentSplineLength = 0f;

    protected float frictionDeceleration = 0.025f;
    [SerializeField] protected float currentSpeedMagnitude = 0f;
    [SerializeField] protected Vector3 currentSpeed = Vector3.zero;

    [Header("Braking")]
    [SerializeField] protected bool braking = false;
    [SerializeField] protected bool releasing = false;
    [SerializeField] protected bool compressorActive = false;
    protected float brakingDeceleration = 1.8f;
    [SerializeField] protected float brakeMagistralPressure = 0f;
    [SerializeField] protected float naporMagistralPressure = 0f;
    [SerializeField] protected float brakeCylindersPressure = 2f;

    [Header("Braking constants")]
    [SerializeField] protected float brakeMagistralMaxPressure = 5.2f;
    [SerializeField] protected float naporMagistralMaxPressure = 7.5f;
    [SerializeField] protected float brakeCylindersMaxPressure = 2f;
    [SerializeField] protected float cylindersPressureChangeSpeed = 1f;
    [SerializeField] protected float magistralPressureChangeSpeed = .5f;
    [SerializeField] protected float compressorPressureChargeSpeed = .125f;

    [Header("Braking system visualisation")]
    [SerializeField] protected PressureGauge brakeMagistralGauge;
    [SerializeField] protected PressureGauge naporMagistralGauge;
    [SerializeField] protected PressureGauge brakeCylindersGauge;

    [Header("Indication lamps")]
    [SerializeField] protected TrainIndicationLampController brakeLamp;
    [SerializeField] protected TrainIndicationLampController doorLamp;

    [Header("Vagon lamps")]
    [SerializeField] protected TrainLampController alarmLampController;
    [SerializeField] protected TrainLampController regularLampController;

    bool regularLampsOn = false;
    
    protected Spline currentSpline;

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

    public bool IsInvertRotation() => invertRotation;
    public float GetSplineDirectionSign() => splineDirectionSign;

    public bool IsBraked() => brakeCylindersPressure > 0f;
    public float GetBrakeStrength() => brakeCylindersPressure / brakeCylindersMaxPressure;

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
        InitializeCurrentSpline();
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

    private void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        InitializeCurrentSpline();
    }

    public void InitializeCurrentSpline()
    {
        // Пример: при инициализации определяем знак один раз
        var native = new NativeSpline(currentSpline);
        currentSplineLength = currentSpline.GetLength();

        float3 localPosition = rail.transform.InverseTransformPoint(transform.position);

        SplineUtility.GetNearestPoint(native, localPosition, out float3 nearest, out float t);
        currentSplineDistance = SplineUtility.ConvertIndexUnit(native, t, PathIndexUnit.Normalized, PathIndexUnit.Distance);

        Vector3 splineTangent = Vector3.Normalize(native.EvaluateTangent(t));

        // Допустим, изначально хотим, чтобы forward вагона совпадал с tangent
        splineDirectionSign = Vector3.Dot(transform.forward, splineTangent) >= 0f ? 1f : -1f;
        // splineDirectionSign *= invertRotation ? -1f : 1f;

        Debug.Log($"{gameObject.name}: forward = {transform.forward}, spline tangent = {splineTangent}, dot = {Vector3.Dot(transform.forward, splineTangent)}, spline direction sign = {splineDirectionSign}");
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

    // Этот метод вызывает голова поезда, передавая вычисленные из истории мировые координаты
    public void MoveVagonExplicitly(Vector3 worldPosition, Quaternion worldRotation)
    {
        rb.MovePosition(worldPosition);
        rb.MoveRotation(worldRotation);
    }
}
