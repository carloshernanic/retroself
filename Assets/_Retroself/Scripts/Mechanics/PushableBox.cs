using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableBox : MonoBehaviour
    {
        public bool requiresAdult = true;
        public float adultPushForce = 1f;
        public float dragWhileResting = 8f;
        public float dragWhilePushed = 1.5f;

        Rigidbody2D rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.linearDamping = dragWhileResting;
        }

        void OnCollisionStay2D(Collision2D c)
        {
            var w = c.collider.GetComponentInParent<WoodyController>();
            if (w == null || !w.IsActive) return;
            if (requiresAdult && w.kind != WoodyKind.Adult) return;

            var motor = w.GetComponent<CharacterMotor>();
            if (motor == null) return;

            float vx = motor.Velocity.x;
            if (Mathf.Abs(vx) < 0.05f) return;

            rb.linearDamping = dragWhilePushed;
            float boxX = rb.linearVelocity.x;
            float target = vx * adultPushForce;
            rb.linearVelocity = new Vector2(Mathf.MoveTowards(boxX, target, 12f * Time.fixedDeltaTime), rb.linearVelocity.y);
        }

        void FixedUpdate()
        {
            if (Mathf.Abs(rb.linearVelocity.x) < 0.05f) rb.linearDamping = dragWhileResting;
        }
    }
}
