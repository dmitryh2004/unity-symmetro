using UnityEngine;

public class SegmentDigit : MonoBehaviour
{
    [SerializeField] GameObject[] segments = new GameObject[7];

    private int value = -1; // -1 гарантирует обновление при первом запуске
    [SerializeField] int startValue = 0;

    readonly int[] segmentVisibilityMasks = {
        0b1111101101, // upper
        0b1101110001, // upper-left
        0b1110011111, // upper-right
        0b1101111100, // middle
        0b0101000101, // bottom-left
        0b1111111011, // bottom-right
        0b1101101101  // bottom
    };

    // Массив для кеширования текущего состояния активации геймобъектов
    private bool[] currentStates = new bool[7];

    private void Start()
    {
        // Принудительно инициализируем массив состояний текущим статусом объектов
        for (int i = 0; i < 7; i++)
        {
            if (segments[i] != null)
                currentStates[i] = segments[i].activeSelf;
        }

        // Валидация startValue перед запуском
        SetValue(Mathf.Clamp(startValue, 0, 9));
    }

    public void SetValue(int newValue)
    {
        newValue = Mathf.Clamp(newValue, 0, 9);

        // Если цифра не поменялась, ничего не перерисовываем (экономим ресурсы)
        if (value == newValue) return;

        value = newValue;
        UpdateSegments();
    }

    void UpdateSegments()
    {
        for (int i = 0; i < 7; i++)
        {
            if (segments[i] == null) continue;

            int visibilityMask = segmentVisibilityMasks[i];

            bool visible = ((visibilityMask >> value) & 1) == 1;

            // Изменение SetActive только при несовпадении с кешем
            if (currentStates[i] != visible)
            {
                segments[i].SetActive(visible);
                currentStates[i] = visible;
            }
        }
    }
}