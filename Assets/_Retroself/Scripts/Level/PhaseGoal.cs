using System.Collections.Generic;
using Retroself.Core;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Level
{
    public class PhaseGoal : MonoBehaviour
    {
        public string phaseId = "phase_1";
        public string nextScene = "Hub";
        public bool requiresBothWoodys = true;

        readonly HashSet<WoodyKind> inside = new HashSet<WoodyKind>();
        bool triggered;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            inside.Add(w.kind);
            CheckTrigger();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            inside.Remove(w.kind);
        }

        void CheckTrigger()
        {
            if (triggered) return;
            bool ok = requiresBothWoodys
                ? (inside.Contains(WoodyKind.Adult) && inside.Contains(WoodyKind.Young))
                : inside.Count > 0;
            if (!ok) return;
            triggered = true;

            if (GameManager.Instance != null) GameManager.Instance.MarkPhaseComplete(phaseId);
            FindFirstObjectByType<global::Retroself.UI.SceneFader>()?.FadeAndLoad(nextScene);
        }
    }
}
