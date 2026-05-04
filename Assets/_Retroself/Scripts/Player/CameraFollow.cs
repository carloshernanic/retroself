using UnityEngine;

namespace Retroself.Player
{
    public class CameraFollow : MonoBehaviour
    {
        public float smoothTime = 0.18f;
        public Vector2 offset = new Vector2(0f, 1f);
        public Vector2 minBounds = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        public Vector2 maxBounds = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        Vector3 vel;

        void LateUpdate()
        {
            var party = PartyManager.Instance;
            if (party == null || party.Active == null) return;

            Vector3 target = party.Active.transform.position + (Vector3)offset;
            target.z = transform.position.z;
            target.x = Mathf.Clamp(target.x, minBounds.x, maxBounds.x);
            target.y = Mathf.Clamp(target.y, minBounds.y, maxBounds.y);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, smoothTime);
        }
    }
}
