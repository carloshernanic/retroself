using UnityEngine;

namespace Retroself.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterMotor : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpVelocity = 10f;
        public float gravityScale = 3f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.1f;
        public LayerMask groundMask;
        public Transform groundCheck;
        public float groundCheckRadius = 0.12f;
        public bool variableJump = true;
        public float jumpCutMultiplier = 0.5f;

        public bool ControlEnabled = true;
        public bool IsGrounded { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;
        public int Facing { get; private set; } = 1;

        Rigidbody2D rb;
        BoxCollider2D box;
        float coyoteCounter;
        float jumpBufferCounter;
        float horizontalInput;
        bool jumpPressed;
        bool jumpHeld;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            rb.gravityScale = gravityScale;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void SetInput(float horizontal, bool jumpDown, bool jumpHold)
        {
            horizontalInput = horizontal;
            if (jumpDown) jumpPressed = true;
            jumpHeld = jumpHold;
        }

        void FixedUpdate()
        {
            CheckGround();

            if (!ControlEnabled)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            if (IsGrounded) coyoteCounter = coyoteTime;
            else coyoteCounter -= Time.fixedDeltaTime;

            if (jumpPressed)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpPressed = false;
            }
            else jumpBufferCounter -= Time.fixedDeltaTime;

            float vx = horizontalInput * moveSpeed;
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

            if (Mathf.Abs(horizontalInput) > 0.1f)
                Facing = horizontalInput > 0 ? 1 : -1;

            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            if (variableJump && !jumpHeld && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }

        void CheckGround()
        {
            if (groundCheck != null)
            {
                IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
            }
            else
            {
                Bounds b = box.bounds;
                Vector2 origin = new Vector2(b.center.x, b.min.y);
                Vector2 size = new Vector2(b.size.x * 0.9f, 0.05f);
                IsGrounded = Physics2D.OverlapBox(origin, size, 0f, groundMask);
            }
        }

        public void Teleport(Vector2 position)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = position;
        }

        public void SetVelocity(Vector2 v) => rb.linearVelocity = v;

        void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
