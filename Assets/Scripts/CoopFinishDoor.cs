using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Porta de fim de fase pra puzzles co-op: dispara o fade + load quando ambos
// os Woody (jovem e adulto) estão dentro do trigger ao mesmo tempo. Sem chave,
// sem inimigo — variante simples do SchoolDoor pra fases que não tenham gate
// extra além da co-presença.
[RequireComponent(typeof(Collider2D))]
public class CoopFinishDoor : MonoBehaviour
{
    // TODO: trocar pra SceneNames.Hub quando o Hub existir.
    public string targetScene = SceneNames.MainMenu;
    public float fadeDuration = 0.6f;

    [Header("Overlay (opcional)")]
    public Image fadeOverlay;

    [Header("Requisitos extras (opcional)")]
    // Se não vazio, a porta só abre quando todas as placas estão ativas.
    public List<PressurePlate> requiredPlates = new List<PressurePlate>();

    // Quando true, a porta exige KeyPickup.Collected (chave coletada).
    public bool requireKey = false;

    [Header("Visual")]
    public SpriteFrameAnimator openAnimator;

    private bool triggered;
    private bool openAnimPlayed;
    private readonly HashSet<PlayerController> inside = new HashSet<PlayerController>();

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        inside.Add(pc);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggered) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        inside.Remove(pc);
    }

    void Update()
    {
        if (triggered) return;
        if (!openAnimPlayed && IsUnlocked())
        {
            openAnimPlayed = true;
            if (openAnimator != null) openAnimator.Play();
        }
        if (inside.Count == 0) return;
        TryFire();
    }

    bool IsUnlocked()
    {
        foreach (var p in requiredPlates)
        {
            if (p == null) continue;
            if (!p.IsActive) return false;
        }
        if (requireKey && !KeyPickup.Collected) return false;
        return true;
    }

    void TryFire()
    {
        bool hasYoung = false, hasAdult = false;
        foreach (var pc in inside)
        {
            if (pc == null) continue;
            if (pc.kind == PlayerKind.Young) hasYoung = true;
            else if (pc.kind == PlayerKind.Adult) hasAdult = true;
        }
        if (!(hasYoung && hasAdult)) return;

        foreach (var p in requiredPlates)
        {
            if (p == null) continue;
            if (!p.IsActive) return;
        }

        if (requireKey && !KeyPickup.Collected) return;

        triggered = true;
        StartCoroutine(GoToScene());
    }

    IEnumerator GoToScene()
    {
        if (fadeOverlay != null)
        {
            Color start = fadeOverlay.color; start.a = 0f;
            Color end = new Color(0f, 0f, 0f, 1f);
            fadeOverlay.color = start;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.color = Color.Lerp(start, end, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            fadeOverlay.color = end;
        }
        SceneManager.LoadScene(targetScene);
    }
}
