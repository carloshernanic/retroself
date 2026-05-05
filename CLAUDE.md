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
├── Prefabs/
├── Scenes/       (MainMenu.unity, Prologue.unity, Memory_01_Patio.unity, SampleScene.unity)
├── Scripts/      (runtime MonoBehaviours — flat, no subfolders)
├── Settings/     (URP renderer assets + Memory01_PostProcess.asset + Tiles/ com Tile sub-assets)
├── Sprites/      (autoral pixel art — added by user)
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

After editing a `.unity` file from outside the editor, the user must `Assets → Refresh` (Ctrl+R) or reopen the scene — Unity caches scene data in memory and won't pick up disk edits otherwise.

## Editor utilities

Builders são idempotentes (re-rodar substitui a cena) e adicionam as scenes no Build Settings automaticamente.

- [Assets/Editor/MainMenuBuilder.cs](Assets/Editor/MainMenuBuilder.cs) — menu **Retroself → Build Main Menu Scene**. Builds `Assets/Scenes/MainMenu.unity`. ⚠️ Still uses legacy `StandaloneInputModule` (line 33) — não foi migrado pro `InputSystemUIInputModule` ainda; corrigir quando tocar nele.
- [Assets/Editor/PrologueBuilder.cs](Assets/Editor/PrologueBuilder.cs) — menu **Retroself → Build Prologue Scene** (gera `Prologue.unity` com 7 painéis populados) e **Retroself → Build Memory_01_Patio Placeholder** (só cria a cena placeholder se ela ainda não existir, pra não sobrescrever o tutorial).
- [Assets/Editor/Memory01Builder.cs](Assets/Editor/Memory01Builder.cs) — menu **Retroself → Build Memory_01_Patio Tutorial**. Monta a fase tutorial completa: câmera + EventSystem (InputSystemUIInputModule), Tilemap_Ground com `CompositeCollider2D` + `Rigidbody2D` Static (geometryType Polygons, `compositeOperation = Merge`), HazardPit, Bully, Player (com PlayerController/Health/Attack/Animator), CameraFollow2D, Volume URP global apontando pra `Assets/Settings/Memory01_PostProcess.asset`, e HUD (título, hint de controles, barra de vida).

## Working style

- **Authorship convention:** sprites and music are autoral (made by the user) and added in post. We work with placeholders or empty `SpriteRenderer`/`AudioSource` slots — never generate or commit asset binaries. The `Audio/` and `Sprites/` folders are intentionally empty.
- **Tutorial reference:** `c:/Users/Usuário/tutorial-base/` (cloned from `carloshernanic/Tutorial-Jogos`) is the structural reference the user follows for Unity patterns (`MenuActions` style, scene flow, etc.). Use it as a *pattern* reference, not as content to copy.
- **Don't introduce abstractions or subfolders** unless asked. Keep the layout flat and the scripts simple — match the tutorial's cadence.

## Sprint deliverables (course context)

This is an academic project for Insper, Jogos Digitais 2026. Tracking docs:
- [docs/sprint2-entrega.md](docs/sprint2-entrega.md) — current sprint deliverable (Milanote link, GitHub repo, YouTube video) + selected rubrics (doubled: #1 Conceito/Narrativa, #2 Mechanics; discarded: #4 UI/HUD, #9 Replayability).
- [docs/gdd.md](docs/gdd.md) — GDD in DDE format (Game Concept / Design / Dynamics / Experience / Inspirações).

Team: Alex Chequer, Carlos Hernani, Lucas Ikawa.

## Estado do tutorial (`Memory_01_Patio`)

Fluxo atual: `MainMenu` → `Prologue` (cutscene 7 painéis) → `Memory_01_Patio` (tutorial jogável). A fase tutorial cobre as 3 rubricas selecionadas:

1. **Player completo** — `PlayerController` (mover A/D/setas, pular Espaço/W/↑, coyote 0.1s + jump buffer 0.1s), `PlayerHealth` (3 HP, knockback, invencibilidade com flicker), `PlayerAttack` (K arremessa pedra), `PlayerAnimator` (squash/stretch + tint placeholder até sprites autorais). Combate não-letal conforme GDD §2.2 atualizado: jovem arremessa pedra que atordoa; sem morte gráfica.
2. **Mapa com pós-processamento** — Tilemap_Ground com chão de grass/dirt e plataformas de madeira (tile assets em `Assets/Settings/Tiles/`), HazardPit no meio. Volume URP global em `Assets/Settings/Memory01_PostProcess.asset` com Bloom (intensity 0.18, threshold 1.1, tint quente), Vignette (0.35) e ColorAdjustments (exposure −0.2, contrast 10, saturation −3, filtro creme).
3. **Inimigo completo** — `BullyController` (patrulha entre `patrolMinX`/`MaxX`, detecta player por |dx|<6 ∧ |dy|<2, persegue mais rápido, troca cor patrol→chase). `EnemyHealth` (3 HP, stun 0.6s ao levar pedrada). Bully toma dano da pedra mas continua patrulhando enquanto não morre. **Bound de patrulha clampa até em chase** pra não cair no poço.

Comandos: A/D ou ←/→ mover, Espaço/W/↑ pular, K arremessar pedra. HUD mostra título da fase, hint de controles e barra de vida. Cair no HazardPit reinicia o checkpoint.

Tom da fase tutorial: o jovem Woody (7 anos, sprite 16×16) é o protagonista jogável principal aqui; o adulto entra como "tio estranho" que ajuda. Curva de aprendizado da Fase 1 segundo GDD §3.2: ensina **movimento e dualidade de tamanho**, sem congelamento de tempo ainda (esse só vem na Fase 2).

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
