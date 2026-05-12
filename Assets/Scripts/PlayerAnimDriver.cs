using UnityEngine;

// Liga o PlayerController ao Animator: traduz MoveX/IsGrounded/Velocity em
// parâmetros (Speed/IsGrounded/VerticalVel) que o controller usa pra escolher
// Idle/Walk/Jump. Substitui o PlayerAnimator placeholder (squash/stretch +
// attack flash); o placeholder distorcia body.localScale a cada LateUpdate
// e brigava visualmente com o sprite autoral.
[RequireComponent(typeof(Animator))]
public class PlayerAnimDriver : MonoBehaviour
{
    public PlayerController controller;
    public Animator animator;

    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
    static readonly int HashVerticalVel = Animator.StringToHash("VerticalVel");

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (controller == null || animator == null) return;
        // Animator sem controller atribuído ainda dá warning ruidoso em SetFloat —
        // pula até o controller ser plugado (geralmente por Build Character Animators).
        if (animator.runtimeAnimatorController == null) return;
        animator.SetFloat(HashSpeed, Mathf.Abs(controller.MoveX));
        animator.SetBool(HashGrounded, controller.IsGrounded);
        animator.SetFloat(HashVerticalVel, controller.Velocity.y);
    }
}
