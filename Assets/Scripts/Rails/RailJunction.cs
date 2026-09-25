using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines; // Используем стандартный пакет Unity Splines

public class RailJunction : MonoBehaviour
{
    [Header("Сплайны")]
    [SerializeField] private SplineContainer inputSpline;
    [SerializeField] private SplineContainer outputSpline0;
    [SerializeField] private SplineContainer outputSpline1;

    [Header("Настройки")]
    [SerializeField] private int activeOutputIndex = 0; // По умолчанию выбран 0

    // Список вагонов, находящихся сейчас в зоне стрелки
    private List<Collider> carsInZone = new List<Collider>();

    /// <summary>
    /// Возвращает true, если в зоне стрелки есть хотя бы один вагон.
    /// </summary>
    public bool IsOccupied => carsInZone.Count > 0;

    /// <summary>
    /// Текущее положение стрелки (0 или 1).
    /// </summary>
    public int ActiveOutputIndex => activeOutputIndex;

    /// <summary>
    /// Переключает стрелку на указанный выход (0 или 1).
    /// </summary>
    /// <param name="index">Индекс нового активного сплайна.</param>
    /// <returns>True, если перевод успешен. False, если стрелка занята или индекс неверен.</returns>
    public bool TrySwitchJunction(int index)
    {
        if (IsOccupied)
        {
            Debug.LogWarning($"[{name}] Нельзя перевести стрелку: в зоне находится вагон!");
            return false;
        }

        if (index != 0 && index != 1)
        {
            Debug.LogError($"[{name}] Неверный индекс стрелки. Используйте 0 или 1.");
            return false;
        }

        activeOutputIndex = index;
        Debug.Log($"[{name}] Стрелка переведена в положение: {activeOutputIndex}");
        return true;
    }

    /// <summary>
    /// Переключает стрелку в противоположное состояние (0 -> 1 или 1 -> 0).
    /// </summary>
    public bool TryToggleJunction()
    {
        int nextIndex = activeOutputIndex == 0 ? 1 : 0;
        return TrySwitchJunction(nextIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что вошедший объект — это вагон
        // (замените TrainCar на имя вашего компонента движения вагона)
        Debug.Log($"{other.name}: junction {gameObject.name} entered");

        if (other.TryGetComponent<TrainJunctionRegister>(out var jr))
        {
            if (!carsInZone.Contains(other))
            {
                carsInZone.Add(other);
                TrainModel car = jr.GetModel();
                Debug.Log("requesting car arrival handle...");
                HandleCarArrival(car);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (carsInZone.Contains(other))
        {
            carsInZone.Remove(other);
        }
    }

    private void HandleCarArrival(TrainModel car)
    {
        // Получаем текущий сплайн, по которому едет вагон. 
        // Предполагается, что у вашего вагона есть свойство или метод для этого (например, currentSpline).
        // Если у вас используется Spline, а не SplineContainer, приведите типы соответственно.
        SplineContainer currentCarSpline = car.GetCurrentRail(); 

        if (currentCarSpline == inputSpline)
        {
            Debug.Log("in -> out");
            // Вагон едет по шерстяному направлению (со входа на один из выходов)
            SplineContainer targetSpline = (activeOutputIndex == 0) ? outputSpline0 : outputSpline1;
            car.HitJunction(targetSpline);
        }
        else if (currentCarSpline == outputSpline0 || currentCarSpline == outputSpline1)
        {
            Debug.Log("out -> in");
            // Вагон едет по противошерстному направлению (с выхода на вход)
            car.HitJunction(inputSpline);
        }
    }
}
