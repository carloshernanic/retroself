using Retroself.Level;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class PatrollingEnemy : MonoBehaviour, IFreezable
    {
        public float speed = 2f;
        public float patrolHalfRange = 3f;
        public Transform body;
        public WoodyKind targetKind = WoodyKind.Young;
        public float catchRadius = 0.5f;
        public bool freezable = true;

        Vector3 origin;
        int dir = 1;
        bool frozen;
        SpriteRenderer sr;

        void Awake()
        {
            origin = transform.position;
            if (body == null) body = transform;
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (freezable) TimeFreezeSystem.Instance?.Register(this);
        }

        void OnDisable()
        {
            if (freezable) TimeFreezeSystem.Instance?.Unregister(this);
        }

        void Update()
        {
            if (frozen) return;

            Vector3 p = body.position;
            p.x += dir * speed * Time.deltaTime;
            float local = p.x - origin.x;
            if (local > patrolHalfRange) { p.x = origin.x + patrolHalfRange; dir = -1; }
            else if (local < -patrolHalfRange) { p.x = origin.x - patrolHalfRange; dir = 1; }
            body.position = p;

            if (sr != null) sr.flipX = dir < 0;

            var party = PartyManager.Instance;
            if (party != null)
            {
                foreach (var m in party.members)
                {
                    if (m.kind != targetKind) continue;
                    if (Vector2.Distance(m.transform.position, body.position) <= catchRadius)
                    {
                        CheckpointManager.Instance?.RespawnAll();
                        return;
                    }
                }
            }
        }

        public void OnFreezeStart()
        {
            frozen = true;
            if (sr != null) sr.color = new Color(0.6f, 0.8f, 1f, 1f);
        }

        public void OnFreezeEnd()
        {
            frozen = false;
            if (sr != null) sr.color = Color.white;
        }
    }
}
