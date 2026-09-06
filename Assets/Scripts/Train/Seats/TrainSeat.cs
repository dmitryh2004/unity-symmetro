using System;
using UnityEngine;

public interface SeatableEntity
{
    void Seat(TrainSeat seat);
    void StandUp();
}

/// <summary>
/// Компонент, описывающий предмет, на котором могут сидеть игроки/NPC.
/// </summary>
public class TrainSeat : MonoBehaviour
{
    [Header("Seats")]
    [Tooltip("Позиции мест для сидения (в локальных координатах объекта).")]
    public Vector3[] seatPositions;

    [Header("Debug / Runtime")]
    [Tooltip("Занятость мест: true — занято, false — свободно.")]
    public bool[] seatOccupied;

    // Сущности, сидящие на местах (по индексу места)
    private SeatableEntity[] seatedEntities;

    private void Awake()
    {
        if (seatPositions == null)
            seatPositions = Array.Empty<Vector3>();

        // Инициализируем массивы занятости и сущностей по количеству мест.
        int seatCount = seatPositions.Length;
        seatOccupied = new bool[seatCount];
        seatedEntities = new SeatableEntity[seatCount];

        for (int i = 0; i < seatCount; i++)
        {
            seatOccupied[i] = false;
            seatedEntities[i] = null;
        }
    }

    #region Public API

    /// <summary>
    /// Сидит ли на этом предмете хотя бы один игрок/NPC.
    /// </summary>
    public bool IsSeated()
    {
        for (int i = 0; i < seatOccupied.Length; i++)
        {
            if (seatOccupied[i])
                return true;
        }
        return false;
    }

    /// <summary>
    /// Свободно ли хотя бы одно место.
    /// </summary>
    public bool HasAvailableSeats()
    {
        for (int i = 0; i < seatOccupied.Length; i++)
        {
            if (!seatOccupied[i])
                return true;
        }
        return false;
    }

    /// <summary>
    /// Свободно ли конкретное место (по индексу).
    /// </summary>
    public bool IsSeatAvailable(int seatIndex)
    {
        if (!IsValidSeatIndex(seatIndex))
            return false;

        return !seatOccupied[seatIndex];
    }

    /// <summary>
    /// Получить сущность, сидящую на конкретном месте.
    /// Возвращает null, если место свободно или индекс невалиден.
    /// </summary>
    public SeatableEntity GetSittingEntity(int seatIndex)
    {
        if (!IsValidSeatIndex(seatIndex))
            return null;

        return seatedEntities[seatIndex];
    }

    /// <summary>
    /// Посадить игрока/NPC на конкретное место.
    /// Возвращает true, если посадка успешна, иначе false.
    /// </summary>
    public bool SitDown(int seatIndex, SeatableEntity entity)
    {
        if (!IsValidSeatIndex(seatIndex))
            return false;

        if (entity == null)
            return false;

        if (seatOccupied[seatIndex])
            return false; // Место уже занято.

        seatOccupied[seatIndex] = true;
        seatedEntities[seatIndex] = entity;

        // Здесь можно добавить логику:
        // - перемещение entity.transform.position в точку сидения;
        // - поворот;
        // - установку состояния "сидит" в компоненте сущности.

        return true;
    }

    /// <summary>
    /// Высадить игрока/NPC с конкретного места.
    /// Возвращает true, если высадка успешна, иначе false.
    /// </summary>
    public bool StandUp(int seatIndex)
    {
        if (!IsValidSeatIndex(seatIndex))
            return false;

        if (!seatOccupied[seatIndex])
            return false; // Место и так свободно.

        // При необходимости можно выполнить дополнительную логику:
        // - сброс состояния "сидит" у сущности;
        // - смещение позиции и т.п.

        seatOccupied[seatIndex] = false;
        seatedEntities[seatIndex] = null;

        return true;
    }

    /// <summary>
    /// Высадить конкретную сущность, где бы она ни сидела.
    /// Возвращает true, если сущность была найдена и высажена.
    /// </summary>
    public bool StandUpEntity(SeatableEntity entity)
    {
        if (entity == null)
            return false;

        for (int i = 0; i < seatedEntities.Length; i++)
        {
            if (seatedEntities[i] == entity)
            {
                return StandUp(i);
            }
        }
        return false;
    }

    /// <summary>
    /// Найти первое свободное место.
    /// Возвращает индекс места или -1, если свободных мест нет.
    /// </summary>
    public int FindFirstFreeSeat()
    {
        for (int i = 0; i < seatOccupied.Length; i++)
        {
            if (!seatOccupied[i])
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Получить мировую позицию конкретного места.
    /// </summary>
    public Vector3 GetSeatWorldPosition(int seatIndex)
    {
        if (!IsValidSeatIndex(seatIndex))
            return transform.position;

        return transform.TransformPoint(seatPositions[seatIndex]);
    }

    /// <summary>
    /// Получить локальную позицию конкретного места.
    /// </summary>
    public Vector3 GetSeatLocalPosition(int seatIndex)
    {
        if (!IsValidSeatIndex(seatIndex))
            return Vector3.zero;

        return seatPositions[seatIndex];
    }

    #endregion

    #region Helpers

    private bool IsValidSeatIndex(int index)
    {
        return index >= 0 && index < seatPositions.Length;
    }

    // Опционально: отрисовка точек сидения в редакторе / gizmos для удобства.
    private void OnDrawGizmos()
    {
        if (seatPositions == null)
            return;

        foreach (var localPos in seatPositions)
        {
            Vector3 worldPos = transform.TransformPoint(localPos);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(worldPos, 0.1f);
        }
    }

    #endregion
}
