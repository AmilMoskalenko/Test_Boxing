using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Quaternion _originalRotation;

    private void Awake()
    {
        Instance = this;
        _originalRotation = transform.localRotation;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float damper = 1.0f - Mathf.Clamp01(elapsed / duration);
            float currentMagnitude = magnitude * damper;
            Vector3 shakeEuler = new Vector3(0f, 0f, Random.Range(-20f, 20f) * currentMagnitude);
            transform.localRotation = _originalRotation * Quaternion.Euler(shakeEuler);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = _originalRotation;
    }
}
