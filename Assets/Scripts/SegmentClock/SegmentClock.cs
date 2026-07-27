using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class SegmentClock : MonoBehaviour
{
    [SerializeField] SegmentDigit hours10, hours, mins10, mins, secs10, secs;
    [SerializeField] GameObject minsSecsDot;

    private List<SegmentDigit> activeDigits = new List<SegmentDigit>();

    int value = 0;

    [Tooltip("Start time in format 3600 * hours + 60 * minutes + seconds. Has no effect if startWithCurrentTime = true")]
    [SerializeField] int startValue = 0;

    [Tooltip("Sets startValue to current time if true")]
    [SerializeField] bool startWithCurrentTime = false;

    [Tooltip("When value > maxValue, dont set it to zero, but disable all segments")]
    [SerializeField] bool disableSegmentsOnOverflow = false;

    int maxValue = 0;
    private bool areDigitsVisible = true; // Кеш видимости всей группы вместо activeInHierarchy
    private Coroutine clockCoroutine;

    private void Start()
    {
        InitializeClockConfiguration();

        if (startWithCurrentTime)
        {
            DateTime now = DateTime.Now;
            startValue = now.Hour * 3600 + now.Minute * 60 + now.Second;
        }

        value = startValue;
        UpdateClockDisplay(false);

        clockCoroutine = StartCoroutine(ClockTickRoutine());
    }

    void InitializeClockConfiguration()
    {
        activeDigits.Clear();

        // Проверяем наличие сегментов по иерархии (от старшего к младшему)
        if (hours10 != null) activeDigits.Add(hours10);
        if (hours != null) activeDigits.Add(hours);
        if (mins10 != null) activeDigits.Add(mins10);
        if (mins != null) activeDigits.Add(mins);
        if (secs10 != null) activeDigits.Add(secs10);
        if (secs != null) activeDigits.Add(secs);

        // Логика определения максимального времени (maxValue)
        if (hours10 != null || hours != null)
        {
            // 1. Полноценные часы (ЧЧ:ММ:СС или ЧЧ:ММ)
            maxValue = (24 * 3600) - 1;
        }
        else if (mins10 != null)
        {
            // 2. Двузначные минуты (ММ:СС) — до 59 минут 59 секунд
            maxValue = (60 * 60) - 1;
        }
        else if (mins != null && secs10 != null && secs != null)
        {
            // 3. Конфигурация М:СС (одна цифра минут и секунды) — до 9 минут 59 секунд (10 минут)
            maxValue = (10 * 60) - 1; // 599 секунд
        }
        else if (secs10 != null || secs != null)
        {
            // 4. Только секунды (СС или С)
            maxValue = 59;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Не удалось определить конфигурацию часов. Назначьте сегменты.");
            enabled = false;
        }
    }

    IEnumerator ClockTickRoutine()
    {
        // Бесконечный цикл с фиксированной задержкой в 1 секунду
        while (true)
        {
            yield return new WaitForSeconds(1f);
            UpdateClockDisplay(true);
        }
    }

    void UpdateClockDisplay(bool changeValue = true)
    {
        if (changeValue)
        {
            if (disableSegmentsOnOverflow)
            {
                if (value <= maxValue) value++;
            }
            else
            {
                value++;
                if (value > maxValue) value = 0;
            }

            if (minsSecsDot != null)
            {
                minsSecsDot.SetActive(!minsSecsDot.activeSelf);
            }
        }

        bool shouldDisable = disableSegmentsOnOverflow && (value > maxValue);
        SetSegmentsVisible(!shouldDisable);

        if (!shouldDisable)
        {
            int currentHours = value / 3600;
            int currentMinutes = (value / 60) % 60;
            int currentSeconds = value % 60;

            // Обновляем только те сегменты, которые физически существуют
            hours10?.SetValue(currentHours / 10);
            hours?.SetValue(currentHours % 10);
            mins10?.SetValue(currentMinutes / 10);
            mins?.SetValue(currentMinutes % 10);
            secs10?.SetValue(currentSeconds / 10);
            secs?.SetValue(currentSeconds % 10);
        }
    }

    void SetSegmentsVisible(bool visible)
    {
        // Проверяем локальный кеш вместо тяжелого вызова activeInHierarchy
        if (areDigitsVisible == visible) return;

        foreach (SegmentDigit digit in activeDigits)
        {
            if (digit != null)
            {
                digit.gameObject.SetActive(visible);
            }
        }
        areDigitsVisible = visible;
    }

    private void OnDestroy()
    {
        // Хорошая практика: останавливать корутину при уничтожении объекта
        if (clockCoroutine != null)
        {
            StopCoroutine(clockCoroutine);
        }
    }
}