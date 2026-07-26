using System.Collections.Generic;
using UnityEngine;

public class TrainNumberGenerator : MonoBehaviour
{
    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    [SerializeField] private int number;
    [SerializeField] private float spacing = 0.1f;
    [SerializeField] private Alignment alignment = Alignment.Left;
    [SerializeField] private float compensatingRotation = 0f; // поправка для поворота по y
    [SerializeField] private DigitPrefabsDatabase database;

    public int Number
    {
        get => number;
        set => number = value;
    }

    public float Spacing
    {
        get => spacing;
        set => spacing = value;
    }

    public Alignment NumberAlignment
    {
        get => alignment;
        set => alignment = value;
    }

    public List<GameObject> GeneratedNumbers { get; private set; } = new List<GameObject>();

    public void GenerateNumber()
    {
        ClearGenerated();

        string text = Mathf.Abs(number).ToString().PadLeft(4, '0');
        if (text.Length == 0 || database == null)
            return;

        float totalWidth = 0f;
        var widths = new List<float>(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            int digit = text[i] - '0';
            if (!database.TryGetEntry(digit, out var entry))
                return;

            widths.Add(entry.width);
            totalWidth += entry.width;
            if (i < text.Length - 1)
                totalWidth += spacing;
        }

        float startZ;
        float firstDigitOffset = widths[0] / 2;
        float lastDigitOffset = widths[widths.Count - 1] / 2;

        switch (alignment)
        {
            case Alignment.Left:
                startZ = firstDigitOffset;
                break;

            case Alignment.Center:
                startZ = -totalWidth * 0.5f;
                break;

            case Alignment.Right:
                startZ = -totalWidth - lastDigitOffset;
                break;

            default:
                startZ = 0f;
                break;
        }

        float currentZ = startZ;

        for (int i = 0; i < text.Length; i++)
        {
            int digit = text[i] - '0';
            database.TryGetEntry(digit, out var entry);

            GameObject obj = Instantiate(entry.prefab, transform);
            obj.transform.localPosition = new Vector3(0f, 0f, currentZ + widths[i] / 2);

            Vector3 compensatedLocalRotation = obj.transform.localRotation.eulerAngles; // фиксим вращение
            compensatedLocalRotation.z += compensatingRotation;

            obj.transform.localRotation = Quaternion.Euler(compensatedLocalRotation);
            GeneratedNumbers.Add(obj);

            currentZ += widths[i];
            if (i < text.Length - 1)
                currentZ += spacing;
        }
    }

    private void ClearGenerated()
    {
        for (int i = 0; i < GeneratedNumbers.Count; i++)
        {
            if (GeneratedNumbers[i] != null)
                Destroy(GeneratedNumbers[i]);
        }

        GeneratedNumbers.Clear();
    }
}