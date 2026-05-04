using Retroself.Player;
using UnityEngine;

namespace Retroself.Level
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }
        public Checkpoint Current { get; private set; }
        public Vector2 fallbackAdultSpawn;
        public Vector2 fallbackYoungSpawn;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetCheckpoint(Checkpoint c) { Current = c; }

        public Vector2 GetSpawn(WoodyKind kind)
        {
            if (Current != null)
            {
                if (kind == WoodyKind.Adult && Current.adultSpawn != null) return Current.adultSpawn.position;
                if (kind == WoodyKind.Young && Current.youngSpawn != null) return Current.youngSpawn.position;
            }
            return kind == WoodyKind.Adult ? fallbackAdultSpawn : fallbackYoungSpawn;
        }

        public void RespawnAll()
        {
            var party = PartyManager.Instance;
            if (party == null) return;
            foreach (var m in party.members)
            {
                var motor = m.GetComponent<CharacterMotor>();
                if (motor != null) motor.Teleport(GetSpawn(m.kind));
            }
            if (Mechanics.CarrySystem.Instance != null) Mechanics.CarrySystem.Instance.Drop();
        }
    }
}
