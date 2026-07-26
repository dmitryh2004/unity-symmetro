using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Digits/Digit Prefabs Database", fileName = "DigitPrefabsDatabase")]
public class DigitPrefabsDatabase : ScriptableObject
{
    [Serializable]
    public class DigitEntry
    {
        [Range(0, 9)]
        public int digit;

        public GameObject prefab;

        [Min(0f)]
        public float width;
    }

    [SerializeField] private List<DigitEntry> digits = new List<DigitEntry>();

    public IReadOnlyList<DigitEntry> Digits => digits;

    public bool TryGetEntry(int digit, out DigitEntry entry)
    {
        entry = null;

        for (int i = 0; i < digits.Count; i++)
        {
            if (digits[i] != null && digits[i].digit == digit)
            {
                entry = digits[i];
                return entry.prefab != null;
            }
        }

        return false;
    }
}