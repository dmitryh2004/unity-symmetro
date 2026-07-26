using UnityEngine;
using UnityEngine.InputSystem;

public class TrainEngine : MonoBehaviour
{
    bool active = false;
    int acceleration = 0; // from 1 to 4 - accelerate, from -4 to -1 - break
    [SerializeField] float minSpeed = 0f, maxSpeed = 20f;

    [SerializeField] Rigidbody rb;

    [SerializeField] private float power;

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

    public void SpeedUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        acceleration += 1;
        if (acceleration > 4) acceleration = 4;
    }

    public void SpeedDown(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        acceleration -= 1;
        if (acceleration < -4) acceleration = -4;
    }

    private void Throttle(float power)
    {
        float factor = acceleration / 4f;
        Vector3 dir = factor * power * transform.forward;
        rb.AddForce(dir);

        float speed = rb.linearVelocity.magnitude * (Vector3.Dot(transform.forward, rb.linearVelocity) < 0 ? -1 : 1);

        if (speed < minSpeed)
        {
            rb.linearVelocity = dir * minSpeed;
        }
        if (speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
