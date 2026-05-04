using Retroself.Core;
using UnityEngine;

namespace Retroself.Player
{
    public enum WoodyKind { Adult, Young }

    [RequireComponent(typeof(CharacterMotor))]
    public class WoodyController : MonoBehaviour
    {
        public WoodyKind kind = WoodyKind.Adult;
        public bool IsActive { get; private set; }

        public SpriteRenderer body;
        public Color activeTint = Color.white;
        public Color inactiveTint = new Color(0.6f, 0.6f, 0.65f, 1f);

        CharacterMotor motor;

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

            if (IsActive && InputReader.Instance != null)
            {
                motor.SetInput(InputReader.Instance.Horizontal,
                    InputReader.Instance.JumpPressed,
                    InputReader.Instance.JumpHeld);
            }
            else
            {
                motor.SetInput(0f, false, false);
            }

            if (body != null)
            {
                body.color = IsActive ? activeTint : inactiveTint;
                if (motor.Facing != 0)
                    body.flipX = motor.Facing < 0;
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            motor.ControlEnabled = active;
        }
    }
}
