#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public static class PrologueBuilder
{
    static readonly Color ColAdult       = new Color(0.45f, 0.32f, 0.22f, 1f);
    static readonly Color ColAlien       = new Color(0.40f, 0.90f, 0.95f, 1f);
    static readonly Color ColNarration   = new Color(0.85f, 0.85f, 0.85f, 1f);

    static readonly float PitchAdult     = 0.75f;
    static readonly float PitchAlien     = 1.45f;
    static readonly float PitchNarration = 1.0f;

    [MenuItem("Retroself/Build Prologue Scene")]
    public static void BuildPrologue()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Camera ----------
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ---------- EventSystem (New Input System) ----------
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ---------- Audio source for typewriter blips ----------
        var audioGO = new GameObject("BlipSource");
        var blipSource = audioGO.AddComponent<AudioSource>();
        blipSource.playOnAwake = false;
        blipSource.volume = 0.5f;

        // ---------- Panels root ----------
        var panelsRoot = new GameObject("Panels").transform;

        var panel1 = BuildPanel_Praca(panelsRoot, "Panel_01_Praca", sleeping: true, showCabinGlow: false);
        var panel2 = BuildPanel_Praca(panelsRoot, "Panel_02_Acorda", sleeping: false, showCabinGlow: true);
        var panel3 = BuildPanel_Cabine(panelsRoot, "Panel_03_Cabine");
        var panel4 = BuildPanel_Flash(panelsRoot, "Panel_04_Flash");
        var panel5 = BuildPanel_Aliens(panelsRoot, "Panel_05_Aliens");
        var panel6 = BuildPanel_Aliens(panelsRoot, "Panel_06_Oferta");
        var panel7 = BuildPanel_Relogio(panelsRoot, "Panel_07_Relogio");

        // ---------- Canvas (UI) ----------
        var canvasGO = new GameObject("CutsceneCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // (Fader overlay criado depois pra ficar como último child = renderiza por cima de tudo)

        // Dialogue box (anchored bottom-center)
        var dialogueBox = new GameObject("DialogueBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dialogueBox.transform.SetParent(canvasGO.transform, false);
        var dbRT = dialogueBox.GetComponent<RectTransform>();
        dbRT.anchorMin = new Vector2(0.5f, 0);
        dbRT.anchorMax = new Vector2(0.5f, 0);
        dbRT.pivot = new Vector2(0.5f, 0);
        dbRT.anchoredPosition = new Vector2(0, 80);
        dbRT.sizeDelta = new Vector2(1500, 260);
        dialogueBox.GetComponent<Image>().color = new Color(0, 0, 0, 0.78f);

        // Portrait (left of box)
        var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGO.transform.SetParent(dialogueBox.transform, false);
        var prRT = portraitGO.GetComponent<RectTransform>();
        prRT.anchorMin = new Vector2(0, 0.5f);
        prRT.anchorMax = new Vector2(0, 0.5f);
        prRT.pivot = new Vector2(0, 0.5f);
        prRT.anchoredPosition = new Vector2(30, 0);
        prRT.sizeDelta = new Vector2(180, 180);
        var portraitImg = portraitGO.GetComponent<Image>();
        portraitImg.color = ColAdult;

        // Speaker label
        var speakerGO = CreateUIText(dialogueBox.transform, "Speaker", "", 28, FontStyles.Bold);
        var spRT = speakerGO.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0, 1);
        spRT.anchorMax = new Vector2(0, 1);
        spRT.pivot = new Vector2(0, 1);
        spRT.anchoredPosition = new Vector2(240, -20);
        spRT.sizeDelta = new Vector2(800, 40);
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.alignment = TextAlignmentOptions.Left;
        speakerTMP.color = new Color(0.78f, 0.95f, 1f, 1f);

        // Body text
        var bodyGO = CreateUIText(dialogueBox.transform, "Body", "", 36, FontStyles.Normal);
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0, 0);
        bodyRT.anchorMax = new Vector2(1, 1);
        bodyRT.offsetMin = new Vector2(240, 30);
        bodyRT.offsetMax = new Vector2(-40, -70);
        var bodyTMP = bodyGO.GetComponent<TextMeshProUGUI>();
        bodyTMP.alignment = TextAlignmentOptions.TopLeft;
        bodyTMP.color = Color.white;

        var typewriter = bodyGO.AddComponent<TypewriterText>();
        typewriter.target = bodyTMP;
        typewriter.charsPerSecond = 32f;
        typewriter.blipSource = blipSource;

        // Continue indicator ([Enter] ▼)
        var contGO = CreateUIText(dialogueBox.transform, "Continue", "[Enter] ▼", 28, FontStyles.Bold);
        var contRT = contGO.GetComponent<RectTransform>();
        contRT.anchorMin = new Vector2(1, 0);
        contRT.anchorMax = new Vector2(1, 0);
        contRT.pivot = new Vector2(1, 0);
        contRT.anchoredPosition = new Vector2(-30, 20);
        contRT.sizeDelta = new Vector2(220, 50);
        contGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        contGO.GetComponent<TextMeshProUGUI>().color = new Color(0.78f, 0.95f, 1f, 0.9f);
        contGO.AddComponent<BlinkUI>();

        // Fader overlay (último child do Canvas → renderiza acima do diálogo durante transições)
        var faderGO = CreateUIImage(canvasGO.transform, "FaderOverlay", new Color(0, 0, 0, 1));
        StretchFull(faderGO.GetComponent<RectTransform>());
        var faderImg = faderGO.GetComponent<Image>();
        faderImg.raycastTarget = false;
        var faderGOObj = faderGO.AddComponent<CutsceneFader>();
        faderGOObj.overlay = faderImg;
        faderGOObj.defaultDuration = 0.6f;

        // Skip hint (top-right) — fica acima do fader pra continuar visível durante transições
        var hintGO = CreateUIText(canvasGO.transform, "SkipHint", "[Enter] avançar   ·   [Esc] pular", 22, FontStyles.Italic);
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(1, 1);
        hintRT.anchorMax = new Vector2(1, 1);
        hintRT.pivot = new Vector2(1, 1);
        hintRT.anchoredPosition = new Vector2(-30, -30);
        hintRT.sizeDelta = new Vector2(420, 40);
        hintGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        hintGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.55f);

        // ---------- CutsceneController ----------
        var ctrlGO = new GameObject("CutsceneController");
        var ctrl = ctrlGO.AddComponent<CutsceneController>();
        ctrl.typewriter = typewriter;
        ctrl.speakerLabel = speakerTMP;
        ctrl.portrait = portraitImg;
        ctrl.dialogueBox = dialogueBox;
        ctrl.continueIndicator = contGO;
        ctrl.fader = faderGOObj;
        ctrl.nextSceneName = SceneNames.Memory_01_Patio;

        ctrl.panels = new[]
        {
            MakePanel("Praca - dormindo", panel1, false, new[] {
                Line("", "Outubro. Décima noite no banco da praça.", ColNarration, PitchNarration),
                Line("", "Para a chuva, mas só por uns minutos.", ColNarration, PitchNarration),
            }),
            MakePanel("Praca - acorda", panel2, false, new[] {
                Line("Woody", "...que barulho é esse.", ColAdult, PitchAdult),
                Line("Woody", "Isso não estava aqui ontem.", ColAdult, PitchAdult),
            }),
            MakePanel("Cabine", panel3, true, new[] {
                Line("Woody", "...tô tão mal assim?", ColAdult, PitchAdult),
                Line("Woody", "Já que cheguei até aqui.", ColAdult, PitchAdult),
            }),
            MakePanel("Flash", panel4, false, new[] {
                Line("", "...", ColNarration, PitchNarration),
            }),
            MakePanel("Aliens - apresentação", panel5, false, new[] {
                Line("???", "Olá, Woody. Te observamos por anos.", ColAlien, PitchAlien),
                Line("Woody", "Cobaia?", ColAdult, PitchAdult),
                Line("???", "Candidato.", ColAlien, PitchAlien),
            }),
            MakePanel("Aliens - oferta", panel6, false, new[] {
                Line("???", "Você não é um homem mau. Apenas tropeçou nas piores horas.", ColAlien, PitchAlien),
                Line("???", "Aceita voltar quatro vezes — e ajudar a si mesmo a atravessá-las?", ColAlien, PitchAlien),
                Line("Woody", "...quatro vezes.", ColAdult, PitchAdult),
            }),
            MakePanel("Relogio", panel7, false, new[] {
                Line("Woody", "Ok. De novo.", ColAdult, PitchAdult),
                Line("Woody", "A gente acerta dessa vez.", ColAdult, PitchAdult),
            }),
        };

        // ---------- Save scene ----------
        const string scenePath = "Assets/Scenes/Prologue.unity";
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[PrologueBuilder] Cena Prologue criada em {scenePath}");

        AddSceneToBuildSettings(scenePath);

        // Memory_01_Patio é montado pelo Memory01Builder (menu separado).
        // Só cria placeholder se ainda não existir, pra não sobrescrever o tutorial.
        if (!System.IO.File.Exists("Assets/Scenes/Memory_01_Patio.unity"))
            BuildMemory01Placeholder();
    }

    [MenuItem("Retroself/Build Memory_01_Patio Placeholder")]
    public static void BuildMemory01Placeholder()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.32f, 0.36f, 0.18f, 1f); // amarelo-esverdeado escolar (paleta GDD)
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var titleGO = CreateUIText(canvasGO.transform, "Title", "MEMORY 01 — O PÁTIO", 96, FontStyles.Bold);
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 80);
        trt.sizeDelta = new Vector2(1600, 160);
        titleGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.92f, 0.55f, 1f);

        var subGO = CreateUIText(canvasGO.transform, "Subtitle", "(em construção — tutorial de movimento na próxima sprint)", 36, FontStyles.Italic);
        var srt = subGO.GetComponent<RectTransform>();
        srt.anchoredPosition = new Vector2(0, -60);
        srt.sizeDelta = new Vector2(1500, 80);
        subGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.7f);

        const string scenePath = "Assets/Scenes/Memory_01_Patio.unity";
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[PrologueBuilder] Placeholder Memory_01_Patio em {scenePath}");
        AddSceneToBuildSettings(scenePath);
    }

    // ===================== Panel builders =====================

    static GameObject BuildPanel_Praca(Transform parent, string name, bool sleeping, bool showCabinGlow)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);

        CreateSprite("Sky", root, new Vector2(0, 0), new Vector2(20, 12), new Color(0.04f, 0.05f, 0.10f), -10);
        CreateSprite("BuildingsBack", root, new Vector2(0, 1.5f), new Vector2(20, 5), new Color(0.07f, 0.08f, 0.15f), -8);
        CreateSprite("Tree", root, new Vector2(-5, -1), new Vector2(2.5f, 5f), new Color(0.08f, 0.13f, 0.09f), -6);

        if (showCabinGlow)
        {
            var glow = CreateSprite("CabinHintGlow", root, new Vector2(-5, -0.5f), new Vector2(1.6f, 2.6f), new Color(0.62f, 0.36f, 0.92f, 0.6f), -5);
            var pulse = glow.AddComponent<CabinPulse>();
            pulse.cabinGlow = glow.GetComponent<SpriteRenderer>();
            pulse.minAlpha = 0.3f;
            pulse.maxAlpha = 0.85f;
            pulse.pulseSpeed = 1.6f;
        }

        CreateSprite("Bench", root, new Vector2(2, -2.5f), new Vector2(3f, 0.7f), new Color(0.30f, 0.20f, 0.13f), -4);
        CreateSprite("BenchLeg1", root, new Vector2(0.8f, -3.1f), new Vector2(0.2f, 0.9f), new Color(0.22f, 0.15f, 0.10f), -4);
        CreateSprite("BenchLeg2", root, new Vector2(3.2f, -3.1f), new Vector2(0.2f, 0.9f), new Color(0.22f, 0.15f, 0.10f), -4);
        CreateSprite("StreetLamp", root, new Vector2(5, 0), new Vector2(0.25f, 4f), new Color(0.30f, 0.25f, 0.18f), -5);
        CreateSprite("LampGlow", root, new Vector2(5, 1.8f), new Vector2(1.5f, 1.5f), new Color(0.95f, 0.78f, 0.42f, 0.45f), -3);

        GameObject woody;
        if (sleeping)
        {
            woody = CreateSprite("Woody_Sleeping", root, new Vector2(2, -2f), new Vector2(2f, 0.5f), new Color(0.45f, 0.32f, 0.22f), -3);
            var br = woody.AddComponent<BreathingScale>();
            br.amplitude = 0.05f; br.speed = 1.4f; br.xWeight = -0.3f;
        }
        else
        {
            woody = CreateSprite("Woody_Sitting", root, new Vector2(2, -1.6f), new Vector2(0.6f, 1.4f), new Color(0.45f, 0.32f, 0.22f), -3);
            var br = woody.AddComponent<BreathingScale>();
            br.amplitude = 0.025f; br.speed = 2f; br.xWeight = -0.4f;
        }

        AddRain(root.gameObject, intensity: 200);
        AddLightning(root.gameObject);
        return root.gameObject;
    }

    static GameObject BuildPanel_Cabine(Transform parent, string name)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);

        CreateSprite("Sky", root, new Vector2(0, 0), new Vector2(20, 12), new Color(0.04f, 0.05f, 0.10f), -10);
        CreateSprite("Tree", root, new Vector2(-3, -1), new Vector2(3f, 5.5f), new Color(0.07f, 0.11f, 0.08f), -7);

        var cabin = CreateSprite("Cabin", root, new Vector2(0, -0.5f), new Vector2(2.6f, 4.4f), new Color(0.18f, 0.10f, 0.28f), -5);
        var glow = CreateSprite("CabinGlow", cabin.transform, Vector2.zero, new Vector2(1.6f, 1.6f), new Color(0.70f, 0.42f, 1f, 0.85f), -4);
        glow.transform.localPosition = new Vector3(0, 0, 0);
        var pulse = cabin.AddComponent<CabinPulse>();
        pulse.cabinGlow = glow.GetComponent<SpriteRenderer>();
        pulse.minAlpha = 0.55f;
        pulse.maxAlpha = 1f;
        pulse.pulseSpeed = 1.8f;

        CreateSprite("CabinDoor", cabin.transform, new Vector2(0, -0.25f), new Vector2(0.4f, 0.5f), new Color(0.92f, 0.78f, 1f, 0.7f), -3);
        var woodyCabine = CreateSprite("Woody", root, new Vector2(3.5f, -2f), new Vector2(0.6f, 1.5f), new Color(0.45f, 0.32f, 0.22f), -2);
        var brWC = woodyCabine.AddComponent<BreathingScale>();
        brWC.amplitude = 0.025f; brWC.speed = 2.2f; brWC.xWeight = -0.4f;

        // Cabine treme suavemente — sensação de "porta vibrando"
        var cabinBob = cabin.AddComponent<IdleBob>();
        cabinBob.amplitude = 0.04f;
        cabinBob.speed = 3.5f;

        AddRain(root.gameObject, intensity: 220);
        AddLightning(root.gameObject);
        return root.gameObject;
    }

    static GameObject BuildPanel_Flash(Transform parent, string name)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);
        CreateSprite("BG", root, new Vector2(0, 0), new Vector2(20, 12), new Color(0.85f, 0.78f, 1f, 1f), -10);
        return root.gameObject;
    }

    static GameObject BuildPanel_Aliens(Transform parent, string name)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);

        CreateSprite("BG", root, new Vector2(0, 0), new Vector2(20, 12), new Color(0.10f, 0.06f, 0.20f), -10);
        CreateSprite("FloorBand", root, new Vector2(0, -3.5f), new Vector2(20, 3), new Color(0.05f, 0.03f, 0.12f), -8);
        CreateSprite("Window", root, new Vector2(-6, 1), new Vector2(3, 2), new Color(0.18f, 0.20f, 0.30f), -7);

        // Trio de aliens (RGB-ish: vermelho, azul, amarelo conforme GDD) — bobbing com fases offset
        var alienR = CreateSprite("Alien_Red", root, new Vector2(-2, 0), new Vector2(0.8f, 0.8f), new Color(0.95f, 0.40f, 0.45f), -3);
        var bobR = alienR.AddComponent<IdleBob>();
        bobR.amplitude = 0.18f; bobR.speed = 2.3f; bobR.phase = 0f;

        var alienB = CreateSprite("Alien_Blue", root, new Vector2(0, 0.4f), new Vector2(0.8f, 0.8f), new Color(0.45f, 0.70f, 1f), -3);
        var bobB = alienB.AddComponent<IdleBob>();
        bobB.amplitude = 0.18f; bobB.speed = 2.3f; bobB.phase = Mathf.PI * 0.66f;

        var alienY = CreateSprite("Alien_Yellow", root, new Vector2(2, 0), new Vector2(0.8f, 0.8f), new Color(0.98f, 0.90f, 0.45f), -3);
        var bobY = alienY.AddComponent<IdleBob>();
        bobY.amplitude = 0.18f; bobY.speed = 2.3f; bobY.phase = Mathf.PI * 1.33f;

        // Adulto à direita
        CreateSprite("Woody", root, new Vector2(5, -1.2f), new Vector2(0.7f, 1.7f), new Color(0.45f, 0.32f, 0.22f), -2);

        return root.gameObject;
    }

    static GameObject BuildPanel_Relogio(Transform parent, string name)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);

        CreateSprite("BG", root, new Vector2(0, 0), new Vector2(20, 12), new Color(0.05f, 0.04f, 0.08f), -10);
        CreateSprite("WatchOuterGlow", root, new Vector2(0, 0), new Vector2(5.5f, 5.5f), new Color(0.55f, 0.85f, 1f, 0.30f), -6);
        CreateSprite("WatchBody", root, new Vector2(0, 0), new Vector2(3.5f, 3.5f), new Color(0.85f, 0.72f, 0.32f), -4);
        CreateSprite("WatchFace", root, new Vector2(0, 0), new Vector2(2.6f, 2.6f), new Color(0.95f, 0.92f, 0.78f), -3);
        CreateSprite("WatchHand1", root, new Vector2(0, 0.4f), new Vector2(0.1f, 1.1f), new Color(0.10f, 0.10f, 0.12f), -2);
        CreateSprite("WatchHand2", root, new Vector2(0.5f, 0), new Vector2(0.9f, 0.1f), new Color(0.10f, 0.10f, 0.12f), -2);
        CreateSprite("Hand_Adult", root, new Vector2(-2.5f, -2.2f), new Vector2(2f, 1.2f), new Color(0.45f, 0.32f, 0.22f), -1);

        return root.gameObject;
    }

    // ===================== Helpers =====================

    static CutsceneController.Panel MakePanel(string panelName, GameObject sceneRoot, bool fadeWhiteOut, CutsceneController.Line[] lines)
    {
        return new CutsceneController.Panel
        {
            panelName = panelName,
            sceneRoot = sceneRoot,
            lines = lines,
            fadeWhiteOut = fadeWhiteOut,
        };
    }

    static CutsceneController.Line Line(string speaker, string text, Color portraitColor, float pitch)
    {
        return new CutsceneController.Line
        {
            speakerName = speaker,
            text = text,
            portraitColor = portraitColor,
            blipPitch = pitch,
        };
    }

    static GameObject CreateSprite(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UnitSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    static Sprite _unitSprite;
    static Sprite UnitSprite()
    {
        if (_unitSprite != null) return _unitSprite;
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        _unitSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        _unitSprite.name = "UnitSquare";
        return _unitSprite;
    }

    static void AddLightning(GameObject parent)
    {
        // Overlay branco em screen-space pra dar a sensação de relâmpago.
        var go = new GameObject("LightningOverlay");
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = new Vector3(0, 0, -0.5f);
        go.transform.localScale = new Vector3(40, 24, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UnitSprite();
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = 8;

        var lf = go.AddComponent<LightningFlash>();
        lf.targetSprite = sr;
        lf.minInterval = 7f;
        lf.maxInterval = 14f;
        lf.flashDuration = 0.07f;
        lf.fadeDuration = 0.45f;
        lf.peakAlpha = 0.55f;
    }

    static void AddRain(GameObject parent, int intensity)
    {
        var rainGO = new GameObject("Rain");
        rainGO.transform.SetParent(parent.transform, false);
        rainGO.transform.localPosition = new Vector3(0, 7, 0);
        var ps = rainGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = -8f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.7f, 0.8f, 1f, 0.55f);
        main.maxParticles = 800;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = ps.emission;
        emission.rateOverTime = intensity;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(22, 1, 1);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 5;
    }

    static GameObject CreateUIText(Transform parent, string name, string text, int fontSize, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return go;
    }

    static GameObject CreateUIImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++) newScenes[i] = scenes[i];
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
#endif
