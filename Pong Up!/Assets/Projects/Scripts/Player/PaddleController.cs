using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 25f;
    public float horrizontalLimit = 7.5f;

    [Header("Bounce Settings")]
    public float bounceForce = 13f;
    public float edgeAngleMultiplier = 6f;
    public float centerThreshold = 0.2f;
    public float minHorizontalPush = 2.0f;

    [Header("Effects & Sounds")]
    public ParticleSystem hitEffect;
    public Color normalHitColor;
    public Color perfectHitColor;
    public AudioClip hitSound;
    private AudioSource _audioSource;

    [Header("Item Settings")]
    public float powerUpDuration = 5f;
    private Vector3 _originalScale;
    private Coroutine _sizeRoutine;

    private GameInputs _inputs;
    private InputAction _moveAction;
    private InputAction _dragAction;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _inputs = new GameInputs();
        _moveAction = _inputs.Player.Move;
        _dragAction = _inputs.Player.Drag;
    }

    void Start()
    {
        _originalScale = transform.localScale;
    }

    void OnEnable()
    {
        _moveAction.Enable();
        _dragAction.Enable();
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _dragAction.Disable();
    }

    void Update()
    {
        if (_dragAction.IsPressed())
        {
            Vector2 screenPos = _moveAction.ReadValue<Vector2>();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            float targetX = Mathf.Clamp(mousePos.x, -horrizontalLimit, horrizontalLimit);
            Vector3 targetPos = new Vector3(targetX, transform.position.y, 0);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
            float hitOffset = collision.transform.position.x - transform.position.x;

            bool isPerfectHit = Mathf.Abs(hitOffset) <= centerThreshold;

            Color finalEffectColor;

            if (isPerfectHit)
            {
                GameManager.Instance.AddScore(2);
                finalEffectColor = perfectHitColor;
                if (_audioSource) _audioSource.pitch = 1.2f;
            }
            else
            {
                GameManager.Instance.AddScore(1);
                finalEffectColor = normalHitColor;
                if (_audioSource) _audioSource.pitch = Random.Range(0.9f, 1.1f);
            }

            float hVel = hitOffset * edgeAngleMultiplier;
            if (Mathf.Abs(hVel) < minHorizontalPush)
            {
                hVel = minHorizontalPush * (Random.value > 0.5f ? 1 : -1);
            }
            ballRb.linearVelocity = new Vector2(hVel, bounceForce);

            if (_audioSource && hitSound)
            {
                _audioSource.PlayOneShot(hitSound);
            }

            if (hitEffect)
            {
                ParticleSystem effect = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
                Vector2 contactPoint = collision.contacts[0].point;

                var mainModule = effect.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(finalEffectColor);

                effect.Play();
                Destroy(effect.gameObject, 1f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            ApplyEffect(item.itemType);
            Destroy(collision.gameObject);
        }
    }

    void ApplyEffect(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.ScorePlus5: GameManager.Instance.AddScore(5); break;
            case ItemType.ScorePlus10: GameManager.Instance.AddScore(10); break;
            case ItemType.ScoreMinus5: GameManager.Instance.AddScore(-5); break;
            case ItemType.ScoreMinus10: GameManager.Instance.AddScore(-10); break;

            case ItemType.LargePaddle:
                if (_sizeRoutine != null)
                {
                    StopCoroutine(_sizeRoutine);
                }
                _sizeRoutine = StartCoroutine(ChangeSizeRoutine(1.5f));
                break;

            case ItemType.SmallPaddle:
                if (_sizeRoutine != null)
                {
                    StopCoroutine(_sizeRoutine);
                }
                _sizeRoutine = StartCoroutine(ChangeSizeRoutine(0.6f));
                break;
        }
    }

    IEnumerator ChangeSizeRoutine(float multiplier)
    {
        transform.localScale = new Vector3(_originalScale.x * multiplier, _originalScale.y, _originalScale.z);
        yield return new WaitForSeconds(powerUpDuration);
        transform.localScale = _originalScale;
    }
}
