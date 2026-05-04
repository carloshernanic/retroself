using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    [Serializable]
    public class Line
    {
        public string speakerName;
        [TextArea(1, 4)] public string text;
        public Color portraitColor = Color.white;
        public float blipPitch = 1f;
    }

    [Serializable]
    public class Panel
    {
        public string panelName;
        public GameObject sceneRoot;
        public Line[] lines;
        public bool fadeWhiteOut;
    }

    public Panel[] panels;
    public string nextSceneName = SceneNames.Memory_01_Patio;

    [Header("UI refs")]
    public TypewriterText typewriter;
    public TMP_Text speakerLabel;
    public Image portrait;
    public GameObject dialogueBox;
    public GameObject continueIndicator;
    public CutsceneFader fader;

    [Header("Timings")]
    public float panelFadeDuration = 0.6f;
    public float openingFadeDuration = 0.8f;

    private int panelIdx;
    private int lineIdx;
    private bool waitingForAdvance;
    private bool busy;

    void Start()
    {
        if (panels != null)
        {
            for (int i = 0; i < panels.Length; i++)
                if (panels[i] != null && panels[i].sceneRoot != null)
                    panels[i].sceneRoot.SetActive(false);
        }
        if (continueIndicator != null) continueIndicator.SetActive(false);
        if (dialogueBox != null) dialogueBox.SetActive(false);
        StartCoroutine(StartCutscene());
    }

    IEnumerator StartCutscene()
    {
        if (panels == null || panels.Length == 0) yield break;
        if (fader != null) fader.SetInstant(new Color(0, 0, 0, 1));
        ActivatePanel(0);
        if (fader != null) yield return fader.FadeOut(openingFadeDuration);
        ShowLine();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        if (busy) return;

        bool advance =
            (kb != null && (kb.spaceKey.wasPressedThisFrame ||
                            kb.enterKey.wasPressedThisFrame ||
                            kb.numpadEnterKey.wasPressedThisFrame)) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (!advance) return;

        if (typewriter != null && typewriter.IsPlaying)
        {
            typewriter.SkipToEnd();
            return;
        }
        if (waitingForAdvance)
        {
            waitingForAdvance = false;
            Advance();
        }
    }

    void ActivatePanel(int idx)
    {
        panelIdx = idx;
        lineIdx = 0;
        var p = panels[idx];
        if (p != null && p.sceneRoot != null) p.sceneRoot.SetActive(true);
    }

    void ShowLine()
    {
        var p = panels[panelIdx];
        if (p.lines == null || p.lines.Length == 0)
        {
            StartCoroutine(GoToNextPanel());
            return;
        }
        var line = p.lines[lineIdx];
        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (speakerLabel != null) speakerLabel.text = line.speakerName ?? "";
        if (portrait != null) portrait.color = line.portraitColor;
        ShowContinueIndicator(false);

        if (typewriter != null)
        {
            typewriter.blipPitch = line.blipPitch;
            typewriter.Play(line.text ?? "", OnLineFinished);
        }
        else
        {
            OnLineFinished();
        }
    }

    void OnLineFinished()
    {
        ShowContinueIndicator(true);
        waitingForAdvance = true;
    }

    void Advance()
    {
        var p = panels[panelIdx];
        lineIdx++;
        if (lineIdx < p.lines.Length)
        {
            ShowLine();
        }
        else
        {
            StartCoroutine(GoToNextPanel());
        }
    }

    IEnumerator GoToNextPanel()
    {
        busy = true;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        ShowContinueIndicator(false);

        var current = panels[panelIdx];
        if (fader != null)
        {
            Color fadeColor = current.fadeWhiteOut ? Color.white : Color.black;
            yield return fader.FadeIn(fadeColor, panelFadeDuration);
        }
        if (current.sceneRoot != null) current.sceneRoot.SetActive(false);

        if (panelIdx + 1 >= panels.Length)
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        ActivatePanel(panelIdx + 1);
        if (fader != null) yield return fader.FadeOut(panelFadeDuration);
        ShowLine();
        busy = false;
    }

    void ShowContinueIndicator(bool show)
    {
        if (continueIndicator != null) continueIndicator.SetActive(show);
    }
}
