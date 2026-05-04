#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class Memory01Builder
{
    const string ScenePath = "Assets/Scenes/Memory_01_Patio.unity";

    static readonly Color BgSky = new Color(0.32f, 0.36f, 0.18f, 1f);   // amarelo-esverdeado escolar
    static readonly Color PlayerCol = new Color(1f, 0.78f, 0.32f, 1f);   // jovem (jaqueta amarela)
    static readonly Color HazardCol = new Color(0.20f, 0.05f, 0.05f, 1f); // poço escuro
    static readonly Color DummyCol = new Color(0.85f, 0.30f, 0.30f, 1f);  // alvo de teste

    [MenuItem("Retroself/Build Memory_01_Patio Tutorial")]
    public static void BuildMemory01Tutorial()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camera = BuildCamera();
        BuildEventSystem();
        BuildGround();
        BuildHazard();
        BuildBully(new Vector3(11f, -2.5f, 0));
        var player = BuildPlayer(new Vector3(-7f, 0f, 0));
        AttachCameraFollow(camera, player);
        BuildPostProcessing(camera);
        BuildHUD(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory01Builder] Tutorial montado em {ScenePath}. Player em {player.transform.position}.");
    }

    static GameObject BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgSky;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();
        return camGO;
    }

    static void BuildPostProcessing(GameObject camGO)
    {
        // 1) Liga post-processing na câmera (persiste via SerializedObject)
        var cam = camGO.GetComponent<Camera>();
        var camData = cam.GetUniversalAdditionalCameraData();
        var so = new SerializedObject(camData);
        var prop = so.FindProperty("m_RenderPostProcessing");
        if (prop != null) prop.boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(camData);

        // 2) Cria/atualiza o Volume Profile asset
        const string profilePath = "Assets/Settings/Memory01_PostProcess.asset";
        if (!System.IO.Directory.Exists("Assets/Settings"))
            System.IO.Directory.CreateDirectory("Assets/Settings");

        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        else
        {
            // limpa overrides antigos pra reaplicar valores frescos
            for (int i = profile.components.Count - 1; i >= 0; i--)
                Object.DestroyImmediate(profile.components[i], true);
            profile.components.Clear();
        }

        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.18f);
        bloom.threshold.Override(1.1f);
        bloom.scatter.Override(0.55f);
        bloom.tint.Override(new Color(1f, 0.94f, 0.7f, 1f));

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.35f);
        vignette.smoothness.Override(0.45f);
        vignette.color.Override(new Color(0.03f, 0.02f, 0.01f, 1f));

        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(-0.2f);
        color.contrast.Override(10f);
        color.saturation.Override(-3f);
        color.colorFilter.Override(new Color(0.97f, 0.95f, 0.85f, 1f));

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // recarrega como asset salvo pra garantir GUID válido na referência
        profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

        // 3) Adiciona um Global Volume na cena ligado ao profile (sharedProfile, não clona)
        var volGO = new GameObject("Global Volume");
        var vol = volGO.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 0;
        vol.weight = 1f;
        vol.sharedProfile = profile;
        EditorUtility.SetDirty(vol);
    }

    static void AttachCameraFollow(GameObject camGO, GameObject player)
    {
        var follow = camGO.AddComponent<CameraFollow2D>();
        follow.target = player.transform;
        follow.offset = new Vector2(0f, 1.5f);
        // Limita pra câmera não passar do extremo esquerdo do mapa nem subir de mais
        follow.minX = -3f;
        follow.maxX = 11f;
        follow.minY = -1f;
        follow.maxY = 2f;
    }

    static void BuildEventSystem()
    {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();
    }

    static void BuildGround()
    {
        var dirtTile = LoadOrCreateTile("Assets/Settings/Tiles/Ground_Dirt.asset", new Color(0.38f, 0.24f, 0.16f));
        var grassTile = LoadOrCreateTile("Assets/Settings/Tiles/Ground_GrassTop.asset", new Color(0.55f, 0.62f, 0.22f));
        var platformTile = LoadOrCreateTile("Assets/Settings/Tiles/Platform_Wood.asset", new Color(0.62f, 0.42f, 0.26f));

        var gridGO = new GameObject("Grid");
        gridGO.AddComponent<Grid>();

        var tmGO = new GameObject("Tilemap_Ground");
        tmGO.transform.SetParent(gridGO.transform, false);
        var tm = tmGO.AddComponent<Tilemap>();
        var tr = tmGO.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 5;

        // Rigidbody2D Static + CompositeCollider2D mescla os colliders dos tiles em
        // uma forma só, eliminando as "costuras" entre tiles que prendem o player/bully
        var tmRb = tmGO.AddComponent<Rigidbody2D>();
        tmRb.bodyType = RigidbodyType2D.Static;

        var tc = tmGO.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;

        var cc = tmGO.AddComponent<CompositeCollider2D>();
        cc.geometryType = CompositeCollider2D.GeometryType.Polygons;

        // Chão esquerdo: x ∈ [-13, -1) — superfície (grass) em y=-4, subterrâneo (dirt) em y=-5
        for (int x = -13; x < -1; x++)
        {
            tm.SetTile(new Vector3Int(x, -4, 0), grassTile);
            tm.SetTile(new Vector3Int(x, -5, 0), dirtTile);
        }

        // Chão direito: x ∈ [3, 15)
        for (int x = 3; x < 15; x++)
        {
            tm.SetTile(new Vector3Int(x, -4, 0), grassTile);
            tm.SetTile(new Vector3Int(x, -5, 0), dirtTile);
        }

        // Gap de morte: x ∈ [-1, 3) — sem tiles

        // Plataformas (top em y=-1 e y=0 respectivamente)
        for (int x = -1; x <= 1; x++) tm.SetTile(new Vector3Int(x, -2, 0), platformTile);
        for (int x = 3; x <= 5; x++) tm.SetTile(new Vector3Int(x, -1, 0), platformTile);
    }

    static Tile LoadOrCreateTile(string path, Color color)
    {
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        var existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.name = "TileTex";

        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        sprite.name = "TileSprite";

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = color;
        tile.colliderType = Tile.ColliderType.Sprite;

        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.AddObjectToAsset(tex, tile);
        AssetDatabase.AddObjectToAsset(sprite, tile);
        AssetDatabase.SaveAssets();
        return tile;
    }

    static void BuildHazard()
    {
        // ocupa o gap (x de -1 a +3) e desce pra fora da tela
        var go = new GameObject("HazardPit");
        go.transform.position = new Vector3(1f, -4f, 0);
        go.transform.localScale = new Vector3(4f, 2f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = HazardCol;
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<HazardZone>();
    }

    static void BuildBully(Vector3 pos)
    {
        var root = new GameObject("Bully");
        root.transform.position = pos;

        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
        var sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = DummyCol;
        sr.sortingOrder = 9;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.mass = 5f;

        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 1.1f);
        col.sharedMaterial = new PhysicsMaterial2D("BullyNoFriction") { friction = 0f, bounciness = 0f };

        var hp = root.AddComponent<EnemyHealth>();
        hp.maxHealth = 3;
        hp.stunTime = 0.6f;

        var ai = root.AddComponent<BullyController>();
        ai.body = sr;
        // patrulha entre x=+5 e x=+13 (chão direito vai de +3 a +15)
        ai.patrolMinX = 5f;
        ai.patrolMaxX = 13f;
        ai.detectionRange = 6f;
    }

    static GameObject BuildPlayer(Vector3 pos)
    {
        var root = new GameObject("Player");
        root.tag = "Player";
        root.transform.position = pos;

        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.6f, 1f, 1f); // jovem ~16×16, mas alongado pra ler em tela
        var sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = PlayerCol;
        sr.sortingOrder = 10;

        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(root.transform, false);
        groundCheck.transform.localPosition = new Vector3(0, -0.6f, 0);

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.55f, 0.95f);
        col.offset = new Vector2(0, 0);
        col.sharedMaterial = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };

        var pc = root.AddComponent<PlayerController>();
        pc.body = sr;
        pc.groundCheck = groundCheck.transform;
        pc.groundCheckRadius = 0.12f;
        pc.groundMask = ~0; // tudo; o controller filtra a si mesmo

        root.AddComponent<PlayerHealth>();
        var atk = root.AddComponent<PlayerAttack>();

        var anim = root.AddComponent<PlayerAnimator>();
        anim.controller = pc;
        anim.attack = atk;
        anim.body = sr;

        return root;
    }

    static void BuildHUD(GameObject player)
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        CreateUIText(canvasGO.transform, "TutorialHint",
            "[A]/[D] mover   ·   [Espaço] pular   ·   [K] arremessar",
            36, FontStyles.Bold,
            new Vector2(0, -440), new Vector2(1400, 80),
            new Color(1, 0.95f, 0.7f, 0.9f));

        CreateUIText(canvasGO.transform, "Title", "MEMORY 01 — O PÁTIO",
            48, FontStyles.Bold,
            new Vector2(0, 460), new Vector2(1200, 80),
            new Color(1, 0.92f, 0.55f, 1f),
            TextAlignmentOptions.Center);

        BuildHealthBar(canvasGO.transform, player);
    }

    static void BuildHealthBar(Transform parent, GameObject player)
    {
        var root = new GameObject("HealthBar");
        root.transform.SetParent(parent, false);
        var rrt = root.AddComponent<RectTransform>();
        rrt.anchorMin = rrt.anchorMax = new Vector2(0, 1);
        rrt.pivot = new Vector2(0, 1);
        rrt.anchoredPosition = new Vector2(40, -40);
        rrt.sizeDelta = new Vector2(360, 32);

        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(root.transform, false);
        var bgRt = bgGO.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.55f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(root.transform, false);
        var fillRt = fillGO.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.pivot = new Vector2(0, 0.5f);
        fillRt.anchoredPosition = new Vector2(4, 0);
        fillRt.sizeDelta = new Vector2(352, -8);
        var fill = fillGO.AddComponent<Image>();
        fill.color = new Color(0.55f, 0.85f, 0.4f);

        CreateUIText(root.transform, "Label", "VIDA", 22, FontStyles.Bold,
            new Vector2(0, 32), new Vector2(120, 24),
            new Color(1, 0.95f, 0.7f, 0.85f), TextAlignmentOptions.Left);

        var hb = root.AddComponent<HealthBarUI>();
        hb.target = player.GetComponent<PlayerHealth>();
        hb.fill = fill;
        hb.fillRect = fillRt;
    }

    // ---------- Helpers ----------

    static Sprite cachedSolid;
    static Sprite SolidSprite()
    {
        if (cachedSolid != null) return cachedSolid;
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        cachedSolid = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        return cachedSolid;
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
