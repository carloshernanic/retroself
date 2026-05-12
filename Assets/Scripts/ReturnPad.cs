using System.Collections.Generic;
using UnityEngine;

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

    private float lastUse = -10f;
    private bool firstUseDone;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        // Some quando a chave do P5 é coletada — não tem mais pra que voltar.
        if (KeyPickup.Collected) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - lastUse < cooldown) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // Modo "só ativo": ignora trigger se quem pisou não é o player ativo.
        // Evita também que o inativo dispare o portal (PlayerSwap desabilita o
        // controller do inativo, mas o collider continua lá).
        if (teleportOnlyActive)
        {
            var activeGO = PlayerSwap.Instance != null ? PlayerSwap.Instance.ActivePlayer : null;
            if (activeGO == null || other.gameObject != activeGO) return;
        }

        lastUse = Time.time;

        if (!firstUseDone)
        {
            firstUseDone = true;
            foreach (var d in doorsToOpenOnFirstUse)
            {
                if (d != null) d.ForceOpen();
            }
        }

        if (teleportOnlyActive)
        {
            // Só o ativo se move (PlayerKind decide qual spawn usar).
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
