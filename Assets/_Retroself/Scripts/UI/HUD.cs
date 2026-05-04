using Retroself.Mechanics;
using UnityEngine;
using UnityEngine.UI;

namespace Retroself.UI
{
    public class HUD : MonoBehaviour
    {
        public Image energyFill;
        public GameObject energyRoot;
        public Image freezeOverlay;
        public Text controlHint;
        public Text characterLabel;

        void Update()
        {
            var freeze = TimeFreezeSystem.Instance;
            bool unlocked = freeze != null && freeze.unlocked;
            if (energyRoot != null) energyRoot.SetActive(unlocked);
            if (energyFill != null && freeze != null)
            {
                energyFill.fillAmount = freeze.EnergyNormalized;
                energyFill.color = freeze.IsFrozen ? new Color(0.6f, 0.85f, 1f) : new Color(0.85f, 0.7f, 0.3f);
            }
            if (freezeOverlay != null && freeze != null)
            {
                var c = freezeOverlay.color;
                c.a = freeze.IsFrozen ? 0.20f : 0f;
                freezeOverlay.color = c;
            }
            var party = Player.PartyManager.Instance;
            if (party != null && party.Active != null && characterLabel != null)
            {
                characterLabel.text = party.Active.kind == Player.WoodyKind.Adult ? "Woody (30)" : "Woody";
            }
        }
    }
}
