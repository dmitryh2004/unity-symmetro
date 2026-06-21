using UnityEngine;

public class SegmentDigit : MonoBehaviour
{
    [SerializeField] GameObject[] segments = new GameObject[7];

    int value = 0;
    [SerializeField] int startValue = 0;

    int[] segmentVisibilityMasks = {
        //9876543210
        0b1111101101, //upper
        0b1101110001, //upper-left
        0b1110011111, //upper-right
        0b1101111100, //middle
        0b0101000101, //bottom-left
        0b1111111011, //bottom-right,
        0b1101101101  //bottom
    };

    const int INDEX_UPPER = 0, INDEX_UPPER_LEFT = 1, INDEX_UPPER_RIGHT = 2, INDEX_MIDDLE = 3, INDEX_BOTTOM_LEFT = 4, INDEX_BOTTOM_RIGHT = 5, INDEX_BOTTOM = 6;

    private void Start()
    {
        value = startValue;
        UpdateSegments();
    }

    void UpdateSegments()
    {
        for (int i = 0; i < 7; i++)
        {
            int visibilityMask = segmentVisibilityMasks[i];
            bool visible = ((visibilityMask >> value) % 2) == 1;
            segments[i].SetActive(visible);
        }
    }

    public void SetValue(int value)
    {
        if (value < 0) value = 0;
        if (value > 9) value = 9;

        this.value = value;
        UpdateSegments();
    }
}
