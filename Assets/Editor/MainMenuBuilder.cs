#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public static class MainMenuBuilder
{
    [MenuItem("Retroself/Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        // Cria uma cena nova vazia
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Camera ----------
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.039f, 0.039f, 0.078f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ---------- EventSystem ----------
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ---------- Background placeholders ----------
        var bgRoot = new GameObject("Background");
        CreatePlaceholderSprite("Sky", bgRoot.transform, new Vector2(0, 0), new Vector2(20, 12), new Color(0.05f, 0.06f, 0.12f), -10);
        CreatePlaceholderSprite("BuildingsBack", bgRoot.transform, new Vector2(0, 1), new Vector2(20, 6), new Color(0.08f, 0.10f, 0.18f), -8);
        CreatePlaceholderSprite("Tree", bgRoot.transform, new Vector2(-4, -1), new Vector2(2.5f, 5f), new Color(0.10f, 0.15f, 0.10f), -6);
        var cabin = CreatePlaceholderSprite("Cabin", bgRoot.transform, new Vector2(-3, -1.5f), new Vector2(1.5f, 2.5f), new Color(0.20f, 0.10f, 0.30f), -5);
        var glow = CreatePlaceholderSprite("CabinGlow", cabin.transform, Vector2.zero, new Vector2(2.2f, 3.2f), new Color(0.61f, 0.36f, 0.90f, 0.7f), -4);
        CreatePlaceholderSprite("Bench", bgRoot.transform, new Vector2(2, -2.5f), new Vector2(2.5f, 0.7f), new Color(0.25f, 0.18f, 0.12f), -5);
        CreatePlaceholderSprite("StreetLamp", bgRoot.transform, new Vector2(4, 0), new Vector2(0.3f, 4f), new Color(0.35f, 0.30f, 0.20f), -5);

        // CabinPulse no Cabin
        var pulse = cabin.AddComponent<CabinPulse>();
        pulse.cabinGlow = glow.GetComponent<SpriteRenderer>();
        pulse.minAlpha = 0.4f;
        pulse.maxAlpha = 1f;
        pulse.pulseSpeed = 1.5f;

        // ---------- Rain Particle System ----------
        var rainGO = new GameObject("Rain");
        rainGO.transform.position = new Vector3(0, 6, 0);
        var ps = rainGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = -8f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.7f, 0.8f, 1f, 0.6f);
        main.maxParticles = 600;
        var emission = ps.emission;
        emission.rateOverTime = 200;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20, 1, 1);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 10;

        // ---------- MenuController ----------
        var ctrlGO = new GameObject("MenuController");
        var actions = ctrlGO.AddComponent<MenuActions>();

        // ---------- Music Source ----------
        var musicGO = new GameObject("MenuMusicSource");
        var src = musicGO.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = true;
        src.volume = 0.6f;
        var menuMusic = musicGO.AddComponent<MenuMusic>();
        menuMusic.fadeInDuration = 2f;
        menuMusic.targetVolume = 0.6f;

        // ---------- Canvas ----------
        var canvasGO = new GameObject("MenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---------- Title ----------
        var titleGO = CreateUIText(canvasGO.transform, "Title", "RETROSELF", 140, FontStyles.Bold);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0, 250);
        titleRT.sizeDelta = new Vector2(1400, 200);
        var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.color = new Color(0.49f, 0.97f, 1f, 1f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 8f;
        var glitch = titleGO.AddComponent<TitleGlitch>();
        glitch.titleText = titleTMP;
        glitch.minInterval = 3f;
        glitch.maxInterval = 6f;
        glitch.glitchDuration = 0.12f;

        // ---------- Subtitle ----------
        var subGO = CreateUIText(canvasGO.transform, "Subtitle", "\"E se você pudesse voltar e ajudar a si mesmo a não desistir?\"", 32, FontStyles.Italic);
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchoredPosition = new Vector2(0, 130);
        subRT.sizeDelta = new Vector2(1500, 60);
        var subTMP = subGO.GetComponent<TextMeshProUGUI>();
        subTMP.color = new Color(1, 1, 1, 0.7f);
        subTMP.alignment = TextAlignmentOptions.Center;

        // ---------- Buttons ----------
        var btnComecar = CreateMenuButton(canvasGO.transform, "Btn_Comecar", "Começar", new Vector2(0, -120));
        var btnCreditos = CreateMenuButton(canvasGO.transform, "Btn_Creditos", "Créditos", new Vector2(0, -210));
        var btnSair = CreateMenuButton(canvasGO.transform, "Btn_Sair", "Sair", new Vector2(0, -300));

        AddPersistentListener(btnComecar, actions, nameof(MenuActions.Comecar));
        AddPersistentListener(btnCreditos, actions, nameof(MenuActions.AbrirCreditos));
        AddPersistentListener(btnSair, actions, nameof(MenuActions.Sair));

        // ---------- Credits Panel ----------
        var creditsPanelGO = new GameObject("CreditsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        creditsPanelGO.transform.SetParent(canvasGO.transform, false);
        var cpRT = creditsPanelGO.GetComponent<RectTransform>();
        cpRT.anchorMin = Vector2.zero;
        cpRT.anchorMax = Vector2.one;
        cpRT.offsetMin = Vector2.zero;
        cpRT.offsetMax = Vector2.zero;
        creditsPanelGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        creditsPanelGO.SetActive(false);

        // Credits text
        var creditsText = CreateUIText(creditsPanelGO.transform, "CreditsText",
            "<b>RETROSELF</b>\n\n<size=32>Direção & Game Design</size>\nAlex Chequer\nCarlos Hernani\nLucas Ikawa\n\n<size=32>Música & Som</size>\n[a definir]\n\n<size=32>Pixel Art</size>\n[a definir]\n\n<size=32>Inspirado em</size>\nBraid · Celeste · Brothers · Inside\nUndertale · About Time\n\n<size=24>Insper · 2026</size>",
            40, FontStyles.Normal);
        var crRT = creditsText.GetComponent<RectTransform>();
        crRT.anchoredPosition = new Vector2(0, 50);
        crRT.sizeDelta = new Vector2(1200, 800);
        creditsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var btnFechar = CreateMenuButton(creditsPanelGO.transform, "Btn_FecharCreditos", "Voltar", new Vector2(0, 0));
        var fecharRT = btnFechar.GetComponent<RectTransform>();
        fecharRT.anchorMin = new Vector2(0.5f, 0);
        fecharRT.anchorMax = new Vector2(0.5f, 0);
        fecharRT.anchoredPosition = new Vector2(0, 80);
        AddPersistentListener(btnFechar, actions, nameof(MenuActions.FecharCreditos));

        // Conecta o painel ao MenuActions
        var so = new SerializedObject(actions);
        so.FindProperty("creditsPanel").objectReferenceValue = creditsPanelGO;
        so.ApplyModifiedProperties();

        // ---------- Salva a cena ----------
        const string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"Cena MainMenu criada em {scenePath}");

        // Adiciona ao Build Settings
        AddSceneToBuildSettings(scenePath);
    }

    static GameObject CreatePlaceholderSprite(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0);
        go.transform.localScale = new Vector3(size.x, size.y, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateUnitSquareSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    static Sprite _unitSprite;
    static Sprite CreateUnitSquareSprite()
    {
        if (_unitSprite != null) return _unitSprite;
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        _unitSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return _unitSprite;
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

    static GameObject CreateMenuButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 70);
        rt.anchoredPosition = anchoredPos;

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.49f, 0.97f, 1f, 1f);
        colors.selectedColor = new Color(0.49f, 0.97f, 1f, 0.6f);
        btn.colors = colors;

        var textGO = new GameObject("Text (TMP)", typeof(RectTransform));
        textGO.transform.SetParent(btnGO.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnGO;
    }

    static void AddPersistentListener(GameObject buttonGO, MenuActions target, string methodName)
    {
        var btn = buttonGO.GetComponent<Button>();
        var method = typeof(MenuActions).GetMethod(methodName);
        var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, method);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
        for (int i = 0; i < scenes.Length; i++) newScenes[i + 1] = scenes[i];
        EditorBuildSettings.scenes = newScenes;
    }
}
#endif
