using UnityEngine;

public class PressureGauge : MonoBehaviour
{
    [Header("Настройки давления (Атм)")]
    [SerializeField] private float minPressure = 0f;
    [SerializeField] private float maxPressure = 10f;
    [SerializeField] private float currentPressure = 0f;

    [Header("Настройки стрелки (Углы в градусах)")]
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private float minPressureAngle = 180f;
    [SerializeField] private float maxPressureAngle = -180f;

    [Header("Плавность хода")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Оформление")]
    [SerializeField] private Color tintColor = Color.white;

    private float targetAngle;
    private Material material;

    private void Start()
    {
        // Устанавливаем начальное положение стрелки без анимации
        UpdateArrowRotation(true);
        material = GetComponent<Renderer>().material;

        material.SetColor("_BaseColor", tintColor);
    }

    private void Update()
    {
        if (smoothMovement)
        {
            // Получаем текущий угол по оси Z
            float currentAngle = arrowTransform.localEulerAngles.y;

            // Превращаем угол Unity (0..360) в диапазон (-180..180), 
            // чтобы правильно соотнести его с нашими углами -135 и 135
            if (currentAngle > 180f) currentAngle -= 360f;

            // Используем обычный Mathf.Lerp вместо LerpAngle, 
            // чтобы движение шло строго по числовой прямой от -135 до 135
            float newAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);

            arrowTransform.localRotation = Quaternion.Euler(0, newAngle, 0);
        }
    }

    /// <summary>
    /// Метод для изменения текущего давления. Значение автоматически зажимается в заданных пределах.
    /// </summary>
    public void SetPressure(float newPressure)
    {
        currentPressure = Mathf.Clamp(newPressure, minPressure, maxPressure);
        UpdateArrowRotation(false);
    }

    /// <summary>
    /// Возвращает текущее давление.
    /// </summary>
    public float GetCurrentPressure() => currentPressure;

    private void UpdateArrowRotation(bool immediate)
    {
        if (arrowTransform == null) return;

        // Находим нормализованное значение давления (от 0 до 1)
        float t = (currentPressure - minPressure) / (maxPressure - minPressure);

        // Линейно интерполируем угол между минимальным и максимальным
        targetAngle = Mathf.Lerp(minPressureAngle, maxPressureAngle, t);

        if (immediate || !smoothMovement)
        {
            arrowTransform.localRotation = Quaternion.Euler(0, targetAngle, 0);
        }
    }

    // Визуализация в инспекторе при изменении значений во время редактирования
    private void OnValidate()
    {
        if (maxPressure <= minPressure) maxPressure = minPressure + 0.1f;
        currentPressure = Mathf.Clamp(currentPressure, minPressure, maxPressure);
        UpdateArrowRotation(true);
    }
}
