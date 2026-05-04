using Retroself.Player;
using UnityEngine;

namespace Retroself.UI
{
    public class InteractPrompt : MonoBehaviour
    {
        public GameObject visual;
        public WoodyKind requiredKind = WoodyKind.Adult;
        public float showRadius = 1.2f;

        void Update()
        {
            var party = PartyManager.Instance;
            bool show = false;
            if (party != null && party.Active != null && party.Active.kind == requiredKind)
            {
                if (Vector2.Distance(party.Active.transform.position, transform.position) <= showRadius)
                    show = true;
            }
            if (visual != null && visual.activeSelf != show) visual.SetActive(show);
        }
    }
}
