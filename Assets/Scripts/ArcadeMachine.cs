using UnityEngine;
using UnityEngine.InputSystem;

// Cabine de fliperama: trigger zone que mostra um prompt "[ESPAÇO]" quando o
// player está dentro e, ao pressionar Espaço, abre o MinigameOverlay associado.
// Quando o minigame vence, `cleared` vira true (permanente) e este componente
// vira `GateSource` ativo — gate seguinte abre.
[RequireComponent(typeof(Collider2D))]
public class ArcadeMachine : GateSource
{
    public MinigameOverlay overlay;

    [Header("Visual")]
    // Prompt opcional (GameObject filho com SpriteRenderer ou TMP world-space).
    public GameObject promptIndicator;

    public override bool IsActive => cleared;
    private bool cleared;
    private bool playerInside;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (promptIndicator != null) promptIndicator.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        playerInside = true;
        UpdatePrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        playerInside = false;
        UpdatePrompt();
    }

    void Update()
    {
        if (!playerInside) return;
        if (cleared) return;
        if (overlay == null) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        // Espaço também é "pular" pro PlayerController, mas o controller só lê
        // input enquanto enabled. O minigame só abre se algum player está aqui
        // e Espaço foi pressionado nesse frame — colisão de input é improvável
        // na prática (Adult inativo, e Young encostado já não anda).
        if (kb.spaceKey.wasPressedThisFrame)
        {
            overlay.Open(OnMinigameWon);
        }
    }

    void OnMinigameWon()
    {
        cleared = true;
        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        if (promptIndicator == null) return;
        promptIndicator.SetActive(playerInside && !cleared);
    }
}
