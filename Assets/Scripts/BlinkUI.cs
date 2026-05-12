using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlinkUI : MonoBehaviour
{
    public float speed = 2.5f;
    public float minAlpha = 0.25f;
    public float maxAlpha = 1f;

    private Graphic graphic;
    private TMP_Text tmp;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        tmp = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        ApplyAlpha(maxAlpha);
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        ApplyAlpha(Mathf.Lerp(minAlpha, maxAlpha, t));
    }

    void ApplyAlpha(float a)
    {
        if (tmp != null)
        {
            var c = tmp.color;
            c.a = a;
            tmp.color = c;
        }
        else if (graphic != null)
        {
            var c = graphic.color;
            c.a = a;
            graphic.color = c;
        }
    }
}
