using Retroself.Player;
using UnityEngine;

namespace Retroself.Level
{
    public class Checkpoint : MonoBehaviour
    {
        public Transform adultSpawn;
        public Transform youngSpawn;
        public bool autoActivateOnTouch = true;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!autoActivateOnTouch) return;
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            CheckpointManager.Instance?.SetCheckpoint(this);
        }
    }
}
