using UnityEngine;
using UnityEngine.Events;

// Alvo na parede que ativa quando uma Stone (do PlayerAttack do Young) colide.
// Mesmo "API" (IsActive via GateSource) que PressurePlate, então o GatedDoor
// pode misturar stone-switch + plate na mesma lista de sources.
//
// Por padrão é latched (uma vez ativo, fica). non-latched faz pouco sentido
// pra esse primitivo (a pedra some no impacto, e o switch não tem como
// "saber" que o player saiu).
[RequireComponent(typeof(Collider2D))]
public class StoneSwitch : GateSource
{
    public bool latched = true;

    public UnityEvent OnHit;

    public override bool IsActive => active;
    private bool active;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Já ativo: ignora qualquer hit duplicado (mesmo non-latched). Pra re-armar,
        // alguém precisa chamar ResetSwitch — caso de uso: SequenceLock que reseta
        // todos os switches quando a sequência erra.
        if (active) return;
        if (other.GetComponent<Stone>() == null) return;
        active = true;
        SfxBeep.PlayTargetHit();
        OnHit?.Invoke();
    }

    // Reset programático — útil se algum dia precisarmos resetar puzzles.
    public void ResetSwitch()
    {
        active = false;
    }
}
