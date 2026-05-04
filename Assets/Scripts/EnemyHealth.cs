using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 2;
    public float stunTime = 0.6f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnStunned;
    public event Action OnDefeated;

    public int CurrentHealth { get; private set; }
    public bool IsStunned => stunCounter > 0f;

    private float stunCounter;
    private SpriteRenderer body;

    void Awake()
    {
        CurrentHealth = maxHealth;
        body = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (stunCounter > 0f)
        {
            stunCounter -= Time.deltaTime;
            if (body != null)
            {
                var c = body.color;
                c.a = Mathf.PingPong(Time.time * 10f, 1f) * 0.4f + 0.6f;
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
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        stunCounter = stunTime;
        OnStunned?.Invoke();

        if (CurrentHealth <= 0)
        {
            OnDefeated?.Invoke();
            Destroy(gameObject);
        }
    }
}
