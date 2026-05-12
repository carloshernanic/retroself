using UnityEngine;
using UnityEngine.InputSystem;

// Diálogo de abertura que congela os controles, exibe uma fala com TypewriterText
// e devolve o controle quando o player avança. Standalone — não usa CutsceneController
// (esse é multi-painel + transição de cena).
public class IntroDialogue : MonoBehaviour
{
    [TextArea(2, 6)]
    public string line = "Lembro bem deste dia… foi quando eu me atrasei pra escola e o porteiro não me deixou entrar. Dessa vez vai ser diferente: vou enfrentar o porteiro, achar a chave perdida, e nós dois vamos entrar juntos.";

    [Header("Refs")]
    public GameObject dialogueBox;
    public TypewriterText typewriter;
    public GameObject continueIndicator;
    public PlayerSwap playerSwap;
    public PlayerController young;
    public PlayerController adult;
    public BullyController bully;

    private bool waitingForAdvance;
    private bool finished;

    void Start()
    {
        SetGameplayFrozen(true);
        if (continueIndicator != null) continueIndicator.SetActive(false);
        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (typewriter != null) typewriter.Play(line, OnTypewriterFinished);
        else OnTypewriterFinished();
    }

    void Update()
    {
        if (finished) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        bool advance = kb.spaceKey.wasPressedThisFrame ||
                       kb.enterKey.wasPressedThisFrame ||
                       kb.numpadEnterKey.wasPressedThisFrame;
        bool skip = kb.escapeKey.wasPressedThisFrame;

        if (skip)
        {
            if (typewriter != null && typewriter.IsPlaying) typewriter.SkipToEnd();
            DismissAndUnfreeze();
            return;
        }

        if (!advance) return;

        if (typewriter != null && typewriter.IsPlaying)
        {
            typewriter.SkipToEnd();
            return;
        }
        if (waitingForAdvance) DismissAndUnfreeze();
    }

    void OnTypewriterFinished()
    {
        waitingForAdvance = true;
        if (continueIndicator != null) continueIndicator.SetActive(true);
    }

    void DismissAndUnfreeze()
    {
        finished = true;
        waitingForAdvance = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        SetGameplayFrozen(false);
    }

    void SetGameplayFrozen(bool frozen)
    {
        // Congela ambos os Woody pra Tab não bagunçar o estado do diálogo.
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
        if (bully != null) bully.enabled = !frozen;

        // Quando libera, deixa o PlayerSwap re-aplicar o estado do ativo
        // (re-habilita PlayerController/Attack/Animator do ativo, desliga do inativo).
        if (!frozen && playerSwap != null) playerSwap.RefreshActive();
    }
}
