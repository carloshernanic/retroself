using UnityEngine;

namespace Retroself.Mechanics
{
    public class SoundWaveEmitter : MonoBehaviour
    {
        public float interval = 1.6f;
        public Vector2 direction = Vector2.right;
        public Color waveColor = new Color(1f, 0.4f, 0.4f);
        public float waveSpeed = 3f;
        public float waveLife = 4f;

        float timer;

        void Update()
        {
            if (TimeFreezeSystem.Instance != null && TimeFreezeSystem.Instance.IsFrozen) return;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = interval;
                Spawn();
            }
        }

        void Spawn()
        {
            var go = new GameObject("SoundWave");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetCircle();
            sr.color = waveColor;
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var hazard = go.AddComponent<SoundWaveHazard>();
            hazard.direction = direction;
            hazard.speed = waveSpeed;
            hazard.lifetime = waveLife;
            hazard.sr = sr;
        }
    }
}
