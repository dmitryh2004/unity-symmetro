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

    [SerializeField] private bool braking = true;
    private float brakingDeceleration = 1.8f;

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

    public void HitJunction(Spline rail)
    {
        currentSpline = rail;
    }

    public float GetCurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
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
        brakeLamp.ChangeState(IsPoweredUp() && braking);
        doorLamp.ChangeState(IsPoweredUp() && (leftDoorsOpened || rightDoorsOpened));
        UpdateLamps();
    }

    private void UpdateLamps() {
        bool alarmLampState = IsPoweredUp();
        bool regularLampState = alarmLampState && regularLampsOn;

        if (alarmLampState != alarmLampController.IsActive()) alarmLampController.SetState(alarmLampState);
        if (regularLampState != regularLampController.IsActive()) regularLampController.SetState(regularLampState);
    }

    private void FixedUpdate()
    {
        var native = new NativeSpline(currentSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        rb.MovePosition(nearest);

        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t)) * (invertRotation ? -1 : 1);
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        rb.MoveRotation(Quaternion.LookRotation(forward, up) * axisRemapRotation);

        Vector3 engineForward = transform.forward;

        if (Vector3.Dot(rb.linearVelocity, transform.forward) < 0)
        {
            engineForward *= -1;
        }

        if (braking)
        {
            Vector3 newVelocity = rb.linearVelocity;
            float newSpeed = Mathf.Clamp(newVelocity.magnitude - brakingDeceleration * Time.fixedDeltaTime, 0f, 25f);

            newVelocity = newVelocity.normalized * newSpeed;
            rb.linearVelocity = newVelocity;
        }

        if (this is HeadTrainModel htm && htm.IsActive())
            rb.linearVelocity = rb.linearVelocity.magnitude * engineForward;
    }
}
