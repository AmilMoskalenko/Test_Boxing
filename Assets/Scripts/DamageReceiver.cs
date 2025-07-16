using UnityEngine;
using DG.Tweening;

public class DamageReceiver : MonoBehaviour
{
    [Header("Head & Blood Settings")]
    [SerializeField] private Renderer _headRenderer;
    [SerializeField] private Texture2D _bloodSpotTexture;
    [Header("Damage Animation Settings")]
    [SerializeField] private float _punchScale;
    [Header("Blood Spot Settings")]
    [SerializeField] private int _spotSize;
    [SerializeField] private float _faceUvX;
    [SerializeField] private float _faceUvY;
    [SerializeField] private float _offsetScale;

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

    public void ApplyDamage(float amount, Vector3 localSwipeDir)
    {
        CameraShake.Instance?.Shake();
        Vector3 punchRotation = new Vector3(
            -Mathf.Abs(Mathf.Clamp(localSwipeDir.x, -1f, 1f)),
            Mathf.Clamp(localSwipeDir.z, -1f, 1f),
            Mathf.Clamp(localSwipeDir.z, -1f, 1f) * 2f
        ) * amount * _punchScale;
        transform.DOPunchRotation(punchRotation, 1f, 1, 0.5f)
            .OnComplete(() => transform.rotation = _initialRotation);
        AddBloodSpotToMaterial(localSwipeDir);
    }

    private void AddBloodSpotToMaterial(Vector3 localSwipeDir)
    {
        if (_editableTexture == null || _bloodSpotTexture == null) return;
        int texWidth = _editableTexture.width;
        int texHeight = _editableTexture.height;
        float offsetX = Mathf.Clamp(localSwipeDir.z, -1f, 1f) * _offsetScale * Random.Range(0f, 2f);
        float offsetY = -Mathf.Clamp(localSwipeDir.x, -1f, 1f) * _offsetScale * Random.Range(0f, 2f);
        float spotUvX = Mathf.Clamp01(_faceUvX + offsetX);
        float spotUvY = Mathf.Clamp01(_faceUvY + offsetY);
        int xPos = Mathf.RoundToInt(spotUvX * texWidth) - _spotSize / 2;
        int yPos = Mathf.RoundToInt(spotUvY * texHeight) - _spotSize / 2;
        Texture2D scaledSpot = new Texture2D(_spotSize, _spotSize, TextureFormat.RGBA32, false);
        for (int y = 0; y < _spotSize; y++)
        {
            for (int x = 0; x < _spotSize; x++)
            {
                float u = (float)x / (_spotSize - 1);
                float v = (float)y / (_spotSize - 1);
                Color color = _bloodSpotTexture.GetPixelBilinear(u, v);
                scaledSpot.SetPixel(x, y, color);
            }
        }
        scaledSpot.Apply();
        for (int y = 0; y < _spotSize; y++)
        {
            for (int x = 0; x < _spotSize; x++)
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
