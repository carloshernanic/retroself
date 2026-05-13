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

Scripts adicionados no Sprint 3 (Dualidade Woody + polish): `PlayerKind`, `PlayerSwap`, `IdleBob`, `BreathingScale`, `LightningFlash`, `ParallaxLayer`, `DustPuff`, `HeavyBox`. Sprint 3.1 (narrativa + porta): `IntroDialogue`, `SchoolDoor`. Sprint 3.2 (pixel art autoral + animator): `PlayerAnimDriver`, `BullyAnimDriver`. Sprint 4 (Memory_02 co-op Pico Park): `PressurePlate`, `PressurePlateVisual`, `GatedDoor`, `CoopFinishDoor`. Sprint 4.1 (puzzles com stone-throw): `GateSource` (base abstrata pra fontes de gate), `StoneSwitch` (alvo ativado por pedra). `BreakablePlank` é só wiring (SpriteRenderer + BoxCollider2D + EnemyHealth(1) — Stone destrói via dano). **Os GUIDs deles ainda não foram registrados aqui** — depois do primeiro `Assets → Refresh` no Unity, rode `grep ^guid Assets/Scripts/<nome>.cs.meta` e cole acima caso vá hand-editar YAML de cena.

After editing a `.unity` file from outside the editor, the user must `Assets → Refresh` (Ctrl+R) or reopen the scene — Unity caches scene data in memory and won't pick up disk edits otherwise.

## Editor utilities

Builders são idempotentes (re-rodar substitui só os roots gerenciados — adições manuais sobrevivem) e adicionam as scenes no Build Settings automaticamente.

**Convenção de rebuild não-destrutivo:** cada builder declara um array `OwnedRoots` com os nomes dos GameObjects de scene-root que ele cria. `SceneRebuildHelpers.OpenOrNew(path)` abre a cena existente em vez de criar vazia, e `WipeOwnedRoots(scene, OwnedRoots)` destrói **só** os roots cujos nomes batem. Tudo fora dessa lista sobrevive — Light2D, Volumes adicionais, decorações que o usuário cria no editor. **Antes** de destruir um root gerenciado, `WipeOwnedRoots` faz um pass de preservação: qualquer `Light2D` ou `Volume` aninhado é unparented pra scene root via `SetParent(null, worldPositionStays: true)` — então uma luz que o usuário colocou como filha de "Lamp" ou "Main Camera" sobrevive ao rebuild, mantendo a posição visual. Profile do `Memory01_PostProcess.asset` também é preservado: `BuildPostProcessing` só inicializa defaults se o `.asset` ainda não existir.

- [Assets/Editor/MainMenuBuilder.cs](Assets/Editor/MainMenuBuilder.cs) — menu **Retroself → Build Main Menu Scene**. Builds `Assets/Scenes/MainMenu.unity` com `InputSystemUIInputModule`, parallax leve em 3 camadas BG (`ParallaxLayer`), overlay de raio (`LightningFlash`) e pulse de 1±2% no título via `BreathingScale`.
- [Assets/Editor/PrologueBuilder.cs](Assets/Editor/PrologueBuilder.cs) — menu **Retroself → Build Prologue Scene** (gera `Prologue.unity` com 7 painéis populados) e **Retroself → Build Memory_01_Patio Placeholder** (só cria a cena placeholder se ela ainda não existir, pra não sobrescrever o tutorial). Painéis 1–3 (chuvosos) têm `LightningFlash` overlay; aliens do painel 5/6 oscilam com `IdleBob` em fases offset; Woody dormindo/sentado/cabine respira com `BreathingScale`. Fade entre painéis já estava no `CutsceneController.GoToNextPanel`.
- [Assets/Editor/Memory01Builder.cs](Assets/Editor/Memory01Builder.cs) — menu **Retroself → Build Memory_01_Patio Tutorial**. Monta a fase tutorial completa: câmera + EventSystem (InputSystemUIInputModule), Tilemap_Ground com `CompositeCollider2D` + `Rigidbody2D` Static (geometryType Polygons, `compositeOperation = Merge`) — agora chama `tm.RefreshAllTiles() + tc.ProcessTilemapChanges() + cc.GenerateGeometry()` ao final pra cachear a geometria no `.unity` e evitar a janela de regeneração na transição Prologue→tutorial. Inclui fresta baixa (teto a y=−2 entre x=16..17), parede em x=20, HeavyBox marrom (RB Dynamic), **porta da escola** (`SchoolDoor`) em x=18.5 que carrega `Memory_02_Domingo` ao toque com fade preto, Bully com `IdleBob` leve, **dois Woody coexistindo** (`PlayerYoung` 0.55×0.95 e `PlayerAdult` 0.55×1.9) + `PlayerSwap`, CameraFollow2D, Volume URP global, HUD com health bar + hint + **diálogo de abertura** (`IntroDialogue` com `TypewriterText` + `BlinkUI` no continue indicator, congela ambos os Woody + Bully + PlayerSwap até o player avançar com Espaço/Enter; Esc pula).
- [Assets/Editor/SchoolPlaceholderBuilder.cs](Assets/Editor/SchoolPlaceholderBuilder.cs) — menu **Retroself → Build Memory_01_School Placeholder**. Gera `Memory_01_School.unity`: cena minimalista com texto "FIM DO TUTORIAL" + botão "Voltar ao Menu" (chama `MenuActions.VoltarMenu`). Destino default da `SchoolDoor` até a fase escola real ser construída.
- [Assets/Editor/SpriteImportConfigurator.cs](Assets/Editor/SpriteImportConfigurator.cs) — menu **Retroself → Configure Character Sprites**. Aplica `TextureImporter` settings de pixel art (PPU per-folder, FilterMode Point, no compression) + slice horizontal por contagem de frames em Multiple mode. Cobre os 3 personagens (Child_3/Homeless_1/Gangsters_2) **e** os 3 monstros decorativos do prologue (`free-pixel-art-tiny-hero-sprites/{Pink,Owlet,Dude}_Monster/*_Idle_4.png`, 4 frames cada). **Não é mais chamado pelo `Memory01Builder` em rebuild** — chama-se manualmente uma vez quando importa um sprite novo. Reescreve `importer.spritesheet` então sobrescreve qualquer slice/pivot manual feito no Sprite Editor; só rode se quiser resetar. Idempotente em si mesmo.
- [Assets/Editor/SceneArtImportConfigurator.cs](Assets/Editor/SceneArtImportConfigurator.cs) — menu **Retroself → Configure Scene Art Imports**. Aplica `TextureImporter` (FilterMode.Point, no compression, PPU + pivot per-folder) **somente** nos packs de cenário (`residential-area-tileset-pixel-art`, `free-scrolling-city-backgrounds-pixel-art`, `ghetto-tileset-pixel-art/3 Objects`, `cyberpunk-market-street-pixel-art/3 Objects/Vending machines`). PPU 32 pra Tiles/Props (1 cell = 1u no Grid), PPU 32 pros backgrounds da cidade (576×324 → 18×10.125u, cobre 16:9 ortho 5 exatamente). Pula explicitamente `Sprites/criancas/`, `Sprites/mendigos/`, `Sprites/gangsters/` — slicing manual frágil. Idempotente. Rode 1× depois de importar um pack novo.
- [Assets/Editor/SceneArtCatalog.cs](Assets/Editor/SceneArtCatalog.cs) — paths constantes pros sprites usados por `Memory01Builder`/`PrologueBuilder`/`MainMenuBuilder` (tiles, props, layers do CityNight, vending machine, sheet do Woody sentado, sheets dos 3 monstros, paths dos `.asset`). Inclui `LoadSprite(path)`, `LoadSpriteFrames(sheetPath, prefix)` (carrega só os sub-sprites do Multiple mode, ordena pelo sufixo `_N`) e `GetPixelFont()` com cache + fallback pra fonte default. Trocar de pack/tile = editar 1 linha aqui.
- [Assets/Editor/PixelFontBuilder.cs](Assets/Editor/PixelFontBuilder.cs) — menu **Retroself → Build Pixel Font Asset**. Lê `Assets/Fonts/PressStart2P-Regular.ttf` (OFL/Google Fonts) e gera `Assets/Fonts/PressStart2P-SDF.asset` via `TMP_FontAsset.CreateFontAsset` (SDFAA, atlas 512×512, padding 5, samplingPointSize 64). Pré-popula o atlas com ASCII + acentos pt-BR + travessão/aspas tipográficas. Embute Texture2D + Material como sub-assets. Re-rodar reescreve. Press Start 2P é citado nos créditos do MainMenu.
- [Assets/Editor/AnimatorBuilder.cs](Assets/Editor/AnimatorBuilder.cs) — menu **Retroself → Build Character Animators**. **DESATIVADO** no fluxo atual: `Memory01Builder` usa `SimpleSpriteAnimator` (runtime, sem AnimatorController). Existe ainda mas não é chamado — junto com `Assets/Animations/`, `PlayerAnimDriver`, `BullyAnimDriver` são órfãos e podem ser removidos no próximo cleanup.
- [Assets/Editor/Memory02Builder.cs](Assets/Editor/Memory02Builder.cs) — menu **Retroself → Build Memory_02_Domingo**. Monta a fase 2 (interior casa, basement-tileset). Espelha `Memory01Builder`: `OwnedRoots` + `SceneRebuildHelpers.OpenOrNew/ConfirmRebuild/WipeOwnedRoots`. **Não chama nada de post-processing** (usuário tuna manualmente). 3 puzzles co-op estilo Pico Park: caixa-em-placa (HeavyBox abre porta), plataforma de impulso (placa Adult eleva plataforma com jovem em cima), dual-plates AND com fresta (jovem na placa esquerda + adulto latched na direita destrava porta vertical). Sem inimigo AI — só `HazardZone` no pit em x=7..8. Saída: `CoopFinishDoor` em x=27 que carrega `MainMenu` (placeholder até Hub existir) quando os 2 Woody entram juntos.

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

**Saída (porta-escola):** `SchoolDoor` em x=18.5 — trigger que faz fade preto via `FadeOverlay` no Canvas e carrega `Memory_02_Domingo` (encadeia direto no tutorial 2; o `Memory_01_School` placeholder fica como fallback do default da classe). Porta exige **3 condições simultâneas**: ambos os Woody dentro do trigger + `bullyToDefeat` (ref `EnemyHealth` do porteiro) destruída + `KeyPickup.Collected == true`. `KeyPickup` é flag estática reseta em `RuntimeInitializeLoadType.SubsystemRegistration`. A chave **dropa do porteiro quando ele cai**: `KeyDropper` no Bully assina `EnemyHealth.OnDefeated` e chama `KeyPickup.Spawn(transform.position + offset)` antes do `Destroy(gameObject)`. **Só o jovem arremessa pedra**: `PlayerAttack.Update` early-returns se `controller.kind != PlayerKind.Young`.

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
- **Mantidos como placeholders** (decisão consciente): cabine roxa do MainMenu (Prologue Panel_03 já trocou pra vending machine cyberpunk), relógio do panel 7, glow do poste, school door — sem assets condizentes ou tom abstrato intencional.

**Assets reais no Prologue (Sprint 3.4):** segunda passada substituindo placeholders cinemáticos por sprites autorais nos painéis do `Prologue.unity`:
- **Panel_01_Praca / Panel_02_Acorda** → Woody adulto deitado/sentado vira sprite real animado (`Assets/Sprites/mendigos/Homeless_1/Idle_2.png`, 11 frames já sliced no Sprite Editor). `PrologueBuilder.CreateAnimatedSprite` instancia `SpriteRenderer` + `SimpleSpriteAnimator` decorativo (sem PlayerController/collider) com `idleFps = 5..6`. Panel_01 inclina o transform 75° pra simular encolhido no banco.
- **Panel_03_Cabine** → cabine roxa vira `cyberpunk-market-street-pixel-art/3 Objects/Vending machines/5.png` (vending machine como portal cyberpunk). Glow roxo continua atrás com `CabinPulse` no próprio glow.
- **Panel_05_Aliens / Panel_06_Oferta** → 3 quads RGB viram os 3 monstros do `free-pixel-art-tiny-hero-sprites` (Pink/Owlet/Dude, cada um com `Idle_4.png` 4 frames). `IdleBob` mantém respiração desencontrada via `phase` offset 0 / 0.66π / 1.33π.
- Painéis 04 (Flash), 07 (Relógio), e a Cabine no MainMenu **mantêm** placeholder — abstração intencional.

Pipeline de import dos novos packs: `SceneArtImportConfigurator` ganhou rule pra `cyberpunk-market-street-pixel-art/3 Objects/Vending machines` (PPU 32, BottomCenter); `SpriteImportConfigurator` ganhou as 3 entries dos monsters (PPU 32, BottomCenter, 4 frames horizontais). Build order: **(a) Configure Scene Art Imports → (b) Configure Character Sprites → (c) Build Prologue Scene**.

Pipeline de import: `SceneArtImportConfigurator` aplica PPU + FilterMode.Point por pasta; `SceneArtCatalog` centraliza paths constantes; `LoadResidentialTile`/`CreateRealSprite` (em cada builder) consomem.

Tom da fase tutorial: o jovem Woody (7 anos, sprite 16×16) e o adulto (16×32) coexistem; o jogador alterna entre os dois pra resolver passagens. Curva de aprendizado da Fase 1 segundo GDD §3.2: ensina **movimento e dualidade de tamanho**, sem congelamento de tempo ainda (esse só vem na Fase 2).

## Estado do tutorial 2 (`Memory_02_Domingo`)

Fase 2 jogável montada por `Memory02Builder.cs` (menu **Retroself → Build Memory_02_Domingo**). Foco: **co-op assimétrico estilo Pico Park** no interior da casa, sem combate, sem time-freeze (esse fica pra Memory_03+). Tom narrativo: domingo dos 12 anos quando o Woody adulto ouvia os pais brigarem do quarto — `IntroDialogue` apresenta a fala ao spawn (Espaço/Enter avança, Esc pula).

**Setting**: tom basement (cores marrom/madeira). **Chão é solid-color quads** (não tilemap) — os PNGs em `basement-tileset-pixel-art/1 Tiles/` são pedaços de **parede vertical** (rivets, pilares, escadas), não floor tile, então renderizam como pilarzinhos quebrados quando usados como ground. `Memory02Builder.BuildPlatform` cria quads `SpriteRenderer` (`SolidSprite()`) + `BoxCollider2D`: `FloorTopCol` (marrom claro) na linha de superfície y=-3.2, `FloorCol` (escuro) na base, `WallCol` na parede de fundo. Props decorativos em `3 Objects/{1 Pipe, 2 Box, 3 Decoration}` continuam vindo dos PNGs reais (esses são sprites coerentes). Convenção Y igual Memory_01: topo do chão em world y=-3.

**Objetos visuais grandes** — placas (1.5×0.4), gates verticais (0.6×3.0, sobem 3.5u quando abertos), HeavyBox via sprite real do basement pack. Layout flat (sem pit/fresta) — friction de plataforma não agrega à coordenação.

**5 puzzles** em sequência — redesenho **v2 (Sprint 4.1)** focado em **fazer pensar**: introduz `K = arremessar pedra` (Young-only) como verbo de puzzle, fecha com **cofre de senha** estilo Genius/Simon (Sprint 4.2), e adiciona **câmara da chave** trancada à esquerda do spawn que só abre via `ReturnPad` (Sprint 4.3). Inspirações: Trine (ferramenta-por-personagem), Portal 2 co-op (alvo remoto), Pico Park (timing), Genius/Simon (memória + ordem).

1. **"A pedra abre caminho" (x=0..10)** — `BreakablePlank` em x=2 bloqueia o caminho do Adult (h=2.5, top y=-0.5; Adult não pula por cima). HeavyBox em x=4, `Plate_Box` (HeavyBox-only) em x=6, gate non-latched em x=8. **Solução**: Young arremessa pedra (K) → plank some → Adult atravessa, empurra caixa pra placa → gate abre enquanto caixa segura. Ensina o verbo "Stone como chave" + ordem obrigatória de personagens. **Pista do cofre**: número "1" em verde na parede em x=3 (1ª cor da senha).
2. **"Mira impossível sem ajuda" (x=12..22)** — `StoneSwitch` (latched) em x=17, y=-0.3 — alto demais pra Young atirar do chão. HeavyBox em x=13. Gate latched em x=20, source = stoneSwitch. **Solução**: Adult empurra caixa até x=17 (debaixo do switch). Tab → Young pula em cima da caixa, **pula da caixa**, atira K horizontal no ápice → acerta. Switch latcheia, gate abre pra sempre. Ensina caixa-como-degrau + mira em movimento. **Pista do cofre**: número "2" em vermelho em x=14.
3. **"Atrás de mim, atire" (x=24..32)** — `Plate_Adult_Hold` (Adult-only) em x=24.5 → Gate_C em x=26 **latched** (assim que Adult pisa uma vez, latcheia aberto pra sempre — facilita revisita via ReturnPad). Pós-gate (câmara do Young): `BreakablePlank` em x=28 + `StoneSwitch` (latched) em x=30, y=-1. Gate_Final latched em x=32. **Solução**: Adult fica em Plate_Adult_Hold (Gate_C abre). Tab → Young (Adult continua na plate mesmo inativo). Young atravessa, quebra plank, atira pulando no switch. Gate_Final latcheia. Young volta atravessando Gate_C. Ambos saem. Ensina o **conceito-chave** dos puzzles assimétricos: um segura, o outro age — e o swap não move o inativo. **Pista do cofre**: número "3" em azul em x=29 (atrás da plank — só visível depois de Young quebrar).
4. **"O cofre da memória" (x=33..42)** — três `StoneSwitch` coloridos (vermelho x=35, verde x=37, azul x=39) **non-latched** em y=-2.5 (Young atira horizontal do chão). Sem instrução escrita — o jogador deduz que é uma senha pelas pistas numeradas dos puzzles anteriores. `SequenceLock` (também `GateSource`) escuta `OnHit` de cada switch, valida ordem, **reseta tudo se errar**. Quando completa, latcheia `IsActive=true` → `Gate_Lock` em x=42 latched abre. **Solução**: combinar pistas (1=verde, 2=vermelho, 3=azul) → ordem é **VERDE → VERMELHO → AZUL**. Errou = todos os switches voltam a off, jogador tenta de novo. Ensina **memória + observação** (não é só motor).
5. **"O degrau invisível" (x=-15..-5)** — câmara da chave que fica trancada por `LeftLockedDoor` em x=-5 (latched, sem sources iniciais). Só abre quando o jogador pisa pela 1ª vez no `ReturnPad` (que liga `ForceOpen()`). Layout: `Plate_Adult_Lift` em x=-7 (Adult-only, **non-latched**). `Elevator` (`GatedDoor` 1.6×0.4, openOffset (0,4), latched=false) em x=-12 carrega Young pelo `MovePosition` do Kinematic RB. `Alcove` plataforma fixa em x=-13 top y=+1.4 (mesma altura do elevador aberto). `KeyPickup.Spawn` em x=-13, y=+1.8. **Solução**: chegou na finish e está trancada → volta no ReturnPad → porta esquerda destranca. Tab→Young entra, sobe no elevador parado. Tab→Adult vai pra placa. Elevador sobe carregando Young. Young pega chave (`KeyPickup.Collected=true`). `ReturnPad` se auto-destrói. Ambos voltam pro x=45 — `CoopFinishDoor.requireKey=true` agora satisfeita. **Sem fresta de teto** — o puzzle se auto-enforça pela placa Adult-only + dependência do elevador (Adult não pode estar nos dois lugares); a fresta visual era redundante e atrapalhava a leitura sobre o botão. Ensina **cooperação espacial-assimétrica**: Adult segura, Young age.

**Primitivas novas (Sprint 4.1 + 4.2 + 4.3)**:
- `GateSource` — base abstrata pra qualquer fonte de gate. `PressurePlate`, `StoneSwitch` e `SequenceLock` herdam. `GatedDoor.sources` é `List<GateSource>` (aceita placas, switches e locks juntos no AND).
- `StoneSwitch` — alvo na parede que vira `IsActive=true` quando colide com componente `Stone`. Suporta `latched` (default true). **Importante**: ignora hits duplicados mesmo com `latched=false` — re-arma só via `ResetSwitch()`. Reusa `PressurePlateVisual` pro feedback de cor.
- `SequenceLock` — cadeado de senha estilo Genius. Lista de `StoneSwitch` + `expectedOrder` (índices). Em `Start`, se inscreve em `OnHit` de cada switch via lambda capturando `idx`. Hit certo → progresso++. Hit errado → `progress=0` + `ResetSwitch()` em todos. Hit completo → `unlocked=true` permanente.
- `BreakablePlank` — não tem script próprio: SpriteRenderer + BoxCollider2D **não-trigger** (bloqueia Adult) + `EnemyHealth(maxHealth=1)`. `Stone.OnTriggerEnter2D` já chama `EnemyHealth.TakeDamage` → `OnDefeated` → `Destroy`. Plank some, caminho libera.
- `GatedDoor.ForceOpen()` — destrava em runtime sem precisar de uma `GateSource`. Usado pra portas que começam fechadas e abrem por evento externo (ex: `LeftLockedDoor` do P5 destravada pelo `ReturnPad`).
- `CoopFinishDoor.requireKey` — flag opcional que exige `KeyPickup.Collected=true` pra disparar. Usada na finish do P5 (e poderia ser usada em qualquer fase futura).
- `KeyPickup.Reset()` — reset estático público (complementa `RuntimeInitializeOnLoadMethod` que só roda no boot do app). Necessário pra cross-cena.
- `SceneStartReset` — MonoBehaviour de `Awake` que chama `KeyPickup.Reset()`. Instalado nos roots de Memory_01 e Memory_02 pra evitar que coletar chave em uma fase deixe a outra "pré-aberta".

**Saída (porta de fim)**: `CoopFinishDoor` em x=45 (logo após o `ReturnPad` em x=43.5 que fica entre Gate_Lock x=42 e a porta). Trigger exige **ambos os Woody dentro** simultaneamente **+ chave coletada** (`requireKey=true` setado pelo builder). Carrega `MainMenu` (TODO: trocar pra `Hub`). Fade preto via `FadeOverlay`.

**ReturnPad** (`Assets/Scripts/ReturnPad.cs`): plataforma cyan na saída. Quando qualquer player pisa, teleporta os 2 Woody pras posições iniciais (x=-3 / -1.5, y=-2) e zera `linearVelocity`. Cooldown de 1.5s evita re-trigger. **Primeira pisada também destrava `LeftLockedDoor` do P5** (`doorsToOpenOnFirstUse` chama `GatedDoor.ForceOpen()`). **Auto-destrói quando `KeyPickup.Collected=true`** (poll em `Update`) — uma vez que a chave foi pega, a câmara já cumpriu seu papel e o pad some. Permite revisitar pistas no pré-chave; pós-chave o jogador só atravessa pra finish.

**Comandos**: A/D mover, Espaço/W/↑ pular, **K atirar pedra** (Young), **Tab trocar Woody**.

**Post-processing + Light2D**: o usuário tuna o Volume e adiciona Light2D manualmente após o build. `Memory02Builder` **não cria** Global Volume nem `Light2D`, mas garante os 2 pré-requisitos pra que o tuning manual funcione: (a) `renderPostProcessing=true` na Main Camera (sem isso Volume é ignorado); (b) todos os `SpriteRenderer` usam `Universal Render Pipeline/2D/Sprite-Lit-Default` (sem isso Light2D não afeta — sintoma é "adicionei luz e nada mudou"). Volume e Light2D adicionados no editor sobrevivem ao rebuild via `SceneRebuildHelpers.WipeOwnedRoots` (lifted pra scene root antes de destruir owned roots).

Build order quando importar pack ou trocar tile: **(a) Configure Scene Art Imports → (b) Build Memory_02_Domingo**. Não rode Configure Character Sprites de novo — slicing manual sobreviveu da Memory_01.

## Estado da fase final (`Memory_04_Sala`)

Fase final do jogo, montada por `Memory04Builder.cs` (menu **Retroself → Build Memory_04_Mercado**). Maior fase: span x=-5..70 (~75u). Setting: mercado cyberpunk noturno (`cyberpunk-market-street-pixel-art` pack). Narrativa: Woody adolescente (15-16 anos) fugindo de casa pro mercado — "o mercado nunca pedia explicação". Tom melancólico-esperançoso, sem palavrões (linha do `IntroDialogue` aprovada pelo usuário).

**6 puzzles** em sequência, todos reusando primitivas existentes (sem types novos de plate/gate):

1. **"A catraca" (x=5..15)** — `HeavyBox` com sprite `business-center/Money.png` em x=8 (coin-as-box). Adult empurra até `Plate_Coin` (HeavyBox-only) em x=12 → `Gate_Entry` (latched) em x=14 abre. Pista do P5: número "1" em **vermelho** em x=4.
2. **"Cabine Snake" (x=18..23)** — `ArcadeMachine` (subclasse de `GateSource`) com sprite `MarketVending1` em x=20. Trigger zone 1.8×2.6 mostra prompt "[ESPAÇO]" quando player encosta. Espaço → `SnakeMinigame.Open()` (overlay Canvas filho, sortingOrder 110). Win = 10 frutas → `cleared=true` → `Gate_Snake` em x=23 abre permanente.
3. **"Errand das comidas" (x=25..35)** — 3 plataformas elevadas em x=26/29/32 (alturas variadas, `Adult` empurra `HeavyBox` em x=24.5 como degrau). 3 `FoodPickup` (`FoodKind.Burger`) nas plataformas, Young coleta via `OnTriggerEnter2D` → `FoodInventory.Add()` estático. `VendorStall` (Foodtruck1.png) em x=33 com `required = [{Burger, 3}]`; quando satisfeito, vira `IsActive=true` latched → `Gate_Vendor` em x=35 abre. Pista do P5: número "2" em **verde**.
4. **"Cabine Guitar Hero" (x=42)** — `ArcadeMachine` com `MarketVending6` + NPC `cyberpunk-pixel-bar-cafe-npc/6/PlayGuitar.png` decorativo na frente. Overlay com 2 lanes verticais (Young amarela esquerda, Adult marrom direita), `BeatMap_M04.asset` carregado de `Assets/Settings/`. Tab alterna `activeLane`, Espaço dá hit na lane ativa, Z/X forçam lane 0/1. Hit window ±0.18s, passThreshold 70%. `AudioClip song` opcional (placeholder usa `SfxBeep.PlayBeatHit/Miss` como metrônomo). Win → `Gate_GH` em x=45. Pista P5: "3" em **azul** em x=44.
5. **"Senha das vending machines" (x=48..58)** — 3 vending decor (`Vending2/3/4`) + 3 `StoneSwitch` coloridos non-latched em y=-1: vermelho x=50, verde x=53, azul x=56. `SequenceLock` com `expectedOrder = [0, 1, 2]` → ordem **VERMELHO → VERDE → AZUL** (combina com clue numbering dos puzzles anteriores: 1=red, 2=green, 3=blue). Errar reseta todos. Acertar → latcha `IsActive=true` → `Gate_Lock` em x=58 abre.
6. **"Saída" (x=63..68)** — `ReturnPad` em x=63 (revisita pistas/respawn). `CoopFinishDoor` em x=68 com `requireKey=false` (sem KeyPickup nessa fase — gates já são o gate) → carrega `SceneNames.Memory_04_Cutscene_Placeholder`.

**Câmera**: bounds minX=-3, maxX=66, minY=-1, maxY=4. Spawn Young x=-3, Adult x=-1.5, y=-2 (convenção das outras fases — ground top y=-3). BG paralax de 5 layers night parented na Main Camera (PPU 32 cobre ortho 5).

**Saída para cutscene**: `Memory_04_Cutscene_Placeholder.unity` (montada por `Memory04CutscenePlaceholderBuilder`) é um placeholder minimalista — texto "FIM DA MEMORIA" + botão "Voltar ao Menu" via `MenuActions.VoltarMenu()`. Substituir pela cutscene real depois.

### Framework de minigame embedded

Primitivas novas pra suportar arcades dentro de fase, sem trocar de cena:

- **`MinigameOverlay`** (`Assets/Scripts/MinigameOverlay.cs`) — base abstrata. `Open(System.Action onWin = null)` faz `Time.timeScale = 0`, congela players via `SetGameplayFrozen` (zera velocidades + disable `PlayerController`/`PlayerAttack`/`PlayerSwap`), ativa o `canvas` filho. Subclasses sobrescrevem `OnStart()` (init), `OnEnd(bool won)` (cleanup) e `TickGame()` (chamado por `Update()` quando aberto). `Win()`/`Lose()` chamam `Close(won)` que restaura `timeScale=1` + dispara `onWinCallback` se won.
- **`ArcadeMachine : GateSource`** (`Assets/Scripts/ArcadeMachine.cs`) — trigger box + sprite renderer. Refs: `MinigameOverlay overlay`, `GameObject promptIndicator`. `OnTriggerEnter/Exit2D` controla `playerInside` (HashSet de PlayerController). `Update` lê `Keyboard.current.spaceKey.wasPressedThisFrame` quando alguém está dentro e o overlay não está aberto → `overlay.Open(() => cleared = true)`. `IsActive => cleared` (latched permanente — gate fica aberto pra sempre após win).
- **`SnakeMinigame : MinigameOverlay`** — grid 16×12 (configurável), snake de 3 segmentos, move a cada 0.15s usando `Time.unscaledDeltaTime` (timeScale=0!). `Keyboard.current.{up,down,left,right}ArrowKey.wasPressedThisFrame` muda direção. Spawna sprite random de `foodSprites` em célula livre. Win = `targetScore` (default 10), Esc = lose, Space re-tenta no game over.
- **`GuitarHeroMinigame : MinigameOverlay`** — 2 RectTransforms lane (`laneYoungArea`/`laneAdultArea`), Image quadrada por nota interpolada do topo até hit zone em `noteFallTime` (1.8s). `BeatMap` ScriptableObject (`{ List<Note { time, lane }> }`) define quando cada nota nasce. `songTime` acumula `unscaledDeltaTime`; spawna nota `noteFallTime` antes do `time`. Tab alterna `activeLane`, Espaço dá hit na ativa, Z/X forçam lane 0/1. `CheckFinish`: accuracy = hits/totalNotes; >=`passThreshold` (default 0.7) = win.
- **`BeatMap`** (`Assets/Scripts/BeatMap.cs`) — `ScriptableObject` simples com `List<Note>`. Permite trocar música/timing sem rebuildar a cena. `BeatMapPlaceholderBuilder` (menu **Retroself → Build BeatMap Placeholder**) gera `Assets/Settings/BeatMap_M04.asset` com 30 notas distribuídas em ~25s (lead 2.5s, intervalo 0.8s, swap lane a cada 4 notas pra ensinar Tab). **Idempotente**: se o `.asset` existir com notas, pula (preserva ajustes manuais — pra regerar, deletar primeiro).
- **`FoodPickup` + `FoodInventory`** (`Assets/Scripts/{FoodPickup,FoodInventory}.cs`) — inventário estático cross-cena. `FoodKind` enum (Burger/Sushi/Noodle/Drink/Dessert). `FoodInventory.Items` é `List<FoodKind>` estática, reseta via `SceneStartReset.Awake()` (junto com `KeyPickup.ResetCollected()`).
- **`VendorStall : GateSource`** — poll em `Update` checa se `FoodInventory.Has(kind, count)` pra cada `Need` em `required`. Quando todos satisfeitos, latcha `IsActive=true` + dispara `OnSatisfied` UnityEvent.

**Como adicionar um minigame novo**: (a) subclassar `MinigameOverlay`, implementar `OnStart`/`OnEnd`/`TickGame`; (b) no builder, construir Canvas filho do ArcadeCabin com `sortingOrder` 110+, panel desativado por default, populá-lo com UI necessária, anexar componente do minigame e wirá-lo; (c) configurar `ArcadeMachine.overlay = <componente>`. O cabin vira `GateSource` automaticamente; usar como source de um `GatedDoor` pra destravar caminho.

### Cuidados (lições novas do M04)

- **`Time.timeScale = 0` exige `unscaledDeltaTime` em tudo.** Qualquer movement, animation, ou interval lógico DENTRO de um `MinigameOverlay` precisa usar `Time.unscaledDeltaTime`/`unscaledTime`, senão congela junto com o resto do jogo. `Keyboard.current.<key>.wasPressedThisFrame` continua funcionando normalmente (Input System bypassa Time).
- **`Keyboard.current.spaceKey.wasPressedThisFrame` na `ArcadeMachine` requer prompt visual claro.** Sem hint na tela, jogador não descobre que Espaço abre a cabine — o `ArcadeMachine.promptIndicator` (filho ativado quando `playerInside == true`) é mandatório, não opcional.
- **Canvas do minigame overlay**: `RenderMode.ScreenSpaceOverlay` + `sortingOrder=110` (HUD principal fica em 100). `CanvasScaler` em `ScaleWithScreenSize` 1920×1080. Painel filho desativado por default — `MinigameOverlay.Open()` ativa.
- **`BeatMap` placeholder vs música real**: o `time` no beatmap é segundos desde Play. Se trocar `AudioClip song` por música real com BPM diferente, regere ou re-mapeie o beatmap manualmente. `BeatMapPlaceholderBuilder` é idempotente pra preservar ajustes — pra regenerar do zero, deletar `BeatMap_M04.asset` primeiro.
- **`FoodInventory` precisa entrar no `SceneStartReset`.** Mesma armadilha do `KeyPickup`: campos estáticos sobrevivem entre `SceneManager.LoadScene`. `SceneStartReset.Awake()` agora chama tanto `KeyPickup.ResetCollected()` quanto `FoodInventory.ResetInventory()` — qualquer flag/inventory estático novo precisa ser adicionado ali.
- **`ArcadeMachine` pós-win**: `cleared=true` persiste no scene runtime mas não cross-cena (instância destruída). Jogador pode revisitar a cabine; prompt some (já cleared). Opcionalmente o `MinigameOverlay` pode permitir replay sem flipar `cleared` (não implementado — gates ficam abertos de qualquer jeito).
- **`PressurePlate` aceita sprite de moeda** pra tematizar como "catraca": a sprite é puramente visual (`SpriteRenderer`); o filtro mecânico (`requirement = HeavyBoxOnly`) é independente. `HeavyBox` com sprite `Money.png` empurrado pra plate vira "coin-operated turnstile" sem código novo.
- **Build order do M04**: (a) **Configure Scene Art Imports** (uma vez, pra cyberpunk-market + food packs); (b) **Build BeatMap Placeholder** (gera `BeatMap_M04.asset`); (c) **Build Memory_04_Cutscene Placeholder**; (d) **Build Memory_04_Mercado**. Sem o BeatMap, `GuitarHeroMinigame.beatMap` fica null e o builder loga warning.

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
- **Builder não pode resetar o VolumeProfile.** Versão antiga do `BuildPostProcessing` rodava `DestroyImmediate` em loop em `profile.components` e re-adicionava overrides hardcoded a cada rebuild — apagava o tuning manual de Vignette/FilmGrain/ColorAdjustments. Hoje o builder só inicializa defaults se o `.asset` não existir; se existir, mantém o que estiver lá. Mesma lição do `SpriteImportConfigurator`: builder não toca em arquivos que o usuário ajusta no Inspector.
- **`EditorSceneManager.OpenScene` recarrega do disco.** Edições não salvas no editor (Inspector tweaks, GameObjects adicionados sem salvar) são descartadas no reload. `SceneRebuildHelpers.OpenOrNew` por isso usa `GetActiveScene()` direto se a cena alvo já está aberta — só recorre a `OpenScene` quando precisa trocar de cena (e nesse caso pede pra salvar dirty scenes primeiro).
- **PPU = pixel size do tile.** Tile de 32×32 px com PPU 100 (default do Unity) vira 0.32u no Grid 1u/cell — tudo desalinhado, gaps entre tiles. `SceneArtImportConfigurator` força PPU 32 nos tiles do residential pack. Mesma regra vale pra qualquer pack futuro: PPU = lado do tile em px.
- **BG cobrindo ortho size 5.** Layer 576×324 px em PPU 100 vira 5.76×3.24u — não cobre. PPU 50 vira 11.52×6.48u — ainda deixa faixas pretas nas laterais e topo (16:9 ortho 5 = 17.78×10u). PPU 32 → 18×10.125u cobre exatamente. Sempre teste em 16:9 antes de fechar a configuração do importer.
- **BG follow camera sem script.** No `Memory_01_Patio` (câmera móvel via `CameraFollow2D`), o `BG_CityNight` fica como child da Main Camera com `localPosition.z = 10` e `sortingOrder` negativo. Segue o player automaticamente, e `ParallaxLayer` (sway-only) ainda dá vida às camadas distantes. Sem precisar reescrever o `ParallaxLayer` pra ler `Camera.main`.
- **Fonte pixel ocupa muito mais largura.** Press Start 2P em 36pt cobre o que LiberationSans cobre em 18pt. Quando trocar de fonte default pra pixel font, **dividir os fontSize por 2** já é uma boa primeira aproximação. Layouts em coordenadas absolutas (sizeDelta de RectTransform) precisam de re-test visual.
- **Atenção com glyphs especiais na fonte pixel.** Press Start 2P não tem `▼`, `→`, aspas tipográficas elegantes (`"…"`), nem alguns acentos compostos. Substituir por `>>`, `--`, ASCII puro. `PixelFontBuilder` loga warning com a lista de glyphs faltantes — se aparecer no console, trocar o caractere no source.
- **`PressurePlate` continua ativa quando o player ativo é trocado (Tab).** A placa detecta colliders no trigger via `OnTriggerEnter2D` e filtra por `PlayerKind` no componente, **não** por `PlayerController.IsActive`. Quando `PlayerSwap` desabilita `PlayerController` no inativo, o RigidBody/Collider continuam lá — então uma placa Adult-only com Adult parado em cima continua ativa mesmo após Tab → Young. **Esse é o fundamento do puzzle 3 do Memory_02**: Adult segura Plate_Adult_Hold (gate abre), Tab → Young, Young usa o gate aberto enquanto Adult fica imóvel mas ainda pesa na placa.
- **Stone vs collider sólido com EnemyHealth.** `Stone.cs` tem `CircleCollider2D isTrigger`. Quando bate em qualquer collider sólido (mesmo non-trigger) com `EnemyHealth`, o `OnTriggerEnter2D` da Stone dispara (Unity 2D: trigger-vs-non-trigger fires triggers em ambos), Stone chama `EnemyHealth.TakeDamage(1)`, e Stone se destrói. Isso permite criar `BreakablePlank` (parede sólida que Adult colide + EnemyHealth + sem script próprio) e Young destruí-la com pedra. Reuso elegante: nem Stone nem EnemyHealth precisaram mudar.
- **Flag estática NÃO sobrevive ao boot mas sobrevive a `LoadScene`.** `RuntimeInitializeOnLoadMethod(SubsystemRegistration)` só roda no boot do app — `SceneManager.LoadScene` mantém todos os campos estáticos. Caso real: `KeyPickup.Collected` virou `true` na Memory_02, o jogador volta pro MainMenu, entra Memory_01: a `SchoolDoor` lê a flag ainda `true` e abre sem o jogador matar o porteiro. Solução: criar um `SceneStartReset : MonoBehaviour` com `Awake() { KeyPickup.Reset(); }` e adicionar uma instância no scene root das duas fases que dependem da flag. Padrão extensível pra qualquer outra flag estática que vier (`StoneCollected`, `Defeated`, etc.) — adicionar no mesmo `Reset()`.
- **`GatedDoor` sem sources + `ForceOpen()`.** Pra portas que começam fechadas e abrem por evento externo (não por placa nem switch), basta criar um `GatedDoor` com `latched=true` e lista `sources` vazia — fica fechada permanente. Em runtime, chamar `door.ForceOpen()` seta o `latchedOpen=true` interno e o `FixedUpdate` move ela pra `closedPos + openOffset` na velocidade configurada. Evita ter que criar um `GateSource` dummy só pra pulsar.
