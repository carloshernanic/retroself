using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneFader : MonoBehaviour
{
    public Image overlay;
    public float defaultDuration = 0.6f;

    public IEnumerator FadeTo(Color target, float duration)
    {
        if (overlay == null) yield break;
        Color start = overlay.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            overlay.color = Color.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        overlay.color = target;
    }

    public IEnumerator FadeIn(Color color, float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        yield return FadeTo(new Color(color.r, color.g, color.b, 1f), duration);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        if (overlay == null) yield break;
        Color c = overlay.color;
        yield return FadeTo(new Color(c.r, c.g, c.b, 0f), duration);
    }

    public void SetInstant(Color color)
    {
        if (overlay != null) overlay.color = color;
    }
}
