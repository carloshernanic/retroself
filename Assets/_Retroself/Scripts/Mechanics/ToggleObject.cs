using UnityEngine;

namespace Retroself.Mechanics
{
    public class ToggleObject : MonoBehaviour
    {
        public GameObject[] enableWhenOn;
        public GameObject[] disableWhenOn;

        public void SetOn(bool on)
        {
            foreach (var g in enableWhenOn) if (g != null) g.SetActive(on);
            foreach (var g in disableWhenOn) if (g != null) g.SetActive(!on);
        }

        public void TurnOn() => SetOn(true);
        public void TurnOff() => SetOn(false);
    }
}
