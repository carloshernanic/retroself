using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Pisca um overlay (Image ou SpriteRenderer) imitando relâmpago. Intervalo aleatório.
public class LightningFlash : MonoBehaviour
{
    public Image targetImage;
    public SpriteRenderer targetSprite;

    public float minInterval = 6f;
    public float maxInterval = 14f;
    public float flashDuration = 0.08f;
    public float fadeDuration = 0.35f;
    [Range(0f, 1f)] public float peakAlpha = 0.6f;

    void Start()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        SetAlpha(0f);
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            yield return Flash();
        }
    }

    IEnumerator Flash()
    {
        SetAlpha(peakAlpha);
        yield return new WaitForSeconds(flashDuration);
        // Fade out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(peakAlpha, 0f, t / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (targetImage != null)
        {
            var c = targetImage.color; c.a = a; targetImage.color = c;
        }
        if (targetSprite != null)
        {
            var c = targetSprite.color; c.a = a; targetSprite.color = c;
        }
    }
}
