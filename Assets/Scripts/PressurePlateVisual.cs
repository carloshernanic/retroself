using UnityEngine;

// Feedback visual de qualquer GateSource (PressurePlate, StoneSwitch): troca a
// cor do SpriteRenderer quando o estado IsActive muda. Poll-based pra que a
// wiring sobreviva ao save da cena (UnityEvent.AddListener via builder editor
// é runtime-only).
public class PressurePlateVisual : MonoBehaviour
{
    public GateSource target;
    public new SpriteRenderer renderer;
    public Color offColor = new Color(0.35f, 0.20f, 0.18f, 1f);
    public Color onColor  = new Color(0.95f, 0.78f, 0.30f, 1f);

    private bool? lastActive;

    void Update()
    {
        if (target == null || renderer == null) return;
        bool active = target.IsActive;
        if (lastActive == active) return;
        lastActive = active;
        renderer.color = active ? onColor : offColor;
    }
}
