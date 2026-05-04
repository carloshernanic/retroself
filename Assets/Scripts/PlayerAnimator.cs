using UnityEngine;

// Animação placeholder via squash/stretch + tint enquanto não chegam sprites autorais.
// Quando os sprites entrarem, esse componente pode virar um Animator de verdade.
public class PlayerAnimator : MonoBehaviour
{
    public PlayerController controller;
    public PlayerAttack attack;
    public SpriteRenderer body;

    [Header("Idle (respiração)")]
    public float idleBreathAmplitude = 0.04f;
    public float idleBreathSpeed = 2.5f;

    [Header("Walk")]
    public float walkBobAmplitude = 0.08f;
    public float walkBobSpeed = 12f;

    [Header("Jump / Fall")]
    public float jumpStretch = 0.18f;
    public float landSquash = 0.22f;
    public float landRecoverSpeed = 8f;

    [Header("Attack flash")]
    public float attackStretch = 0.25f;
    public float attackFlashDuration = 0.18f;
    public Color attackFlashColor = new Color(1f, 0.9f, 0.6f, 1f);

    private Vector3 baseScale;
    private Color baseColor;
    private float walkPhase;
    private bool wasGrounded;
    private float landTimer;
    private float lastHandledAttack = -999f;
    private float attackTimer;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (attack == null) attack = GetComponent<PlayerAttack>();
        if (body == null) body = GetComponentInChildren<SpriteRenderer>();
        if (body != null)
        {
            baseScale = body.transform.localScale;
            baseColor = body.color;
        }
    }

    void LateUpdate()
    {
        if (body == null || controller == null) return;

        bool grounded = controller.IsGrounded;
        float moveAbs = Mathf.Abs(controller.MoveX);
        float vy = controller.Velocity.y;

        // Detecta pouso
        if (grounded && !wasGrounded) landTimer = 1f;
        wasGrounded = grounded;
        landTimer = Mathf.Max(0f, landTimer - Time.deltaTime * landRecoverSpeed);

        // Detecta novo ataque
        if (attack != null && attack.LastAttackTime > lastHandledAttack)
        {
            lastHandledAttack = attack.LastAttackTime;
            attackTimer = attackFlashDuration;
        }
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);

        float scaleX = 1f;
        float scaleY = 1f;

        if (!grounded)
        {
            // No ar: estica vertical se subindo, achata se caindo
            float t = Mathf.Clamp(vy / 10f, -1f, 1f);
            scaleY = 1f + jumpStretch * t;
            scaleX = 1f - jumpStretch * 0.5f * t;
        }
        else if (moveAbs > 0.01f)
        {
            walkPhase += Time.deltaTime * walkBobSpeed;
            float bob = Mathf.Sin(walkPhase) * walkBobAmplitude;
            scaleY = 1f + bob;
            scaleX = 1f - bob * 0.6f;
        }
        else
        {
            float breath = Mathf.Sin(Time.time * idleBreathSpeed) * idleBreathAmplitude;
            scaleY = 1f + breath;
            scaleX = 1f - breath * 0.4f;
        }

        // Squash de pouso
        if (landTimer > 0f)
        {
            scaleY *= 1f - landSquash * landTimer;
            scaleX *= 1f + landSquash * 0.6f * landTimer;
        }

        // Stretch horizontal de ataque (na direção que está virado)
        if (attackTimer > 0f)
        {
            float p = attackTimer / attackFlashDuration;
            scaleX *= 1f + attackStretch * p;
            scaleY *= 1f - attackStretch * 0.4f * p;
            body.color = Color.Lerp(baseColor, attackFlashColor, p);
        }
        else
        {
            body.color = baseColor;
        }

        body.transform.localScale = new Vector3(baseScale.x * scaleX, baseScale.y * scaleY, baseScale.z);
    }
}
