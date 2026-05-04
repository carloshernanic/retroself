using System.Collections.Generic;
using Retroself.Core;
using UnityEngine;

namespace Retroself.Player
{
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }

        public List<WoodyController> members = new List<WoodyController>();
        public int activeIndex;
        public bool allowSwitch = true;

        public WoodyController Active => (members.Count > 0) ? members[Mathf.Clamp(activeIndex, 0, members.Count - 1)] : null;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            ApplyActive();
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (!allowSwitch) return;
            if (InputReader.Instance != null && InputReader.Instance.SwitchPressed)
                Switch();
        }

        public void Register(WoodyController w)
        {
            if (!members.Contains(w)) members.Add(w);
            ApplyActive();
        }

        public void Switch()
        {
            if (members.Count < 2) return;
            activeIndex = (activeIndex + 1) % members.Count;
            ApplyActive();
        }

        public void SetActive(WoodyKind kind)
        {
            for (int i = 0; i < members.Count; i++)
                if (members[i].kind == kind) { activeIndex = i; break; }
            ApplyActive();
        }

        void ApplyActive()
        {
            for (int i = 0; i < members.Count; i++)
                members[i].SetActive(i == activeIndex);
        }
    }
}
