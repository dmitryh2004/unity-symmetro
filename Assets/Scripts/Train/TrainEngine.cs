using UnityEngine;
using UnityEngine.InputSystem;

public class TrainEngine : MonoBehaviour
{
    bool active = false;
    int acceleration = 0; // from 1 to 4 - accelerate, from -4 to -1 - break
    [SerializeField] float minSpeed = 0f, maxSpeed = 20f;
    [SerializeField] float accelerationSpeed = 1.2f; // ��������� ����������� ����� ��� ������ ���������

    [SerializeField] HeadTrainModel headTrainModel;
    [SerializeField] Rigidbody rb;

    [SerializeField] private float power;
    [SerializeField] private Animator stickAnim;
    
    public void SetActive(bool active) => this.active = active;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!active) return;

        Throttle(power);
    }

    public void SpeedUp()
    {
        acceleration += 1;
        if (acceleration > 4) acceleration = 4;
        if (stickAnim != null) stickAnim.SetInteger("acceleration", acceleration);
        Debug.Log($"increased acceleration, now {acceleration}");
    }

    public void SpeedDown()
    {
        acceleration -= 1;
        if (acceleration < -4) acceleration = -4;
        if (stickAnim != null) stickAnim.SetInteger("acceleration", acceleration);
        Debug.Log($"decreased acceleration, now {acceleration}");
    }

    private void Throttle(float power)
    {
        bool invertRotation = headTrainModel.IsInvertRotation();

        float factor = acceleration / 4f;
        float speedChange = factor * accelerationSpeed * Time.fixedDeltaTime;
        Vector3 direction = transform.forward * (invertRotation ? -1 : 1);

        float newSpeed = (headTrainModel.GetCurrentSpeed() + speedChange) * (invertRotation ? -1 : 1);
        newSpeed = invertRotation ? Mathf.Clamp(newSpeed, -maxSpeed, -minSpeed) : Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        // if (newSpeed != 0f)
        //     Debug.Log($"newSpeed = {newSpeed}");

        headTrainModel.SetCurrentSpeed(newSpeed);
        headTrainModel.SetCurrentSpeedVector(direction * newSpeed);
    }
}
