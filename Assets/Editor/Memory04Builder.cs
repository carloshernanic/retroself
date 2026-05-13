#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builder de Memory_04_Sala — fase final, cenário cyberpunk-market. Maior fase do jogo
// (~75u de span, x=-5..70). 6 puzzles em sequência: catraca→Snake arcade→food errand→
// GuitarHero arcade→sequence lock→finish.
//
// Tom: Woody 15-16 anos fugindo de casa pro mercado. Mistura comidas como itens-puzzle
// + 2 cabines arcade embedded (Snake e Guitar Hero co-op).
//
// Mesmo padrão de Memory_03: OwnedRoots + SceneRebuildHelpers, post-processing + Light2D
// tunados manualmente sobrevivem rebuild. Não cria Volume/Light2D — só liga o
// renderPostProcessing da câmera e Sprite-Lit-Default nos SpriteRenderers.
public static class Memory04Builder
{
    const string ScenePath = "Assets/Scenes/Memory_04_Sala.unity";
    const string BeatMapPath = "Assets/Settings/BeatMap_M04.asset";
    const string GuitarHeroSongPath = "Assets/Audio/retroself-gh.wav";

    static readonly string[] OwnedRoots = {
        "Main Camera", "EventSystem", "Grid", "BG_Market",
        "Decor", "Hazards", "Arcades", "Puzzles",
        "PlayerYoung", "PlayerAdult", "PlayerSwap",
        "FinishDoor", "Canvas", "SceneStartReset",
    };

    // ----- Paleta cyberpunk-market -----
    static readonly Color CreamCol     = new Color(1f, 0.92f, 0.55f, 1f);
    static readonly Color AdultCol     = new Color(0.45f, 0.32f, 0.22f, 1f);
    static readonly Color PlateOffCol  = new Color(0.20f, 0.18f, 0.30f, 1f);
    static readonly Color PlateOnCol   = new Color(0.95f, 0.65f, 0.85f, 1f);   // neon pink
    static readonly Color GateCol      = new Color(0.30f, 0.25f, 0.45f, 1f);   // roxo escuro
    static readonly Color PlatformCol  = new Color(0.30f, 0.32f, 0.45f, 1f);   // azul-cinza
    static readonly Color FloorTopCol  = new Color(0.20f, 0.18f, 0.28f, 1f);   // asfalto noturno (topo)
    static readonly Color FloorBaseCol = new Color(0.10f, 0.08f, 0.16f, 1f);   // calçada profunda
    static readonly Color WallCol      = new Color(0.16f, 0.14f, 0.22f, 1f);

    // ----- Cofre (cores dos switches do P5) -----
    static readonly Color LockRed      = new Color(0.95f, 0.30f, 0.30f, 1f);
    static readonly Color LockRedOff   = new Color(0.45f, 0.18f, 0.18f, 1f);
    static readonly Color LockGreen    = new Color(0.45f, 0.90f, 0.40f, 1f);
    static readonly Color LockGreenOff = new Color(0.20f, 0.40f, 0.20f, 1f);
    static readonly Color LockBlue     = new Color(0.40f, 0.65f, 0.95f, 1f);
    static readonly Color LockBlueOff  = new Color(0.18f, 0.28f, 0.45f, 1f);

    [MenuItem("Retroself/Build Memory_04_Mercado")]
    public static void BuildMemory04Market()
    {
        var scene = SceneRebuildHelpers.OpenOrNew(ScenePath);
        if (!SceneRebuildHelpers.ConfirmRebuild(scene, OwnedRoots)) return;
        SceneRebuildHelpers.WipeOwnedRoots(scene, OwnedRoots);

        var camera = BuildCamera();
        BuildEventSystem();
        BuildMarketBackground(camera);
        BuildMarketFloor();
        BuildDecor();
        BuildHazardsRoot();

        var young = BuildYoung(new Vector3(-3f, -2f, 0));
        var adult = BuildAdult(new Vector3(-1.5f, -2f, 0));
        var swap = BuildSwap(young, adult);

        var arcadesRoot = new GameObject("Arcades");
        var puzzlesRoot = new GameObject("Puzzles");

        BuildP1_Entrance(puzzlesRoot.transform);
        var snakeArcade = BuildP2_SnakeArcade(arcadesRoot.transform, puzzlesRoot.transform, young, adult, swap);
        BuildP3_FoodErrand(puzzlesRoot.transform);
        var ghArcade = BuildP4_GuitarHeroArcade(arcadesRoot.transform, puzzlesRoot.transform, young, adult, swap);
        BuildP5_VendingSequence(puzzlesRoot.transform);
        BuildP6_ReturnPad(puzzlesRoot.transform, young, adult);

        AttachCameraFollow(camera, young);

        var finishDoor = BuildFinishDoor(new Vector3(68f, -1.9f, 0));
        var finishDoorComp = finishDoor.GetComponent<CoopFinishDoor>();
        if (finishDoorComp != null)
        {
            finishDoorComp.requireKey = false;
            finishDoorComp.targetScene = SceneNames.Memory_04_Cutscene_Placeholder;
        }

        var resetGO = new GameObject("SceneStartReset");
        resetGO.AddComponent<SceneStartReset>();

        BuildHUD(young, swap, young, adult, finishDoor);

        EnableCameraPostProcessing(camera);
        ApplyLitMaterialToSprites(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory04Builder] Memory_04 montada em {ScenePath}.");
    }

    // ---------- Camera / EventSystem ----------

    static GameObject BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.10f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();
        return camGO;
    }

    static void BuildEventSystem()
    {
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();
    }

    static void AttachCameraFollow(GameObject camGO, GameObject player)
    {
        var follow = camGO.AddComponent<CameraFollow2D>();
        follow.target = player.transform;
        follow.offset = new Vector2(0f, 1.5f);
        follow.minX = -3f;
        follow.maxX = 66f;
        follow.minY = -1f;
        follow.maxY = 4f;
    }

    static void EnableCameraPostProcessing(GameObject camGO)
    {
        var cam = camGO.GetComponent<Camera>();
        if (cam == null) return;
        var camData = cam.GetUniversalAdditionalCameraData();
        var so = new SerializedObject(camData);
        var prop = so.FindProperty("m_RenderPostProcessing");
        if (prop != null) prop.boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(camData);
    }

    static void ApplyLitMaterialToSprites(Scene scene)
    {
        const string litMatPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
        var litMat = AssetDatabase.LoadAssetAtPath<Material>(litMatPath);
        if (litMat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader == null)
            {
                Debug.LogWarning("[Memory04Builder] Sprite-Lit-Default shader não encontrado.");
                return;
            }
            litMat = new Material(shader);
        }
        int touched = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (sr.sharedMaterial != null && sr.sharedMaterial != litMat &&
                    sr.sharedMaterial.shader != null &&
                    sr.sharedMaterial.shader.name != "Sprites/Default") continue;
                sr.sharedMaterial = litMat;
                touched++;
            }
        }
        Debug.Log($"[Memory04Builder] Sprite-Lit-Default aplicado em {touched} SpriteRenderer(s).");
    }

    // ---------- Background ----------

    static void BuildMarketBackground(GameObject camGO)
    {
        var bgRoot = new GameObject("BG_Market");
        bgRoot.transform.SetParent(camGO.transform, false);
        bgRoot.transform.localPosition = new Vector3(0, 0f, 10f);

        for (int i = 0; i < SceneArtCatalog.MarketBgNightLayers.Length; i++)
        {
            var path = SceneArtCatalog.MarketBgNightLayers[i];
            var sprite = SceneArtCatalog.LoadSprite(path);
            var go = new GameObject($"Layer_{i}");
            go.transform.SetParent(bgRoot.transform, false);
            go.transform.localPosition = new Vector3(0, 0, i * 0.01f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100 + i;

            if (i <= 2)
            {
                var px = go.AddComponent<ParallaxLayer>();
                px.swayAmplitude = 0.05f - i * 0.012f;
                px.swaySpeed = 0.20f + i * 0.05f;
                px.phase = i * 0.7f;
            }
        }
    }

    // ---------- Floor (solid quads — span x=-5..70, sem gaps) ----------

    static void BuildMarketFloor()
    {
        var grid = new GameObject("Grid");

        // Span total x=-15..70 (85u), 1 segmento contínuo. Top y=-3 (convenção M02/M03).
        // Estendido 10u à esquerda do spawn pra cobrir todo o frustum quando a câmera
        // bate em minX=-3 (ortho 5 em 16:9 → meio-frustum ~8.88u, vê até x≈-11.88).
        // Top y=-3 ↔ collider top: cy=-3.25 com h=0.5 → top y=-3.0.
        BuildFloorSegment(grid.transform, "Floor_Top", 27.5f, -3.25f, 85f, 0.5f, FloorTopCol, sortingOrder: 5);

        // Base profunda (visual, sem collider).
        BuildSolidQuad(grid.transform, "Floor_Base", 27.5f, -5.5f, 85f, 4f, FloorBaseCol, sortingOrder: 4);

        // Paredes laterais invisíveis — bloqueiam o player.
        var wallLeft = BuildPlatform(grid.transform, "Wall_Left", -15f, -1f, 0.4f, 4f, WallCol, 5);
        var wallLeftSr = wallLeft.GetComponent<SpriteRenderer>(); if (wallLeftSr != null) wallLeftSr.enabled = false;
        var wallRight = BuildPlatform(grid.transform, "Wall_Right", 70.5f, -1f, 0.4f, 4f, WallCol, 5);
        var wallRightSr = wallRight.GetComponent<SpriteRenderer>(); if (wallRightSr != null) wallRightSr.enabled = false;
    }

    static GameObject BuildFloorSegment(Transform parent, string name, float cx, float cy, float w, float h, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(cx, cy, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;
        var tileSprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.MarketTileGround);
        if (tileSprite != null)
        {
            sr.sprite = tileSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(w, h);
        }
        else
        {
            sr.sprite = SolidSprite();
            sr.color = color;
            go.transform.localScale = new Vector3(w, h, 1f);
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
        return go;
    }

    static GameObject BuildSolidQuad(Transform parent, string name, float cx, float cy, float w, float h, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(cx, cy, 0);
        go.transform.localScale = new Vector3(w, h, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    // ---------- Decor ----------

    static void BuildDecor()
    {
        var root = new GameObject("Decor");
        // Lamps, signboards, terrace espalhados pra dar identidade cyberpunk-market.
        AddProp(root.transform, "Lamp_1",   SceneArtCatalog.MarketLamp1, new Vector3(-2f,   -3f, 0), 4);
        AddProp(root.transform, "Sign_1",   SceneArtCatalog.MarketSign1, new Vector3(10f,   -3f, 0), 4);
        AddProp(root.transform, "Lamp_2",   SceneArtCatalog.MarketLamp2, new Vector3(16f,   -3f, 0), 4);
        AddProp(root.transform, "Terrace_1",SceneArtCatalog.MarketTerrace1, new Vector3(26f, -3f, 0), 4);
        AddProp(root.transform, "Sign_4",   SceneArtCatalog.MarketSign4, new Vector3(36f,   -3f, 0), 4);
        AddProp(root.transform, "Lamp_3",   SceneArtCatalog.MarketLamp3, new Vector3(47f,   -3f, 0), 4);
        AddProp(root.transform, "Terrace_3",SceneArtCatalog.MarketTerrace3, new Vector3(60f, -3f, 0), 4);
        AddProp(root.transform, "Sign_7",   SceneArtCatalog.MarketSign7, new Vector3(65f,   -3f, 0), 4);
    }

    static void AddProp(Transform parent, string name, string spritePath, Vector3 worldPos, int sortingOrder)
    {
        var sprite = SceneArtCatalog.LoadSprite(spritePath);
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        // `worldPos.y` é o chão (-3). Sprites com pivot Center precisam subir
        // pra base sentar no chão; sprites com pivot BottomCenter já estão certos.
        if (sprite != null)
            go.transform.position = new Vector3(worldPos.x, worldPos.y + GroundSitOffsetY(sprite), worldPos.z);
        else
            go.transform.position = worldPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }

    // Retorna o offset y (em unidades de mundo) que faz a BASE de um sprite
    // sentar no y do parent — compensa pivot.
    //   BottomCenter (pivot.y=0)   → offset 0 (já está certo)
    //   Center       (pivot.y=h/2) → offset = bounds.extents.y
    // Funciona pra qualquer pivot intermediário (TopCenter retorna offset negativo).
    static float GroundSitOffsetY(Sprite sprite)
    {
        if (sprite == null) return 0f;
        var rect = sprite.rect;
        var pivot = sprite.pivot;
        float ppu = sprite.pixelsPerUnit;
        // pivot.y em pixels → unidade de mundo.
        return pivot.y / ppu;
    }

    // ---------- Hazards (vazio — fase sem pit, layout flat) ----------

    static void BuildHazardsRoot()
    {
        new GameObject("Hazards");
    }

    // ===========================================================================
    // PUZZLE 1 — "A entrada do mercado" (x=5..15) — caixa de carga + plate (HeavyBox)
    // Adult empurra uma caixa de carga até a placa → Gate_Entry abre.
    // (Era "coin" empurrando uma pilha de dinheiro — ficou estranho visualmente,
    // troquei pra crate normal do basement pack. Tema "catraca" abandonado.)
    // Pista do cofre: número "1" VERMELHO em x=4 (1ª cor da senha).
    // ===========================================================================
    static void BuildP1_Entrance(Transform parent)
    {
        var p1 = new GameObject("P1_Entrance");
        p1.transform.SetParent(parent, false);

        // Crate em x=8 (basement box, mesmo sprite do P3 mas em escala menor).
        BuildBasementHeavyBox(p1.transform, "P1_Crate", new Vector3(8f, -2.3f, 0));

        // Placa em x=12 (HeavyBox-only).
        var plate = BuildPlate(p1.transform, "P1_Plate_Crate",
            new Vector3(12f, -2.8f, 0), PressurePlate.Requirement.HeavyBox);

        // Gate latched em x=14.
        var gate = BuildGate(p1.transform, "P1_Gate_Entry", new Vector3(14f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(plate);

        // Pista 1: "1" VERMELHO em x=4.
        BuildClue(p1.transform, "P1_Clue", new Vector3(4f, 1.0f, 0), "1", LockRed);
    }

    // ===========================================================================
    // PUZZLE 2 — Snake arcade (x=18..23)
    // Cabine em x=20: ArcadeMachine trigger → Espaço abre SnakeMinigame em overlay.
    // Win = coletar 10 frutas → cabine vira GateSource → Gate_Snake em x=23 abre.
    // ===========================================================================
    static ArcadeMachine BuildP2_SnakeArcade(Transform arcadesRoot, Transform puzzlesRoot,
        GameObject young, GameObject adult, GameObject swap)
    {
        var cabin = BuildArcadeCabin(arcadesRoot, "P2_SnakeArcade", new Vector3(20f, -3f, 0),
            SceneArtCatalog.BusinessVending1, npcSprite: null);

        // Canvas filho com overlay do Snake (começa inativo).
        var overlay = BuildSnakeOverlay(cabin, young, adult, swap);

        // BoxCollider2D PRECISA vir antes do ArcadeMachine — [RequireComponent(typeof(Collider2D))]
        // não auto-adiciona porque Collider2D é abstrato. Sem isso, AddComponent<ArcadeMachine>
        // retorna null e o `arcade.overlay = ...` joga NRE.
        var col = cabin.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.8f, 2.6f);
        col.offset = new Vector2(0f, 1.3f);

        var arcade = cabin.AddComponent<ArcadeMachine>();
        arcade.overlay = overlay;
        arcade.promptIndicator = BuildPromptIndicator(cabin.transform, "[ESPACO]", new Vector3(0f, 2.7f, 0));

        // Gate latched depois da cabine.
        var gate = BuildGate(puzzlesRoot, "P2_Gate_Snake", new Vector3(23f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(arcade);

        return arcade;
    }

    // ===========================================================================
    // PUZZLE 3 — Errand das comidas (x=25..35)
    // 3 FoodPickup espalhados em plataformas. Adult empurra HeavyBox como degrau,
    // Young sobe e coleta. VendorStall em x=33 exige 3 burgers → Gate_Vendor x=35.
    // Pista 2: "2" VERDE em x=33.
    // ===========================================================================
    static void BuildP3_FoodErrand(Transform parent)
    {
        var p3 = new GameObject("P3_FoodErrand");
        p3.transform.SetParent(parent, false);

        // 3 plataformas elevadas pros burgers — alturas pequenas pra Young pular sentando em cima da caixa.
        BuildPlatform(p3.transform, "P3_Pf_A", cx: 26f, cy: 0.3f, w: 1.5f, h: 0.3f, PlatformCol, 5);
        BuildPlatform(p3.transform, "P3_Pf_B", cx: 29f, cy: 1.0f, w: 1.5f, h: 0.3f, PlatformCol, 5);
        BuildPlatform(p3.transform, "P3_Pf_C", cx: 32f, cy: 0.5f, w: 1.5f, h: 0.3f, PlatformCol, 5);

        // HeavyBox em x=24.5 (Adult empurra).
        BuildBasementHeavyBox(p3.transform, "P3_Heavy", new Vector3(24.5f, -2.3f, 0));

        // 3 BurgerPickups em cima das plataformas.
        BuildFoodPickup(p3.transform, "P3_Food_A", new Vector3(26f,  1.0f, 0), FoodKind.Burger);
        BuildFoodPickup(p3.transform, "P3_Food_B", new Vector3(29f,  1.7f, 0), FoodKind.Burger);
        BuildFoodPickup(p3.transform, "P3_Food_C", new Vector3(32f,  1.2f, 0), FoodKind.Burger);

        // VendorStall em x=33 — foodtruck do business-center pack.
        var stall = BuildVendorStall(p3.transform, "P3_Vendor", new Vector3(33f, -2f, 0));
        // Precisa de 3 burgers.
        stall.required.Clear();
        stall.required.Add(new VendorStall.Need { kind = FoodKind.Burger, count = 3 });

        // Gate latched em x=35.
        var gate = BuildGate(p3.transform, "P3_Gate_Vendor", new Vector3(35f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(stall);

        // Pista 2: "2" VERDE em x=33 (em cima do foodtruck).
        BuildClue(p3.transform, "P3_Clue", new Vector3(33f, 1.5f, 0), "2", LockGreen);
    }

    // ===========================================================================
    // PUZZLE 4 — Guitar Hero arcade (x=38..45)
    // Cabine em x=42 com NPC guitarrista decorativo. ArcadeMachine → GuitarHeroMinigame.
    // Pista 3: "3" AZUL em x=44.
    // ===========================================================================
    static ArcadeMachine BuildP4_GuitarHeroArcade(Transform arcadesRoot, Transform puzzlesRoot,
        GameObject young, GameObject adult, GameObject swap)
    {
        var cabin = BuildArcadeCabin(arcadesRoot, "P4_GuitarHeroArcade", new Vector3(42f, -3f, 0),
            SceneArtCatalog.BusinessVending6, npcSprite: SceneArtCatalog.MarketGuitarNpcPlay);

        var overlay = BuildGuitarHeroOverlay(cabin, young, adult, swap);

        // Mesmo cuidado de P2 — Collider2D abstrato impede auto-add via RequireComponent.
        var col = cabin.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.2f, 2.6f);
        col.offset = new Vector2(0f, 1.3f);

        var arcade = cabin.AddComponent<ArcadeMachine>();
        arcade.overlay = overlay;
        arcade.promptIndicator = BuildPromptIndicator(cabin.transform, "[ESPACO]", new Vector3(0f, 2.7f, 0));

        var gate = BuildGate(puzzlesRoot, "P4_Gate_GH", new Vector3(45f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(arcade);

        BuildClue(puzzlesRoot, "P4_Clue", new Vector3(44f, 1.0f, 0), "3", LockBlue);
        return arcade;
    }

    // ===========================================================================
    // PUZZLE 5 — Senha das vending machines (x=48..58)
    // 3 StoneSwitch coloridos atrás de vending machines (vermelho x=50, verde x=53,
    // azul x=56). SequenceLock → ordem VERMELHO → VERDE → AZUL (pistas P1/P3/P4).
    // ===========================================================================
    static void BuildP5_VendingSequence(Transform parent)
    {
        var p5 = new GameObject("P5_VendingSequence");
        p5.transform.SetParent(parent, false);

        // Vending machines decorativas como visual atrás de cada switch.
        AddProp(p5.transform, "P5_Vending_Red",   SceneArtCatalog.BusinessVending2, new Vector3(50f, -3f, 0), 4);
        AddProp(p5.transform, "P5_Vending_Green", SceneArtCatalog.BusinessVending3, new Vector3(53f, -3f, 0), 4);
        AddProp(p5.transform, "P5_Vending_Blue",  SceneArtCatalog.BusinessVending4, new Vector3(56f, -3f, 0), 4);

        // 3 switches coloridos non-latched (SequenceLock reseta em erro). y=-1 — Young
        // dá hit fácil pulando do chão.
        var swRed = BuildStoneSwitchColored(p5.transform, "P5_Switch_Red",
            new Vector3(50f, -1f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockRedOff, onColor: LockRed);
        SwitchIconHelper.Attach(swRed, SwitchIconHelper.Color.Red);
        var swGreen = BuildStoneSwitchColored(p5.transform, "P5_Switch_Green",
            new Vector3(53f, -1f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockGreenOff, onColor: LockGreen);
        SwitchIconHelper.Attach(swGreen, SwitchIconHelper.Color.Green);
        var swBlue = BuildStoneSwitchColored(p5.transform, "P5_Switch_Blue",
            new Vector3(56f, -1f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockBlueOff, onColor: LockBlue);
        SwitchIconHelper.Attach(swBlue, SwitchIconHelper.Color.Blue);

        // SequenceLock — switches[0]=red, [1]=green, [2]=blue.
        // ExpectedOrder: 0,1,2 = RED, GREEN, BLUE (pistas: 1=vermelho, 2=verde, 3=azul).
        var seqGO = new GameObject("P5_SequenceLock");
        seqGO.transform.SetParent(p5.transform, false);
        var seqLock = seqGO.AddComponent<SequenceLock>();
        seqLock.switches.Add(swRed);
        seqLock.switches.Add(swGreen);
        seqLock.switches.Add(swBlue);
        seqLock.expectedOrder.Add(0);
        seqLock.expectedOrder.Add(1);
        seqLock.expectedOrder.Add(2);

        BuildPasswordHint(p5.transform, new Vector3(53f, 2.5f, 0), "SENHA - ORDEM CERTA");

        var gateLock = BuildGate(p5.transform, "P5_Gate_Lock", new Vector3(58f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gateLock.GetComponent<GatedDoor>().sources.Add(seqLock);
    }

    // ===========================================================================
    // P6 — Saída (x=60..70): ReturnPad em x=63 (volta pro spawn pra revisitar
    // pistas), CoopFinishDoor em x=68 → Memory_04_Cutscene_Placeholder.
    // ===========================================================================
    static void BuildP6_ReturnPad(Transform parent, GameObject young, GameObject adult)
    {
        var p6 = new GameObject("P6_Exit");
        p6.transform.SetParent(parent, false);

        var pad = new GameObject("P6_ReturnPad");
        pad.transform.SetParent(p6.transform, false);
        pad.transform.position = new Vector3(63f, -2.8f, 0);

        var col = pad.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 0.4f);
        col.isTrigger = true;

        var visual = new GameObject("PadVisual");
        visual.transform.SetParent(pad.transform, false);
        visual.transform.localScale = new Vector3(1.2f, 0.2f, 1f);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = new Color(0.4f, 0.85f, 0.95f, 0.8f);
        sr.sortingOrder = 6;

        var returnPad = pad.AddComponent<ReturnPad>();
        returnPad.teleportOnlyActive = false;
        if (young != null)
        {
            returnPad.young = young.transform;
            returnPad.youngSpawn = new Vector3(-3f, -2f, 0);
        }
        if (adult != null)
        {
            returnPad.adult = adult.transform;
            returnPad.adultSpawn = new Vector3(-1.5f, -2f, 0);
        }
    }

    // ---------- Arcade cabin (chassi visual + collider trigger via ArcadeMachine) ----------

    static GameObject BuildArcadeCabin(Transform parent, string name, Vector3 floorPos,
        string vendingSpritePath, string npcSprite)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = floorPos;

        var cabinVisual = new GameObject("CabinVisual");
        cabinVisual.transform.SetParent(root.transform, false);
        var sr = cabinVisual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;
        var vending = SceneArtCatalog.LoadSprite(vendingSpritePath);
        if (vending != null)
        {
            sr.sprite = vending;
            // Pivot-aware: assenta a base do sprite no y do parent (floorPos.y).
            cabinVisual.transform.localPosition = new Vector3(0f, GroundSitOffsetY(vending), 0f);
        }
        else
        {
            sr.sprite = SolidSprite();
            sr.color = new Color(0.30f, 0.20f, 0.45f, 1f);
            cabinVisual.transform.localScale = new Vector3(1.5f, 2.5f, 1f);
            cabinVisual.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        }

        if (!string.IsNullOrEmpty(npcSprite))
        {
            var npc = new GameObject("NPC");
            npc.transform.SetParent(root.transform, false);
            var nsr = npc.AddComponent<SpriteRenderer>();
            // Tenta carregar frames sliciados (Multiple mode); se houver, anima.
            var frames = SceneArtCatalog.LoadSpriteFrames(npcSprite);
            Sprite first = (frames != null && frames.Length > 0) ? frames[0] : SceneArtCatalog.LoadSprite(npcSprite);
            if (first != null)
            {
                nsr.sprite = first;
                npc.transform.localPosition = new Vector3(1.4f, GroundSitOffsetY(first), 0f);
            }
            else { nsr.sprite = SolidSprite(); nsr.color = new Color(0.85f, 0.6f, 0.4f, 1f); npc.transform.localScale = new Vector3(0.6f, 1.2f, 1f); npc.transform.localPosition = new Vector3(1.4f, 0.6f, 0f); }
            nsr.sortingOrder = 6;

            if (frames != null && frames.Length > 1)
            {
                var anim = npc.AddComponent<SimpleSpriteAnimator>();
                anim.idleSprites = frames;
                anim.walkSprites = frames;
                anim.jumpSprites = frames;
                anim.idleFps = 8f;
            }
        }

        return root;
    }

    static GameObject BuildPromptIndicator(Transform parent, string text, Vector3 localPos)
    {
        var go = new GameObject("Prompt");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = CreamCol;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 2.2f;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 1f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 10;

        go.AddComponent<BlinkUI>();
        return go;
    }

    // ---------- Snake overlay ----------

    static MinigameOverlay BuildSnakeOverlay(GameObject cabinRoot, GameObject young, GameObject adult, GameObject swap)
    {
        var canvasGO = new GameObject("SnakeCanvas");
        canvasGO.transform.SetParent(cabinRoot.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.08f, 0.96f);

        var title = CreateUIText(panel.transform, "Title", "COBRINHA - COMA 10",
            56f, FontStyles.Bold, new Vector2(0, 380), new Vector2(1600, 80), CreamCol);

        var grid = new GameObject("GridArea", typeof(RectTransform), typeof(Image));
        grid.transform.SetParent(panel.transform, false);
        var grt = grid.GetComponent<RectTransform>();
        grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f);
        grt.pivot = new Vector2(0.5f, 0.5f);
        grt.anchoredPosition = new Vector2(0, 0);
        grt.sizeDelta = new Vector2(960, 720);
        grid.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.12f, 1f);

        var score = CreateUIText(panel.transform, "Score", "0 / 10",
            48f, FontStyles.Bold, new Vector2(-700, 380), new Vector2(400, 60),
            new Color(0.7f, 1f, 0.7f, 1f), TextAlignmentOptions.Left);
        var status = CreateUIText(panel.transform, "Status", "Setas ou WASD pra mover. Esc pra desistir.",
            36f, FontStyles.Italic, new Vector2(0, -420), new Vector2(1600, 60),
            new Color(1f, 0.94f, 0.78f, 0.9f));

        var snakeTutorial = BuildTutorialPanel(canvasGO.transform, "COBRINHA", new[]
        {
            "Coma 10 frutas pra ganhar.",
            "Setas ou WASD = mover a cobra.",
            "Esc = desistir.",
        });

        var snake = canvasGO.AddComponent<SnakeMinigame>();
        snake.tutorialPanel = snakeTutorial;
        snake.canvas = canvas;
        snake.panel = panel;
        snake.gridArea = grt;
        snake.cols = 16; snake.rows = 12;
        snake.moveInterval = 0.16f;
        snake.targetScore = 10;
        snake.scoreText = score.GetComponent<TMP_Text>();
        snake.statusText = status.GetComponent<TMP_Text>();
        if (young != null) snake.young = young.GetComponent<PlayerController>();
        if (adult != null) snake.adult = adult.GetComponent<PlayerController>();
        if (swap != null) snake.playerSwap = swap.GetComponent<PlayerSwap>();

        foreach (var path in SceneArtCatalog.FoodSnakePool)
        {
            var sp = SceneArtCatalog.LoadSprite(path);
            if (sp != null) snake.foodSprites.Add(sp);
        }

        canvasGO.SetActive(false);
        return snake;
    }

    // ---------- Guitar Hero overlay ----------

    static MinigameOverlay BuildGuitarHeroOverlay(GameObject cabinRoot, GameObject young, GameObject adult, GameObject swap)
    {
        var canvasGO = new GameObject("GuitarHeroCanvas");
        canvasGO.transform.SetParent(cabinRoot.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.03f, 0.02f, 0.08f, 0.96f);

        CreateUIText(panel.transform, "Title", "GUITARRA - WOODY & WOODY",
            56f, FontStyles.Bold, new Vector2(0, 400), new Vector2(1600, 80), CreamCol);

        // Lane Young (esquerda), Lane Adult (direita). Cada uma 360×720.
        var laneYoung = BuildLane(panel.transform, "LaneYoung", new Vector2(-320, -20),
            new Color(1f, 0.85f, 0.25f, 0.10f));
        var laneAdult = BuildLane(panel.transform, "LaneAdult", new Vector2( 320, -20),
            new Color(0.7f, 0.45f, 0.25f, 0.10f));

        var hitYoung = BuildHitZone(laneYoung, new Color(1f, 0.85f, 0.25f, 0.4f));
        var hitAdult = BuildHitZone(laneAdult, new Color(0.7f, 0.45f, 0.25f, 0.4f));

        // Marcadores de coluna (A/S/D) abaixo de cada hit zone.
        BuildColumnLabels(laneYoung);
        BuildColumnLabels(laneAdult);

        var overlayYoung = BuildLaneOverlay(laneYoung);
        var overlayAdult = BuildLaneOverlay(laneAdult);

        // Labels acima das lanes.
        CreateUIText(panel.transform, "LabelY", "JOVEM",
            36f, FontStyles.Bold, new Vector2(-320, 320), new Vector2(360, 60),
            new Color(1f, 0.85f, 0.25f, 1f));
        CreateUIText(panel.transform, "LabelA", "ADULTO",
            36f, FontStyles.Bold, new Vector2( 320, 320), new Vector2(360, 60),
            new Color(0.95f, 0.7f, 0.45f, 1f));

        var score = CreateUIText(panel.transform, "Score", "0 / 0",
            48f, FontStyles.Bold, new Vector2(-700, 400), new Vector2(400, 60),
            new Color(0.7f, 1f, 0.7f, 1f), TextAlignmentOptions.Left);
        var status = CreateUIText(panel.transform, "Status", "A/S/D ou setas pra acertar. Tab troca guitarra. Esc desiste.",
            32f, FontStyles.Italic, new Vector2(0, -440), new Vector2(1600, 60),
            new Color(1f, 0.94f, 0.78f, 0.9f));

        var gh = canvasGO.AddComponent<GuitarHeroMinigame>();
        gh.canvas = canvas;
        gh.panel = panel;
        gh.laneYoungArea = laneYoung;
        gh.laneAdultArea = laneAdult;
        gh.hitZoneYoung = hitYoung;
        gh.hitZoneAdult = hitAdult;
        gh.laneYoungOverlay = overlayYoung;
        gh.laneAdultOverlay = overlayAdult;
        gh.scoreText = score.GetComponent<TMP_Text>();
        gh.statusText = status.GetComponent<TMP_Text>();
        gh.noteFallTime = 3.0f;
        gh.hitWindow = 0.35f;
        gh.passThreshold = 0.4f;
        gh.inactiveOverlayColor = new Color(0f, 0f, 0f, 0.45f);
        if (young != null) gh.young = young.GetComponent<PlayerController>();
        if (adult != null) gh.adult = adult.GetComponent<PlayerController>();
        if (swap != null) gh.playerSwap = swap.GetComponent<PlayerSwap>();

        var beatMap = AssetDatabase.LoadAssetAtPath<BeatMap>(BeatMapPath);
        if (beatMap == null)
            Debug.LogWarning($"[Memory04Builder] BeatMap não encontrado em {BeatMapPath} — rode 'Retroself → Build BeatMap Placeholder' antes.");
        gh.beatMap = beatMap;

        var song = AssetDatabase.LoadAssetAtPath<AudioClip>(GuitarHeroSongPath);
        if (song == null)
            Debug.LogWarning($"[Memory04Builder] AudioClip não encontrado em {GuitarHeroSongPath} — placeholder metronômico será usado.");
        gh.song = song;

        var ghTutorial = BuildTutorialPanel(canvasGO.transform, "GUITAR HERO CO-OP", new[]
        {
            "Cada guitarra tem 3 colunas de hit.",
            "A / Seta-esquerda = coluna 1",
            "S / Seta-baixo    = coluna 2",
            "D / Seta-direita  = coluna 3",
            "Tab troca de guitarra (so a ativa registra hit).",
            "Esc = desistir.",
        });
        gh.tutorialPanel = ghTutorial;

        canvasGO.SetActive(false);
        return gh;
    }

    // ---------- Tutorial panel comum ----------

    static GameObject BuildTutorialPanel(Transform canvasParent, string title, string[] lines)
    {
        var tut = new GameObject("Tutorial", typeof(RectTransform), typeof(Image));
        tut.transform.SetParent(canvasParent, false);
        var rt = tut.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        tut.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.06f, 0.98f);

        CreateUIText(tut.transform, "TutTitle", title,
            72f, FontStyles.Bold, new Vector2(0, 350), new Vector2(1600, 100), CreamCol);

        var body = string.Join("\n\n", lines);
        CreateUIText(tut.transform, "TutBody", body,
            36f, FontStyles.Normal, new Vector2(0, 0), new Vector2(1400, 600),
            new Color(1f, 0.94f, 0.78f, 1f));

        CreateUIText(tut.transform, "TutHint", "Pressione [Enter] pra comecar",
            42f, FontStyles.Italic, new Vector2(0, -400), new Vector2(1600, 80),
            new Color(0.8f, 1f, 0.8f, 1f));

        tut.SetActive(false);
        return tut;
    }

    static RectTransform BuildLane(Transform parent, string name, Vector2 anchored, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = new Vector2(360, 720);
        go.GetComponent<Image>().color = bgColor;
        return rt;
    }

    static RectTransform BuildHitZone(RectTransform lane, Color color)
    {
        var go = new GameObject("HitZone", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(lane, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(340, 60);
        go.GetComponent<Image>().color = color;
        return rt;
    }

    static void BuildColumnLabels(RectTransform lane)
    {
        // Divide a lane em 3 colunas visualmente: 2 separadores verticais finos
        // e 3 labels de tecla logo abaixo da hit zone.
        float w = lane.sizeDelta.x; // 360
        float colW = w / 3f;        // 120

        // Separadores (em x = -w/6 e +w/6, ou seja borda entre col 0-1 e 1-2).
        for (int s = 0; s < 2; s++)
        {
            var div = new GameObject($"ColDiv{s}", typeof(RectTransform), typeof(Image));
            div.transform.SetParent(lane, false);
            var drt = div.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = new Vector2((s == 0 ? -1f : 1f) * colW * 0.5f, 0);
            drt.sizeDelta = new Vector2(2f, 720f);
            div.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);
            div.GetComponent<Image>().raycastTarget = false;
        }

        // Labels logo abaixo da hit zone (hit zone fica em y=20 com altura 60,
        // então centro y≈50 dentro da lane; abaixo dela y≈-5).
        string[] labels = { "A / <-", "S / v", "D / ->" };
        for (int c = 0; c < 3; c++)
        {
            float cx = (c - 1) * colW;
            var lbl = new GameObject($"ColLabel{c}", typeof(RectTransform));
            lbl.transform.SetParent(lane, false);
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 0f);
            lrt.anchorMax = new Vector2(0.5f, 0f);
            lrt.pivot = new Vector2(0.5f, 1f);
            lrt.anchoredPosition = new Vector2(cx, -10f);
            lrt.sizeDelta = new Vector2(colW, 36f);
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text = labels[c];
            tmp.fontSize = 24f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.94f, 0.78f, 0.9f);
            tmp.raycastTarget = false;
            var pixelFont = SceneArtCatalog.GetPixelFont();
            if (pixelFont != null) tmp.font = pixelFont;
        }
    }

    static Image BuildLaneOverlay(RectTransform lane)
    {
        var go = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(lane, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.55f);
        img.raycastTarget = false;
        return img;
    }

    // ---------- VendorStall (foodtruck) ----------

    static VendorStall BuildVendorStall(Transform parent, string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        var visual = new GameObject("VendorVisual");
        visual.transform.SetParent(go.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        var truck = SceneArtCatalog.LoadSprite(SceneArtCatalog.MarketFoodtruck);
        if (truck != null) sr.sprite = truck;
        else { sr.sprite = SolidSprite(); sr.color = new Color(0.85f, 0.55f, 0.30f, 1f); visual.transform.localScale = new Vector3(2.2f, 1.4f, 1f); visual.transform.localPosition = new Vector3(0f, 0.7f, 0f); }
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 1.4f);
        col.offset = new Vector2(0f, 0.7f);
        col.isTrigger = true;

        var stall = go.AddComponent<VendorStall>();
        stall.satisfiedSpriteHost = sr;
        stall.waitingSprite = truck;
        stall.satisfiedSprite = truck;
        return stall;
    }

    // ---------- FoodPickup (icon flutuante) ----------

    static GameObject BuildFoodPickup(Transform parent, string name, Vector3 worldPos, FoodKind kind)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        var sr = go.AddComponent<SpriteRenderer>();
        var iconPath = kind switch
        {
            FoodKind.Burger  => SceneArtCatalog.FoodIconBurger,
            FoodKind.Sushi   => SceneArtCatalog.FoodIconSushi,
            FoodKind.Noodle  => SceneArtCatalog.FoodIconNoodle,
            FoodKind.Drink   => SceneArtCatalog.FoodIconDrink,
            FoodKind.Dessert => SceneArtCatalog.FoodIconDessert,
            _ => SceneArtCatalog.FoodIconBurger,
        };
        var sprite = SceneArtCatalog.LoadSprite(iconPath);
        if (sprite != null) sr.sprite = sprite;
        else { sr.sprite = SolidSprite(); sr.color = new Color(1f, 0.7f, 0.3f, 1f); go.transform.localScale = new Vector3(0.7f, 0.7f, 1f); }
        sr.sortingOrder = 7;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var pickup = go.AddComponent<FoodPickup>();
        pickup.kind = kind;

        var bob = go.AddComponent<IdleBob>();
        bob.amplitude = 0.12f;
        bob.speed = 1.8f;
        return go;
    }

    // ---------- HeavyBox (basement-style pra puzzle 3) ----------

    static GameObject BuildBasementHeavyBox(Transform parent, string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        var visual = new GameObject("BoxVisual");
        visual.transform.SetParent(go.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        var crateSprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.BasementBox);
        if (crateSprite != null)
        {
            // BasementBox é 32×32 PPU 32 → 1×1u, pivot BottomCenter. Sprite cresce
            // pra cima a partir do pivot — offset y=-0.7 (metade da altura×scale) pra
            // centro do sprite coincidir com centro do BoxCollider.
            sr.sprite = crateSprite;
            visual.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            visual.transform.localPosition = new Vector3(0f, -0.7f, 0f);
        }
        else
        {
            sr.sprite = SolidSprite();
            sr.color = new Color(0.55f, 0.36f, 0.22f, 1f);
            visual.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        }
        sr.sortingOrder = 7;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.4f, 1.4f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        go.AddComponent<HeavyBox>();
        return go;
    }

    // ---------- Helpers comuns (plate/gate/switch/clue) ----------

    static GameObject BuildPlatform(Transform parent, string name, float cx, float cy, float w, float h, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(cx, cy, 0);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go;
    }

    static PressurePlate BuildPlate(Transform parent, string name, Vector3 worldPos, PressurePlate.Requirement req)
    {
        const float W = 1.5f;
        const float H = 0.4f;

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one;

        BuildPlateLayer(go.transform, "Shadow", new Vector3(0f, -0.05f, 0f), new Vector3(W + 0.1f, H, 1f),
            new Color(0.05f, 0.04f, 0.08f, 1f), sortingOrder: 5);
        var bodySr = BuildPlateLayer(go.transform, "Body", Vector3.zero, new Vector3(W, H, 1f),
            PlateOffCol, sortingOrder: 6);
        BuildPlateLayer(go.transform, "Rim", new Vector3(0f, H * 0.4f, 0f), new Vector3(W - 0.15f, 0.06f, 1f),
            new Color(0.9f, 0.7f, 0.95f, 0.55f), sortingOrder: 7);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(W, H);

        var plate = go.AddComponent<PressurePlate>();
        plate.requirement = req;

        var colorizer = go.AddComponent<PressurePlateVisual>();
        colorizer.target = plate;
        colorizer.renderer = bodySr;
        colorizer.offColor = PlateOffCol;
        colorizer.onColor = PlateOnCol;
        return plate;
    }

    static SpriteRenderer BuildPlateLayer(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    static GameObject BuildGate(Transform parent, string name, Vector3 worldPos, Vector2 size, Vector2 openOffset, bool latched)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one;

        var visual = new GameObject("GateVisual");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = GateCol;
        sr.sortingOrder = 7;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x, size.y);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var gate = go.AddComponent<GatedDoor>();
        gate.openOffset = openOffset;
        gate.latched = latched;
        gate.moveSpeed = 10f;
        return go;
    }

    static StoneSwitch BuildStoneSwitch(Transform parent, string name, Vector3 worldPos, Vector2 size, bool latched)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = PlateOffCol;
        sr.sortingOrder = 6;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;

        var sw = go.AddComponent<StoneSwitch>();
        sw.latched = latched;

        var visual = go.AddComponent<PressurePlateVisual>();
        visual.target = sw;
        visual.renderer = sr;
        visual.offColor = PlateOffCol;
        visual.onColor = PlateOnCol;
        return sw;
    }

    static StoneSwitch BuildStoneSwitchColored(Transform parent, string name, Vector3 worldPos, Vector2 size,
        bool latched, Color offColor, Color onColor)
    {
        var sw = BuildStoneSwitch(parent, name, worldPos, size, latched);
        var sr = sw.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = offColor;
        var visual = sw.GetComponent<PressurePlateVisual>();
        if (visual != null) { visual.offColor = offColor; visual.onColor = onColor; }
        return sw;
    }

    static void BuildClue(Transform parent, string name, Vector3 worldPos, string text, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 3f;
        tmp.rectTransform.sizeDelta = new Vector2(2f, 2f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    static void BuildPasswordHint(Transform parent, Vector3 worldPos, string text)
    {
        var go = new GameObject("PasswordHint");
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = CreamCol;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 1.6f;
        tmp.rectTransform.sizeDelta = new Vector2(10f, 1.2f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    // ---------- Players ----------

    static GameObject BuildYoung(Vector3 pos) => BuildPlayerInternal("PlayerYoung", pos, PlayerKind.Young,
        colliderSize: new Vector2(0.55f, 0.85f),
        spriteFolder: "Assets/Sprites/criancas/Child_3",
        hasJump: false, groundCheckY: -0.5f);

    static GameObject BuildAdult(Vector3 pos) => BuildPlayerInternal("PlayerAdult", pos, PlayerKind.Adult,
        colliderSize: new Vector2(0.55f, 1.9f),
        spriteFolder: "Assets/Sprites/mendigos/Homeless_1",
        hasJump: true, groundCheckY: -1.05f);

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
        root.AddComponent<PlayerAttack>();

        var anim = body.AddComponent<SimpleSpriteAnimator>();
        anim.idleSprites = LoadSpriteFrames($"{spriteFolder}/Idle.png");
        anim.walkSprites = LoadSpriteFrames($"{spriteFolder}/Walk.png");
        if (hasJump) anim.jumpSprites = LoadSpriteFrames($"{spriteFolder}/Jump.png");
        anim.playerController = pc;
        return root;
    }

    static Sprite[] LoadSpriteFrames(string sheetPath)
    {
        var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
        string baseName = System.IO.Path.GetFileNameWithoutExtension(sheetPath);
        string prefix = baseName + "_";
        var list = new List<Sprite>();
        foreach (var obj in subs)
            if (obj is Sprite s && s.name.StartsWith(prefix)) list.Add(s);
        list.Sort((a, b) => SpriteIndex(a.name).CompareTo(SpriteIndex(b.name)));
        if (list.Count == 0)
            Debug.LogWarning($"[Memory04Builder] sem sub-sprites em {sheetPath} — rode Retroself → Configure Character Sprites");
        return list.ToArray();
    }

    static int SpriteIndex(string n)
    {
        int u = n.LastIndexOf('_');
        if (u < 0 || u + 1 >= n.Length) return 0;
        return int.TryParse(n.Substring(u + 1), out var i) ? i : 0;
    }

    static GameObject BuildSwap(GameObject young, GameObject adult)
    {
        var go = new GameObject("PlayerSwap");
        var sw = go.AddComponent<PlayerSwap>();
        sw.young = young.GetComponent<PlayerController>();
        sw.adult = adult.GetComponent<PlayerController>();
        return go;
    }

    // ---------- Finish door ----------

    static GameObject BuildFinishDoor(Vector3 pos)
    {
        var root = new GameObject("FinishDoor");
        root.transform.position = pos;

        var visual = new GameObject("DoorVisual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0f, -1.1f, 0f);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;
        var doorFrames = SceneArtCatalog.LoadSpriteFrames(SceneArtCatalog.DoorWoodenSheet, "1_");
        var anim = visual.AddComponent<SpriteFrameAnimator>();
        anim.frames = doorFrames;
        anim.fps = 8f;
        anim.loop = false;
        anim.autoPlay = false;
        if (doorFrames != null && doorFrames.Length > 0) sr.sprite = doorFrames[0];
        else { sr.sprite = SolidSprite(); sr.color = new Color(0.32f, 0.18f, 0.10f, 1f); visual.transform.localScale = new Vector3(1f, 2.2f, 1f); visual.transform.localPosition = new Vector3(0f, 0f, 0f); }

        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 2f);
        col.isTrigger = true;

        var doorComp = root.AddComponent<CoopFinishDoor>();
        doorComp.openAnimator = anim;

        var bob = root.AddComponent<IdleBob>();
        bob.amplitude = 0.05f;
        bob.speed = 1.5f;
        return root;
    }

    // ---------- HUD + IntroDialogue ----------

    static void BuildHUD(GameObject player, GameObject swap, GameObject young, GameObject adult, GameObject finishDoor)
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
            "[A]/[D] mover  [Espaco] pular  [K] atirar pedra (jovem)  [Tab] trocar Woody",
            35.84f, FontStyles.Bold,
            new Vector2(0, -440), new Vector2(1820, 60),
            new Color(1, 0.95f, 0.7f, 0.9f));

        CreateUIText(canvasGO.transform, "Title", "MEMORY 04 - MERCADO",
            71.68f, FontStyles.Bold,
            new Vector2(0, 460), new Vector2(1400, 100),
            CreamCol, TextAlignmentOptions.Center);

        BuildHealthBar(canvasGO.transform, player);
        BuildIntroDialogue(canvasGO.transform, swap, young, adult);

        var fadeGO = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        var fadeRt = fadeGO.AddComponent<RectTransform>();
        fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one;
        fadeRt.offsetMin = fadeRt.offsetMax = Vector2.zero;
        var fadeImg = fadeGO.AddComponent<Image>();
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        fadeImg.raycastTarget = false;
        fadeGO.transform.SetAsLastSibling();

        if (finishDoor != null)
        {
            var fd = finishDoor.GetComponent<CoopFinishDoor>();
            if (fd != null) fd.fadeOverlay = fadeImg;
        }
    }

    static void BuildIntroDialogue(Transform parent, GameObject swap, GameObject young, GameObject adult)
    {
        var box = new GameObject("IntroDialogueBox");
        box.transform.SetParent(parent, false);
        var rt = box.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(1700, 280);

        var bg = box.AddComponent<Image>();
        bg.color = Color.black;
        bg.raycastTarget = false;

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

        var labelGO = CreateUIText(box.transform, "SpeakerLabel", "Woody (adulto)", 46.08f, FontStyles.Bold,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Left);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = labelRt.anchorMax = new Vector2(0, 1);
        labelRt.pivot = new Vector2(0, 1);
        labelRt.anchoredPosition = new Vector2(250, -20);
        labelRt.sizeDelta = new Vector2(1200, 36);

        var textGO = CreateUIText(box.transform, "DialogText", "", 46.08f, FontStyles.Normal,
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

        var contGO = CreateUIText(box.transform, "Continue", ">> Espaco/Enter", 35.84f, FontStyles.Italic,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Right);
        var contRt = contGO.GetComponent<RectTransform>();
        contRt.anchorMin = contRt.anchorMax = new Vector2(1, 0);
        contRt.pivot = new Vector2(1, 0);
        contRt.anchoredPosition = new Vector2(-30, 18);
        contRt.sizeDelta = new Vector2(360, 32);
        contGO.AddComponent<BlinkUI>();

        var intro = box.AddComponent<IntroDialogue>();
        intro.line = "Quinze anos. Quando a casa virava barulho, eu sumia pra ca. " +
                     "O mercado nunca pedia explicacao - so uns trocados pelo yakisoba " +
                     "e umas fichas pros fliperamas. Aqui eu era so mais um.";
        intro.dialogueBox = box;
        intro.typewriter = typewriter;
        intro.continueIndicator = contGO;
        if (swap != null) intro.playerSwap = swap.GetComponent<PlayerSwap>();
        if (young != null) intro.young = young.GetComponent<PlayerController>();
        if (adult != null) intro.adult = adult.GetComponent<PlayerController>();
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

        CreateUIText(root.transform, "Label", "VIDA", 35.84f, FontStyles.Bold,
            new Vector2(0, 40), new Vector2(160, 32),
            new Color(1, 0.95f, 0.7f, 0.85f), TextAlignmentOptions.Left);

        var hb = root.AddComponent<HealthBarUI>();
        hb.target = player.GetComponent<PlayerHealth>();
        hb.fill = fill;
        hb.fillRect = fillRt;
    }

    // ---------- Sprite/UI helpers ----------

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

    static GameObject CreateUIText(Transform parent, string name, string text, float size, FontStyles style,
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
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = scenes.Exists(s => s.path == scenePath);
        if (!exists)
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
