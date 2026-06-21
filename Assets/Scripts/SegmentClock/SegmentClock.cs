using UnityEngine;
using System;
using System.Collections.Generic;

public class SegmentClock : MonoBehaviour
{
    [SerializeField] SegmentDigit hours10, hours, mins10, mins, secs10, secs;
    [SerializeField] GameObject minsSecsDot;
    List<SegmentDigit> segments = new();

    int value = 0;

    [Tooltip("Start time in format 3600 * hours + 60 * minutes + seconds. Has no effect if startWithCurrentTime = true")]
    [SerializeField] int startValue = 0;

    [Tooltip("Sets startValue to current time if true")]
    [SerializeField] bool startWithCurrentTime = false;

    [Tooltip("When value > maxValue, dont set it to zero, but disable all segments")]
    [SerializeField] bool disableSegmentsOnOverflow = false;

    int maxValue = 0;

    private void Start()
    {
        int segmentsUsed = 0;
        if (secs != null)
        {
            segments.Add(secs);
            segmentsUsed++;
            if (secs10 != null)
            {
                segments.Add(secs10);
                segmentsUsed++;
                if (mins != null)
                {
                    segments.Add(mins);
                    segmentsUsed++;
                    if (mins10 != null)
                    {
                        segments.Add(mins10);
                        segmentsUsed++;
                        if (hours != null)
                        {
                            segments.Add(hours);
                            segmentsUsed++;
                            if (hours10 != null)
                            {
                                segments.Add(hours10);
                                segmentsUsed++;
                            }
                        }
                    }
                }
            }
        }
        switch (segmentsUsed)
        {
            case 1:
                maxValue = 9;
                break;
            case 2:
                maxValue = 59;
                break;
            case 3:
                maxValue = 10 * 60 - 1;
                break;
            case 4:
                maxValue = 3600 - 1;
                break;
            case 5:
                maxValue = 10 * 3600 - 1;
                break;
            case 6:
                maxValue = 24 * 3600 - 1;
                break;
            default:
                Debug.LogError($"{gameObject.name}: intialization error (invalid segments)");
                return;
        }

        if (startWithCurrentTime)
        {
            DateTime now = DateTime.Now;
            startValue = now.Hour * 3600 + now.Minute * 60 + now.Second;
        }
        value = startValue;
        UpdateSegments(false);
        InvokeRepeating("UpdateSegmentsInvoker", 1f, 1f);
    }

    void UpdateSegmentsInvoker()
    {
        UpdateSegments();
    }

    void UpdateSegments(bool changeValue = true)
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
                minsSecsDot.gameObject.SetActive(!minsSecsDot.gameObject.activeInHierarchy);
            }
        }

        bool segmentsDisabled = disableSegmentsOnOverflow && (value > maxValue);
        SetSegmentsVisible(!segmentsDisabled);

        if (!segmentsDisabled)
        {
            int hours = value / 3600;
            int minutes = value / 60 % 60;
            int seconds = value % 60;

            this.hours10?.SetValue(hours / 10);
            this.hours?.SetValue(hours % 10);
            this.mins10?.SetValue(minutes / 10);
            this.mins?.SetValue(minutes % 10);
            this.secs10?.SetValue(seconds / 10);
            this.secs?.SetValue(seconds % 10);
        }
    }

    void SetSegmentsVisible(bool visible)
    {
        foreach(SegmentDigit digit in segments)
        {
            if (digit.gameObject.activeInHierarchy != visible)
            {
                digit.gameObject.SetActive(visible);
            }
        }
    }
}
