using UnityEngine;
using DG.Tweening;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] private Renderer _headRenderer;
    [SerializeField] private Texture2D _bloodSpotTexture;

    private Texture2D _editableTexture;
    private Quaternion _initialRotation;

    void Awake()
    {
        _initialRotation = transform.rotation;
        if (_headRenderer != null && _headRenderer.material.mainTexture is Texture2D mainTex)
        {
            _editableTexture = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGBA32, false);
            _editableTexture.SetPixels(mainTex.GetPixels());
            _editableTexture.Apply();
            _headRenderer.material.mainTexture = _editableTexture;
        }
    }

    public void ApplyDamage(int amount, Vector3 localSwipeDir)
    {
        CameraShake.Instance?.Shake(0.5f, 0.1f);
        Vector3 punchRotation = new Vector3(
            -Mathf.Abs(Mathf.Clamp(localSwipeDir.x, -1f, 1f)),
            0,
            Mathf.Clamp(localSwipeDir.z, -1f, 1f)
        ) * Mathf.Clamp(amount, 0f, 100f) * 0.25f;
        transform.DOPunchRotation(punchRotation, 1f, 1, 0.5f)
            .OnComplete(() => transform.rotation = _initialRotation);
        AddBloodSpotToMaterial(localSwipeDir);
    }

    private void AddBloodSpotToMaterial(Vector3 localSwipeDir)
    {
        if (_editableTexture == null || _bloodSpotTexture == null) return;
        int texWidth = _editableTexture.width;
        int texHeight = _editableTexture.height;
        int spotWidth = 10;
        int spotHeight = 10;
        float faceUvX = 0.25f;
        float faceUvY = 0.21f;
        float offsetScaleY = 0.03f;
        float offsetScaleX = 0.03f;
        float randomScatterX = Random.Range(0f, 2f);
        float randomScatterY = Random.Range(0f, 2f);
        float offsetX = Mathf.Clamp(localSwipeDir.z, -1f, 1f) * offsetScaleY * randomScatterX;
        float offsetY = -Mathf.Clamp(localSwipeDir.x, -1f, 1f) * offsetScaleX * randomScatterY;
        float spotUvX = Mathf.Clamp01(faceUvX + offsetX);
        float spotUvY = Mathf.Clamp01(faceUvY + offsetY);
        int xPos = Mathf.RoundToInt(spotUvX * texWidth) - spotWidth / 2;
        int yPos = Mathf.RoundToInt(spotUvY * texHeight) - spotHeight / 2;
        Texture2D scaledSpot = new Texture2D(spotWidth, spotHeight, TextureFormat.RGBA32, false);
        for (int y = 0; y < spotHeight; y++)
        {
            for (int x = 0; x < spotWidth; x++)
            {
                float u = (float)x / (spotWidth - 1);
                float v = (float)y / (spotHeight - 1);
                Color color = _bloodSpotTexture.GetPixelBilinear(u, v);
                scaledSpot.SetPixel(x, y, color);
            }
        }
        scaledSpot.Apply();
        for (int y = 0; y < spotHeight; y++)
        {
            for (int x = 0; x < spotWidth; x++)
            {
                int tx = xPos + x;
                int ty = yPos + y;
                if (tx >= 0 && tx < texWidth && ty >= 0 && ty < texHeight)
                {
                    Color baseColor = _editableTexture.GetPixel(tx, ty);
                    Color spotColor = scaledSpot.GetPixel(x, y);
                    Color blended = Color.Lerp(baseColor, spotColor, spotColor.a);
                    _editableTexture.SetPixel(tx, ty, blended);
                }
            }
        }
        _editableTexture.Apply();
        Destroy(scaledSpot);
    }
}
