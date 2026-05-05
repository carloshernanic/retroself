using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth target;
    public Image fill;
    public RectTransform fillRect;

    private float maxWidth;

    void Start()
    {
        if (fillRect != null) maxWidth = fillRect.sizeDelta.x;
        if (PlayerSwap.Instance != null)
        {
            PlayerSwap.Instance.OnActiveChanged += HandleActiveChanged;
            if (PlayerSwap.Instance.ActivePlayer != null)
                Bind(PlayerSwap.Instance.ActivePlayer.GetComponent<PlayerHealth>());
            else
                Bind(target);
        }
        else
        {
            Bind(target);
        }
    }

    void OnDestroy()
    {
        if (PlayerSwap.Instance != null)
            PlayerSwap.Instance.OnActiveChanged -= HandleActiveChanged;
        Unbind();
    }

    void HandleActiveChanged(GameObject newActive)
    {
        if (newActive == null) return;
        Bind(newActive.GetComponent<PlayerHealth>());
    }

    void Bind(PlayerHealth ph)
    {
        Unbind();
        target = ph;
        if (target == null) return;
        target.OnHealthChanged += HandleChanged;
        HandleChanged(target.CurrentHealth, target.maxHealth);
    }

    void Unbind()
    {
        if (target != null) target.OnHealthChanged -= HandleChanged;
    }

    void HandleChanged(int current, int max)
    {
        if (fillRect == null || max <= 0) return;
        float ratio = (float)current / max;
        fillRect.sizeDelta = new Vector2(maxWidth * ratio, fillRect.sizeDelta.y);
        if (fill != null) fill.color = ratio > 0.5f ? new Color(0.55f, 0.85f, 0.4f) :
                                       ratio > 0.25f ? new Color(0.95f, 0.75f, 0.3f) :
                                                       new Color(0.85f, 0.3f, 0.3f);
    }
}
