#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builder de Memory_03_Floresta — fase 3, cenário de floresta, 5 puzzles co-op
// reusando ReturnPad como primitiva de portal (a fase 2 já o introduziu como o
// "pad de volta" pra ReturnPad). Aqui ReturnPads aparecem como portais
// bidirecionais pareados por configuração: o pad A teleporta os 2 Woody pra um
// ponto perto do pad B; o pad B teleporta de volta pra um ponto perto do pad A.
// Pra evitar re-trigger imediato, os spawns são offset 0.5..1u lateral do parceiro
// (cooldown 1.5s do próprio script garante o resto).
//
// IMPORTANTE: ReturnPad.Update auto-destroi o pad quando KeyPickup.Collected == true.
// Por isso, a chave é coletada SÓ no Puzzle 5 (último) — os portais de P1..P4 sempre
// rodam com Collected==false. O auto-destroy pós-chave é narrativamente OK ("os portais
// da memória somem depois que você lembrou").
public static class Memory03Builder
{
    const string ScenePath = "Assets/Scenes/Memory_03_Floresta.unity";

    static readonly string[] OwnedRoots = {
        "Main Camera", "EventSystem", "Grid", "BG_Forest",
        "Decor", "Hazards", "Puzzles",
        "PlayerYoung", "PlayerAdult", "PlayerSwap",
        "FinishDoor", "Canvas", "SceneStartReset",
    };

    // ----- Paleta floresta -----
    static readonly Color BgGreen     = new Color(0.30f, 0.50f, 0.25f, 1f);  // grama escura (fallback)
    static readonly Color CreamCol    = new Color(1f, 0.92f, 0.55f, 1f);
    static readonly Color AdultCol    = new Color(0.45f, 0.32f, 0.22f, 1f);
    static readonly Color HazardRiver = new Color(0.25f, 0.60f, 1.0f, 1f);   // riacho azul vivo (mais saturado pra ser óbvio)
    static readonly Color HazardRiverFoam = new Color(0.85f, 0.95f, 1.0f, 1f); // espuma branca-azulada (decor)
    static readonly Color HazardPit   = new Color(0.06f, 0.04f, 0.04f, 1f);  // poço escuro
    static readonly Color PlateOffCol = new Color(0.35f, 0.28f, 0.20f, 1f);
    static readonly Color PlateOnCol  = new Color(0.95f, 0.78f, 0.30f, 1f);
    static readonly Color GateCol     = new Color(0.45f, 0.35f, 0.22f, 1f);  // marrom escuro (galho)
    static readonly Color PlatformCol = new Color(0.35f, 0.55f, 0.25f, 1f);  // grama clara (topo chão + plataformas)
    // Paleta de terra estratificada — tons earth reais (umber/marrom), do mais claro
    // logo abaixo da grama ao mais escuro no fundo. Cada camada é uma faixa horizontal
    // sólida; a transição entre faixas dá efeito visual de estratos geológicos.
    static readonly Color DirtRootCol    = new Color(0.22f, 0.14f, 0.08f, 1f);   // dark band logo abaixo da grama (raízes/solo compactado)
    static readonly Color DirtSurfaceCol = new Color(0.48f, 0.34f, 0.20f, 1f);   // umber quente (terra de superfície)
    static readonly Color DirtPebbleCol  = new Color(0.40f, 0.30f, 0.22f, 1f);   // marrom-cinza (camada com pedrinhas)
    static readonly Color DirtMidCol     = new Color(0.30f, 0.21f, 0.14f, 1f);   // marrom escuro (solo médio)
    static readonly Color DirtDeepCol    = new Color(0.16f, 0.11f, 0.07f, 1f);   // dark earth (profundo)
    static readonly Color StoneAccentCol = new Color(0.55f, 0.48f, 0.40f, 1f);   // pedra cinza-clara (decor scattered)
    static readonly Color PlankCol       = new Color(0.40f, 0.27f, 0.16f, 1f);   // tronco marrom
    static readonly Color BoxCol      = new Color(0.55f, 0.36f, 0.22f, 1f);
    static readonly Color WallCol     = new Color(0.22f, 0.18f, 0.10f, 1f);

    // ----- Portais (cor por par pra feedback visual) -----
    static readonly Color PortalAzul    = new Color(0.40f, 0.70f, 1f, 1f);    // P1 (A↔B)
    static readonly Color PortalRoxo    = new Color(0.80f, 0.40f, 1f, 1f);    // P2 (C↔D)
    static readonly Color PortalVerde   = new Color(0.40f, 0.95f, 0.50f, 1f); // P4 (G↔G')
    static readonly Color PortalVermelho= new Color(0.95f, 0.40f, 0.40f, 1f); // P4 (H↔H')
    static readonly Color PortalCiano   = new Color(0.30f, 0.85f, 0.95f, 1f); // P4 (I↔I')
    static readonly Color PortalDourado = new Color(0.95f, 0.80f, 0.30f, 1f); // P5 (J↔K)

    // ----- Cofre (cores dos switches do P4) -----
    static readonly Color LockGreen    = new Color(0.45f, 0.90f, 0.40f, 1f);
    static readonly Color LockGreenOff = new Color(0.20f, 0.40f, 0.20f, 1f);
    static readonly Color LockRed      = new Color(0.95f, 0.30f, 0.30f, 1f);
    static readonly Color LockRedOff   = new Color(0.45f, 0.18f, 0.18f, 1f);
    static readonly Color LockBlue     = new Color(0.40f, 0.65f, 0.95f, 1f);
    static readonly Color LockBlueOff  = new Color(0.18f, 0.28f, 0.45f, 1f);

    [MenuItem("Retroself/Build Memory_03_Floresta")]
    public static void BuildMemory03Forest()
    {
        var scene = SceneRebuildHelpers.OpenOrNew(ScenePath);
        if (!SceneRebuildHelpers.ConfirmRebuild(scene, OwnedRoots)) return;
        SceneRebuildHelpers.WipeOwnedRoots(scene, OwnedRoots);

        var camera = BuildCamera();
        BuildEventSystem();
        BuildForestBackground(camera);
        BuildForestFloor();
        BuildDecor();
        BuildHazardsRoot();

        var young = BuildYoung(new Vector3(-3f, -2f, 0));
        var adult = BuildAdult(new Vector3(-1.5f, -2f, 0));
        var swap = BuildSwap(young, adult);

        var puzzlesRoot = new GameObject("Puzzles");
        BuildPuzzle1_RiverCrossing(puzzlesRoot.transform, young, adult);
        BuildPuzzle2_PortalAsElevator(puzzlesRoot.transform, young, adult);
        BuildPuzzle3_HoldAndShoot(puzzlesRoot.transform);
        BuildPuzzle4_PortalCofre(puzzlesRoot.transform, young, adult);
        BuildPuzzle5_PlateElevatorPortalKey(puzzlesRoot.transform, young, adult);

        AttachCameraFollow(camera, young);

        var finishDoor = BuildFinishDoor(new Vector3(58f, -1.9f, 0));
        // -1.9 = ground top -3 + half door height 1.1 → door base no chão.
        var finishDoorComp = finishDoor.GetComponent<CoopFinishDoor>();
        if (finishDoorComp != null)
        {
            finishDoorComp.requireKey = true;
            finishDoorComp.targetScene = SceneNames.Memory_04_Sala;
            // Memory_04_Sala ainda não existe — Unity loga erro ao disparar. Aceitável até M_04 ser construído.
        }

        var resetGO = new GameObject("SceneStartReset");
        resetGO.AddComponent<SceneStartReset>();

        BuildHUD(young, swap, young, adult, finishDoor);

        // Igual M_02: pós-processamento + Light2D tunados manualmente pelo usuário,
        // sobrevivem rebuild via WipeOwnedRoots. Builder só liga os 2 pré-requisitos:
        EnableCameraPostProcessing(camera);
        ApplyLitMaterialToSprites(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory03Builder] Memory_03 montada em {ScenePath}.");
    }

    // ---------- Camera / EventSystem ----------

    static GameObject BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Fallback verde-floresta caso o BG do green-zone não carregue.
        cam.backgroundColor = new Color(0.35f, 0.55f, 0.40f, 1f);
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
        // Mapa vai de x=-3 (spawn) a x=58 (porta de fim). Bounds permitem
        // ver as ilhas altas do P4 (y=4.5) e a alcove do P5 (y=2.5).
        follow.minX = -3f;
        follow.maxX = 56f;
        follow.minY = -1f;
        follow.maxY = 5f;
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
                Debug.LogWarning("[Memory03Builder] Sprite-Lit-Default shader não encontrado — Light2D não vai afetar sprites.");
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
                    sr.sharedMaterial.shader.name != "Sprites/Default")
                    continue;
                sr.sharedMaterial = litMat;
                touched++;
            }
        }
        Debug.Log($"[Memory03Builder] Sprite-Lit-Default aplicado em {touched} SpriteRenderer(s).");
    }

    // ---------- Background (5 layers paralax green-zone Day) ----------

    static void BuildForestBackground(GameObject camGO)
    {
        var bgRoot = new GameObject("BG_Forest");
        bgRoot.transform.SetParent(camGO.transform, false);
        bgRoot.transform.localPosition = new Vector3(0, 0f, 10f);

        for (int i = 0; i < SceneArtCatalog.GreenZoneBgDayLayers.Length; i++)
        {
            var path = SceneArtCatalog.GreenZoneBgDayLayers[i];
            var sprite = SceneArtCatalog.LoadSprite(path);
            var go = new GameObject($"Layer_{i}");
            go.transform.SetParent(bgRoot.transform, false);
            go.transform.localPosition = new Vector3(0, 0, i * 0.01f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100 + i;

            // Sway leve nas 3 camadas mais distantes — dá vida ao skyline da floresta.
            if (i <= 2)
            {
                var px = go.AddComponent<ParallaxLayer>();
                px.swayAmplitude = 0.06f - i * 0.015f;
                px.swaySpeed = 0.18f + i * 0.05f;
                px.phase = i * 0.7f;
            }
        }
    }

    // ---------- Forest floor (solid quads — fallback igual M_02) ----------
    // Trocou tilemap por solid-color quads. Razão: green-zone tile sprites podem
    // ter PPU inconsistente após import via LFS (causando fragmentação visual e
    // colisão minúscula que faz player cair pelos gaps). Solid quad = colisão
    // garantida + visual coerente. Tilemap fica disponível pra usuário pintar
    // manualmente no editor sobre o solid quad se quiser detalhar.
    //
    // Convenção (M_01/M_02): topo do chão em world y=-3. Players spawnam em y=-2.
    // Hazards (riacho, poço) ficam por CIMA do chão sólido como triggers — player
    // que toca morre e respawna. Sem precisar de buracos.
    static void BuildForestFloor()
    {
        var grid = new GameObject("Grid");

        // Ground_Top = grama (sprite tile real, drawMode=Tiled).
        // Span total x=-13..60. **Gaps reais** em x=3..7 (riacho P1) e x=54..56 (poço P5)
        // — player cai pelo gap e bate no hazard. Segmentos:
        //   A: x=-13..3  (cx=-5,   w=16) — estendido pra esquerda pra não deixar vazio
        //     atrás da árvore canto-esquerdo. Wall_Left bloqueia em x=-5; tudo a oeste
        //     disso é decorativo (player não alcança).
        //   B: x=7..54   (cx=30.5, w=47)
        //   C: x=56..60  (cx=58,   w=4)
        // Altura 0.5u (y=-3.5..-3.0). Top y=-3 = ponto onde players spawnam em pé.
        BuildGrassFloor(grid.transform, "Ground_Top_A", -5f,   -3.25f, 16f, 0.5f);
        BuildGrassFloor(grid.transform, "Ground_Top_B", 30.5f, -3.25f, 47f, 0.5f);
        BuildGrassFloor(grid.transform, "Ground_Top_C", 58f,   -3.25f, 4f,  0.5f);

        // Ground_Base = 5 camadas earth-tone (solid color) com pedras decorativas.
        // Tile brick do green-zone era estranho (parecia parede); agora paleta marrom real.
        // Top y=-3.5 (alinhado com bottom da grama — SEM GAP). Bottom y=-10.5 cobre BG.
        // Progressão visual de cima pra baixo:
        //   1. Banda escura logo abaixo da grama (raízes/solo compactado)
        //   2. Terra de superfície (umber quente)
        //   3. Camada de pedrinhas (marrom-cinza com pedras decorativas espalhadas)
        //   4. Solo médio (marrom escuro)
        //   5. Profundo (dark earth)
        BuildSolidEarthLayer(grid.transform, "Ground_Dirt_Root",    23.5f, -3.6f,  73f, 0.2f, DirtRootCol,    sortingOrder: 4); // banda fina escura (raízes) — sort 4 pra não competir com hazard sort 7
        BuildSolidEarthLayer(grid.transform, "Ground_Dirt_Surface", 23.5f, -4.0f,  73f, 0.6f, DirtSurfaceCol, sortingOrder: 4); // umber quente
        BuildSolidEarthLayer(grid.transform, "Ground_Dirt_Pebble",  23.5f, -5.0f,  73f, 1.4f, DirtPebbleCol,  sortingOrder: 4); // marrom-cinza (pedras embedded)
        BuildSolidEarthLayer(grid.transform, "Ground_Dirt_Mid",     23.5f, -6.7f,  73f, 2.0f, DirtMidCol,     sortingOrder: 4); // marrom escuro
        BuildSolidEarthLayer(grid.transform, "Ground_Dirt_Deep",    23.5f, -9.0f,  73f, 3.0f, DirtDeepCol,    sortingOrder: 4); // dark earth

        // Pedras decorativas embedded na camada Pebble (sortingOrder 5 — em cima do solid).
        BuildEmbeddedRocks(grid.transform);

        // Paredes laterais — barram o player de sair do mapa.
        // Wall_Left fica INVISÍVEL: a árvore canto-esquerdo (Tree_A1 em x=-4.2) é o
        // delimitador visual. Retângulo marrom poluía a leitura da entrada da fase.
        var wallLeft = BuildPlatform(grid.transform, "Wall_Left",   cx: -5f, cy: -1f, w: 0.4f, h: 4f, WallCol, sortingOrder: 5);
        var wallLeftSr = wallLeft.GetComponent<SpriteRenderer>();
        if (wallLeftSr != null) wallLeftSr.enabled = false;
        BuildPlatform(grid.transform, "Wall_Right",  cx: 60f, cy: -1f, w: 0.4f, h: 4f, WallCol, sortingOrder: 5);
    }

    // Cria segmento de grama: sprite real do green-zone (Tile_02 = grass top) com
    // drawMode=Tiled pra repetir a textura sem distorcer. Collider sólido pra player.
    // Fallback (sprite ausente): quad colorido como antes.
    static GameObject BuildGrassFloor(Transform parent, string name, float cx, float cy, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(cx, cy, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        var sprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.GreenZoneTileGrassTop);
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(w, h);
        }
        else
        {
            sr.sprite = SolidSprite();
            sr.color = PlatformCol;
            go.transform.localScale = new Vector3(w, h, 1f);
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
        return go;
    }

    // Camada sólida earth-tone (sem collider). Player nunca chega aqui — grama com
    // collider em cima + hazard mata quem cai no gap.
    static GameObject BuildSolidEarthLayer(Transform parent, string name, float cx, float cy, float w, float h, Color color, int sortingOrder)
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

    // Pedras decorativas embedded nas camadas de terra. Cria múltiplas pedrinhas
    // espalhadas pelo eixo X em y variado (camada Pebble + Mid). Variação de tamanho
    // pra parecer pedras naturais, não grid uniforme.
    static void BuildEmbeddedRocks(Transform parent)
    {
        var rockRoot = new GameObject("EmbeddedRocks");
        rockRoot.transform.SetParent(parent, false);

        // Posições escolhidas a mão pra distribuição visual agradável (não random
        // pra ser idempotente entre builds).
        // Formato: (x, y, size, alpha-tint)
        var rocks = new (float x, float y, float size)[]
        {
            // Camada Pebble (y=-4.3..-5.7) — pedras médias
            (-3.5f, -4.6f, 0.5f),  (1.5f, -5.2f, 0.4f),   (5.5f, -4.7f, 0.6f),
            (10f,   -5.0f, 0.5f),  (14f,  -4.6f, 0.4f),   (18f,  -5.3f, 0.55f),
            (22f,   -4.8f, 0.5f),  (26f,  -5.1f, 0.45f),  (31f,  -4.7f, 0.5f),
            (35f,   -5.2f, 0.55f), (40f,  -4.6f, 0.4f),   (44f,  -5.0f, 0.5f),
            (49f,   -4.7f, 0.5f),  (53f,  -5.3f, 0.45f),  (57f,  -4.8f, 0.55f),
            // Camada Mid (y=-5.7..-7.7) — pedras menores, mais raras
            (-2f,   -6.5f, 0.4f),  (8f,   -7.2f, 0.35f),  (16f,  -6.8f, 0.4f),
            (28f,   -7.0f, 0.35f), (38f,  -6.5f, 0.4f),   (50f,  -7.3f, 0.35f),
            // Camada Deep (y=-7.5..-10.5) — pedras maiores ocasionais
            (5f,    -8.5f, 0.7f),  (25f,  -9.0f, 0.6f),   (45f,  -8.2f, 0.65f),
        };

        foreach (var r in rocks)
        {
            var go = new GameObject($"Rock_{r.x:F0}_{r.y:F0}");
            go.transform.SetParent(rockRoot.transform, false);
            go.transform.position = new Vector3(r.x, r.y, 0);
            go.transform.localScale = new Vector3(r.size, r.size * 0.7f, 1f); // pedras achatadas (eliptical)
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SolidSprite();
            sr.color = StoneAccentCol;
            sr.sortingOrder = 5; // em cima das solid layers (4)
        }
    }

    // ---------- Decor ----------

    static void BuildDecor()
    {
        var root = new GameObject("Decor");
        // Árvores espalhadas FORA das posições dos portais e plates (evita sobreposição visual
        // que confunde o jogador). Spawns: young -3, adult -1.5. Portais P1: 2, 8. P2 plank: 14.
        // P2 portal C: 12.5. P3 plate: 25. P4 portais: 38, 42, 46. P5 plate: 50. Portal J: 52.5.
        AddProp(root.transform, "Tree_A1", SceneArtCatalog.GreenZoneTree1, new Vector3(-4.2f, -3f, 0), 4);  // canto esquerdo
        AddProp(root.transform, "Tree_A2", SceneArtCatalog.GreenZoneTree2, new Vector3(10.3f, -3f, 0), 4); // entre P1 e P2
        AddProp(root.transform, "Tree_A3", SceneArtCatalog.GreenZoneTree3, new Vector3(23.2f, -3f, 0), 4); // antes P3
        AddProp(root.transform, "Tree_A4", SceneArtCatalog.GreenZoneTree4, new Vector3(35.5f, -3f, 0), 4); // entre P3 e P4
        AddProp(root.transform, "Tree_A5", SceneArtCatalog.GreenZoneTree2, new Vector3(49.2f, -3f, 0), 4); // antes P5 portal

        // Bushes em posições neutras (sem cobrir portais ou placas).
        AddProp(root.transform, "Bush_A",  SceneArtCatalog.GreenZoneBush1, new Vector3(-0.5f, -3f, 0), 4);
        AddProp(root.transform, "Bush_B",  SceneArtCatalog.GreenZoneBush2, new Vector3(10.0f, -3f, 0), 4);
        AddProp(root.transform, "Bush_C",  SceneArtCatalog.GreenZoneBush3, new Vector3(22.0f, -3f, 0), 4);
        AddProp(root.transform, "Bush_D",  SceneArtCatalog.GreenZoneBush1, new Vector3(48.5f, -3f, 0), 4);

        // Pedras decorativas.
        AddProp(root.transform, "Stone_1", SceneArtCatalog.GreenZoneStone1, new Vector3(2.5f, -3f, 0), 4);
        AddProp(root.transform, "Stone_2", SceneArtCatalog.GreenZoneStone2, new Vector3(34f, -3f, 0), 4);
        AddProp(root.transform, "Stone_3", SceneArtCatalog.GreenZoneStone3, new Vector3(52f, -3f, 0), 4);

        // Fence nas bordas do mapa (delimita visualmente o "limite da floresta").
        AddProp(root.transform, "Fence_L", SceneArtCatalog.GreenZoneFence, new Vector3(-3f, -3f, 0), 4);
        AddProp(root.transform, "Fence_R", SceneArtCatalog.GreenZoneFence, new Vector3(58f, -3f, 0), 4);
    }

    static void AddProp(Transform parent, string name, string spritePath, Vector3 worldPos, int sortingOrder)
    {
        var sprite = SceneArtCatalog.LoadSprite(spritePath);
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }

    // ---------- Hazards ----------

    static void BuildHazardsRoot()
    {
        var root = new GameObject("Hazards");

        // Hazards posicionados DENTRO dos gaps do chão (x=3..7 riacho, x=54..56 poço).
        // Top y=-3.05 (0.05u abaixo do top da grama pra não disparar quando player tá em pé
        // na grama adjacente — edge-touch unreliable no Unity). h=0.9 → bottom y=-3.95
        // (penetra na terra → ilusão de "profundidade do rio na escavação").
        BuildHazard(root.transform, "P1_River", new Vector3(5f, -3.5f, 0), new Vector2(4f, 0.9f), HazardRiver);

        // Espuma decorativa em cima do rio (sem collider) — linha mais clara reforça
        // visualmente "isso é água, não pintura no chão". Sort 8 = à frente do rio (7).
        BuildDecorStrip(root.transform, "P1_River_Foam", new Vector3(5f, -3.15f, 0), new Vector2(4f, 0.12f), HazardRiverFoam, sortingOrder: 8);

        // Poço P5 — mesma lógica visual, color escuro (abismo).
        BuildHazard(root.transform, "P5_Pit", new Vector3(55f, -3.5f, 0), new Vector2(2f, 0.9f), HazardPit);
    }

    // Faixa visual sem collider (decoração — espuma do rio, marcação de pista, etc.).
    static void BuildDecorStrip(Transform parent, string name, Vector3 worldPos, Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    static void BuildHazard(Transform parent, string name, Vector3 worldPos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        // Sort 7 = à frente da grama (5) E da camada Root da terra (5) — sem o
        // bumpe o rio era cortado pelo Ground_Dirt_Root no overlap y=-3.7..-3.5.
        sr.sortingOrder = 7;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        go.AddComponent<HazardZone>();
    }

    // ===========================================================================
    // PUZZLE 1 — "A travessia do riacho" (x=0..10) — tutorial do verbo portal
    // Pergunta: como atravesso o riacho que mata ao tocar?
    // Resposta: pisa em Portal_A (lado esquerdo) → ambos teleportam pro lado direito
    // (perto do Portal_B). Pra revisitar, pisa em B → volta pra perto de A.
    // ===========================================================================
    static void BuildPuzzle1_RiverCrossing(Transform parent, GameObject young, GameObject adult)
    {
        // Portais pareados: A em x=2, B em x=8. Spawn 1.5u lateral do parceiro
        // (longe do trigger 1.2 wide). Y=-2.0 (acima do chão pros 2 personagens, sem afundar).
        var portalA = BuildPortal(parent, "P1_Portal_A", new Vector3(2f, -2.8f, 0), PortalAzul,
            young, adult,
            youngSpawn: new Vector3(9.5f, -2.0f, 0),  // 1.5u à direita do Portal_B (trigger 7.4..8.6)
            adultSpawn: new Vector3(9.8f, -2.0f, 0));
        var portalB = BuildPortal(parent, "P1_Portal_B", new Vector3(8f, -2.8f, 0), PortalAzul,
            young, adult,
            youngSpawn: new Vector3(0.5f, -2.0f, 0),  // 1.5u à esquerda do Portal_A (trigger 1.4..2.6)
            adultSpawn: new Vector3(0.2f, -2.0f, 0));

        // Pista 1 do cofre: "1" verde em x=-1 (antes do primeiro portal — player vê ao iniciar).
        BuildClue(parent, "P1_Clue", new Vector3(-1f, 1.0f, 0), "1", LockGreen);
    }

    // ===========================================================================
    // PUZZLE 2 — "Pedra no alto" (x=12..22) — portal como elevador
    // Pergunta: o switch está alto demais; como Young atira nele?
    // Resposta: portal C (chão) ↔ D (plataforma alta). Pisa em C → ambos vão pra
    // plataforma. Tab → Young, atira K horizontal → switch latcheia → plank quebra
    // + Gate abre. Adult atravessa pelo chão. Player pula da plataforma alta (sem
    // dano de queda) ou usa Portal_D pra voltar.
    // ===========================================================================
    static void BuildPuzzle2_PortalAsElevator(Transform parent, GameObject young, GameObject adult)
    {
        // Plank bloqueando o chão em x=14 (Adult não pula 2.5u de altura).
        BuildBreakablePlank(parent, "P2_Plank", new Vector3(14f, -1.75f, 0), new Vector2(0.6f, 2.5f));

        // Plataforma alta x=15..21 (w=6, ALARGADA pra spawn caber sem entrar no Portal_D),
        // top y=1.0 (cy=0.8, h=0.4 → top 1.0).
        BuildPlatform(parent, "P2_Platform", cx: 18f, cy: 0.8f, w: 6f, h: 0.4f, PlatformCol, 5);

        // StoneSwitch alto: y=1.5, x=20. Young no chão fira em y=-2.4 (não chega).
        // Em cima da plataforma (top y=1) fira em y≈1.4 — entra no rect 1..2 → acerta.
        var stoneSwitch = BuildStoneSwitch(parent, "P2_Switch_High",
            new Vector3(20f, 1.5f, 0), new Vector2(1.0f, 1.0f), latched: true);
        SwitchIconHelper.Attach(stoneSwitch, SwitchIconHelper.Color.Yellow);

        var gate = BuildGate(parent, "P2_Gate", new Vector3(22f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(stoneSwitch);

        // Portal_C em x=12.5 (chão pré-plank), Portal_D em x=18 (centro da plataforma).
        // C.spawn → lateral ESQUERDA da plataforma (x=15.5), longe do trigger de D (x=17.4..18.6).
        // D.spawn → chão à esquerda de C, longe do trigger de C (x=11.9..13.1).
        BuildPortal(parent, "P2_Portal_C", new Vector3(12.5f, -2.8f, 0), PortalRoxo,
            young, adult,
            youngSpawn: new Vector3(15.5f, 2.0f, 0),
            adultSpawn: new Vector3(15.8f, 2.0f, 0));
        BuildPortal(parent, "P2_Portal_D", new Vector3(18f, 1.4f, 0), PortalRoxo,
            young, adult,
            youngSpawn: new Vector3(11.5f, -2.0f, 0),
            adultSpawn: new Vector3(11.2f, -2.0f, 0));

        // Pista 2: "2" vermelho perto do plank.
        BuildClue(parent, "P2_Clue", new Vector3(13.5f, 1.0f, 0), "2", LockRed);
    }

    // ===========================================================================
    // PUZZLE 3 — "Atrás do portão" (x=24..36) — coop assimétrico SEM portal
    // Descansa o verbo novo. Reforça a mecânica de Pico Park de M_02: Adult segura
    // a placa (latched abre gate pra sempre), Tab → Young atravessa, quebra plank,
    // atira no switch alto.
    // ===========================================================================
    static void BuildPuzzle3_HoldAndShoot(Transform parent)
    {
        var plateHold = BuildPlate(parent, "P3_Plate_Adult_Hold",
            new Vector3(25f, -2.8f, 0), PressurePlate.Requirement.Adult);

        var gateC = BuildGate(parent, "P3_Gate_C", new Vector3(27f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gateC.GetComponent<GatedDoor>().sources.Add(plateHold);

        // Plank atrás do gate.
        BuildBreakablePlank(parent, "P3_Plank", new Vector3(29f, -1.75f, 0), new Vector2(0.6f, 2.5f));

        // Switch a y=-1 (Young pula do chão pra fira em y~+1 — alcança).
        var stoneSwitch = BuildStoneSwitch(parent, "P3_Switch",
            new Vector3(32f, -1f, 0), new Vector2(1.2f, 1.2f), latched: true);
        SwitchIconHelper.Attach(stoneSwitch, SwitchIconHelper.Color.Yellow);

        var gateFinal = BuildGate(parent, "P3_Gate_Final", new Vector3(34f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gateFinal.GetComponent<GatedDoor>().sources.Add(stoneSwitch);

        // Pista 3: "3" azul atrás do plank (só vê depois de quebrar).
        BuildClue(parent, "P3_Clue", new Vector3(30f, 1.0f, 0), "3", LockBlue);
    }

    // ===========================================================================
    // PUZZLE 4 — "O cofre do bosque" (x=37..46) — SequenceLock atrás de portais
    // 3 ilhas isoladas no alto da câmera (y=4), cada uma com 1 StoneSwitch colorido.
    // Cada ilha tem portal de ida (no chão) e portal de volta (na ilha).
    // Ordem certa: VERDE → VERMELHO → AZUL (das pistas dos puzzles 1/2/3).
    // Erro = todos os switches resetam (SequenceLock chama ResetSwitch nelles).
    // ===========================================================================
    static void BuildPuzzle4_PortalCofre(Transform parent, GameObject young, GameObject adult)
    {
        var lockRoot = new GameObject("P4_Cofre");
        lockRoot.transform.SetParent(parent, false);

        // 3 ilhas alargadas (w=3, antes era 2 — agora cabem spawn fora do trigger de retorno).
        // Posições centrais: x=38, 42, 46. Spans: x=36.5..39.5 / 40.5..43.5 / 44.5..47.5.
        BuildPlatform(lockRoot.transform, "P4_Island_Green",  cx: 38f, cy: 3.8f, w: 3f, h: 0.4f, PlatformCol, 5);
        BuildPlatform(lockRoot.transform, "P4_Island_Red",    cx: 42f, cy: 3.8f, w: 3f, h: 0.4f, PlatformCol, 5);
        BuildPlatform(lockRoot.transform, "P4_Island_Blue",   cx: 46f, cy: 3.8f, w: 3f, h: 0.4f, PlatformCol, 5);

        // Switches no centro de cada ilha. Non-latched (SequenceLock reseta em erro).
        var swGreen = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Green",
            new Vector3(38f, 4.5f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockGreenOff, onColor: LockGreen);
        SwitchIconHelper.Attach(swGreen, SwitchIconHelper.Color.Green);
        var swRed   = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Red",
            new Vector3(42f, 4.5f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockRedOff,   onColor: LockRed);
        SwitchIconHelper.Attach(swRed, SwitchIconHelper.Color.Red);
        var swBlue  = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Blue",
            new Vector3(46f, 4.5f, 0), new Vector2(1f, 1f),
            latched: false, offColor: LockBlueOff,  onColor: LockBlue);
        SwitchIconHelper.Attach(swBlue, SwitchIconHelper.Color.Blue);

        // SequenceLock — switches[0]=green, [1]=red, [2]=blue.
        // ExpectedOrder: 0,1,2 = green, red, blue (ordem das pistas: 1=verde, 2=vermelho, 3=azul).
        var seqGO = new GameObject("P4_SequenceLock");
        seqGO.transform.SetParent(lockRoot.transform, false);
        var seqLock = seqGO.AddComponent<SequenceLock>();
        seqLock.switches.Add(swGreen);
        seqLock.switches.Add(swRed);
        seqLock.switches.Add(swBlue);
        seqLock.expectedOrder.Add(0); // 1º: GREEN
        seqLock.expectedOrder.Add(1); // 2º: RED
        seqLock.expectedOrder.Add(2); // 3º: BLUE

        // Texto indicativo grande acima das ilhas (centro x=42, alto y=6).
        BuildPasswordHint(lockRoot.transform, new Vector3(42f, 6f, 0), "COLOCAR A SENHA NA ORDEM");

        // 3 pares de portais. IDA no chão (centro da ilha). VOLTA no lado DIREITO da ilha.
        // Spawns: ida → lado ESQUERDO da ilha (longe do trigger de volta); volta → chão à direita
        // do portal de ida (longe do trigger de ida). Todos os spawns y=-2.0 (chão) ou y=5.0 (alto).
        BuildPortal(lockRoot.transform, "P4_Portal_G", new Vector3(38f, -2.8f, 0), PortalVerde,
            young, adult,
            youngSpawn: new Vector3(37f, 5.0f, 0),    // esquerda da ilha verde, longe de G_Return em x=39
            adultSpawn: new Vector3(37.3f, 5.0f, 0));
        BuildPortal(lockRoot.transform, "P4_Portal_GReturn", new Vector3(39f, 4.2f, 0), PortalVerde,
            young, adult,
            youngSpawn: new Vector3(36.7f, -2.0f, 0), // chão à esquerda de G (x=38, trigger 37.4..38.6)
            adultSpawn: new Vector3(37.0f, -2.0f, 0));

        BuildPortal(lockRoot.transform, "P4_Portal_H", new Vector3(42f, -2.8f, 0), PortalVermelho,
            young, adult,
            youngSpawn: new Vector3(41f, 5.0f, 0),
            adultSpawn: new Vector3(41.3f, 5.0f, 0));
        BuildPortal(lockRoot.transform, "P4_Portal_HReturn", new Vector3(43f, 4.2f, 0), PortalVermelho,
            young, adult,
            youngSpawn: new Vector3(40.7f, -2.0f, 0),
            adultSpawn: new Vector3(41.0f, -2.0f, 0));

        BuildPortal(lockRoot.transform, "P4_Portal_I", new Vector3(46f, -2.8f, 0), PortalCiano,
            young, adult,
            youngSpawn: new Vector3(45f, 5.0f, 0),
            adultSpawn: new Vector3(45.3f, 5.0f, 0));
        BuildPortal(lockRoot.transform, "P4_Portal_IReturn", new Vector3(47f, 4.2f, 0), PortalCiano,
            young, adult,
            youngSpawn: new Vector3(44.7f, -2.0f, 0),
            adultSpawn: new Vector3(45.0f, -2.0f, 0));

        // Gate_Lock latched, source = SequenceLock. Posição x=48 (após ilha azul em x=47.5).
        var gateLock = BuildGate(lockRoot.transform, "P4_Gate_Lock", new Vector3(48f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gateLock.GetComponent<GatedDoor>().sources.Add(seqLock);
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
        tmp.fontSize = 2f;
        tmp.rectTransform.sizeDelta = new Vector2(10f, 1.2f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    // ===========================================================================
    // PUZZLE 5 — "Saída perdida" (x=49..58) — combina plate + portal + chave + drop
    // Adult-only plate latcheia gate (libera caminho). Os 2 atravessam, pisam em
    // Portal_J → vão pra alcove K (sobre o poço). Young pega chave (KeyPickup.Collected
    // = true). Portais auto-destroem (narrativamente OK). Ambos descem da alcove pelo
    // lado direito (drop safe pra ground em x=57.5) e atravessam pra CoopFinishDoor.
    // ===========================================================================
    static void BuildPuzzle5_PlateElevatorPortalKey(Transform parent, GameObject young, GameObject adult)
    {
        var p5 = new GameObject("P5_KeyExit");
        p5.transform.SetParent(parent, false);

        // Plate Adult-only, latched: Adult pisa uma vez, gate destrava pra sempre.
        var plateHold = BuildPlate(p5.transform, "P5_Plate_Adult_Hold",
            new Vector3(50f, -2.8f, 0), PressurePlate.Requirement.Adult);

        var gateC = BuildGate(p5.transform, "P5_Gate_C", new Vector3(51.5f, 0.5f, 0),
            size: new Vector2(0.7f, 7f), openOffset: new Vector2(0f, 8f), latched: true);
        gateC.GetComponent<GatedDoor>().sources.Add(plateHold);

        // Alcove platform alta — ALARGADA pra w=3 (cobre x=56..59, top y=2.7). Estende-se à
        // direita do poço (x=54..55), então drop do lado direito (x=59) cai em ground safe.
        BuildPlatform(p5.transform, "P5_Alcove", cx: 57.5f, cy: 2.5f, w: 3f, h: 0.4f, PlatformCol, 5);

        // KeyPickup no centro da alcove.
        var key = KeyPickup.Spawn(new Vector3(57.5f, 2.9f, 0));
        key.transform.SetParent(p5.transform, false);
        key.transform.position = new Vector3(57.5f, 2.9f, 0);

        // Portal_J chão (x=52.5), Portal_K alcove (x=56.5, esquerda da alcove pra spawns terem
        // espaço pra direita). Trigger de K: x=55.9..57.1. Spawn de J → lado DIREITO da alcove
        // (x=58.3) fora do trigger de K. Spawn de K → chão à esquerda de J (x=51, trigger J 51.9..53.1).
        BuildPortal(p5.transform, "P5_Portal_J", new Vector3(52.5f, -2.8f, 0), PortalDourado,
            young, adult,
            youngSpawn: new Vector3(58.3f, 3.5f, 0),
            adultSpawn: new Vector3(58.6f, 3.5f, 0));
        BuildPortal(p5.transform, "P5_Portal_K", new Vector3(56.5f, 2.7f, 0), PortalDourado,
            young, adult,
            youngSpawn: new Vector3(51.0f, -2.0f, 0),
            adultSpawn: new Vector3(51.3f, -2.0f, 0));

        // Pista de transição: hint visual em texto que "a saída fica do outro lado".
        // Pequeno, atrás do plate, pra o player explorar.
        BuildClue(p5.transform, "P5_Hint", new Vector3(48f, -0.2f, 0), ">>", CreamCol);
    }

    // ---------- Helpers comuns ----------

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

    static GameObject BuildBreakablePlank(Transform parent, string name, Vector3 worldPos, Vector2 size)
    {
        // BoxCollider2D NON-trigger (sólido pro Adult) + EnemyHealth(1) no root.
        // Visual = FenceStackHelper (bottom/middle/top empilhados, sprites do business-center pack).
        // Stone trigger atravessa o sólido mas dispara OnTriggerEnter → TakeDamage → Destroy.
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one; // root sem scale; visual carrega o tamanho.

        FenceStackHelper.Build(go.transform, name + "_Visual", size.x, size.y, 7);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        var hp = go.AddComponent<EnemyHealth>();
        hp.maxHealth = 1;

        // FX: partículas de madeira + crash sound em OnDefeated (antes do Destroy).
        go.AddComponent<PlankBreakFx>();
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

        // 3 camadas (Shadow base / Body / Rim highlight) pra dar leitura de "botão".
        BuildPlateLayer(go.transform, "Shadow", new Vector3(0f, -0.05f, 0f), new Vector3(W + 0.1f, H, 1f),
            new Color(0.10f, 0.08f, 0.06f, 1f), sortingOrder: 5);
        var bodySr = BuildPlateLayer(go.transform, "Body", Vector3.zero, new Vector3(W, H, 1f),
            PlateOffCol, sortingOrder: 6);
        BuildPlateLayer(go.transform, "Rim", new Vector3(0f, H * 0.4f, 0f), new Vector3(W - 0.15f, 0.06f, 1f),
            new Color(1f, 0.96f, 0.78f, 0.55f), sortingOrder: 7);

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
        if (visual != null)
        {
            visual.offColor = offColor;
            visual.onColor = onColor;
        }
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
        // Bumpado de 1.2 → 3 (world units) pra ficar legível em 900×600.
        tmp.fontSize = 3f;
        tmp.rectTransform.sizeDelta = new Vector2(2f, 2f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    // Portal = ReturnPad com spawn customizado. Bidirecional só por configuração:
    // builder cria 2 pads, cada um aponta pro território perto do outro. Visual:
    // sprite real do doors-and-portals pack (mesmo do ReturnPad de M_02), com tint
    // da cor do par pra distinguir A↔B, C↔D, etc.
    static GameObject BuildPortal(Transform parent, string name, Vector3 worldPos, Color color,
        GameObject young, GameObject adult, Vector3 youngSpawn, Vector3 adultSpawn)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        // Trigger collider 1.2×0.4 na base (igual M_02 ReturnPad).
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 0.4f);
        col.isTrigger = true;

        // Visual: portal de pedra animado (6 frames idle) do doors-and-portals pack.
        // PPU 32, pivot BottomCenter → 64×64 px = 2u×2u. localY -0.2 alinha bottom
        // do sprite com chão (worldPos.y = -2.8 → bottom em -3, top em -1).
        var visual = new GameObject("PortalVisual");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;
        sr.color = color; // tint do par
        var portalFrames = SceneArtCatalog.LoadSpriteFrames(SceneArtCatalog.PortalStoneSheet, "2_");
        var anim = visual.AddComponent<SpriteFrameAnimator>();
        anim.frames = portalFrames;
        anim.fps = 6f;
        anim.loop = true;
        anim.autoPlay = true;
        if (portalFrames != null && portalFrames.Length > 0) sr.sprite = portalFrames[0];
        else
        {
            // Fallback caso o sheet do portal não esteja sliced: quad colorido grande.
            sr.sprite = SolidSprite();
            visual.transform.localScale = new Vector3(1.6f, 2f, 1f);
            visual.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        }

        var pad = go.AddComponent<ReturnPad>();
        pad.teleportOnlyActive = true; // Memory_03: cada Woody atravessa o portal individualmente.
        if (young != null)
        {
            pad.young = young.transform;
            pad.youngSpawn = youngSpawn;
        }
        if (adult != null)
        {
            pad.adult = adult.transform;
            pad.adultSpawn = adultSpawn;
        }
        return go;
    }

    // ---------- Players ----------

    static GameObject BuildYoung(Vector3 pos)
    {
        return BuildPlayerInternal("PlayerYoung", pos, PlayerKind.Young,
            colliderSize: new Vector2(0.55f, 0.85f),
            spriteFolder: "Assets/Sprites/criancas/Child_3",
            hasJump: false,
            groundCheckY: -0.5f);
    }

    static GameObject BuildAdult(Vector3 pos)
    {
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
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var obj in subs)
        {
            if (obj is Sprite s && s.name.StartsWith(prefix))
                list.Add(s);
        }
        list.Sort((a, b) => SpriteIndex(a.name).CompareTo(SpriteIndex(b.name)));
        if (list.Count == 0)
            Debug.LogWarning($"[Memory03Builder] sem sub-sprites em {sheetPath} — rode Retroself → Configure Character Sprites primeiro");
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
        else { sr.sprite = SolidSprite(); sr.color = new Color(0.32f, 0.18f, 0.10f, 1f); }

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

        CreateUIText(canvasGO.transform, "Title", "MEMORY 03 - FLORESTA",
            71.68f, FontStyles.Bold,
            new Vector2(0, 460), new Vector2(1400, 100),
            CreamCol,
            TextAlignmentOptions.Center);

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
        intro.line = "Devia ter uns nove anos... Entrei nesta floresta atras de uma borboleta " +
                     "e quando vi, nao sabia mais voltar pra casa. A noite chegava devagar entre as arvores. " +
                     "Eu tive medo. Hoje eu te encontro, e a gente sai daqui juntos.";
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
