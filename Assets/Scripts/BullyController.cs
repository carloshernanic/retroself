using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BullyController : MonoBehaviour
{
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float detectionRange = 6f;
    public float verticalDetect = 2f;
    public float patrolMinX = -5f;
    public float patrolMaxX = 5f;
    public int contactDamage = 1;
    public float attackCooldown = 1f;

    [Header("Visual")]
    public SpriteRenderer body;
    // tintColors fica false quando há sprite autoral — tintar com vermelho mascara
    // o pixel art. Mantido como flag pra reativar em testes/placeholder.
    public bool tintColors = false;
    public Color patrolColor = new Color(0.85f, 0.30f, 0.30f, 1f);
    public Color chaseColor = new Color(0.95f, 0.45f, 0.20f, 1f);

    private Rigidbody2D rb;
    private EnemyHealth health;
    private Transform target;
    private int dir = 1;
    private float attackCounter;
    private bool chasing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        if (body == null) body = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (attackCounter > 0f) attackCounter -= Time.deltaTime;

        // Polling do alvo: evita race entre Awake/OnEnable e a inscrição em PlayerSwap.
        if (PlayerSwap.Instance != null && PlayerSwap.Instance.ActivePlayer != null)
        {
            target = PlayerSwap.Instance.ActivePlayer.transform;
        }
        else if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (health != null && health.IsStunned)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        chasing = false;
        if (target != null)
        {
            float dx = target.position.x - transform.position.x;
            float dy = target.position.y - transform.position.y;
            if (Mathf.Abs(dx) < detectionRange && Mathf.Abs(dy) < verticalDetect)
            {
                chasing = true;
                // Deadzone: sem isso, dx oscila perto de 0 quando o player está sobre
                // o bully, e dir flipa toda FixedUpdate → flickering visual ("travando
                // virando de lugar"). Mantém dir até o player se afastar > 0.5u.
                if (Mathf.Abs(dx) > 0.5f) dir = dx > 0f ? 1 : -1;
            }
        }

        // Mantém o bully dentro da área de patrulha (mesmo perseguindo) pra não cair em poço
        if (transform.position.x <= patrolMinX && dir < 0) dir = 1;
        else if (transform.position.x >= patrolMaxX && dir > 0) dir = -1;

        float speed = chasing ? chaseSpeed : patrolSpeed;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        if (body != null)
        {
            body.flipX = dir < 0;
            if (tintColors) body.color = chasing ? chaseColor : patrolColor;
        }
    }

    void OnCollisionEnter2D(Collision2D other) { TryHitPlayer(other.gameObject); }
    void OnCollisionStay2D(Collision2D other) { TryHitPlayer(other.gameObject); }

    void TryHitPlayer(GameObject other)
    {
        if (attackCounter > 0f) return;
        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;
        ph.TakeDamage(contactDamage, transform.position);
        attackCounter = attackCooldown;
    }
}
