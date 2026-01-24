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
    private AudioSource audioSource;

    private GameInputs inputs;
    private InputAction moveAction;
    private InputAction dragAction;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inputs = new GameInputs();
        moveAction = inputs.Player.Move;
        dragAction = inputs.Player.Drag;
    }

    void OnEnable()
    {
        moveAction.Enable();
        dragAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        dragAction.Disable();
    }

    void Update()
    {
        if (dragAction.IsPressed())
        {
            Vector2 screenPos = moveAction.ReadValue<Vector2>();
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
                if (audioSource) audioSource.pitch = 1.2f;
            }
            else
            {
                GameManager.Instance.AddScore(1);
                finalEffectColor = normalHitColor;
                if (audioSource) audioSource.pitch = Random.Range(0.9f, 1.1f);
            }

            float hVel = hitOffset * edgeAngleMultiplier;
            if (Mathf.Abs(hVel) < minHorizontalPush)
            {
                hVel = minHorizontalPush * (Random.value > 0.5f ? 1 : -1);
            }
            ballRb.linearVelocity = new Vector2(hVel, bounceForce);

            if (audioSource && hitSound)
            {
                audioSource.PlayOneShot(hitSound);
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
}
