using System.Collections;
using UnityEngine;

// Mostra um ícone (target/aim) em cima do quadrado do StoneSwitch e pisca uma
// variante "glitch" por uma fração de segundo quando o switch é ativado.
// Poll de IsActive (não UnityEvent) pra que a wiring sobreviva ao save do
// builder — mesma razão do PressurePlateVisual.
[RequireComponent(typeof(StoneSwitch))]
public class StoneSwitchIcon : MonoBehaviour
{
    public SpriteRenderer iconRenderer;
    public Sprite normalSprite;
    public Sprite glitchSprite;
    public float glitchFlashDuration = 0.45f;
    public int glitchFlashCycles = 3;

    StoneSwitch sw;
    bool wasActive;
    Coroutine running;

    void Awake()
    {
        sw = GetComponent<StoneSwitch>();
        if (iconRenderer != null && normalSprite != null) iconRenderer.sprite = normalSprite;
    }

    void Update()
    {
        if (sw == null) return;
        bool now = sw.IsActive;
        if (now && !wasActive)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(FlashGlitch());
        }
        wasActive = now;
    }

    IEnumerator FlashGlitch()
    {
        if (iconRenderer == null || glitchSprite == null || normalSprite == null) yield break;
        int frames = Mathf.Max(2, glitchFlashCycles * 2);
        float per = glitchFlashDuration / frames;
        for (int i = 0; i < frames; i++)
        {
            iconRenderer.sprite = (i % 2 == 0) ? glitchSprite : normalSprite;
            yield return new WaitForSeconds(per);
        }
        iconRenderer.sprite = normalSprite;
        running = null;
    }
}
