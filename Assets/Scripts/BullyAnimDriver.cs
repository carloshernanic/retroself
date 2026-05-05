using UnityEngine;

// Liga o Rigidbody2D do bully ao Animator dele: usa velocidade horizontal pra
// escolher Idle/Walk e velocidade vertical pra detectar "no chão" (bully não
// pula no gameplay, então a heurística é simples).
[RequireComponent(typeof(Animator))]
public class BullyAnimDriver : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;

    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
    static readonly int HashVerticalVel = Animator.StringToHash("VerticalVel");

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (rb == null || animator == null) return;
        if (animator.runtimeAnimatorController == null) return;
        var v = rb.linearVelocity;
        animator.SetFloat(HashSpeed, Mathf.Abs(v.x));
        animator.SetFloat(HashVerticalVel, v.y);
        animator.SetBool(HashGrounded, Mathf.Abs(v.y) < 0.05f);
    }
}
