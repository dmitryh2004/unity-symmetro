using UnityEngine;
using System.Collections.Generic;

public class EntityAttachmentZone : MonoBehaviour
{
    [SerializeField] private List<SeatableEntityMover> _entitiesInside = new();

	public void AddNewEntity(SeatableEntity entity) {
		if (entity != null && !_entitiesInside.Contains(entity.GetMover()))
        {
            _entitiesInside.Add(entity.GetMover());
            entity.SetZone(this);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSeatable(other)) return;

        var entity = other.GetComponent<SeatableEntityMover>();
        if (entity != null && !_entitiesInside.Contains(entity))
        {
            _entitiesInside.Add(entity);
            entity.SetZone(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsSeatable(other)) return;

        var entity = other.GetComponent<SeatableEntityMover>();
        if (entity != null)
        {
            _entitiesInside.Remove(entity);
            if (entity.CurrentZone == this)
                entity.SetZone(null);
        }
    }

	private bool IsSeatable(Collider other) {
		if (other.isTrigger) return false;

        if (other.GetComponent<SeatableEntity>() != null)
            return true;

        return false;
	}

    public Vector3 WorldVelocity { get; private set; }
    public Vector3 WorldAngularVelocity { get; private set; }

    private Rigidbody rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // Вычисляем линейную и угловую скорость транспорта
        var dt = Time.fixedDeltaTime;
        if (dt > 0f)
        {
            var deltaPos = transform.position - lastPosition;
            WorldVelocity = deltaPos / dt;

            // Угловая скорость (упрощённо, если нужно)
            var deltaAngle = GetAngleDelta(lastRotation, transform.rotation, dt);
            WorldAngularVelocity = deltaAngle;
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private static Vector3 GetAngleDelta(Quaternion from, Quaternion to, float dt)
    {
        var delta = to * Quaternion.Inverse(from);
        delta.ToAngleAxis(out var angle, out var axis);
        if (angle > 180f) angle -= 360f;
        return axis * (angle * Mathf.Deg2Rad) / dt;
    }
}