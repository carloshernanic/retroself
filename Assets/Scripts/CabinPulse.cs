using UnityEngine;

public class CabinPulse : MonoBehaviour
{
    public SpriteRenderer cabinGlow;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1f;
    public float pulseSpeed = 1.5f;

    void Update()
    {
        if (cabinGlow == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = cabinGlow.color;
        c.a = alpha;
        cabinGlow.color = c;
    }
}
