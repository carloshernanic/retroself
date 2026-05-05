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
    static readonly Color YoungCol = new Color(1f, 0.78f, 0.32f, 1f);    // jovem (jaqueta amarela)
    static readonly Color AdultCol = new Color(0.45f, 0.32f, 0.22f, 1f); // adulto (sobretudo marrom)
    static readonly Color HazardCol = new Color(0.20f, 0.05f, 0.05f, 1f); // poço escuro
    static readonly Color DummyCol = new Color(0.85f, 0.30f, 0.30f, 1f);  // alvo de teste
    static readonly Color BoxCol = new Color(0.55f, 0.36f, 0.22f, 1f);    // caixa pesada
    static readonly Color DoorCol = new Color(0.32f, 0.18f, 0.10f, 1f);   // porta da escola
    static readonly Color DoorFrameCol = new Color(0.62f, 0.46f, 0.22f, 1f); // moldura clara
    static readonly Color CreamCol = new Color(1f, 0.92f, 0.55f, 1f);     // creme do tema

    [MenuItem("Retroself/Build Memory_01_Patio Tutorial")]
    public static void BuildMemory01Tutorial()
    {
        // NÃO chama SpriteImportConfigurator aqui — ele reescreve o spritesheet do
        // importer e zera qualquer slicing/pivot manual feito no Sprite Editor.
        // Pra setar PPU/Point/no-compression em sprites NOVOS, rode
        // "Retroself → Configure Character Sprites" manualmente uma vez.

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camera = BuildCamera();
        BuildEventSystem();
        BuildPatioBackground(camera);
        BuildGround();
        BuildPatioProps();
        BuildHeavyBox(new Vector3(0f, -3f, 0));
        // Ground top y=-3 (tilemap grass at cell y=-4, span -4..-3). Visual frame
        // 2.2u tall com pivot center → pos.y = -3 + 1.1 = -1.9 deixa a base no chão.
        var door = BuildSchoolDoor(new Vector3(18.5f, -1.9f, 0));
        var bully = BuildBully(new Vector3(11f, -2.5f, 0));

        var young = BuildYoung(new Vector3(-7f, -2f, 0));
        var adult = BuildAdult(new Vector3(-5.5f, -2f, 0));
        var swap = BuildSwap(young, adult);

        AttachCameraFollow(camera, young);
        BuildPostProcessing(camera);
        BuildHUD(young, swap, young, adult, bully, door);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory01Builder] Tutorial montado em {ScenePath}. Jovem em {young.transform.position}, Adulto em {adult.transform.position}.");
    }

    static GameObject BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        // BG é coberto pelas layers do CityNight parentadas na câmera; cor sólida só
        // aparece se as layers falharem (asset não importado).
        cam.backgroundColor = new Color(0.04f, 0.05f, 0.10f, 1f);
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
        // Limita pra câmera não passar dos extremos do mapa (chão vai de -16 a +20).
        // minX=-7 deixa a câmera seguir o jovem (spawn -7) e o adulto (spawn -5.5)
        // em posições distintas, então o Tab faz a câmera realmente saltar entre eles.
        follow.minX = -7f;
        follow.maxX = 16f;
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
        // Tiles do residential-area pack. Sprites reais (32×32 px, PPU 32 → 1 cell = 1u).
        var dirtTile = LoadResidentialTile(SceneArtCatalog.TileAssetGround,   SceneArtCatalog.TileGround);
        var grassTile = LoadResidentialTile(SceneArtCatalog.TileAssetGrassTop, SceneArtCatalog.TileGrassTop);
        var wallTile = LoadResidentialTile(SceneArtCatalog.TileAssetWall,     SceneArtCatalog.TileWall);
        var platformTile = LoadResidentialTile(SceneArtCatalog.TileAssetPlatform, SceneArtCatalog.TilePlatform);

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

        // Chão contínuo: x ∈ [-16, 20) — superfície (grass) em y=-4, subterrâneo (dirt) em y=-5
        for (int x = -16; x < 20; x++)
        {
            tm.SetTile(new Vector3Int(x, -4, 0), grassTile);
            tm.SetTile(new Vector3Int(x, -5, 0), dirtTile);
        }

        // Parede de 3 tiles em x=4 (y=-3,-2,-1): topo em y=0, alta demais
        // pra ambos pularem do chão. Precisa da HeavyBox como degrau (top em y=-2.2):
        // do degrau, jovem alcança bottom y=0.26 (passa); adulto fica abaixo.
        for (int y = -3; y <= -1; y++) tm.SetTile(new Vector3Int(4, y, 0), wallTile);

        // "Ponte" decorativa por cima do bully (y=-1, x=8..10) — caminho alternativo aéreo.
        for (int x = 8; x <= 10; x++) tm.SetTile(new Vector3Int(x, -1, 0), platformTile);

        // Fresta baixa em x=16..17 (teto a y=-2): jovem cabe (1u alto), adulto (~2u) bate a cabeça.
        tm.SetTile(new Vector3Int(16, -2, 0), platformTile);
        tm.SetTile(new Vector3Int(17, -2, 0), platformTile);
        // Parede de fundo em x=20 (bloqueia também por cima)
        for (int y = -3; y <= 0; y++) tm.SetTile(new Vector3Int(20, y, 0), wallTile);

        // Força a geração da geometria do CompositeCollider2D no momento do build
        // pra que ela já fique cacheada no .unity. Sem isso, a 1ª FixedUpdate após
        // SceneManager.LoadScene roda enquanto o composite ainda regenera, e os Woody
        // atravessam o chão. Em Play direto na cena o Editor já pré-processa.
        tm.RefreshAllTiles();
        tc.ProcessTilemapChanges();
        cc.GenerateGeometry();
        EditorUtility.SetDirty(tmGO);
    }

    // Cria/atualiza um Tile.asset apontando pro sprite real do residential pack.
    // Idempotente: só atualiza a ref de sprite se já existir.
    static Tile LoadResidentialTile(string assetPath, string spritePath)
    {
        var dir = System.IO.Path.GetDirectoryName(assetPath);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError($"[Memory01Builder] sprite não encontrado em {spritePath} — rode Retroself → Configure Scene Art Imports.");
        }

        var existing = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
        if (existing != null)
        {
            existing.sprite = sprite;
            existing.color = Color.white;
            existing.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        tile.colliderType = Tile.ColliderType.Sprite;
        AssetDatabase.CreateAsset(tile, assetPath);
        AssetDatabase.SaveAssets();
        return tile;
    }

    // BG do pátio: 5 layers do CityNight pack parentadas na câmera (seguem o player
    // automaticamente, sem precisar de script de parallax tracking). Camera ortho
    // size 5 = 10u altura; layers em PPU 50 ficam 11.52×6.48u — cobre a altura
    // visível e o ortho center fica abaixo do skyline. Sortings -100..-92 deixam
    // tudo atrás do tilemap (sortingOrder 5).
    static void BuildPatioBackground(GameObject camGO)
    {
        var bgRoot = new GameObject("Background_CityNight");
        bgRoot.transform.SetParent(camGO.transform, false);
        // Z=10 fica à frente da câmera (câmera em z=-10) e atrás de tudo na cena.
        // Y=0 centraliza vertical (PPU 32 → 10.125u alto cobre ortho 10u).
        bgRoot.transform.localPosition = new Vector3(0, 0f, 10f);

        for (int i = 0; i < SceneArtCatalog.CityNightLayers.Length; i++)
        {
            var path = SceneArtCatalog.CityNightLayers[i];
            var sprite = SceneArtCatalog.LoadSprite(path);
            var go = new GameObject($"Layer_{i}");
            go.transform.SetParent(bgRoot.transform, false);
            go.transform.localPosition = new Vector3(0, 0, i * 0.01f); // pequena separação z só pra evitar z-fighting
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100 + i; // -100 (céu) → -96 (chão BG)

            // Sway leve só nas camadas mais distantes (i=0..2). Camera move horizontal
            // → BG inteiro segue (parented), mas sway dá vida ao skyline.
            if (i <= 2)
            {
                var px = go.AddComponent<ParallaxLayer>();
                px.swayAmplitude = 0.06f - i * 0.015f;
                px.swaySpeed = 0.18f + i * 0.05f;
                px.phase = i * 0.7f;
            }
        }
    }

    // Props decorativos no pátio (sem colliders) — só dão presença visual ao chão
    // que antes era só tilemap. Sortings 4 ficam à frente do BG (-100..-96) e do
    // tilemap (5? — na verdade o tilemap tá em 5; usamos 4 pros props ficarem
    // ATRÁS da grama sliced top mas à frente do BG).
    static void BuildPatioProps()
    {
        AddProp("Bench",    SceneArtCatalog.PropBench,    new Vector3(-10f, -3f, 0), 4);
        AddProp("Trashcan", SceneArtCatalog.PropTrashcan, new Vector3(  2f, -3f, 0), 4);
        AddProp("Lamp",     SceneArtCatalog.PropLamp,     new Vector3( 14f, -3f, 0), 4);
    }

    static void AddProp(string name, string spritePath, Vector3 worldPos, int sortingOrder)
    {
        var sprite = SceneArtCatalog.LoadSprite(spritePath);
        var go = new GameObject(name);
        go.transform.position = worldPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }

    static void BuildHazard()
    {
        // ocupa o gap (x de -1 a +5) e desce pra fora da tela
        var go = new GameObject("HazardPit");
        go.transform.position = new Vector3(2f, -4f, 0);
        go.transform.localScale = new Vector3(6f, 2f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = HazardCol;
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<HazardZone>();
    }

    static GameObject BuildBully(Vector3 pos)
    {
        var root = new GameObject("Bully");
        root.transform.position = pos;

        var colliderSize = new Vector2(0.7f, 1.1f);
        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0, -colliderSize.y * 0.5f, 0);
        body.transform.localScale = Vector3.one;
        var sr = body.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 9;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.mass = 5f;

        var col = root.AddComponent<BoxCollider2D>();
        col.size = colliderSize;
        col.sharedMaterial = new PhysicsMaterial2D("BullyNoFriction") { friction = 0f, bounciness = 0f };

        var hp = root.AddComponent<EnemyHealth>();
        hp.maxHealth = 3;
        hp.stunTime = 0.6f;

        var ai = root.AddComponent<BullyController>();
        ai.body = sr;
        ai.patrolMinX = 5f;
        ai.patrolMaxX = 13f;
        ai.detectionRange = 6f;

        // Animator simples: recebe listas de Sprite já fatiadas (Multiple mode no
        // SpriteImportConfigurator) e cicla. Sem Unity Animator nem Sprite.Create.
        var anim = body.AddComponent<SimpleSpriteAnimator>();
        anim.idleSprites = LoadSpriteFrames("Assets/Sprites/gangsters/Gangsters_2/Idle.png");
        anim.walkSprites = LoadSpriteFrames("Assets/Sprites/gangsters/Gangsters_2/Walk.png");
        anim.jumpSprites = LoadSpriteFrames("Assets/Sprites/gangsters/Gangsters_2/Jump.png");
        anim.rb = rb;

        return root;
    }

    static GameObject BuildYoung(Vector3 pos)
    {
        // Jovem: 16×16 (collider 0.55×0.85). Cabe em fresta baixa com folga ~0.15u.
        return BuildPlayerInternal("PlayerYoung", pos, PlayerKind.Young,
            colliderSize: new Vector2(0.55f, 0.85f),
            spriteFolder: "Assets/Sprites/criancas/Child_3",
            hasJump: false, // Child_3 sem Jump.png — fallback p/ Idle
            groundCheckY: -0.5f);
    }

    static GameObject BuildAdult(Vector3 pos)
    {
        // Adulto: 16×32 (collider 0.55×1.9). Não cabe em fresta de 1u, empurra HeavyBox.
        return BuildPlayerInternal("PlayerAdult", pos, PlayerKind.Adult,
            colliderSize: new Vector2(0.55f, 1.9f),
            spriteFolder: "Assets/Sprites/mendigos/Homeless_1",
            hasJump: true,
            groundCheckY: -1.05f);
    }

    static GameObject BuildPlayerInternal(string name, Vector3 pos, PlayerKind kind,
        Vector2 colliderSize, string spriteFolder, bool hasJump, float groundCheckY)
    {
        var root = new GameObject(name);
        root.tag = "Player";
        root.transform.position = pos;

        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0, -colliderSize.y * 0.5f, 0);
        body.transform.localScale = Vector3.one;
        var sr = body.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 10;

        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(root.transform, false);
        groundCheck.transform.localPosition = new Vector3(0, groundCheckY, 0);

        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        var col = root.AddComponent<BoxCollider2D>();
        col.size = colliderSize;
        col.offset = Vector2.zero;
        col.sharedMaterial = new PhysicsMaterial2D(name + "NoFriction") { friction = 0f, bounciness = 0f };

        var pc = root.AddComponent<PlayerController>();
        pc.body = sr;
        pc.groundCheck = groundCheck.transform;
        pc.groundCheckRadius = 0.12f;
        pc.groundMask = ~0;
        pc.kind = kind;

        root.AddComponent<PlayerHealth>();
        var atk = root.AddComponent<PlayerAttack>();

        // Animator simples — listas de Sprite já fatiadas pelo SpriteImportConfigurator.
        var anim = body.AddComponent<SimpleSpriteAnimator>();
        anim.idleSprites = LoadSpriteFrames($"{spriteFolder}/Idle.png");
        anim.walkSprites = LoadSpriteFrames($"{spriteFolder}/Walk.png");
        if (hasJump) anim.jumpSprites = LoadSpriteFrames($"{spriteFolder}/Jump.png");
        anim.playerController = pc;

        return root;
    }

    // Carrega só os sub-sprites (slices) de um spritesheet em Multiple mode,
    // ordenados pelo índice numérico no sufixo "_N". Filtra fora a Texture2D
    // principal e qualquer sprite "whole-texture" que sobre quando o import vai
    // de Single→Multiple.
    static Sprite[] LoadSpriteFrames(string sheetPath)
    {
        var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
        string baseName = System.IO.Path.GetFileNameWithoutExtension(sheetPath);
        string prefix = baseName + "_";
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var obj in subs)
        {
            if (obj is Sprite s && s.name.StartsWith(prefix))
                list.Add(s);
        }
        list.Sort((a, b) => SpriteIndex(a.name).CompareTo(SpriteIndex(b.name)));
        if (list.Count == 0)
            Debug.LogWarning($"[Memory01Builder] sem sub-sprites em {sheetPath} — rode Retroself → Configure Character Sprites primeiro");
        return list.ToArray();
    }

    static int SpriteIndex(string spriteName)
    {
        int u = spriteName.LastIndexOf('_');
        if (u < 0 || u + 1 >= spriteName.Length) return 0;
        return int.TryParse(spriteName.Substring(u + 1), out var i) ? i : 0;
    }

    static GameObject BuildSwap(GameObject young, GameObject adult)
    {
        var go = new GameObject("PlayerSwap");
        var sw = go.AddComponent<PlayerSwap>();
        sw.young = young.GetComponent<PlayerController>();
        sw.adult = adult.GetComponent<PlayerController>();
        return go;
    }

    static void BuildHeavyBox(Vector3 pos)
    {
        var go = new GameObject("HeavyBox");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;

        var sr = go.AddComponent<SpriteRenderer>();
        var boxSprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.PropBox);
        sr.sprite = boxSprite != null ? boxSprite : SolidSprite();
        sr.color = boxSprite != null ? Color.white : BoxCol;
        sr.sortingOrder = 8;

        // Box é PPU 32 com pivot BottomCenter (config do Configurator). 1 cell = 1u,
        // sprite ~1×1u → collider 0.95×0.95 com offset y=0.5 fica certinho na base.
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.95f, 0.95f);
        col.offset = new Vector2(0f, 0.5f);

        // O componente HeavyBox configura o RB no Awake.
        go.AddComponent<Rigidbody2D>();
        go.AddComponent<HeavyBox>();
    }

    static GameObject BuildSchoolDoor(Vector3 pos)
    {
        var root = new GameObject("SchoolDoor");
        root.transform.position = pos;

        // Moldura externa (mais clara) — visual de parede ao redor da porta
        var frame = new GameObject("Frame");
        frame.transform.SetParent(root.transform, false);
        frame.transform.localScale = new Vector3(1.1f, 2.2f, 1f);
        var fsr = frame.AddComponent<SpriteRenderer>();
        fsr.sprite = SolidSprite();
        fsr.color = DoorFrameCol;
        fsr.sortingOrder = 6;

        // Folha da porta (mais escura)
        var leaf = new GameObject("Leaf");
        leaf.transform.SetParent(root.transform, false);
        leaf.transform.localScale = new Vector3(0.85f, 1.95f, 1f);
        var lsr = leaf.AddComponent<SpriteRenderer>();
        lsr.sprite = SolidSprite();
        lsr.color = DoorCol;
        lsr.sortingOrder = 7;

        // Maçaneta (creme)
        var knob = new GameObject("Knob");
        knob.transform.SetParent(root.transform, false);
        knob.transform.localPosition = new Vector3(0.25f, 0f, 0f);
        knob.transform.localScale = new Vector3(0.12f, 0.12f, 1f);
        var ksr = knob.AddComponent<SpriteRenderer>();
        ksr.sprite = SolidSprite();
        ksr.color = CreamCol;
        ksr.sortingOrder = 8;

        // Trigger que dispara o SchoolDoor
        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.0f, 2.0f);
        col.isTrigger = true;

        root.AddComponent<SchoolDoor>();

        // Bobbing leve pra a porta "respirar" e chamar atenção
        var bob = root.AddComponent<IdleBob>();
        bob.amplitude = 0.05f;
        bob.speed = 1.5f;
        return root;
    }

    static void BuildHUD(GameObject player, GameObject swap, GameObject young, GameObject adult, GameObject bully, GameObject door)
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        CreateUIText(canvasGO.transform, "TutorialHint",
            "[A]/[D] mover  [Espaço] pular  [K] arremessar  [Tab] trocar Woody  -  leve os dois ate a porta",
            16, FontStyles.Bold,
            new Vector2(0, -440), new Vector2(1820, 60),
            new Color(1, 0.95f, 0.7f, 0.9f));

        CreateUIText(canvasGO.transform, "Title", "MEMORY 01 - O PATIO",
            28, FontStyles.Bold,
            new Vector2(0, 460), new Vector2(1200, 60),
            CreamCol,
            TextAlignmentOptions.Center);

        BuildHealthBar(canvasGO.transform, player);
        BuildIntroDialogue(canvasGO.transform, swap, young, adult, bully);

        // Overlay de fade pra porta usar (Image preto cobrindo a tela, alpha 0).
        var fadeGO = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        var fadeRt = fadeGO.AddComponent<RectTransform>();
        fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one;
        fadeRt.offsetMin = fadeRt.offsetMax = Vector2.zero;
        var fadeImg = fadeGO.AddComponent<Image>();
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        fadeImg.raycastTarget = false;
        // O FadeOverlay precisa estar sobre o intro dialogue mas ele só mostra quando porta dispara.
        fadeGO.transform.SetAsLastSibling();

        if (door != null)
        {
            var sd = door.GetComponent<SchoolDoor>();
            if (sd != null) sd.fadeOverlay = fadeImg;
        }
    }

    static void BuildIntroDialogue(Transform parent, GameObject swap, GameObject young, GameObject adult, GameObject bully)
    {
        // Painel inferior tipo dialog box. Layout em coordenadas absolutas dentro do box:
        //   Portrait esquerda (x=40..220), texto à direita (x=260..1660), labels do topo
        //   ancorados em top-left, continue ancorado bottom-right.
        var box = new GameObject("IntroDialogueBox");
        box.transform.SetParent(parent, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(1700, 280);

        var bg = box.AddComponent<Image>();
        // Preto sólido (sem transparência) — o usuário pediu fundo opaco.
        bg.color = Color.black;
        bg.raycastTarget = false;

        // Portrait (quad colorido — placeholder do retrato do adulto)
        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(box.transform, false);
        var prt = portraitGO.AddComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = new Vector2(0, 0.5f);
        prt.pivot = new Vector2(0, 0.5f);
        prt.anchoredPosition = new Vector2(40, 0);
        prt.sizeDelta = new Vector2(180, 220);
        var pimg = portraitGO.AddComponent<Image>();
        pimg.color = AdultCol;
        pimg.raycastTarget = false;

        // Speaker label — ancorada ao top-left do box (não centralizada, pra não sobrepor texto).
        var labelGO = CreateUIText(box.transform, "SpeakerLabel", "Woody (adulto)", 18, FontStyles.Bold,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Left);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = labelRt.anchorMax = new Vector2(0, 1);
        labelRt.pivot = new Vector2(0, 1);
        labelRt.anchoredPosition = new Vector2(250, -20);
        labelRt.sizeDelta = new Vector2(1200, 36);

        // Texto principal — abaixo da label, ancorado ao top-left.
        var textGO = CreateUIText(box.transform, "DialogText", "", 18, FontStyles.Normal,
            Vector2.zero, Vector2.zero,
            new Color(1f, 0.96f, 0.85f, 1f), TextAlignmentOptions.TopLeft);
        var textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = textRt.anchorMax = new Vector2(0, 1);
        textRt.pivot = new Vector2(0, 1);
        textRt.anchoredPosition = new Vector2(250, -65);
        textRt.sizeDelta = new Vector2(1400, 160);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Truncate;

        var typewriter = box.AddComponent<TypewriterText>();
        typewriter.target = tmp;
        typewriter.charsPerSecond = 38f;

        // Continue indicator (seta piscando no canto inferior direito).
        // Usa ">>" em vez de "▶" porque a LiberationSans SDF default não tem esse glyph.
        var contGO = CreateUIText(box.transform, "Continue", ">> Espaco/Enter", 14, FontStyles.Italic,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Right);
        var contRt = contGO.GetComponent<RectTransform>();
        contRt.anchorMin = contRt.anchorMax = new Vector2(1, 0);
        contRt.pivot = new Vector2(1, 0);
        contRt.anchoredPosition = new Vector2(-30, 18);
        contRt.sizeDelta = new Vector2(360, 32);
        contGO.AddComponent<BlinkUI>();

        // Componente que orquestra
        var intro = box.AddComponent<IntroDialogue>();
        intro.dialogueBox = box;
        intro.typewriter = typewriter;
        intro.continueIndicator = contGO;
        if (swap != null) intro.playerSwap = swap.GetComponent<PlayerSwap>();
        if (young != null) intro.young = young.GetComponent<PlayerController>();
        if (adult != null) intro.adult = adult.GetComponent<PlayerController>();
        if (bully != null) intro.bully = bully.GetComponent<BullyController>();
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

        CreateUIText(root.transform, "Label", "VIDA", 14, FontStyles.Bold,
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
        var pixelFont = SceneArtCatalog.GetPixelFont();
        if (pixelFont != null) tmp.font = pixelFont;
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
