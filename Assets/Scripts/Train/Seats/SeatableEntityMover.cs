using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SeatableEntityMover : MonoBehaviour
{
    public EntityAttachmentZone CurrentZone { get; private set; }

    [Header("Input velocity in local space of zone")]
    public Vector3 inputVelocityLocal; // то, что ты задаёшь как "скорость игрока"

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetZone(EntityAttachmentZone zone)
    {
        CurrentZone = zone;
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocityWorld = inputVelocityLocal;

        if (CurrentZone != null)
        {
            targetVelocityWorld += CurrentZone.WorldVelocity;

            var omega = CurrentZone.WorldAngularVelocity; // в рад/с
            var r = transform.position - CurrentZone.transform.position;
            var rotationVelocity = Vector3.Cross(omega, r);

            targetVelocityWorld += rotationVelocity;
        }
        else
        {

        }

        rb.linearVelocity = targetVelocityWorld;
        // Debug.Log($"inputVelocityLocal = {inputVelocityLocal}; current zone velocity = {CurrentZone.WorldVelocity}; result = {rb.linearVelocity}");
    }
}
