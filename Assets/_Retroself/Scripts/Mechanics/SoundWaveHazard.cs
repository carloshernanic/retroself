using Retroself.Level;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class SoundWaveHazard : MonoBehaviour, IFreezable
    {
        public float speed = 3f;
        public float lifetime = 4f;
        public Vector2 direction = Vector2.right;
        public SpriteRenderer sr;

        bool frozen;
        float age;

        void Awake() { if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(); }
        void OnEnable() { TimeFreezeSystem.Instance?.Register(this); }
        void OnDisable() { TimeFreezeSystem.Instance?.Unregister(this); }

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void Update()
        {
            if (frozen) return;
            transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
            age += Time.deltaTime;
            if (age > lifetime) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            CheckpointManager.Instance?.RespawnAll();
        }

        public void OnFreezeStart() { frozen = true; if (sr != null) sr.color = new Color(0.6f, 0.85f, 1f); }
        public void OnFreezeEnd() { frozen = false; if (sr != null) sr.color = Color.white; }
    }
}
