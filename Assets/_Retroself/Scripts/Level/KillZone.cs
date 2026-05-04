using Retroself.Player;
using UnityEngine;

namespace Retroself.Level
{
    public class KillZone : MonoBehaviour
    {
        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            CheckpointManager.Instance?.RespawnAll();
        }
    }
}
