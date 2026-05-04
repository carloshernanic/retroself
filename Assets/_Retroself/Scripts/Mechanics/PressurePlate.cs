using System.Collections.Generic;
using Retroself.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Retroself.Mechanics
{
    public class PressurePlate : MonoBehaviour
    {
        public enum WeightRequirement { Any, Adult, Young, AdultOrBox }
        public WeightRequirement requirement = WeightRequirement.Any;
        public bool latched;
        public UnityEvent onActivated = new UnityEvent();
        public UnityEvent onDeactivated = new UnityEvent();
        public SpriteRenderer plateSprite;
        public Color idleColor = new Color(0.5f, 0.5f, 0.55f);
        public Color activeColor = new Color(1f, 0.85f, 0.4f);

        readonly HashSet<Collider2D> overlapping = new HashSet<Collider2D>();
        bool active;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other) { overlapping.Add(other); Reevaluate(); }
        void OnTriggerExit2D(Collider2D other) { overlapping.Remove(other); Reevaluate(); }

        void Reevaluate()
        {
            bool ok = false;
            foreach (var c in overlapping)
            {
                if (c == null) continue;
                if (Matches(c)) { ok = true; break; }
            }
            if (ok && !active) { active = true; onActivated?.Invoke(); UpdateColor(); }
            else if (!ok && active && !latched) { active = false; onDeactivated?.Invoke(); UpdateColor(); }
            else UpdateColor();
        }

        bool Matches(Collider2D c)
        {
            var w = c.GetComponentInParent<WoodyController>();
            if (w != null)
            {
                switch (requirement)
                {
                    case WeightRequirement.Any: return true;
                    case WeightRequirement.Young: return w.kind == WoodyKind.Young;
                    case WeightRequirement.Adult:
                    case WeightRequirement.AdultOrBox:
                        return w.kind == WoodyKind.Adult;
                }
            }
            if (requirement == WeightRequirement.AdultOrBox)
            {
                if (c.GetComponentInParent<PushableBox>() != null) return true;
            }
            return false;
        }

        void UpdateColor()
        {
            if (plateSprite != null) plateSprite.color = active ? activeColor : idleColor;
        }
    }
}
