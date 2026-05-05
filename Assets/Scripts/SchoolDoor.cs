using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Trigger da porta de saída do tutorial. Só dispara o fade + load quando AMBOS
// os Woody (jovem e adulto) estão dentro do trigger ao mesmo tempo — a fala da
// intro deixa claro que essa fase é sobre os dois entrarem juntos na escola.
[RequireComponent(typeof(Collider2D))]
public class SchoolDoor : MonoBehaviour
{
    public string targetScene = SceneNames.Memory_01_School;
    public float fadeDuration = 0.6f;

    [Header("Overlay (opcional)")]
    public Image fadeOverlay;

    private bool triggered;
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
        TryFire();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggered) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        inside.Remove(pc);
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
