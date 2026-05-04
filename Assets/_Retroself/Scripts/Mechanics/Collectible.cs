using Retroself.Core;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class Collectible : MonoBehaviour
    {
        public string collectibleId = "photo_default";
        public SpriteRenderer icon;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            if (GameManager.Instance != null) GameManager.Instance.collectiblesFound++;
            global::Retroself.Audio.AudioManager.Instance?.PlayBeep(880f, 0.12f, 0.4f);
            Destroy(gameObject);
        }
    }
}
