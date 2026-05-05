# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Retroself** — narrative puzzle-platformer in 8-bit pixel art, single-player, with time-manipulation mechanics. Side-scroller 2D, target platform Web (itch.io WebGL build). Designed for ~45–60 min playthrough across 4 memory levels + hub + epilogue.

Story-mechanic core: the adult protagonist (16×32 sprite) and his younger self (16×16 sprite) cooperate asymmetrically — adult freezes time and reaches high places, kid fits through gaps. The metaphor is self-compassion: the time-freeze is a tool of care, not power.

Full design intent lives in [docs/gdd.md](docs/gdd.md). Read it before substantive design or content decisions — it's the source of truth for tone, mechanics, and what the game is *not*.

## Stack

- **Unity 6000.3.13f1** with **URP** (Universal Render Pipeline)
- **New Input System** is active — do **not** use `UnityEngine.Input` (legacy). Use `InputSystemUIInputModule` on `EventSystem` GameObjects, and `InputAction` references in scripts. The legacy `StandaloneInputModule` will throw `InvalidOperationException` on Play.
- **TextMeshPro** for all UI text. TMP Essentials must be imported once via `Window → TextMeshPro → Import TMP Essential Resources`.
- 2D toolset: Tilemap, Sprite, Aseprite importer, 2D Animation. URP 2D Renderer.

## Project layout

Scripts and assets live **directly under `Assets/`** (not in a nested `_Retroself/` folder) — the user prefers the flat tutorial-style layout. Don't introduce subfolders unless asked.

```
Assets/
├── Audio/        (autoral chiptune tracks — added by user, not us)
├── Editor/       (editor-only scripts; gated with #if UNITY_EDITOR)
├── Fonts/        (PressStart2P-Regular.ttf — OFL — + PressStart2P-SDF.asset gerado)
├── Prefabs/
├── Scenes/       (MainMenu.unity, Prologue.unity, Memory_01_Patio.unity, SampleScene.unity)
├── Scripts/      (runtime MonoBehaviours — flat, no subfolders)
├── Settings/     (URP renderer assets + Memory01_PostProcess.asset + Tiles/ com Tile sub-assets — Residential_*.asset apontam pro residential-area pack)
├── Sprites/      (autoral pixel art — added by user; packs de cenário em residential-area-tileset, free-scrolling-city-backgrounds, ghetto-tileset)
└── TextMesh Pro/ (auto-generated when TMP Essentials are imported)
```

Scenes flow: `MainMenu` → `Prologue` → `Hub` → `Memory_01..04` → `Epilogue`. Constants for these names live in [Assets/Scripts/SceneNames.cs](Assets/Scripts/SceneNames.cs); always use them with `SceneManager.LoadScene(SceneNames.X)` rather than scene indices or string literals.

## Editing Unity scenes (`.unity` YAML)

Hand-editing scene YAML is supported but fragile. Two rules learned the hard way:

1. **Use real GUIDs from `Library/PackageCache/`**, never invented ones. Unity will silently rewrite the scene back to a "clean" state if it can't resolve a `m_Script` GUID. Run `find Library/PackageCache -name "ComponentName.cs.meta"` to get them.
2. **Set `m_fontAsset: {fileID: 0}`** for TMP text components when TMP Essentials may not be imported yet. Unity falls back to the default font and shows a warning rather than crashing.

Known-good GUIDs in this project (verified from `com.unity.ugui@8ccc29d23a79` and `com.unity.inputsystem@21a28c3a6c83`):
- `TextMeshProUGUI` → `f4688fdb7df04437aeb418b961361dc5`
- `Image` → `fe87c0e1cc204ed48ad3b37840f39efc`
- `Button` → `4e29b1a8efbd4b44bb3f3716e73f07ff`
- `CanvasScaler` → `0cd44c1031e13a943bb63640046fad76`
- `GraphicRaycaster` → `dc42784cf147c0c48a680349fa168899`
- `EventSystem` → `76c392e42b5098c458856cdf6ecaaaa1`
- `InputSystemUIInputModule` → `01614664b831546d2ae94a42149d80ac` (use this, not `StandaloneInputModule`)

Project script GUIDs (in `Assets/Scripts/*.meta`):
- `MenuActions` → `262f35aaca3933c4b97823e32a77038f`
- `TitleGlitch` → `aa7076beaecf18e4c8bfb64df14f0128`
- `MenuMusic` → `d53f3246e3ae931458c1c89e61651333`
- `CabinPulse` → `05c761f7ebed82d4f8fe0284038dc14f`
- `SceneNames` → `c0ccef824e254c7459c0e7c66693f46f`
- `CutsceneController` → `9f64ff45bd52b63489bbae7d7d579685`
- `TypewriterText` → `1312f3e45bbc55247bfef98501666c88`
- `CutsceneFader` → `71905f66a1a0e4d4cbab0ad8b6bcc1cb`
- `BlinkUI` → `6039163247a0b09488946dd6e21898d5`
- `PlayerController` → `cce41d70ca982fb448293af7836e2b2c`
- `PlayerHealth` → `5d02bae0c97e3834eaa496366c231946`
- `PlayerAttack` → `7aebca1103a9f924c81b61ae92bffe0d`
- `PlayerAnimator` → `1af84f91ac166724084d9ec9048f021b`
- `Stone` → `03c7b7a978fcb704e8cb6aaf72332bc8`
- `EnemyHealth` → `412d764571113a04887f4dc0aba4a8ba`
- `BullyController` → `90d7bc0af69165243b0a252b0ce8ff18`
- `HealthBarUI` → `6478bf8ac4d28174faac49074f0a910b`
- `HazardZone` → `496fa41d517cbd5419bd306829ad275c`
- `CameraFollow2D` → `5cdcb08ba65ff27449ccb799356295aa`

Scripts adicionados no Sprint 3 (Dualidade Woody + polish): `PlayerKind`, `PlayerSwap`, `IdleBob`, `BreathingScale`, `LightningFlash`, `ParallaxLayer`, `DustPuff`, `HeavyBox`. Sprint 3.1 (narrativa + porta): `IntroDialogue`, `SchoolDoor`. Sprint 3.2 (pixel art autoral + animator): `PlayerAnimDriver`, `BullyAnimDriver`. **Os GUIDs deles ainda não foram registrados aqui** — depois do primeiro `Assets → Refresh` no Unity, rode `grep ^guid Assets/Scripts/<nome>.cs.meta` e cole acima caso vá hand-editar YAML de cena.

After editing a `.unity` file from outside the editor, the user must `Assets → Refresh` (Ctrl+R) or reopen the scene — Unity caches scene data in memory and won't pick up disk edits otherwise.

## Editor utilities

Builders são idempotentes (re-rodar substitui a cena) e adicionam as scenes no Build Settings automaticamente.

- [Assets/Editor/MainMenuBuilder.cs](Assets/Editor/MainMenuBuilder.cs) — menu **Retroself → Build Main Menu Scene**. Builds `Assets/Scenes/MainMenu.unity` com `InputSystemUIInputModule`, parallax leve em 3 camadas BG (`ParallaxLayer`), overlay de raio (`LightningFlash`) e pulse de 1±2% no título via `BreathingScale`.
- [Assets/Editor/PrologueBuilder.cs](Assets/Editor/PrologueBuilder.cs) — menu **Retroself → Build Prologue Scene** (gera `Prologue.unity` com 7 painéis populados) e **Retroself → Build Memory_01_Patio Placeholder** (só cria a cena placeholder se ela ainda não existir, pra não sobrescrever o tutorial). Painéis 1–3 (chuvosos) têm `LightningFlash` overlay; aliens do painel 5/6 oscilam com `IdleBob` em fases offset; Woody dormindo/sentado/cabine respira com `BreathingScale`. Fade entre painéis já estava no `CutsceneController.GoToNextPanel`.
- [Assets/Editor/Memory01Builder.cs](Assets/Editor/Memory01Builder.cs) — menu **Retroself → Build Memory_01_Patio Tutorial**. Monta a fase tutorial completa: câmera + EventSystem (InputSystemUIInputModule), Tilemap_Ground com `CompositeCollider2D` + `Rigidbody2D` Static (geometryType Polygons, `compositeOperation = Merge`) — agora chama `tm.RefreshAllTiles() + tc.ProcessTilemapChanges() + cc.GenerateGeometry()` ao final pra cachear a geometria no `.unity` e evitar a janela de regeneração na transição Prologue→tutorial. Inclui fresta baixa (teto a y=−2 entre x=16..17), parede em x=20, HeavyBox marrom (RB Dynamic), **porta da escola** (`SchoolDoor`) em x=18.5 que carrega `Memory_01_School` ao toque com fade preto, Bully com `IdleBob` leve, **dois Woody coexistindo** (`PlayerYoung` 0.55×0.95 e `PlayerAdult` 0.55×1.9) + `PlayerSwap`, CameraFollow2D, Volume URP global, HUD com health bar + hint + **diálogo de abertura** (`IntroDialogue` com `TypewriterText` + `BlinkUI` no continue indicator, congela ambos os Woody + Bully + PlayerSwap até o player avançar com Espaço/Enter; Esc pula).
- [Assets/Editor/SchoolPlaceholderBuilder.cs](Assets/Editor/SchoolPlaceholderBuilder.cs) — menu **Retroself → Build Memory_01_School Placeholder**. Gera `Memory_01_School.unity`: cena minimalista com texto "FIM DO TUTORIAL" + botão "Voltar ao Menu" (chama `MenuActions.VoltarMenu`). Destino default da `SchoolDoor` até a fase escola real ser construída.
- [Assets/Editor/SpriteImportConfigurator.cs](Assets/Editor/SpriteImportConfigurator.cs) — menu **Retroself → Configure Character Sprites**. Aplica `TextureImporter` settings de pixel art (PPU per-folder, FilterMode Point, no compression) + slice horizontal por contagem de frames em Multiple mode. **Não é mais chamado pelo `Memory01Builder` em rebuild** — chama-se manualmente uma vez quando importa um sprite novo. Reescreve `importer.spritesheet` então sobrescreve qualquer slice/pivot manual feito no Sprite Editor; só rode se quiser resetar. Idempotente em si mesmo.
- [Assets/Editor/SceneArtImportConfigurator.cs](Assets/Editor/SceneArtImportConfigurator.cs) — menu **Retroself → Configure Scene Art Imports**. Aplica `TextureImporter` (FilterMode.Point, no compression, PPU + pivot per-folder) **somente** nos packs de cenário (`residential-area-tileset-pixel-art`, `free-scrolling-city-backgrounds-pixel-art`, `ghetto-tileset-pixel-art/3 Objects`). PPU 32 pra Tiles/Props (1 cell = 1u no Grid), PPU 50 pros backgrounds da cidade (576×324 → 11.52×6.48u, cobre ortho size 5). Pula explicitamente `Sprites/criancas/`, `Sprites/mendigos/`, `Sprites/gangsters/` — slicing manual frágil. Idempotente. Rode 1× depois de importar um pack novo.
- [Assets/Editor/SceneArtCatalog.cs](Assets/Editor/SceneArtCatalog.cs) — paths constantes pros sprites usados por `Memory01Builder`/`PrologueBuilder`/`MainMenuBuilder` (tiles, props, layers do CityNight, paths dos `.asset`). Inclui `LoadSprite(path)` (warning se não achar) e `GetPixelFont()` com cache + fallback pra fonte default se o `.asset` não existir. Trocar de pack/tile = editar 1 linha aqui.
- [Assets/Editor/PixelFontBuilder.cs](Assets/Editor/PixelFontBuilder.cs) — menu **Retroself → Build Pixel Font Asset**. Lê `Assets/Fonts/PressStart2P-Regular.ttf` (OFL/Google Fonts) e gera `Assets/Fonts/PressStart2P-SDF.asset` via `TMP_FontAsset.CreateFontAsset` (SDFAA, atlas 512×512, padding 5, samplingPointSize 64). Pré-popula o atlas com ASCII + acentos pt-BR + travessão/aspas tipográficas. Embute Texture2D + Material como sub-assets. Re-rodar reescreve. Press Start 2P é citado nos créditos do MainMenu.
- [Assets/Editor/AnimatorBuilder.cs](Assets/Editor/AnimatorBuilder.cs) — menu **Retroself → Build Character Animators**. **DESATIVADO** no fluxo atual: `Memory01Builder` usa `SimpleSpriteAnimator` (runtime, sem AnimatorController). Existe ainda mas não é chamado — junto com `Assets/Animations/`, `PlayerAnimDriver`, `BullyAnimDriver` são órfãos e podem ser removidos no próximo cleanup.

## Working style

- **Authorship convention:** sprites e música autorais (do usuário) — não geramos nem commitamos asset binaries por conta própria. Quando o usuário adiciona pacotes de pixel art (ex: Craftpix em `Assets/Sprites/criancas`, `mendigos`, `gangsters`), nosso trabalho é wirar os GameObjects + `SimpleSpriteAnimator` apontando pra esses sprites. `Audio/` ainda é vazio. Guia auxiliar: [docs/sprites-import.md](docs/sprites-import.md).
- **Tutorial reference:** `c:/Users/Usuário/tutorial-base/` (cloned from `carloshernanic/Tutorial-Jogos`) is the structural reference the user follows for Unity patterns (`MenuActions` style, scene flow, etc.). Use it as a *pattern* reference, not as content to copy.
- **Don't introduce abstractions or subfolders** unless asked. Keep the layout flat and the scripts simple — match the tutorial's cadence.

## Sprint deliverables (course context)

This is an academic project for Insper, Jogos Digitais 2026. Tracking docs:
- [docs/sprint2-entrega.md](docs/sprint2-entrega.md) — current sprint deliverable (Milanote link, GitHub repo, YouTube video) + selected rubrics (doubled: #1 Conceito/Narrativa, #2 Mechanics; discarded: #4 UI/HUD, #9 Replayability).
- [docs/gdd.md](docs/gdd.md) — GDD in DDE format (Game Concept / Design / Dynamics / Experience / Inspirações).

Team: Alex Chequer, Carlos Hernani, Lucas Ikawa.

## Estado do tutorial (`Memory_01_Patio`)

Fluxo atual: `MainMenu` → `Prologue` (cutscene 7 painéis) → `Memory_01_Patio` (tutorial jogável). A fase tutorial cobre as 3 rubricas selecionadas:

1. **Player completo + dualidade Woody** — Dois `PlayerController` coexistindo: `PlayerYoung` (collider 0.55×0.95, jaqueta amarela) e `PlayerAdult` (collider 0.55×1.9, sobretudo marrom). `PlayerSwap` (Tab) alterna o ativo, desligando `PlayerController`/`PlayerAttack`/`PlayerAnimator` do inativo e zerando velocidade horizontal pra ele não escorregar. `BullyController`, `CameraFollow2D` e `HealthBarUI` se inscrevem em `PlayerSwap.OnActiveChanged` e re-bind no novo ativo. `PlayerController` agora carrega `public PlayerKind kind` e dispara `DustPuff.Spawn` no momento de pouso. `PlayerHealth` (3 HP, knockback, flicker), `PlayerAttack` (K arremessa pedra). Combate não-letal conforme GDD §2.2: jovem arremessa, adulto faz puzzle físico (caixa).
2. **Mapa com pós-processamento + puzzle de tamanho** — Tilemap_Ground (grass/dirt/plataforma de madeira, tile assets em `Assets/Settings/Tiles/`) com chão estendido até x=20, **fresta baixa** com teto em y=−2 entre x=16..17 (jovem cabe, adulto bate a cabeça) e parede de fundo em x=20. **HeavyBox** dinâmico em x=−3.5 que só o adulto empurra (`OnCollisionStay2D` checa `kind == Adult`). HazardPit no gap x∈[−1,3). Recompensa estrelinha amarela após a fresta com `IdleBob`. Volume URP global em `Assets/Settings/Memory01_PostProcess.asset` com Bloom 0.18 (tint quente), Vignette 0.35, ColorAdjustments (exposure −0.2, contrast 10, saturation −3, filtro creme).
3. **Inimigo completo** — `BullyController` (patrulha x∈[5,13] antes da fresta, detecta ativo por |dx|<6 ∧ |dy|<2, persegue mais rápido, troca cor patrol→chase). `EnemyHealth` (3 HP, stun 0.6s ao levar pedrada). Bobbing leve no body via `IdleBob`. **Bound de patrulha clampa até em chase** pra não cair no poço.

Comandos: A/D ou ←/→ mover, Espaço/W/↑ pular, K arremessar pedra, **Tab trocar Woody**. HUD mostra título da fase, hint de controles e barra de vida do ativo. Cair no HazardPit reinicia o checkpoint do ativo.

**Abertura narrativa (Memory_01):** ao entrar na fase, `IntroDialogue` exibe uma fala do Woody adulto ("Lembro bem deste dia… foi quando eu me atrasei pra escola…") com `TypewriterText` e congela controles dos dois Woody, `PlayerSwap` e `BullyController`. Espaço/Enter avança/pula; Esc dispensa. Quando dispensa, chama `PlayerSwap.RefreshActive()` pra restaurar enabled/alpha do ativo.

**Saída (porta-escola):** `SchoolDoor` em x=18.5 — trigger que faz fade preto via `FadeOverlay` no Canvas e carrega `Memory_01_School` (placeholder de fim de tutorial gerado por `SchoolPlaceholderBuilder`).

**Fix do bug de queda Prologue→Memory_01:** `Memory01Builder.BuildGround` agora força `cc.GenerateGeometry()` no momento do build (geometria fica cacheada no `.unity`), e `PlayerController.Awake` chama `Physics2D.SyncTransforms()`. Sem isso, a primeira `FixedUpdate` após `SceneManager.LoadScene` rodava antes do composite collider regenerar e os Woody atravessavam o chão.

**Pixel art autoral + SimpleSpriteAnimator (Sprint 3.2):** os 3 personagens visíveis na fase usam sprites autorais do Craftpix, fatiados manualmente no Sprite Editor do Unity (Multiple mode) e ciclados pelo componente `SimpleSpriteAnimator`:
- `PlayerYoung/Body` → `Assets/Sprites/criancas/Child_3/` (Idle, Walk; sem Jump.png — fallback p/ Idle).
- `PlayerAdult/Body` → `Assets/Sprites/mendigos/Homeless_1/` (Idle, Walk, Jump).
- `Bully/Body` → `Assets/Sprites/gangsters/Gangsters_2/` (porteiro reframed: monstro do tutorial, alinha com a fala "o porteiro não me deixou entrar").

Pipeline: `Memory01Builder.LoadSpriteFrames` carrega os sub-sprites via `AssetDatabase.LoadAllAssetRepresentationsAtPath`, filtra por prefixo `Filename_` e ordena pelo sufixo numérico. Setta como `Sprite[] idleSprites/walkSprites/jumpSprites` no `SimpleSpriteAnimator`. O componente troca `sr.sprite` por estado (Idle/Walk/Jump derivado de `PlayerController.MoveX/IsGrounded` ou `Rigidbody2D.linearVelocity.x`). **Não usa AnimatorController nem `.anim`.**

`SimpleSpriteAnimator.AlignFeetToTransform` no Awake puxa o `BoxCollider2D` do parent e força `body.localPosition.y` tal que `bounds.min.y` do sprite coincida com a base do collider — funciona para qualquer pivot que vier do Sprite Editor (Center, Bottom, etc.).

**Crítico:** o builder **não chama mais `SpriteImportConfigurator.Configure()` em rebuild**. Esse Configure reescrevia o `importer.spritesheet` e zerava qualquer slice/pivot manual feito no Sprite Editor. Hoje o slicing manual sobrevive entre rebuilds.

**Importante para artistas:** os 3 estados (Idle/Walk/Jump) precisam ter slices com **mesmo Y e mesma altura** dentro de cada folha. Slices com tamanhos diferentes entre estados fazem o `bounds.min.y` mudar entre os sprites e o personagem "salta" de altura ao trocar de animação (foi um bug que custou uma sessão inteira de debug).

Os scripts `PlayerAnimDriver`, `BullyAnimDriver`, `AnimatorBuilder.cs` e os `.controller`/`.anim` em `Assets/Animations/` ficaram **órfãos** mas inofensivos — podem ser deletados. `PlayerAnimator` placeholder (squash/stretch) também foi removido do builder.

**Restilização das 3 cenas com pixel art autoral (Sprint 3.3):** as 3 cenas existentes (`MainMenu`, `Prologue`, `Memory_01_Patio`) substituem placeholders coloridos por sprites reais dos packs em `Assets/Sprites/`:
- **Tilemap do pátio** → `residential-area-tileset-pixel-art/1 Tiles/` (Tile_01/02/05/15.png, 32×32 px, PPU 32, pivot Center). `Memory01Builder.LoadResidentialTile` carrega o sprite e cria/atualiza um `Tile.asset` em `Assets/Settings/Tiles/Residential_*.asset` (Ground, GrassTop, Wall, Platform). Os antigos `Ground_Dirt/GrassTop/Platform_Wood.asset` ficaram órfãos.
- **Props do pátio + praça** → `residential-area-tileset-pixel-art/3 Objects/` (Bench.png, Lamp_post.png, Trashcan.png, Boxes/1.png). PPU 32, pivot BottomCenter (sentam no chão). HeavyBox usa `Boxes/1.png` no lugar do quad marrom; Bench/Lamp/Trashcan decorativos no Memory_01 e na praça do prologue/menu.
- **BG paralax** → `free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/` (5 layers 576×324, PPU 50 → 11.52×6.48u, cobre ortho size 5). No `Memory_01_Patio` o BG fica **parented na Main Camera** (segue automaticamente sem script de tracking). No MainMenu/Prologue panel-1/panel-2 fica static no scene root. Layers 0–2 (mais distantes) ganham `ParallaxLayer` com sway leve.
- **Fonte UI** → Press Start 2P (Google Fonts, OFL) em `Assets/Fonts/PressStart2P-Regular.ttf` + `PressStart2P-SDF.asset` gerado pelo `PixelFontBuilder`. Todos os builders chamam `SceneArtCatalog.GetPixelFont()` no `CreateUIText`/`CreateMenuButton`; fallback pra default se a fonte não existir. Press Start 2P é **muito mais largo** que LiberationSans, então os tamanhos de fonte dos diálogos/título caíram drasticamente (Title 96→28, Body 36→18, Hint 30→16, RETROSELF 140→80).
- **Mantidos como placeholders** (decisão consciente): cabine roxa do MainMenu/Prologue panel 3 (metáfora narrativa), aliens RGB do panel 5/6, relógio do panel 7, glow do poste, school door — sem assets condizentes ou tom abstrato intencional.

Pipeline de import: `SceneArtImportConfigurator` aplica PPU + FilterMode.Point por pasta; `SceneArtCatalog` centraliza paths constantes; `LoadResidentialTile`/`CreateRealSprite` (em cada builder) consomem.

Tom da fase tutorial: o jovem Woody (7 anos, sprite 16×16) e o adulto (16×32) coexistem; o jogador alterna entre os dois pra resolver passagens. Curva de aprendizado da Fase 1 segundo GDD §3.2: ensina **movimento e dualidade de tamanho**, sem congelamento de tempo ainda (esse só vem na Fase 2).

## Lições aprendidas montando a fase

Gotchas do Unity 2D + URP que custaram tempo — leia antes de editar `Memory01Builder.cs` ou criar outra fase:

- **Volume URP precisa de `sharedProfile`, não `profile`.** `vol.profile = …` clona o asset em runtime e não persiste no `.unity` salvo (sai como `sharedProfile: {fileID: 0}`). Use `vol.sharedProfile = …` + `EditorUtility.SetDirty(vol)` no builder.
- **`renderPostProcessing` da câmera só persiste via `SerializedObject`.** Setar direto em `cam.GetUniversalAdditionalCameraData().renderPostProcessing` não escreve no scene YAML. Use `SerializedObject` na `UniversalAdditionalCameraData`, mexa em `m_RenderPostProcessing`, `ApplyModifiedPropertiesWithoutUndo()` + `SetDirty`.
- **Triggers precisam de pelo menos um `Rigidbody2D` na colisão.** Stone (collider trigger) só dispara `OnTriggerEnter2D` no Bully porque tem `Rigidbody2D` Kinematic com `gravityScale = 0` — sem isso, dois colliders puros se ignoram.
- **Tilemap individual prende inimigos.** Tile colliders são polígonos por célula; sem merge, a costura entre tiles cria uma "borda" em V que segura entidades. Solução: `Rigidbody2D` Static + `TilemapCollider2D.compositeOperation = Merge` + `CompositeCollider2D` com `geometryType = Polygons`.
- **Player gruda em parede no ar.** `BoxCollider2D` com material default tem `friction > 0`; pressionando uma direção contra a parede em queda ele "agarra". Solução: `col.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f }`.
- **`Physics2D.OverlapCircleAll` no ground check pega a si mesmo.** Filtre `hits[i].attachedRigidbody != rb` no `CheckGrounded()` ou o player se considera no chão eternamente.
- **`rb.linearVelocity`, não `rb.velocity`.** Unity 6 renomeou — `velocity` ainda compila mas dá warning. Usar `linearVelocity` consistente.
- **Tile assets como sub-assets.** Tile criados em `Assets/Settings/Tiles/*.asset` embedam `Texture2D` + `Sprite` como sub-assets via `AssetDatabase.AddObjectToAsset`. Pra trocar pelo sprite autoral depois, basta substituir o campo `m_Sprite` do `.asset` (ou repintar o tilemap com palette nova).
- **Slices de animação precisam ter altura/Y uniformes.** Se Idle e Walk de um personagem foram fatiados com Y ou altura diferentes, o `SimpleSpriteAnimator.AlignFeetToTransform` recalcula a posição na hora que o sprite muda — mas como ele só roda no `Awake`, mudar de animação faz o personagem "saltar" de altura. Padronize os retângulos de slice no Sprite Editor.
- **Builder de fase não pode tocar no `TextureImporter` de sprites em uso.** Cuidado ao chamar `importer.SaveAndReimport()` ou setar `importer.spritesheet` em editor utilities — sobrescreve qualquer slice/pivot manual e quebra a animação. `SpriteImportConfigurator` foi tirado do `Memory01Builder.BuildMemory01Tutorial` por isso.
- **PPU = pixel size do tile.** Tile de 32×32 px com PPU 100 (default do Unity) vira 0.32u no Grid 1u/cell — tudo desalinhado, gaps entre tiles. `SceneArtImportConfigurator` força PPU 32 nos tiles do residential pack. Mesma regra vale pra qualquer pack futuro: PPU = lado do tile em px.
- **BG cobrindo ortho size 5.** Layer 576×324 px em PPU 100 vira 5.76×3.24u — não cobre. PPU 50 vira 11.52×6.48u — ainda deixa faixas pretas nas laterais e topo (16:9 ortho 5 = 17.78×10u). PPU 32 → 18×10.125u cobre exatamente. Sempre teste em 16:9 antes de fechar a configuração do importer.
- **BG follow camera sem script.** No `Memory_01_Patio` (câmera móvel via `CameraFollow2D`), o `BG_CityNight` fica como child da Main Camera com `localPosition.z = 10` e `sortingOrder` negativo. Segue o player automaticamente, e `ParallaxLayer` (sway-only) ainda dá vida às camadas distantes. Sem precisar reescrever o `ParallaxLayer` pra ler `Camera.main`.
- **Fonte pixel ocupa muito mais largura.** Press Start 2P em 36pt cobre o que LiberationSans cobre em 18pt. Quando trocar de fonte default pra pixel font, **dividir os fontSize por 2** já é uma boa primeira aproximação. Layouts em coordenadas absolutas (sizeDelta de RectTransform) precisam de re-test visual.
- **Atenção com glyphs especiais na fonte pixel.** Press Start 2P não tem `▼`, `→`, aspas tipográficas elegantes (`"…"`), nem alguns acentos compostos. Substituir por `>>`, `--`, ASCII puro. `PixelFontBuilder` loga warning com a lista de glyphs faltantes — se aparecer no console, trocar o caractere no source.
