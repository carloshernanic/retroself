using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Plataforma de retorno: quando qualquer player pisa, teleporta os dois Woody
// pras posições iniciais. Permite revisitar pistas dos puzzles 1/2/3 sem precisar
// resetar a fase. Cooldown evita re-trigger imediato (player aparece em cima do
// pad de novo no spawn? não — spawn fica em x=-3, longe do pad).
//
// No primeiro uso, também destrava portas registradas (ForceOpen) — usado pra
// revelar a câmara da chave (Puzzle 5). Auto-destrói quando KeyPickup.Collected
// vira true (chave já foi pega — não precisa mais voltar).
[RequireComponent(typeof(Collider2D))]
public class ReturnPad : MonoBehaviour
{
    public Transform young;
    public Transform adult;
    public Vector3 youngSpawn;
    public Vector3 adultSpawn;
    public float cooldown = 1.5f;

    // Portas que ficam destrancadas na primeira pisada (ex: LeftLockedDoor do P5).
    public List<GatedDoor> doorsToOpenOnFirstUse = new List<GatedDoor>();

    // Quando true, só o player ATIVO (PlayerSwap.Instance.ActivePlayer) é teleportado.
    // O outro Woody fica parado. Usado em Memory_03_Floresta pra cada Woody atravessar
    // o portal individualmente. Default false = teleporta os 2 (comportamento M_02).
    public bool teleportOnlyActive = false;

    // Quando true, pisar no pad não teleporta — o player precisa apertar `E` enquanto
    // está dentro do trigger. Usado em Memory_03 pra evitar teleporte acidental quando
    // o pad fica sobreposto a uma plataforma jogável (portal centralizado na ilha).
    public bool requireKeyPress = false;

    // Filho com TMP/Sprite que aparece quando algum player está dentro do trigger
    // (sinaliza "aperte E"). Só usado se requireKeyPress == true.
    public GameObject promptIndicator;

    private float lastUse = -10f;
    private bool firstUseDone;
    private readonly HashSet<PlayerController> inside = new HashSet<PlayerController>();

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (promptIndicator != null) promptIndicator.SetActive(false);
    }

    void Update()
    {
        // Some quando a chave do P5 é coletada — não tem mais pra que voltar.
        if (KeyPickup.Collected) { Destroy(gameObject); return; }

        if (!requireKeyPress) return;

        // Atualiza prompt enquanto houver player ativo dentro.
        var activeInside = AnyValidInsider();
        if (promptIndicator != null && promptIndicator.activeSelf != (activeInside != null))
            promptIndicator.SetActive(activeInside != null);

        if (activeInside == null) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.eKey.wasPressedThisFrame) Teleport(activeInside);
    }

    PlayerController AnyValidInsider()
    {
        inside.RemoveWhere(p => p == null);
        if (inside.Count == 0) return null;
        if (!teleportOnlyActive)
        {
            foreach (var p in inside) return p;
        }
        var activeGO = PlayerSwap.Instance != null ? PlayerSwap.Instance.ActivePlayer : null;
        if (activeGO == null) return null;
        foreach (var p in inside)
            if (p.gameObject == activeGO) return p;
        return null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        inside.Add(pc);
        if (requireKeyPress) return; // aguarda input em Update().

        if (Time.time - lastUse < cooldown) return;
        if (teleportOnlyActive)
        {
            var activeGO = PlayerSwap.Instance != null ? PlayerSwap.Instance.ActivePlayer : null;
            if (activeGO == null || other.gameObject != activeGO) return;
        }
        Teleport(pc);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        inside.Remove(pc);
    }

    void Teleport(PlayerController pc)
    {
        if (Time.time - lastUse < cooldown) return;
        lastUse = Time.time;

        if (!firstUseDone)
        {
            firstUseDone = true;
            foreach (var d in doorsToOpenOnFirstUse)
                if (d != null) d.ForceOpen();
        }

        if (teleportOnlyActive)
        {
            if (pc.kind == PlayerKind.Young) Recall(young, youngSpawn);
            else Recall(adult, adultSpawn);
        }
        else
        {
            Recall(young, youngSpawn);
            Recall(adult, adultSpawn);
        }
    }

    static void Recall(Transform t, Vector3 pos)
    {
        if (t == null) return;
        t.position = pos;
        var rb = t.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
