using UnityEngine;

public class StationPointerInitializer : MonoBehaviour
{
    [SerializeField] Texture2D pointerTexture;
    [SerializeField] Renderer infoRenderer;
    Material material;
    private void Awake()
    {
        material = infoRenderer.material;
        if (pointerTexture != null)
        {
            material.SetTexture("_BaseMap", pointerTexture);
            material.SetTexture("_EmissionMap", pointerTexture);
        }
    }
}
