using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class GapTrigger : MonoBehaviour
    {
        public WoodyKind allowedKind = WoodyKind.Young;
        public Collider2D blocker;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null || blocker == null) return;
            if (w.kind == allowedKind) blocker.enabled = false;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null || blocker == null) return;
            blocker.enabled = true;
        }
    }
}
