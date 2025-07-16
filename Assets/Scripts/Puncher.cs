using UnityEngine;

public class Puncher : MonoBehaviour
{
    [SerializeField] private DamageReceiver _targetDamageable;
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private AudioClip _punchSound;

    private AudioSource _audioSource;
    private Vector2 _mouseStart;
    private bool _isSwiping;
    private Camera _mainCamera;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseStart = Input.mousePosition;
            _isSwiping = true;
        }
        if (Input.GetMouseButtonUp(0) && _isSwiping)
        {
            Vector2 mouseEnd = Input.mousePosition;
            Vector2 swipe = mouseEnd - _mouseStart;
            Ray ray = _mainCamera.ScreenPointToRay(mouseEnd);
            RaycastHit hit;
            bool hitHead = false;
            Vector2 hitUV = Vector2.zero;
            if (Physics.Raycast(ray, out hit))
            {
                DamageReceiver receiver = hit.collider.GetComponent<DamageReceiver>();
                if (receiver != null && receiver == _targetDamageable)
                {
                    hitHead = true;
                    hitUV = hit.textureCoord;
                }
            }
            if (hitHead)
            {
                float vertical = swipe.y;
                float horizontal = -swipe.x;
                Vector3 localSwipeDir = new Vector3(vertical, 0, horizontal).normalized;
                _targetDamageable.ApplyDamage(swipe.magnitude, localSwipeDir);
                if (swipe.magnitude > 0f)
                {
                    if (_hitEffect)
                        Instantiate(_hitEffect, hit.point, Quaternion.identity);
                    if (_punchSound && _audioSource)
                        _audioSource.PlayOneShot(_punchSound);
                }
            }
            _isSwiping = false;
        }
    }
}
