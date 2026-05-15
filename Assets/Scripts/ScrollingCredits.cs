using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Rola um RectTransform de baixo pra cima estilo créditos de filme. Quando o bloco
// inteiro sai por cima do viewport, espera `endHoldSeconds` e carrega `finalScene`.
// Esc/Espaço pulam direto pra `finalScene`.
//
// Usa unscaledDeltaTime defensivamente (cutscene anterior pode ter deixado timeScale=0).
public class ScrollingCredits : MonoBehaviour
{
    public RectTransform creditsBlock;
    public RectTransform viewport;
    public float scrollSpeed = 60f;
    public float endHoldSeconds = 1.5f;
    public string finalScene = "MainMenu";

    bool started;
    bool done;
    float doneTimer;

    void OnEnable()
    {
        StartScroll();
    }

    void StartScroll()
    {
        if (started) return;
        if (creditsBlock == null || viewport == null) return;
        // Posiciona o bloco de modo que o TOPO entre na metade inferior do viewport
        // (não totalmente embaixo). Title fica visível imediatamente — evita ~10s
        // de "esperando o texto aparecer" estilo Star Wars puro.
        float startY = -creditsBlock.rect.height * 0.5f - viewport.rect.height * 0.25f;
        creditsBlock.anchoredPosition = new Vector2(creditsBlock.anchoredPosition.x, startY);
        started = true;
    }

    void Update()
    {
        if (!started) StartScroll();
        if (creditsBlock == null || viewport == null) return;

        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
        {
            SceneManager.LoadScene(finalScene);
            return;
        }

        if (!done)
        {
            creditsBlock.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;
            // "Saiu da tela" = bottom do bloco passou do top do viewport.
            float blockBottom = creditsBlock.anchoredPosition.y - creditsBlock.rect.height * 0.5f;
            float viewportTop = viewport.rect.height * 0.5f;
            if (blockBottom > viewportTop) done = true;
        }
        else
        {
            doneTimer += Time.unscaledDeltaTime;
            if (doneTimer >= endHoldSeconds) SceneManager.LoadScene(finalScene);
        }
    }
}
