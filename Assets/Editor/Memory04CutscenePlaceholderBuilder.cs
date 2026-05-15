#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Cutscene final pós-Memory_04: 5 painéis de lição de moral (auto-compaixão)
// seguidos de créditos com rolagem vertical estilo Star Wars (2D). Reusa
// CutsceneController + TypewriterText + CutsceneFader do Prologue.
//
// O nome do arquivo de cena continua "Memory_04_Cutscene_Placeholder.unity"
// pra não tocar em SceneNames.cs nem em Memory04Builder.CoopFinishDoor.targetScene
// — só o conteúdo da cena muda.
public static class Memory04CutscenePlaceholderBuilder
{
    const string ScenePath = "Assets/Scenes/Memory_04_Cutscene_Placeholder.unity";

    static readonly Color BgCol      = new Color(0.02f, 0.03f, 0.06f, 1f);
    static readonly Color CreamCol   = new Color(1f, 0.92f, 0.55f, 1f);
    static readonly Color SubtleCol  = new Color(0.78f, 0.95f, 1f, 0.9f);
    static readonly Color WoodyCol   = new Color(0.45f, 0.32f, 0.22f, 1f);

    static readonly string[] OwnedRoots = {
        "Main Camera", "EventSystem", "BlipSource", "Panels",
        "CutsceneCanvas", "CreditsCanvas", "CutsceneController",
        // Resquícios do placeholder antigo — limpam no rebuild.
        "Canvas", "MenuActions",
    };

    [MenuItem("Retroself/Build Memory_04 Outro Cutscene")]
    public static void BuildOutroCutscene()
    {
        var scene = SceneRebuildHelpers.OpenOrNew(ScenePath);
        if (!SceneRebuildHelpers.ConfirmRebuild(scene, OwnedRoots)) return;
        SceneRebuildHelpers.WipeOwnedRoots(scene, OwnedRoots);

        BuildCamera();
        BuildEventSystem();
        var blipSource = BuildBlipSource();
        var (typewriter, speakerTMP, portraitImg, dialogueBox, contGO, fader, cutsceneCanvasGO)
            = BuildCutsceneCanvas(blipSource);
        var creditsCanvasGO = BuildCreditsCanvas();
        BuildCutsceneController(typewriter, speakerTMP, portraitImg, dialogueBox, contGO, fader,
                                cutsceneCanvasGO, creditsCanvasGO);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory04CutscenePlaceholderBuilder] Outro cutscene salva em {ScenePath}");
    }

    // ---------- Camera / EventSystem ----------

    static void BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgCol;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();
    }

    static void BuildEventSystem()
    {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();
    }

    static AudioSource BuildBlipSource()
    {
        var go = new GameObject("BlipSource");
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.volume = 0.5f;
        return src;
    }

    // ---------- Cutscene canvas (DialogueBox + Fader) ----------

    static (TypewriterText typewriter, TMP_Text speaker, Image portrait, GameObject dialogueBox,
            GameObject continueIndicator, CutsceneFader fader, GameObject canvasGO)
        BuildCutsceneCanvas(AudioSource blipSource)
    {
        var canvasGO = new GameObject("CutsceneCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // DialogueBox (anchored bottom-center) — mesmas proporções do IntroDialogue M04.
        var dialogueBox = new GameObject("DialogueBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dialogueBox.transform.SetParent(canvasGO.transform, false);
        var dbRT = dialogueBox.GetComponent<RectTransform>();
        dbRT.anchorMin = new Vector2(0.5f, 0);
        dbRT.anchorMax = new Vector2(0.5f, 0);
        dbRT.pivot = new Vector2(0.5f, 0);
        dbRT.anchoredPosition = new Vector2(0, 60);
        dbRT.sizeDelta = new Vector2(1700, 280);
        dialogueBox.GetComponent<Image>().color = Color.black;

        // Portrait (Woody adulto à esquerda) — sprite real do Homeless_1 Idle_2 (1º frame).
        var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGO.transform.SetParent(dialogueBox.transform, false);
        var prRT = portraitGO.GetComponent<RectTransform>();
        prRT.anchorMin = new Vector2(0, 0.5f);
        prRT.anchorMax = new Vector2(0, 0.5f);
        prRT.pivot = new Vector2(0, 0.5f);
        prRT.anchoredPosition = new Vector2(40, 0);
        prRT.sizeDelta = new Vector2(180, 220);
        var portraitImg = portraitGO.GetComponent<Image>();
        portraitImg.color = Color.white;
        var woodyFrames = SceneArtCatalog.LoadSpriteFrames(SceneArtCatalog.WoodySittingSheet, "Idle_2_");
        if (woodyFrames != null && woodyFrames.Length > 0) portraitImg.sprite = woodyFrames[0];
        else portraitImg.color = WoodyCol; // fallback pro quad colorido

        // Speaker label — fontSize 46 igual IntroDialogue do M04.
        var speakerGO = CreateUIText(dialogueBox.transform, "Speaker", "", 46, FontStyles.Bold);
        var spRT = speakerGO.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0, 1);
        spRT.anchorMax = new Vector2(0, 1);
        spRT.pivot = new Vector2(0, 1);
        spRT.anchoredPosition = new Vector2(250, -20);
        spRT.sizeDelta = new Vector2(1200, 50);
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.alignment = TextAlignmentOptions.Left;
        speakerTMP.color = CreamCol;

        // Body — fontSize 46.
        var bodyGO = CreateUIText(dialogueBox.transform, "Body", "", 46, FontStyles.Normal);
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0, 1);
        bodyRT.anchorMax = new Vector2(0, 1);
        bodyRT.pivot = new Vector2(0, 1);
        bodyRT.anchoredPosition = new Vector2(250, -75);
        bodyRT.sizeDelta = new Vector2(1400, 180);
        var bodyTMP = bodyGO.GetComponent<TextMeshProUGUI>();
        bodyTMP.alignment = TextAlignmentOptions.TopLeft;
        bodyTMP.color = new Color(1f, 0.96f, 0.85f, 1f);
        bodyTMP.enableWordWrapping = true;
        bodyTMP.overflowMode = TextOverflowModes.Truncate;

        var typewriter = bodyGO.AddComponent<TypewriterText>();
        typewriter.target = bodyTMP;
        typewriter.charsPerSecond = 32f;
        typewriter.blipSource = blipSource;

        // Continue indicator pulsante — fontSize 36 igual IntroDialogue do M04.
        var contGO = CreateUIText(dialogueBox.transform, "Continue", ">> Espaco/Enter", 36, FontStyles.Italic);
        var contRT = contGO.GetComponent<RectTransform>();
        contRT.anchorMin = new Vector2(1, 0);
        contRT.anchorMax = new Vector2(1, 0);
        contRT.pivot = new Vector2(1, 0);
        contRT.anchoredPosition = new Vector2(-30, 18);
        contRT.sizeDelta = new Vector2(360, 36);
        contGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        contGO.GetComponent<TextMeshProUGUI>().color = CreamCol;
        contGO.AddComponent<BlinkUI>();

        // Fader (último child do canvas → renderiza acima do diálogo).
        var faderGO = new GameObject("FaderOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        faderGO.transform.SetParent(canvasGO.transform, false);
        var faderRT = faderGO.GetComponent<RectTransform>();
        faderRT.anchorMin = Vector2.zero; faderRT.anchorMax = Vector2.one;
        faderRT.offsetMin = Vector2.zero; faderRT.offsetMax = Vector2.zero;
        var faderImg = faderGO.GetComponent<Image>();
        faderImg.color = new Color(0, 0, 0, 1f);
        faderImg.raycastTarget = false;
        var fader = faderGO.AddComponent<CutsceneFader>();
        fader.overlay = faderImg;
        fader.defaultDuration = 0.8f;

        // Skip hint (top-right) — mesmo tamanho do continue indicator.
        var hintGO = CreateUIText(canvasGO.transform, "SkipHint", "[Enter] avancar  [Esc] pular", 36, FontStyles.Italic);
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(1, 1); hintRT.anchorMax = new Vector2(1, 1);
        hintRT.pivot = new Vector2(1, 1);
        hintRT.anchoredPosition = new Vector2(-30, -30);
        hintRT.sizeDelta = new Vector2(900, 60);
        hintGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        hintGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.7f);

        return (typewriter, speakerTMP, portraitImg, dialogueBox, contGO, fader, canvasGO);
    }

    // ---------- Credits canvas (começa inativo) ----------

    static GameObject BuildCreditsCanvas()
    {
        var canvasGO = new GameObject("CreditsCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // BG preto sólido fullscreen.
        var bgGO = new GameObject("CreditsBG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        bgGO.GetComponent<Image>().color = Color.black;
        bgGO.GetComponent<Image>().raycastTarget = false;

        // Viewport com RectMask2D — corta o texto que sai pelo topo/baixo.
        var viewGO = new GameObject("Viewport", typeof(RectTransform));
        viewGO.transform.SetParent(canvasGO.transform, false);
        var viewRT = viewGO.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero; viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = Vector2.zero; viewRT.offsetMax = Vector2.zero;
        viewGO.AddComponent<RectMask2D>();

        // CreditsBlock — TMP único com todo o texto. Ancorado center pra que
        // anchoredPosition.y = 0 signifique "centralizado no viewport".
        var blockGO = new GameObject("CreditsBlock", typeof(RectTransform));
        blockGO.transform.SetParent(viewGO.transform, false);
        var blockRT = blockGO.GetComponent<RectTransform>();
        blockRT.anchorMin = new Vector2(0.5f, 0.5f);
        blockRT.anchorMax = new Vector2(0.5f, 0.5f);
        blockRT.pivot = new Vector2(0.5f, 0.5f);
        blockRT.sizeDelta = new Vector2(1500, 5000); // altura grande pra caber todo o texto
        blockRT.anchoredPosition = new Vector2(0, -3000); // ScrollingCredits recalcula no enable

        var tmp = blockGO.AddComponent<TextMeshProUGUI>();
        var pixelFont = SceneArtCatalog.GetPixelFont();
        if (pixelFont != null) tmp.font = pixelFont;
        tmp.fontSize = 40; // base — rich-text inline ajusta título/subtítulo
        tmp.enableAutoSizing = false;
        tmp.alignment = TextAlignmentOptions.Top; // texto começa do topo do rect (não do centro)
        tmp.color = CreamCol;
        tmp.lineSpacing = 14f;
        tmp.richText = true;
        tmp.raycastTarget = false;
        tmp.text = BuildCreditsText();

        var scroller = canvasGO.AddComponent<ScrollingCredits>();
        scroller.creditsBlock = blockRT;
        scroller.viewport = viewRT;
        scroller.scrollSpeed = 180f; // ~30s pra rolar tudo @ 1080p — antes em 60 era ~100s
        scroller.endHoldSeconds = 1.0f;
        scroller.finalScene = SceneNames.MainMenu;

        canvasGO.SetActive(false); // ativado por OnFinished do CutsceneController
        return canvasGO;
    }

    static string BuildCreditsText()
    {
        // fontSize base = 40 no TMP; rich-text inline ajusta título/subtítulo.
        return
            "<size=110><b>RETROSELF</b></size>\n\n" +
            "<size=34><i>— UMA HISTORIA DE AUTO-COMPAIXAO —</i></size>\n\n\n\n" +
            "<size=50><b>DIRECAO & GAME DESIGN</b></size>\n" +
            "Alex Chequer\n" +
            "Carlos Hernani\n" +
            "Lucas Ikawa\n\n\n" +
            "<size=50><b>PROGRAMACAO</b></size>\n" +
            "Alex Chequer\n" +
            "Carlos Hernani\n" +
            "Lucas Ikawa\n\n\n" +
            "<size=50><b>NARRATIVA & ROTEIRO</b></size>\n" +
            "Alex Chequer\n" +
            "Carlos Hernani\n" +
            "Lucas Ikawa\n\n\n" +
            "<size=50><b>MUSICA & SOM</b></size>\n" +
            "Carlos Hernani\n\n\n" +
            "<size=50><b>PIXEL ART</b></size>\n" +
            "Craftpix.net (licenciado)\n" +
            "residential-area-tileset\n" +
            "basement-tileset\n" +
            "green-zone\n" +
            "cyberpunk-market-street\n" +
            "doors-and-portals\n" +
            "business-center\n" +
            "tiny-hero-sprites\n\n\n" +
            "<size=50><b>FONTES</b></size>\n" +
            "Press Start 2P (OFL)\n" +
            "VT323 (OFL)\n\n\n" +
            "<size=50><b>INSPIRADO EM</b></size>\n" +
            "Braid\n" +
            "Celeste\n" +
            "Brothers\n" +
            "Inside\n" +
            "Undertale\n" +
            "About Time\n\n\n" +
            "<size=32><i>Feito com Unity 6000.3.13f1</i></size>\n\n\n" +
            "<size=70><b>OBRIGADO POR JOGAR.</b></size>";
    }

    // ---------- CutsceneController + painéis ----------

    static void BuildCutsceneController(TypewriterText typewriter, TMP_Text speaker, Image portrait,
        GameObject dialogueBox, GameObject continueIndicator, CutsceneFader fader,
        GameObject cutsceneCanvasGO, GameObject creditsCanvasGO)
    {
        // Painéis: um root GameObject por painel (sceneRoot do CutsceneController).
        // Sem cenário real — fundo preto + portrait do Woody adulto basta pro tom.
        var panelsRoot = new GameObject("Panels");

        var p1 = BuildPanel(panelsRoot.transform, "Panel_01_Reconhecimento",
            woodyPos: new Vector3(2.5f, -1f, 0), flipX: false, scale: 3.5f);
        var p2 = BuildPanel(panelsRoot.transform, "Panel_02_Espelho",
            woodyPos: new Vector3(-2.5f, -1f, 0), flipX: true, scale: 3.5f);
        var p3 = BuildPanel(panelsRoot.transform, "Panel_03_Briga",
            woodyPos: new Vector3(0f, -0.5f, 0), flipX: false, scale: 4.0f);
        var p4 = BuildPanel(panelsRoot.transform, "Panel_04_Crianca",
            woodyPos: new Vector3(1.5f, -1.2f, 0), flipX: false, scale: 3.0f);
        var p5 = BuildPanel(panelsRoot.transform, "Panel_05_Suficiente",
            woodyPos: new Vector3(0f, -1f, 0), flipX: false, scale: 3.5f);

        var ctrlGO = new GameObject("CutsceneController");
        var ctrl = ctrlGO.AddComponent<CutsceneController>();
        ctrl.typewriter = typewriter;
        ctrl.speakerLabel = speaker;
        ctrl.portrait = portrait;
        ctrl.dialogueBox = dialogueBox;
        ctrl.continueIndicator = continueIndicator;
        ctrl.fader = fader;
        ctrl.panelFadeDuration = 0.4f; // fade entre painéis mais ágil
        ctrl.openingFadeDuration = 1.2f;
        ctrl.nextSceneName = ""; // vazio → dispara OnFinished

        ctrl.panels = new[]
        {
            MakePanel("Reconhecimento", p1, new[] {
                Line("Woody", "Demorei muito pra entender uma coisa simples."),
            }),
            MakePanel("Espelho", p2, new[] {
                Line("Woody", "Aquele menino que eu via no espelho — sozinho, com medo, sem entender —"),
                Line("Woody", "nao tinha pra onde correr."),
            }),
            MakePanel("Briga", p3, new[] {
                Line("Woody", "Por muito tempo eu briguei com ele."),
                Line("Woody", "Como se a culpa fosse dele por nao saber o que fazer."),
            }),
            MakePanel("Crianca", p4, new[] {
                Line("Woody", "Mas ele era so uma crianca."),
                Line("Woody", "E ninguem escolhe os domingos que herda."),
            }),
            MakePanel("Suficiente", p5, new[] {
                Line("Woody", "Hoje eu congelo o tempo so por um instante."),
                Line("Woody", "Pra dizer que ele foi suficiente."),
                Line("Woody", "E que eu, finalmente, tambem sou."),
            }),
        };

        // OnFinished → SwitchToCredits (helper que desativa cutscene + ativa créditos).
        // Mais simples que wirar 2 persistent listeners de SetActive(bool) via UnityEventTools.
        var switchGO = new GameObject("OutroSwitch");
        switchGO.transform.SetParent(ctrlGO.transform, false);
        var sw = switchGO.AddComponent<CutsceneOutroSwitch>();
        sw.cutsceneCanvas = cutsceneCanvasGO;
        sw.creditsCanvas = creditsCanvasGO;
        if (ctrl.OnFinished == null) ctrl.OnFinished = new UnityEngine.Events.UnityEvent();
        var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), sw, typeof(CutsceneOutroSwitch).GetMethod(nameof(CutsceneOutroSwitch.SwitchToCredits)));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(ctrl.OnFinished, action);
    }

    // Cria root de painel com sprite do Woody adulto (Homeless_1 Idle_2 animado).
    // O sceneRoot do CutsceneController controla SetActive — sprite só aparece no panel ativo.
    static GameObject BuildPanel(Transform parent, string name, Vector3 woodyPos, bool flipX, float scale)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);

        var frames = SceneArtCatalog.LoadSpriteFrames(SceneArtCatalog.WoodySittingSheet, "Idle_2_");
        var woodyGO = new GameObject("Woody");
        woodyGO.transform.SetParent(root.transform, false);
        woodyGO.transform.localPosition = woodyPos;
        woodyGO.transform.localScale = Vector3.one * scale;
        var sr = woodyGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.flipX = flipX;
        if (frames != null && frames.Length > 0)
        {
            sr.sprite = frames[0];
            var anim = woodyGO.AddComponent<SimpleSpriteAnimator>();
            anim.idleSprites = frames;
            anim.idleFps = 5f;
        }
        else
        {
            sr.color = WoodyCol;
        }

        // Respiração lenta — vibe de stillness/contemplação.
        var br = woodyGO.AddComponent<BreathingScale>();
        br.amplitude = 0.025f;
        br.speed = 1.4f;

        return root;
    }

    static CutsceneController.Panel MakePanel(string name, GameObject root, CutsceneController.Line[] lines)
    {
        return new CutsceneController.Panel
        {
            panelName = name,
            sceneRoot = root,
            lines = lines,
            fadeWhiteOut = false,
        };
    }

    static CutsceneController.Line Line(string speaker, string text)
    {
        return new CutsceneController.Line
        {
            speakerName = speaker,
            text = text,
            portraitColor = WoodyCol,
            blipPitch = 0.75f,
        };
    }

    // ---------- helpers ----------

    static GameObject CreateUIText(Transform parent, string name, string text, int fontSize, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == scenePath)) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
