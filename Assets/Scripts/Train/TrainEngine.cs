using UnityEngine;
using UnityEngine.InputSystem;

public class TrainEngine : MonoBehaviour
{
    bool active = false;
    int acceleration = 0; // from 1 to 4 - accelerate, from -4 to -1 - break
    [SerializeField] float minSpeed = 0f, maxSpeed = 20f;

    [SerializeField] HeadTrainModel headTrainModel;
    [SerializeField] Rigidbody rb;

    [SerializeField] private float power;
    [SerializeField] private Animator stickAnim;

    PlayerControls controls;
    
    public void SetActive(bool active) => this.active = active;

    private void Awake()
    {
        controls = new();
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

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
    }

    public void SpeedDown()
    {
        acceleration -= 1;
        if (acceleration < -4) acceleration = -4;
        if (stickAnim != null) stickAnim.SetInteger("acceleration", acceleration);
    }

    private void Throttle(float power)
    {
        float factor = acceleration / 4f;
        Vector3 dir = factor * power * transform.forward;
        rb.AddForce(dir);

        float speed = headTrainModel.GetCurrentSpeed() * (Vector3.Dot(transform.forward, headTrainModel.GetCurrentSpeedVector()) < 0 ? -1 : 1);

        if (speed < minSpeed)
        {
            headTrainModel.SetCurrentSpeedVector(dir * minSpeed);
        }
        if (speed > maxSpeed)
        {
            headTrainModel.SetCurrentSpeedVector(headTrainModel.GetCurrentSpeedVector().normalized * maxSpeed);
        }
    }
}
