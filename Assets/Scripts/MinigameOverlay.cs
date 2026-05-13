using System;
using UnityEngine;

// Base abstrata pra minigames de fliperama. Subclasses (Snake, GuitarHero)
// montam o conteúdo do painel e implementam TickGame.
//
// Comportamento comum:
//   Open(onWin)    — congela gameplay, ativa o canvas próprio, Time.timeScale=0,
//                     chama OnStart na subclasse, e guarda callback pra disparar
//                     em vitória.
//   Close(won)     — descongela, desativa o canvas, restaura Time.timeScale,
//                     dispara onWin se won==true.
//   TickGame()     — chamada em Update se isOpen; subclasse lê input (Keyboard.current)
//                     e atualiza estado usando Time.unscaledDeltaTime.
//   Win()/Lose()   — helpers pra subclasse fechar com resultado.
//
// Quem congela: PlayerController + PlayerAttack + PlayerSwap (mesmo padrão do
// IntroDialogue). Não congelamos Rigidbody2D — só desabilitamos controles,
// mas o Time.timeScale=0 zera Physics passo, então corpos param naturalmente.
public abstract class MinigameOverlay : MonoBehaviour
{
    [Header("Overlay")]
    public Canvas canvas;
    public GameObject panel;

    [Header("Players (pra congelar)")]
    public PlayerController young;
    public PlayerController adult;
    public PlayerSwap playerSwap;

    protected bool isOpen;
    Action onWinCallback;

    protected virtual void Awake()
    {
        // NÃO desligar o canvas aqui. Race condition: o builder grava o canvasGO
        // como inactive na scene. Se o player abre o minigame, Open() chama
        // canvas.gameObject.SetActive(true), o que dispara Awake na primeira
        // ativação — se este Awake desligar o canvas, ele some no mesmo frame
        // que abriu. O canvas já vem inactive do builder; nada a fazer aqui.
        if (isOpen && canvas != null) canvas.gameObject.SetActive(true);
    }

    public void Open(Action onWin)
    {
        if (isOpen) return;
        isOpen = true;
        onWinCallback = onWin;
        SetGameplayFrozen(true);
        Time.timeScale = 0f;
        if (canvas != null) canvas.gameObject.SetActive(true);
        // Layout do canvas só acontece num PreRender posterior. Subclasses leem
        // RectTransform.rect em OnStart (ex: SnakeMinigame.RebuildTiles → grid.rect.width),
        // então sem este force update gridArea fica 0×0 e a cobrinha aparece como
        // tiles de tamanho 0 — visualmente parece "jogo travado".
        Canvas.ForceUpdateCanvases();
        SfxBeep.PlayMinigameStart();
        OnStart();
    }

    protected void Win()
    {
        Close(true);
    }

    protected void Lose()
    {
        SfxBeep.PlayGameOver();
        Close(false);
    }

    void Close(bool won)
    {
        if (!isOpen) return;
        isOpen = false;
        OnEnd(won);
        if (canvas != null) canvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
        SetGameplayFrozen(false);
        if (won && onWinCallback != null) onWinCallback.Invoke();
        onWinCallback = null;
    }

    protected virtual void Update()
    {
        if (!isOpen) return;
        TickGame();
    }

    protected abstract void OnStart();
    protected abstract void OnEnd(bool won);
    protected abstract void TickGame();

    void SetGameplayFrozen(bool frozen)
    {
        if (young != null)
        {
            young.enabled = !frozen;
            var atk = young.GetComponent<PlayerAttack>();
            if (atk != null) atk.enabled = !frozen;
            var rb = young.GetComponent<Rigidbody2D>();
            if (rb != null && frozen) rb.linearVelocity = Vector2.zero;
        }
        if (adult != null)
        {
            adult.enabled = !frozen;
            var atk = adult.GetComponent<PlayerAttack>();
            if (atk != null) atk.enabled = !frozen;
            var rb = adult.GetComponent<Rigidbody2D>();
            if (rb != null && frozen) rb.linearVelocity = Vector2.zero;
        }
        if (playerSwap != null) playerSwap.enabled = !frozen;
        if (!frozen && playerSwap != null) playerSwap.RefreshActive();
    }
}
