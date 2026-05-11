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

// Builder de Memory_02_Domingo — fase co-op estilo Pico Park no interior da casa.
// Tom basement (cores marrom/madeira) usando sprites solid-color — o pack
// basement-tileset-pixel-art tem tiles desenhados como pedaços de parede vertical
// (não floor tile), então renderizam como pilarzinhos quebrados se usados como
// chão. Voltamos pro approach de quad solid-color (igual placeholders pré-pack do
// Memory_01); os props de 3 Objects/ continuam vindo dos PNGs reais.
// 3 puzzles em sequência (caixa peso-combinado, plataforma de impulso, dual-plates
// AND com fresta). Sem inimigo AI — só hazard ambiental. Usuário tuna
// post-processing manualmente; builder NÃO mexe em
// Light2D/Volume (preservados via SceneRebuildHelpers).
public static class Memory02Builder
{
    const string ScenePath = "Assets/Scenes/Memory_02_Domingo.unity";

    static readonly string[] OwnedRoots = {
        "Main Camera", "EventSystem", "Grid",
        "Decor", "Hazards", "Puzzles",
        "PlayerYoung", "PlayerAdult", "PlayerSwap",
        "FinishDoor", "Canvas", "SceneStartReset",
    };

    static readonly Color CreamCol     = new Color(1f, 0.92f, 0.55f, 1f);
    static readonly Color AdultCol     = new Color(0.45f, 0.32f, 0.22f, 1f);
    static readonly Color HazardCol    = new Color(0.10f, 0.05f, 0.10f, 1f);
    static readonly Color PlateOffCol  = new Color(0.35f, 0.20f, 0.18f, 1f);
    static readonly Color PlateOnCol   = new Color(0.95f, 0.78f, 0.30f, 1f);
    static readonly Color GateCol      = new Color(0.55f, 0.40f, 0.25f, 1f);
    static readonly Color DoorCol      = new Color(0.32f, 0.18f, 0.10f, 1f);
    static readonly Color DoorFrameCol = new Color(0.62f, 0.46f, 0.22f, 1f);
    static readonly Color BoxCol       = new Color(0.55f, 0.36f, 0.22f, 1f);
    static readonly Color FloorCol     = new Color(0.36f, 0.24f, 0.16f, 1f); // marrom escuro
    static readonly Color FloorTopCol  = new Color(0.50f, 0.34f, 0.22f, 1f); // marrom claro (topo)
    static readonly Color WallCol      = new Color(0.22f, 0.16f, 0.12f, 1f); // quase preto

    // Cores do cofre (puzzle 4) — switches coloridos + clues correspondentes.
    // Off é a versão dessaturada/escura, On é a saturada (PressurePlateVisual troca).
    static readonly Color LockRed      = new Color(0.95f, 0.30f, 0.30f, 1f);
    static readonly Color LockRedOff   = new Color(0.45f, 0.18f, 0.18f, 1f);
    static readonly Color LockGreen    = new Color(0.45f, 0.90f, 0.40f, 1f);
    static readonly Color LockGreenOff = new Color(0.20f, 0.40f, 0.20f, 1f);
    static readonly Color LockBlue     = new Color(0.40f, 0.65f, 0.95f, 1f);
    static readonly Color LockBlueOff  = new Color(0.18f, 0.28f, 0.45f, 1f);

    [MenuItem("Retroself/Build Memory_02_Domingo")]
    public static void BuildMemory02()
    {
        var scene = SceneRebuildHelpers.OpenOrNew(ScenePath);
        if (!SceneRebuildHelpers.ConfirmRebuild(scene, OwnedRoots)) return;
        SceneRebuildHelpers.WipeOwnedRoots(scene, OwnedRoots);

        var camera = BuildCamera();
        BuildEventSystem();
        BuildGround();
        BuildDecor();

        // Hazards root vazio (mantido pra OwnedRoots não quebrar; sem pit nessa fase).
        new GameObject("Hazards");

        var puzzlesRoot = new GameObject("Puzzles");
        BuildPuzzle1_StoneClears(puzzlesRoot.transform);
        BuildPuzzle2_BoxAsLadder(puzzlesRoot.transform);
        BuildPuzzle3_HoldAndShoot(puzzlesRoot.transform);
        BuildPuzzle4_PasswordLock(puzzlesRoot.transform);
        var leftLockedDoor = BuildPuzzle5_KeyChamber(puzzlesRoot.transform);

        var young = BuildYoung(new Vector3(-3f, -2f, 0));
        var adult = BuildAdult(new Vector3(-1.5f, -2f, 0));
        var swap = BuildSwap(young, adult);

        AttachCameraFollow(camera, young);
        var finishDoor = BuildFinishDoor(new Vector3(45f, -1.9f, 0));
        // -1.9 = ground top -3 + half door height 1.1 → door base no chão.
        // Posição x=45 fica logo depois do ReturnPad (x=43.5) e antes da parede direita (x=46).
        var finishDoorComp = finishDoor.GetComponent<CoopFinishDoor>();
        if (finishDoorComp != null) finishDoorComp.requireKey = true;

        // ReturnPad: pisar = teleporta os 2 Woody pra spawn. Permite revisitar
        // pistas. Posicionado entre Gate_Lock (x=42) e FinishDoor (x=45) — caminho
        // obrigatório de saída, mas player escolhe se pisa ou não. Primeira pisada
        // também destrava a LeftLockedDoor (revelando a câmara da chave do P5).
        var returnPad = BuildReturnPad(puzzlesRoot.transform, new Vector3(43.5f, -2.8f, 0), young, adult);
        var padComp = returnPad.GetComponent<ReturnPad>();
        if (padComp != null && leftLockedDoor != null)
            padComp.doorsToOpenOnFirstUse.Add(leftLockedDoor);

        // SceneStartReset: zera KeyPickup.Collected ao carregar a cena (cross-cena).
        var resetGO = new GameObject("SceneStartReset");
        resetGO.AddComponent<SceneStartReset>();

        BuildHUD(young, swap, young, adult, finishDoor);

        // Post-processing + Light2D são tunados manualmente pelo usuário (Volume e
        // Light2D adicionados no editor sobrevivem rebuild via WipeOwnedRoots — eles
        // são lifted pra scene root antes dos owned roots morrerem). Aqui só
        // garantimos os DOIS pré-requisitos pra esse tuning manual funcionar:
        //   1. Câmera com renderPostProcessing=true (Volume é ignorado sem isso).
        //   2. Todos os SpriteRenderer usando o material 2D/Sprite-Lit-Default
        //      (Sprites/Default é unlit — Light2D não afeta). Sem isso, "adicionei
        //      uma luz e nada mudou na cena" é o sintoma esperado.
        EnableCameraPostProcessing(camera);
        ApplyLitMaterialToSprites(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Debug.Log($"[Memory02Builder] Memory_02 montada em {ScenePath}.");
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
        // Material default do URP 2D, ships dentro do package. Sprite-Lit-Default
        // é o único shader 2D do URP que recebe luz de Light2D.
        const string litMatPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
        var litMat = AssetDatabase.LoadAssetAtPath<Material>(litMatPath);
        if (litMat == null)
        {
            // Fallback: cria material via shader. Funciona mas não compartilha com
            // outros assets já configurados.
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader == null)
            {
                Debug.LogWarning("[Memory02Builder] Sprite-Lit-Default shader não encontrado — Light2D não vai afetar sprites. Verifique se URP 2D está instalado.");
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
                // Não tocar em renderers de Canvas/UI (eles usam Image, não SpriteRenderer
                // — então safe), nem em renderers que já têm material customizado.
                if (sr.sharedMaterial != null && sr.sharedMaterial != litMat &&
                    sr.sharedMaterial.shader != null &&
                    sr.sharedMaterial.shader.name != "Sprites/Default")
                    continue;
                sr.sharedMaterial = litMat;
                touched++;
            }
        }
        Debug.Log($"[Memory02Builder] Sprite-Lit-Default aplicado em {touched} SpriteRenderer(s).");
    }

    // ---------- Camera / EventSystem ----------

    static GameObject BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Interior escuro: marrom-cinza pro caso do BG não carregar.
        cam.backgroundColor = new Color(0.10f, 0.07f, 0.08f, 1f);
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
        follow.minX = -13f;
        follow.maxX = 42f;
        follow.minY = -1f;
        follow.maxY = 3f;
    }

    // ---------- Ground (solid quads, sem tilemap) ----------

    static void BuildGround()
    {
        // Convenção (Memory_01): chão tem topo em world y=-3. Players spawnam em y=-2
        // e caem ~0.5u no chão.
        // Layout flat — sem pit, sem fresta. Os 3 puzzles co-op exigem foco de coordenação
        // e não friction de plataforma. Variedade visual vem dos props (Decor) e dos gates.
        var grid = new GameObject("Grid");

        // Linha de superfície (h=0.4u no topo) + base larga abaixo. Continuum de x=-15 a x=46
        // (a área x=-15..-5 é a câmara da chave do P5 — começa fechada via LeftLockedDoor).
        BuildPlatform(grid.transform, "Ground_Top",  cx: 15.5f, cy: -3.2f, w: 61f, h: 0.4f, FloorTopCol, sortingOrder: 5);
        BuildPlatform(grid.transform, "Ground_Base", cx: 15.5f, cy: -4.5f, w: 61f, h: 2f,   FloorCol,    sortingOrder: 4);

        // Paredes laterais pra player não sair do mapa.
        BuildPlatform(grid.transform, "Wall_Left",   cx: -15f,  cy: -1f,   w: 0.4f, h: 4f,  WallCol,    sortingOrder: 5);
        BuildPlatform(grid.transform, "Wall_Right",  cx: 46f,   cy: -1f,   w: 0.4f, h: 4f,  WallCol,    sortingOrder: 5);
    }

    static GameObject BuildPlatform(Transform parent, string name, float cx, float cy, float w, float h, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(cx, cy, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go;
    }

    // ---------- Decor (sem colliders) ----------

    static void BuildDecor()
    {
        // Props PPU 32 com pivot BottomCenter (Configurator) → y = ground top (-3) deixa a base no chão.
        var root = new GameObject("Decor");
        AddProp(root.transform, "Pipe_L",   SceneArtCatalog.BasementPipe,    new Vector3(-4f,  -3f, 0), 4);
        AddProp(root.transform, "Decor_1",  SceneArtCatalog.BasementDecor1,  new Vector3(-2f,  -3f, 0), 4);
        AddProp(root.transform, "Decor_4",  SceneArtCatalog.BasementDecor4,  new Vector3(11f,  -3f, 0), 4);
        AddProp(root.transform, "Pipe_M",   SceneArtCatalog.BasementPipe,    new Vector3(23f,  -3f, 0), 4);
        AddProp(root.transform, "Decor_R",  SceneArtCatalog.BasementDecor1,  new Vector3(33f,  -3f, 0), 4);
        AddProp(root.transform, "Pipe_R",   SceneArtCatalog.BasementPipe,    new Vector3(45f,  -3f, 0), 4);
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

    // ---------- Puzzles ----------
    // Convenção Y (ground top -3): placas h=0.4 → centro y=-2.8 (bottom -3, top -2.6).
    // Gates h=3.0, w=0.7 → centro y=-1.5 (bottom -3, top 0). openOffset (0, 5.5) → bottom +2.5,
    // top +5.5 (quase fora da câmera ortho 5 com offset 1.5). Adult head em y=-1.1 → 3.6u
    // clearance quando aberto. moveSpeed 10u/s — abre em 0.55s.

    // ===========================================================================
    // PUZZLE 1 — "A pedra abre caminho" (x=0..10)
    // Pergunta: como o Adult passa por uma parede que ele não consegue quebrar?
    // Resposta: Young arremessa pedra (K) na BreakablePlank → some → Adult atravessa,
    // empurra caixa pra Plate_Box (HeavyBox-only) → Gate abre enquanto caixa segura
    // a placa. Ensina o verbo "Stone como chave" + sequência obrigatória de personagens.
    // ===========================================================================
    static void BuildPuzzle1_StoneClears(Transform parent)
    {
        // Plank em x=2, h=2.5 → top y=-0.5 (Adult não pula por cima — pula ~2u, head em y=0.9).
        BuildBreakablePlank(parent, "P1_Plank", new Vector3(2f, -1.75f, 0), new Vector2(0.6f, 2.5f));

        // Caixa do lado direito da plank — Adult só alcança depois que plank some.
        var box = BuildHeavyBox(new Vector3(4f, -3f, 0));
        box.transform.SetParent(parent, false);

        var plate = BuildPlate(parent, "P1_Plate_Box", new Vector3(6f, -2.8f, 0), PressurePlate.Requirement.HeavyBox);
        // Gate non-latched: caixa precisa ficar na placa enquanto ambos atravessam.
        var gate = BuildGate(parent, "P1_Gate", new Vector3(8f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: false);
        gate.GetComponent<GatedDoor>().sources.Add(plate);

        // Pista 1 do cofre: "1: VERDE" — pintada na parede em verde, próxima da plank pra
        // o jogador notar enquanto Young arremessa a primeira pedra.
        BuildClue(parent, "P1_Clue", new Vector3(3f, -0.2f, 0), "1", LockGreen);
    }

    // ===========================================================================
    // PUZZLE 2 — "Mira impossível sem ajuda" (x=12..22)
    // Pergunta: o switch alvo está alto demais. Como Young alcança?
    // Resposta: Adult empurra HeavyBox até ficar debaixo do switch. Young pula em cima
    // da caixa, pula DA caixa, atira K horizontal no ápice → acerta StoneSwitch (latched).
    // Ensina: caixa = degrau improvisado; pedra = mira; coop = vantagem geométrica.
    // ===========================================================================
    static void BuildPuzzle2_BoxAsLadder(Transform parent)
    {
        var box = BuildHeavyBox(new Vector3(13f, -3f, 0));
        box.transform.SetParent(parent, false);

        // Switch alto em x=17, y=-0.3 (acima da plataforma da caixa). Young no chão fira em
        // y=-2.4 — não chega. Em cima da caixa (top y=-1.6) fira em y=-1.0 — também baixo.
        // Pulando da caixa: pico ~+1.5u acima do top da caixa → fira em y=~-0.5 → ENTRA no
        // alvo (1.0u alto: y=-0.8 a +0.2). Janela apertada mas alcançável.
        var stoneSwitch = BuildStoneSwitch(parent, "P2_Switch_High", new Vector3(17f, -0.3f, 0),
            new Vector2(1.0f, 1.0f), latched: true);

        // Gate único, source = stoneSwitch latched. Latched gate só abre uma vez.
        var gate = BuildGate(parent, "P2_Gate", new Vector3(20f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(stoneSwitch);

        // Pista 2: "2: VERMELHO" — perto da caixa, na parede.
        BuildClue(parent, "P2_Clue", new Vector3(14f, -0.2f, 0), "2", LockRed);
    }

    // ===========================================================================
    // PUZZLE 3 — "Atrás de mim, atire" (x=24..32)
    // Pergunta: Young precisa entrar numa câmara isolada e ativar algo lá dentro,
    // mas o gate só abre enquanto Adult pisa numa placa fora.
    // Resposta: Adult fica na Plate_Adult_Hold (não-latched, não filtrada por player
    // ATIVO — só por kind). Tab → Young. Adult continua na placa mesmo inativo (collider
    // permanece no trigger). Young atravessa Gate_C, quebra plank, atira no StoneSwitch.
    // Gate_Final latcheia. Young volta atravessando Gate_C (Adult ainda na plate). Ambos
    // saem pelo CoopFinishDoor.
    // Ensina: o conceito-chave dos puzzles assimétricos — um segura, o outro age.
    // ===========================================================================
    static void BuildPuzzle3_HoldAndShoot(Transform parent)
    {
        // Placa Adult-only. Gate_C latched: assim que Adult pisa, latcheia aberto pra
        // sempre. Player ainda precisa entender que precisa do Adult (que fica
        // disponível mesmo após Tab) — mas não é mais um timing-puzzle de "Adult sai
        // da plate e gate fecha". Permite revisita pelo ReturnPad sem re-trabalho.
        var plateHold = BuildPlate(parent, "P3_Plate_Adult_Hold",
            new Vector3(24.5f, -2.8f, 0), PressurePlate.Requirement.Adult);

        var gateC = BuildGate(parent, "P3_Gate_C", new Vector3(26f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: true);
        gateC.GetComponent<GatedDoor>().sources.Add(plateHold);

        // Câmara do Young (x=27..32). Plank esconde o switch da visão da entrada.
        BuildBreakablePlank(parent, "P3_Plank", new Vector3(28f, -1.75f, 0), new Vector2(0.6f, 2.5f));

        // StoneSwitch em altura média (y=-1) — Young fira do chão em y=-2.4, não chega.
        // Em pulo (~+2u) fira em y=-0.4 — acerta. Janela é mais larga que P2 (alvo 1.2u).
        var stoneSwitch = BuildStoneSwitch(parent, "P3_Switch", new Vector3(30f, -1f, 0),
            new Vector2(1.2f, 1.2f), latched: true);

        // Gate final latched + non-Adult-Hold dependent — uma vez switch acertado, fica aberto pra sempre.
        var gateFinal = BuildGate(parent, "P3_Gate_Final", new Vector3(32f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: true);
        gateFinal.GetComponent<GatedDoor>().sources.Add(stoneSwitch);

        // Pista 3: "3: AZUL" — dentro da câmara, atrás da plank, pra Young descobrir
        // só depois de quebrar o caminho.
        BuildClue(parent, "P3_Clue", new Vector3(29f, -0.2f, 0), "3", LockBlue);
    }

    // ===========================================================================
    // PUZZLE 4 — "O cofre da memória" (x=33..42)
    // Pergunta: três botões coloridos, uma porta. Qual a ordem certa?
    // Resposta: as pistas espalhadas pelos puzzles 1/2/3 montam a senha
    // (1: VERDE, 2: VERMELHO, 3: AZUL → Young precisa atirar nessa ordem).
    // SequenceLock observa OnHit de cada switch, valida ordem, reseta tudo
    // se errar. Quando completa, latcheia IsActive=true → gate final abre.
    // Ensina: memória + observação (não é só motor). Errar não pune com morte —
    // só reseta os switches, jogador tenta de novo. Inspirado em Genius/Simon.
    // ===========================================================================
    static void BuildPuzzle4_PasswordLock(Transform parent)
    {
        var lockRoot = new GameObject("P4_Lock");
        lockRoot.transform.SetParent(parent, false);

        // 3 switches coloridos no chão (ground top -3, switch h=1.0 → centro y=-2.5).
        // Stone do Young sai em y≈-2.4 do nível do chão — atinge alvos centrados em y=-2.5.
        // x=35,37,39 dão espaçamento de 2u entre switches (Young pula entre).
        var swRed   = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Red",
            new Vector3(35f, -2.5f, 0), new Vector2(1.0f, 1.0f),
            latched: false, offColor: LockRedOff,   onColor: LockRed);
        var swGreen = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Green",
            new Vector3(37f, -2.5f, 0), new Vector2(1.0f, 1.0f),
            latched: false, offColor: LockGreenOff, onColor: LockGreen);
        var swBlue  = BuildStoneSwitchColored(lockRoot.transform, "P4_Switch_Blue",
            new Vector3(39f, -2.5f, 0), new Vector2(1.0f, 1.0f),
            latched: false, offColor: LockBlueOff,  onColor: LockBlue);

        // SequenceLock — ordem: VERDE(idx 1), VERMELHO(idx 0), AZUL(idx 2).
        // Os índices da lista switches precisam bater com os números das pistas:
        // switch[0]=RED→pista 2, switch[1]=GREEN→pista 1, switch[2]=BLUE→pista 3.
        var lockGO = new GameObject("P4_SequenceLock");
        lockGO.transform.SetParent(lockRoot.transform, false);
        var seqLock = lockGO.AddComponent<SequenceLock>();
        seqLock.switches.Add(swRed);
        seqLock.switches.Add(swGreen);
        seqLock.switches.Add(swBlue);
        seqLock.expectedOrder.Add(1); // 1º: GREEN
        seqLock.expectedOrder.Add(0); // 2º: RED
        seqLock.expectedOrder.Add(2); // 3º: BLUE

        // Texto indicativo acima dos switches: "COLOCAR A SENHA".
        BuildPasswordHint(lockRoot.transform);

        // Gate final do cofre — latched, source única é o SequenceLock (também GateSource).
        var gate = BuildGate(lockRoot.transform, "P4_Gate_Lock", new Vector3(42f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: true);
        gate.GetComponent<GatedDoor>().sources.Add(seqLock);
    }

    static void BuildPasswordHint(Transform parent)
    {
        // Indicação maior acima da área dos switches (centro em x=37): "COLOCAR A SENHA".
        var go = new GameObject("P4_PasswordHint");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(37f, 0.5f, 0);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = "COLOCAR A SENHA";
        tmp.color = CreamCol;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 2.0f;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    // ===========================================================================
    // PUZZLE 5 — "O degrau invisível" (x=-15..-5)
    // Pergunta: como o Young alcança a alcove alta com a chave, se Adult não
    // consegue passar pela fresta baixa que separa as duas áreas?
    // Resposta: Adult fica numa placa Adult-only no lado direito da fresta. Esta
    // placa controla um ELEVADOR no lado esquerdo (longe). Adult presta mais que
    // ativa ou desativa — ele virtualmente "empurra" o elevador pelo gate. Tab →
    // Young (que já está em cima do elevador parado) sobe carregado pela
    // plataforma móvel. Pega a chave em cima → KeyPickup.Collected = true → o
    // ReturnPad some + CoopFinishDoor passa a aceitar atravessar.
    // Ensina: cooperação espacial-assimétrica. Adult longe SEGURA, Young perto AGE.
    // ===========================================================================
    static GatedDoor BuildPuzzle5_KeyChamber(Transform parent)
    {
        var p5 = new GameObject("P5_KeyChamber");
        p5.transform.SetParent(parent, false);

        // LeftLockedDoor em x=-5 — começa fechada (latched=true, sources vazia).
        // ReturnPad chama ForceOpen() na primeira pisada → fica aberta pra sempre.
        // Mesma altura/scale que os outros gates da fase pra continuidade visual.
        var doorGO = BuildGate(p5.transform, "P5_LeftLockedDoor", new Vector3(-5f, 0.5f, 0),
            size: new Vector2(0.7f, 7.0f), openOffset: new Vector2(0f, 8.0f), latched: true);
        var leftDoor = doorGO.GetComponent<GatedDoor>();
        // Sem sources — só ForceOpen() destranca.

        // (Ceiling fresta removida — atrapalhava a leitura visual sobre o botão. O
        // puzzle continua se auto-enforçando: a placa em x=-7 é Adult-only e o elevador
        // só sobe enquanto ela está ativa, então Adult precisa ficar nela enquanto Young
        // sobe — sem precisar de barreira de altura.)

        // Placa Adult-only em x=-7. Non-latched: solta = elevador desce. Garantia de
        // descida controlada pro Young não ficar preso lá em cima.
        var plateLift = BuildPlate(p5.transform, "P5_Plate_Adult_Lift",
            new Vector3(-7f, -2.8f, 0), PressurePlate.Requirement.Adult);

        // Elevador em x=-12. Box 1.6×0.4u centrado y=-2.8 (top y=-2.6 = ground+0.4).
        // openOffset (0, 4) → top aberto y=+1.4. Young em cima sobe carregado pelo
        // MovePosition do Kinematic RB.
        var elevatorGO = BuildGate(p5.transform, "P5_Elevator", new Vector3(-12f, -2.8f, 0),
            size: new Vector2(1.6f, 0.4f), openOffset: new Vector2(0f, 4f), latched: false);
        var elevator = elevatorGO.GetComponent<GatedDoor>();
        elevator.sources.Add(plateLift);
        // moveSpeed default 4u/s — sobe 4u em 1s (lento o suficiente pra parecer elevador, rápido pra não cansar).
        elevator.moveSpeed = 4f;

        // Alcove à esquerda do elevador: plataforma fixa onde Young desce ao chegar no topo.
        // Top y=+1.4 (igual ao topo do elevador aberto) — sem step. Width 2u, x=[-14..-12].
        BuildPlatform(p5.transform, "P5_Alcove", cx: -13f, cy: +1.2f, w: 2f, h: 0.4f, FloorTopCol, sortingOrder: 5);

        // Chave em cima da alcove. KeyPickup.Spawn cria GO completo (sprite + collider + bob).
        var key = KeyPickup.Spawn(new Vector3(-13f, +1.8f, 0));
        key.transform.SetParent(p5.transform, false);
        key.transform.position = new Vector3(-13f, +1.8f, 0); // SetParent local-space — força world.

        return leftDoor;
    }

    static GameObject BuildCeilingBlock(Transform parent, string name, Vector3 worldPos, Vector2 size)
    {
        // Bloco de teto sólido — bloqueia colisão (não-trigger), Young passa por baixo
        // se a janela for >= altura dele, Adult bate a cabeça por ser mais alto.
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = WallCol;
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go;
    }

    // ---------- Puzzle helpers ----------

    static GameObject BuildBreakablePlank(Transform parent, string name, Vector3 worldPos, Vector2 size)
    {
        // Parede destrutível: SpriteRenderer quad + BoxCollider2D NÃO-trigger (bloqueia
        // colisão sólida do Adult) + EnemyHealth(1). Stone (trigger) atravessa o sólido
        // mas dispara OnTriggerEnter2D → Stone.OnTriggerEnter pega EnemyHealth → TakeDamage(1)
        // → HP=0 → OnDefeated → Destroy. Plank some, Adult passa.
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = new Color(0.40f, 0.27f, 0.16f, 1f); // marrom-tabua
        sr.sortingOrder = 7;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        // NÃO trigger — bloqueia Adult fisicamente.

        var hp = go.AddComponent<EnemyHealth>();
        hp.maxHealth = 1;
        return go;
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
        // Pista 3D world-space (TextMeshPro standalone, não UGUI). Usa a mesma
        // Press Start 2P SDF do HUD. Numerinho discreto (~0.4u tall) — pista visual
        // de canto-da-tela, não placa de aviso.
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
        tmp.fontSize = 1.2f;
        tmp.rectTransform.sizeDelta = new Vector2(0.8f, 0.8f);
        var font = SceneArtCatalog.GetPixelFont();
        if (font != null) tmp.font = font;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;
    }

    static GameObject BuildReturnPad(Transform parent, Vector3 worldPos, GameObject young, GameObject adult)
    {
        var go = new GameObject("ReturnPad");
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        // Collider 1.2×0.4 (trigger no pé do portal). Sem localScale no root pra
        // não distorcer o sprite filho.
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 0.4f);
        col.isTrigger = true;

        // Visual: portal de pedra (6 frames idle) do doors-and-portals pack.
        // PPU 32, pivot BottomCenter → 64×64 px = 2u×2u. Root em y=-2.8; chão em
        // y=-3 → offset -0.2 alinha bottom no chão (portal sobe 2u, top em y=-1).
        var visual = new GameObject("PortalVisual");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;
        var portalFrames = SceneArtCatalog.LoadSpriteFrames(SceneArtCatalog.PortalStoneSheet, "2_");
        var anim = visual.AddComponent<SpriteFrameAnimator>();
        anim.frames = portalFrames;
        anim.fps = 6f;
        anim.loop = true;
        anim.autoPlay = true;
        if (portalFrames != null && portalFrames.Length > 0) sr.sprite = portalFrames[0];
        else { sr.sprite = SolidSprite(); sr.color = new Color(0.4f, 0.85f, 0.95f, 1f); }

        var pad = go.AddComponent<ReturnPad>();
        if (young != null)
        {
            pad.young = young.transform;
            pad.youngSpawn = young.transform.position;
        }
        if (adult != null)
        {
            pad.adult = adult.transform;
            pad.adultSpawn = adult.transform.position;
        }
        return go;
    }

    static StoneSwitch BuildStoneSwitch(Transform parent, string name, Vector3 worldPos, Vector2 size, bool latched)
    {
        // Alvo na parede que vira on quando Stone bate. Trigger collider — Stone passa
        // através mas dispara OnTriggerEnter no StoneSwitch.
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

        // Reusa PressurePlateVisual (poll-based, target é GateSource).
        var visual = go.AddComponent<PressurePlateVisual>();
        visual.target = sw;
        visual.renderer = sr;
        visual.offColor = PlateOffCol;
        visual.onColor = PlateOnCol;
        return sw;
    }

    static PressurePlate BuildPlate(Transform parent, string name, Vector3 worldPos, PressurePlate.Requirement req)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        // Placa: 1.5×0.4 (60% maior que original) — clara visualmente.
        // Centro y=-2.8 deixa bottom em -3 (no chão), top em -2.6.
        go.transform.localScale = new Vector3(1.5f, 0.4f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite();
        sr.color = PlateOffCol;
        sr.sortingOrder = 6;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var plate = go.AddComponent<PressurePlate>();
        plate.requirement = req;

        // Visual: poll IsActive em Update e troca cor. UnityEvents não persistem
        // bem em scene file via builder editor (AddListener é runtime-only); poll
        // simples evita o problema.
        var colorizer = go.AddComponent<PressurePlateVisual>();
        colorizer.target = plate;
        colorizer.renderer = sr;
        colorizer.offColor = PlateOffCol;
        colorizer.onColor = PlateOnCol;
        return plate;
    }

    static GameObject BuildGate(Transform parent, string name, Vector3 worldPos, Vector2 size, Vector2 openOffset, bool latched)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        // Root sem scale; filho visual carrega o tamanho via stretch calculado.
        go.transform.localScale = Vector3.one;

        // Visual granular: empilha tiles do basement-tileset verticalmente pra dar
        // aparência de parede texturizada (vs solid color). Fallback: quad sólido.
        var wallSprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.BasementTileWall);
        if (wallSprite != null && wallSprite.bounds.size.x > 0 && wallSprite.bounds.size.y > 0)
        {
            BuildVerticalTiledVisual(go.transform, "GateVisual", wallSprite, size.x, size.y, 7);
        }
        else
        {
            var visual = new GameObject("GateVisual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = SolidSprite();
            sr.color = GateCol;
            sr.sortingOrder = 7;
        }

        // Collider em world units no root (sem scale herdado).
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x, size.y);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var gate = go.AddComponent<GatedDoor>();
        gate.openOffset = openOffset;
        gate.latched = latched;
        // Velocidade alta (10u/s) pro player não esbarrar na borda da porta meio-fechada
        // quando ativa a placa e tenta atravessar imediatamente.
        gate.moveSpeed = 10f;
        return go;
    }

    // Cria um filho visual com sprite stretched pra preencher (totalW, totalH) world units,
    // centrado no parent. Usa sprite.bounds.size pra calcular scale (PPU-agnostic) e compensa
    // pivot do sprite via localPosition. Necessário porque os sprites do basement-tileset têm
    // PPU 100 + pivot bottom-left → stretch direto via transform.localScale fica minúsculo
    // e desalinhado.
    static GameObject CreateStretchedSpriteChild(Transform parent, string name, Sprite sprite,
        float totalW, float totalH, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        var native = sprite.bounds.size;
        go.transform.localScale = new Vector3(totalW / native.x, totalH / native.y, 1f);

        // pivotNorm = pivot/rect; localPosition = (pivotNorm - 0.5) * size compensa o pivot
        // pra que o center do sprite caia em (0,0) do parent (validado por geometria).
        var pivotNorm = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height);
        go.transform.localPosition = new Vector3(
            (pivotNorm.x - 0.5f) * totalW,
            (pivotNorm.y - 0.5f) * totalH, 0f);
        return go;
    }

    // Empilha múltiplas instâncias do tileSprite verticalmente preenchendo (totalW, totalH),
    // mantendo aspect ratio nativo do tile. Cada tile vira um filho com nome Tile_i. Granular
    // look pra portas (vs single stretched sprite que ficaria com pixels esticados).
    static void BuildVerticalTiledVisual(Transform parent, string namePrefix, Sprite tileSprite,
        float totalW, float totalH, int sortingOrder)
    {
        var native = tileSprite.bounds.size;
        // Cada tile tem largura totalW e altura proporcional ao aspect nativo do sprite.
        float baseTileH = totalW * (native.y / native.x);
        if (baseTileH < 0.3f) baseTileH = 0.5f;
        int nTiles = Mathf.Max(1, Mathf.RoundToInt(totalH / baseTileH));
        float tileH = totalH / nTiles;

        for (int i = 0; i < nTiles; i++)
        {
            var tile = CreateStretchedSpriteChild(parent, $"{namePrefix}_{i}", tileSprite,
                totalW, tileH, sortingOrder);
            // Reposiciona pra empilhar do bottom ao top em y=[-totalH/2, +totalH/2].
            float tileCenterY = -totalH * 0.5f + tileH * (i + 0.5f);
            var pos = tile.transform.localPosition;
            pos.y += tileCenterY;
            tile.transform.localPosition = pos;
        }
    }

    static GameObject BuildHeavyBox(Vector3 pos)
    {
        // pos.y é o top do chão (-3). Caixa 1.6×1.4u sentada nesse top.
        // Center pivot: transform.y = ground_top + half-height = -3 + 0.7 = -2.3.
        var go = new GameObject("HeavyBox");
        const float w = 1.6f, h = 1.4f;
        go.transform.position = new Vector3(pos.x, pos.y + h * 0.5f, 0);
        // Root sem scale; filho visual carrega o stretch calculado via bounds.
        go.transform.localScale = Vector3.one;

        // Visual: sprite real do BasementBox esticado pra preencher 1.6×1.4u via
        // CreateStretchedSpriteChild (compensa PPU 100 + pivot bottom-left do asset).
        // Fallback: quad sólido se o sprite não carregar.
        var boxSprite = SceneArtCatalog.LoadSprite(SceneArtCatalog.BasementBox);
        if (boxSprite != null && boxSprite.bounds.size.x > 0 && boxSprite.bounds.size.y > 0)
        {
            CreateStretchedSpriteChild(go.transform, "BoxVisual", boxSprite, w, h, 8);
        }
        else
        {
            var visual = new GameObject("BoxVisual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(w, h, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = SolidSprite();
            sr.color = BoxCol;
            sr.sortingOrder = 8;
        }

        // Collider em world units no root (sem scale herdado).
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
        col.offset = Vector2.zero;
        col.sharedMaterial = new PhysicsMaterial2D("HeavyBoxNoFriction") { friction = 0f, bounciness = 0f };

        go.AddComponent<Rigidbody2D>();
        go.AddComponent<HeavyBox>();
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
            Debug.LogWarning($"[Memory02Builder] sem sub-sprites em {sheetPath} — rode Retroself → Configure Character Sprites primeiro");
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

        // Visual: sprite real do doors-and-portals pack (6 frames de abertura).
        // Pivot BottomCenter, PPU 32 → 1u×2u. Door root em y=-1.9; chão em y=-3
        // → offset -1.1 alinha bottom do sprite com chão.
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
        else { sr.sprite = SolidSprite(); sr.color = DoorCol; }

        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.0f, 2.0f);
        col.isTrigger = true;

        var doorComp = root.AddComponent<CoopFinishDoor>();
        doorComp.openAnimator = anim;

        var bob = root.AddComponent<IdleBob>();
        bob.amplitude = 0.05f;
        bob.speed = 1.5f;
        return root;
    }

    // ---------- HUD ----------

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
            14, FontStyles.Bold,
            new Vector2(0, -440), new Vector2(1820, 60),
            new Color(1, 0.95f, 0.7f, 0.9f));

        CreateUIText(canvasGO.transform, "Title", "MEMORY 02 - DOMINGO",
            28, FontStyles.Bold,
            new Vector2(0, 460), new Vector2(1200, 60),
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

        var labelGO = CreateUIText(box.transform, "SpeakerLabel", "Woody (adulto)", 18, FontStyles.Bold,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Left);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = labelRt.anchorMax = new Vector2(0, 1);
        labelRt.pivot = new Vector2(0, 1);
        labelRt.anchoredPosition = new Vector2(250, -20);
        labelRt.sizeDelta = new Vector2(1200, 36);

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

        var contGO = CreateUIText(box.transform, "Continue", ">> Espaco/Enter", 14, FontStyles.Italic,
            Vector2.zero, Vector2.zero, CreamCol, TextAlignmentOptions.Right);
        var contRt = contGO.GetComponent<RectTransform>();
        contRt.anchorMin = contRt.anchorMax = new Vector2(1, 0);
        contRt.pivot = new Vector2(1, 0);
        contRt.anchoredPosition = new Vector2(-30, 18);
        contRt.sizeDelta = new Vector2(360, 32);
        contGO.AddComponent<BlinkUI>();

        var intro = box.AddComponent<IntroDialogue>();
        intro.line = "Domingo. Eu tinha doze anos. Lembro do silencio depois... do barulho. " +
                     "A porta do quarto deles fechada. Eu ficava ouvindo, fingindo que nao. " +
                     "Hoje eu vou estar aqui com voce. A gente atravessa juntos.";
        intro.dialogueBox = box;
        intro.typewriter = typewriter;
        intro.continueIndicator = contGO;
        if (swap != null) intro.playerSwap = swap.GetComponent<PlayerSwap>();
        if (young != null) intro.young = young.GetComponent<PlayerController>();
        if (adult != null) intro.adult = adult.GetComponent<PlayerController>();
        // Sem bully nesta fase — guard em IntroDialogue.SetGameplayFrozen aceita null.
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
