using System.Collections.Generic;
using Retroself.Core;
using Retroself.Player;
using UnityEngine;

namespace Retroself.Mechanics
{
    public class TimeFreezeSystem : MonoBehaviour
    {
        public static TimeFreezeSystem Instance { get; private set; }

        public float maxEnergy = 5f;
        public float drainPerSecond = 1f;
        public float rechargePerSecond = 0.6f;
        public float rechargeDelay = 0.5f;

        public bool IsFrozen { get; private set; }
        public float Energy { get; private set; }
        public float EnergyNormalized => Mathf.Clamp01(Energy / maxEnergy);

        public bool unlocked = true;

        readonly List<IFreezable> freezables = new List<IFreezable>();
        float rechargeTimer;

        public System.Action<bool> OnFreezeChanged;

        void Awake()
        {
            Instance = this;
            Energy = maxEnergy;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Register(IFreezable f) { if (!freezables.Contains(f)) freezables.Add(f); }
        public void Unregister(IFreezable f) { freezables.Remove(f); }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (!unlocked) { if (IsFrozen) StopFreeze(); return; }

            var party = PartyManager.Instance;
            bool adultActive = party != null && party.Active != null && party.Active.kind == WoodyKind.Adult;

            bool wantFreeze = adultActive && InputReader.Instance != null && InputReader.Instance.FreezeHeld && Energy > 0f;

            if (wantFreeze && !IsFrozen) StartFreeze();
            else if (!wantFreeze && IsFrozen) StopFreeze();

            if (IsFrozen)
            {
                Energy -= drainPerSecond * Time.unscaledDeltaTime;
                rechargeTimer = rechargeDelay;
                if (Energy <= 0f) { Energy = 0f; StopFreeze(); }
            }
            else
            {
                if (rechargeTimer > 0f) rechargeTimer -= Time.unscaledDeltaTime;
                else Energy = Mathf.Min(maxEnergy, Energy + rechargePerSecond * Time.unscaledDeltaTime);
            }
        }

        void StartFreeze()
        {
            IsFrozen = true;
            for (int i = 0; i < freezables.Count; i++) freezables[i]?.OnFreezeStart();
            OnFreezeChanged?.Invoke(true);
        }

        void StopFreeze()
        {
            IsFrozen = false;
            for (int i = 0; i < freezables.Count; i++) freezables[i]?.OnFreezeEnd();
            OnFreezeChanged?.Invoke(false);
        }
    }

    public interface IFreezable
    {
        void OnFreezeStart();
        void OnFreezeEnd();
    }
}
