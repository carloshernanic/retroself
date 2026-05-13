using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Barraca de comida. Latched GateSource que ativa quando o inventário do player
// contém os items pedidos (countagem por kind). Uma vez ativa, fica — o gate
// associado abre permanente.
//
// Visual: optional `satisfiedSprite` swap quando ativa (sprite "feliz"), e
// optional `requirementLabel` (TMP) pra debug/dicas.
public class VendorStall : GateSource
{
    [System.Serializable]
    public struct Need { public FoodKind kind; public int count; }

    public List<Need> required = new List<Need>();

    [Header("Visual")]
    public SpriteRenderer satisfiedSpriteHost;
    public Sprite waitingSprite;
    public Sprite satisfiedSprite;

    public UnityEvent OnSatisfied;

    public override bool IsActive => satisfied;
    private bool satisfied;

    void Update()
    {
        if (satisfied) return;
        foreach (var n in required)
        {
            if (!FoodInventory.Has(n.kind, Mathf.Max(1, n.count))) return;
        }
        satisfied = true;
        if (satisfiedSpriteHost != null && satisfiedSprite != null) satisfiedSpriteHost.sprite = satisfiedSprite;
        SfxBeep.PlayPlateOn();
        OnSatisfied?.Invoke();
    }
}
