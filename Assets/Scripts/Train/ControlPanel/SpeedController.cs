using TMPro;
using UnityEngine;

public class SpeedController : MonoBehaviour
{
    [SerializeField] TMP_Text speedText;

    public void SetSpeedText(float speed_ms)
    {
        int kmh = (int) (speed_ms * 3.6f);
        speedText.text = $"{kmh}";
    }
}
