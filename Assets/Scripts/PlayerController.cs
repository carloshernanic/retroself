using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 13f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.1f;

    [Header("Ground check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [Header("Visual")]
    public SpriteRenderer body;

    private Rigidbody2D rb;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private float moveX;
    private bool facingRight = true;

    public bool IsGrounded => isGrounded;
    public float MoveX => moveX;
    public bool FacingRight => facingRight;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        if (body == null) body = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        ReadInput();
        UpdateTimers();
        TryJump();
        Flip();
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        var hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].attachedRigidbody != rb) return true;
        }
        return false;
    }

    void ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null)
        {
            moveX = 0f;
            return;
        }

        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        moveX = x;

        if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBuffer;
    }

    void UpdateTimers()
    {
        coyoteCounter = isGrounded ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);
        jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
    }

    void TryJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    void Flip()
    {
        if (body == null) return;
        if (moveX > 0.01f && !facingRight) { facingRight = true; body.flipX = false; }
        else if (moveX < -0.01f && facingRight) { facingRight = false; body.flipX = true; }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
