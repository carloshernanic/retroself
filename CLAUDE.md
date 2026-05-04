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
├── Settings/     (URP renderer assets)
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

After editing a `.unity` file from outside the editor, the user must `Assets → Refresh` (Ctrl+R) or reopen the scene — Unity caches scene data in memory and won't pick up disk edits otherwise.

## Editor utilities

Both builders are idempotent (re-running replaces the scene file) and add their scenes to Build Settings automatically.

- [Assets/Editor/MainMenuBuilder.cs](Assets/Editor/MainMenuBuilder.cs) — menu **Retroself → Build Main Menu Scene**. Builds `Assets/Scenes/MainMenu.unity`. ⚠️ Still uses legacy `StandaloneInputModule` (line 33) — não foi migrado pro `InputSystemUIInputModule` ainda; corrigir quando tocar nele.
- [Assets/Editor/PrologueBuilder.cs](Assets/Editor/PrologueBuilder.cs) — menu **Retroself → Build Prologue Scene** (gera `Prologue.unity` com 7 painéis populados + `Memory_01_Patio.unity` placeholder) e **Retroself → Build Memory_01_Patio Placeholder** (só o placeholder). Já usa `InputSystemUIInputModule`.

## Working style

- **Authorship convention:** sprites and music are autoral (made by the user) and added in post. We work with placeholders or empty `SpriteRenderer`/`AudioSource` slots — never generate or commit asset binaries. The `Audio/` and `Sprites/` folders are intentionally empty.
- **Tutorial reference:** `c:/Users/Usuário/tutorial-base/` (cloned from `carloshernanic/Tutorial-Jogos`) is the structural reference the user follows for Unity patterns (`MenuActions` style, scene flow, etc.). Use it as a *pattern* reference, not as content to copy.
- **Don't introduce abstractions or subfolders** unless asked. Keep the layout flat and the scripts simple — match the tutorial's cadence.

## Sprint deliverables (course context)

This is an academic project for Insper, Jogos Digitais 2026. Tracking docs:
- [docs/sprint2-entrega.md](docs/sprint2-entrega.md) — current sprint deliverable (Milanote link, GitHub repo, YouTube video) + selected rubrics (doubled: #1 Conceito/Narrativa, #2 Mechanics; discarded: #4 UI/HUD, #9 Replayability).
- [docs/gdd.md](docs/gdd.md) — GDD in DDE format (Game Concept / Design / Dynamics / Experience / Inspirações).

Team: Alex Chequer, Carlos Hernani, Lucas Ikawa.

## Próxima entrega (tutorial em `Memory_01_Patio`)

Estado atual: `MainMenu` → `Prologue` (cutscene de 7 painéis funcional) → `Memory_01_Patio` (cena placeholder vazia, só com texto "em construção"). Próximo passo é transformar `Memory_01_Patio` no **nível tutorial** que ensina os fundamentos do jogo enquanto cumpre 3 rubricas:

1. **Player completo** — movimento (correr/pular), animações (idle/walk/jump/attack), e ataque não-letal. Decidido (opção **a** do conflito GDD vs rubric, 2026-05-04): GDD foi atualizado pra incluir combate. Jovem arremessa pedras (curto alcance, atordoa); adulto golpeia corpo-a-corpo (empurra/derruba). Combate sem morte gráfica — inimigo cai/foge ou é atordoado por X segundos. Barra de vida do personagem ativo no HUD; zerou → reinicia checkpoint.
2. **Mapa com pós-processamento** — primeiro nível real (não placeholder) com tilemap, paleta amarelo-escolar/marrom-tijolo da Fase 1 (GDD §2.1 Locations), e Volume URP com Bloom + Vignette + Color Adjustments + Channel Mixer (paleta retrô). Tutorial Notion compartilhado pelo usuário não retornou conteúdo via WebFetch (provavelmente protegido); seguir com defaults URP 2D (Pixel Perfect Camera + 2D Renderer + Volume global) até o usuário colar o conteúdo do tutorial.
3. **Inimigo completo** — primeiro antagonista (bully da Fase 1 segundo o GDD): patrulha, detecção do player, ataque não-letal (empurra/derruba o jovem; adulto contra-ataca). Animações idle/walk/attack/hurt. Vide GDD §2.2 atualizado ("inimigos patrulham, perseguem ou bloqueiam. Podem ser combatidos — atordoados/empurrados, sem morte gráfica — evitados ou manipulados") e §3.3 ("Tempo congelado ↔ Movimento de inimigos: congelar abre janelas seguras").

Tom da fase tutorial: o jovem Woody (7 anos, sprite 16×16) é o protagonista jogável principal aqui; o adulto entra como "tio estranho" que ajuda. Curva de aprendizado da Fase 1 segundo GDD §3.2: ensina **movimento e dualidade de tamanho**, sem congelamento de tempo ainda (esse só vem na Fase 2).
