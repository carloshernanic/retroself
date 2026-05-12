#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SchoolPlaceholderBuilder
{
    const string ScenePath = "Assets/Scenes/Memory_01_School.unity";

    static readonly Color BgCol = new Color(0.05f, 0.04f, 0.03f, 1f);
    static readonly Color CreamCol = new Color(1f, 0.92f, 0.55f, 1f);
    static readonly Color SubtleCol = new Color(1f, 0.95f, 0.7f, 0.85f);

    [MenuItem("Retroself/Build Memory_01_School Placeholder")]
    public static void BuildSchoolPlaceholder()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        BuildCamera();
        BuildEventSystem();
        var menuGO = BuildMenuActions();
        BuildCanvas(menuGO);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[SchoolPlaceholderBuilder] Cena salva em {ScenePath}.");
    }

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

    static GameObject BuildMenuActions()
    {
        var go = new GameObject("MenuActions");
        go.AddComponent<MenuActions>();
        return go;
    }

    static void BuildCanvas(GameObject menuGO)
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        CreateUIText(canvasGO.transform, "Title", "FIM DO TUTORIAL", 96, FontStyles.Bold,
            new Vector2(0, 200), new Vector2(1600, 140), CreamCol);

        CreateUIText(canvasGO.transform, "Subtitle",
            "A escola te espera — em breve, na próxima sprint.",
            36, FontStyles.Italic,
            new Vector2(0, 60), new Vector2(1600, 80), SubtleCol);

        BuildBackButton(canvasGO.transform, menuGO);
    }

    static void BuildBackButton(Transform parent, GameObject menuGO)
    {
        var btnGO = new GameObject("BackButton");
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -120);
        rt.sizeDelta = new Vector2(420, 80);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.18f, 0.14f, 0.10f, 0.85f);

        var btn = btnGO.AddComponent<Button>();
        var menu = menuGO != null ? menuGO.GetComponent<MenuActions>() : null;
        if (menu != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick,
                new UnityEngine.Events.UnityAction(menu.VoltarMenu));
        }

        CreateUIText(btnGO.transform, "Label", "Voltar ao Menu", 36, FontStyles.Bold,
            Vector2.zero, new Vector2(420, 80), CreamCol);
    }

    static GameObject CreateUIText(Transform parent, string name, string text, int size, FontStyles style,
        Vector2 anchored, Vector2 sizeDelta, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = scenes.Exists(s => s.path == scenePath);
        if (!exists)
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
