using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invincibilityTime = 0.8f;
    public float knockbackForce = 6f;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    public int CurrentHealth { get; private set; }
    public bool IsInvincible => invincibilityCounter > 0f;

    private float invincibilityCounter;
    private Vector3 spawnPosition;
    private Rigidbody2D rb;
    private SpriteRenderer body;

    void Awake()
    {
        CurrentHealth = maxHealth;
        spawnPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        var pc = GetComponent<PlayerController>();
        if (pc != null) body = pc.body;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update()
    {
        if (invincibilityCounter > 0f)
        {
            invincibilityCounter -= Time.deltaTime;
            if (body != null)
            {
                var c = body.color;
                c.a = Mathf.PingPong(Time.time * 8f, 1f) * 0.5f + 0.5f;
                body.color = c;
            }
        }
        else if (body != null && body.color.a < 1f)
        {
            var c = body.color; c.a = 1f; body.color = c;
        }
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsInvincible || CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invincibilityCounter = invincibilityTime;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.up;
            rb.linearVelocity = new Vector2(dir.x * knockbackForce, knockbackForce * 0.7f);
        }

        if (CurrentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Die()
    {
        OnDied?.Invoke();
        Respawn();
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        CurrentHealth = maxHealth;
        invincibilityCounter = invincibilityTime;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
