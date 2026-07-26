using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrainModel : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject headJoint, tailJoint;
    [SerializeField] Animator anim;

    [Header("Train number management")]
    [Space(10f)]
    [SerializeField] int trainNumber = 2646;
    [SerializeField] TrainNumberGenerator trainNumberLeft, trainNumberRight;

    [Header("Doors management")]
    [Space(10f)]
    [SerializeField] bool invertDoors = false;

    [Header("Movement")]
    [SerializeField] private SplineContainer rail;
    [SerializeField] private bool invertRotation = false;

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

    public Animator GetAnimator()
    {
        return anim;
    }

    public bool DoorsInverted()
    {
        return invertDoors;
    }

    public void InvertDoors(bool invert)
    {
        invertDoors = invert;
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
            Debug.LogWarning($"{gameObject.name}: задан неправильный номер вагона ({number})");
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

    private void Awake()
    {
        if (rail != null) currentSpline = rail.Splines[0];
        SetTrainNumber(trainNumber);
    }

    private void FixedUpdate()
    {
        var native = new NativeSpline(currentSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        transform.position = nearest;

        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t)) * (invertRotation ? -1 : 1);
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        Vector3 engineForward = transform.forward;

        if (Vector3.Dot(rb.linearVelocity, transform.forward) < 0)
        {
            engineForward *= -1;
        }

        if (this is HeadTrainModel htm && htm.IsActive())
            rb.linearVelocity = rb.linearVelocity.magnitude * engineForward;
    }
}
