using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Botão de pressão para puzzles co-op estilo Pico Park. Ativa enquanto algum
// ocupante válido está em cima (PlayerController do kind certo, ou HeavyBox).
// Filtra via componente, não tag — sem strings mágicas.
[RequireComponent(typeof(Collider2D))]
public class PressurePlate : GateSource
{
    public enum Requirement { Any, Young, Adult, HeavyBox }

    public Requirement requirement = Requirement.Any;

    [Tooltip("Quando true, a 1ª ativação trava a plate ON pra sempre. Se o ocupante for " +
             "HeavyBox, a caixa também é snapada no centro X da plate e congelada.")]
    public bool latch = false;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    public override bool IsActive => active;
    private bool active;
    private bool latched;

    private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (latched) return;
        if (!Matches(other)) return;
        occupants.Add(other);
        Recheck();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (latched) return;
        if (!occupants.Remove(other)) return;
        Recheck();
    }

    bool Matches(Collider2D other)
    {
        switch (requirement)
        {
            case Requirement.HeavyBox:
                return other.GetComponent<HeavyBox>() != null;
            case Requirement.Young:
            case Requirement.Adult:
            {
                var pc = other.GetComponent<PlayerController>();
                if (pc == null) return false;
                return requirement == Requirement.Young
                    ? pc.kind == PlayerKind.Young
                    : pc.kind == PlayerKind.Adult;
            }
            case Requirement.Any:
            default:
                return other.GetComponent<PlayerController>() != null
                    || other.GetComponent<HeavyBox>() != null;
        }
    }

    void Recheck()
    {
        // Limpa ocupantes que foram destruídos sem disparar OnTriggerExit2D
        // (raro, mas acontece se o GO some por Destroy enquanto sobreposto).
        occupants.RemoveWhere(c => c == null);

        bool nowActive = occupants.Count > 0;
        if (nowActive == active) return;
        active = nowActive;
        if (active)
        {
            SfxBeep.PlayPlateOn();
            OnActivated?.Invoke();
            if (latch) LatchNow();
        }
        else
        {
            SfxBeep.PlayPlateOff();
            OnDeactivated?.Invoke();
        }
    }

    // Trava a plate ON permanente. Se algum ocupante é HeavyBox, snapa no centro X
    // da plate e congela o RB (Static) — visual de "caixa encaixada".
    void LatchNow()
    {
        latched = true;
        foreach (var occ in occupants)
        {
            if (occ == null) continue;
            var box = occ.GetComponent<HeavyBox>();
            if (box == null) continue;
            var rb = box.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            var pos = box.transform.position;
            box.transform.position = new Vector3(transform.position.x, pos.y, pos.z);
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            box.enabled = false; // não tenta mais aplicar empurrão / constraints
        }
    }
}
