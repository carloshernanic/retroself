using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Trigger da porta de saída do tutorial. Quando qualquer Woody encosta, faz um
// fade preto e carrega a cena alvo. Por padrão vai pra Memory_01_School (placeholder
// "Fim do tutorial").
[RequireComponent(typeof(Collider2D))]
public class SchoolDoor : MonoBehaviour
{
    public string targetScene = SceneNames.Memory_01_School;
    public float fadeDuration = 0.6f;

    [Header("Overlay (opcional)")]
    public Image fadeOverlay;

    private bool triggered;

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
