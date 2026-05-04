using Retroself.Core;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class CarrySystem : MonoBehaviour
    {
        public static CarrySystem Instance { get; private set; }

        public float pickupRange = 1.0f;
        public float carryYOffset = 0.9f;
        public float carrySpeedMultiplier = 0.7f;

        public bool IsCarrying { get; private set; }
        WoodyController carrier;
        WoodyController carried;
        float originalSpeed;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (InputReader.Instance == null || !InputReader.Instance.InteractPressed) return;

            var party = PartyManager.Instance;
            if (party == null || party.Active == null) return;

            if (IsCarrying) Drop();
            else TryPickup(party.Active);
        }

        void LateUpdate()
        {
            if (IsCarrying && carrier != null && carried != null)
            {
                carried.transform.position = carrier.transform.position + Vector3.up * carryYOffset;
                var rb = carried.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        void TryPickup(WoodyController active)
        {
            if (active.kind != WoodyKind.Adult) return;
            var party = PartyManager.Instance;
            foreach (var m in party.members)
            {
                if (m == active || m.kind != WoodyKind.Young) continue;
                if (Vector2.Distance(m.transform.position, active.transform.position) <= pickupRange)
                {
                    carrier = active;
                    carried = m;
                    var motor = carrier.GetComponent<CharacterMotor>();
                    if (motor != null) { originalSpeed = motor.moveSpeed; motor.moveSpeed *= carrySpeedMultiplier; }
                    var rb = carried.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
                    var col = carried.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                    IsCarrying = true;
                    return;
                }
            }
        }

        public void Drop()
        {
            if (!IsCarrying) return;
            var motor = carrier.GetComponent<CharacterMotor>();
            if (motor != null) motor.moveSpeed = originalSpeed;
            var rb = carried.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
            var col = carried.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
            carried.transform.position = carrier.transform.position + Vector3.up * carryYOffset + Vector3.right * carrier.GetComponent<CharacterMotor>().Facing * 0.4f;
            IsCarrying = false;
            carrier = null;
            carried = null;
        }
    }
}
