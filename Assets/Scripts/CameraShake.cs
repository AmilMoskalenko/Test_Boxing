using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float _shakeAngle;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeMagnitude;

    public static CameraShake Instance;

    private Quaternion _originalRotation;

    private void Awake()
    {
        Instance = this;
        _originalRotation = transform.localRotation;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < _shakeDuration)
        {
            float damper = 1.0f - Mathf.Clamp01(elapsed / _shakeDuration);
            float currentMagnitude = _shakeMagnitude * damper;
            float shakeZ = Random.Range(-_shakeAngle, _shakeAngle) * currentMagnitude;
            Vector3 shakeEuler = new Vector3(0f, 0f, shakeZ);
            transform.localRotation = _originalRotation * Quaternion.Euler(shakeEuler);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = _originalRotation;
    }
}
