# Guia de import de sprites — Retroself

Este guia descreve como trocar os placeholders coloridos por sprites de pixel art autorais. As cenas são geradas por scripts em `Assets/Editor/` (builders) — entender quem cria o quê é o atalho mais rápido pra trocar arte sem quebrar nada.

## 1. Settings de import (pixel art)

Pra cada PNG/Aseprite que você adicionar em `Assets/Sprites/`, abra o Inspector e configure:

| Campo | Valor |
|---|---|
| **Texture Type** | Sprite (2D and UI) |
| **Sprite Mode** | Single (1 sprite por arquivo) ou Multiple (atlas/spritesheet) |
| **Pixels Per Unit** | **16** (consistente com o tilemap atual) |
| **Mesh Type** | Full Rect |
| **Pivot** | Center (ou Bottom pro player, se quiser que `transform.position` seja a base) |
| **Filter Mode** | **Point (no filter)** — sem isso o pixel art fica borrado |
| **Compression** | **None** — ou Crunched se for fechar build |
| **Generate Mip Maps** | desligado |
| **sRGB** | ligado |

Se for spritesheet, abra o **Sprite Editor** e use **Slice → Grid By Cell Size** com o tamanho do frame (ex: 16×16 pro jovem, 16×32 pro adulto).

Aseprite: o importador (já na stack) entende `.aseprite`/`.ase` direto. Cada layer/tag vira um sub-sprite.

## 2. Onde está cada placeholder

| Elemento | GameObject (na cena) | Builder | Cor / forma atual |
|---|---|---|---|
| Jovem (corpo) | `PlayerYoung/Body` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildYoung` | Quadrado amarelo, scale (0.6, 1, 1) |
| Adulto (corpo) | `PlayerAdult/Body` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildAdult` | Quadrado marrom, scale (0.6, 2, 1) |
| Bully (corpo) | `Bully/Body` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildBully` | Quadrado vermelho |
| Caixa pesada | `HeavyBox` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildHeavyBox` | Quadrado marrom 0.8×0.8 |
| Recompensa | `Reward_Star` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildStar` | Quadrado amarelo 0.4×0.4 |
| Pedra (arremessada) | `Stone` (runtime) | [PlayerAttack.cs](../Assets/Scripts/PlayerAttack.cs) `ThrowStone` | Quadrado cinza |
| Tiles do chão / plataforma | `Tilemap_Ground` | [Memory01Builder.cs](../Assets/Editor/Memory01Builder.cs) `BuildGround` + `LoadOrCreateTile` | Tile sub-asset em `Assets/Settings/Tiles/*.asset` |
| Cabine (mainmenu/prologue) | `Cabin` + `CabinGlow` | [MainMenuBuilder.cs](../Assets/Editor/MainMenuBuilder.cs), [PrologueBuilder.cs](../Assets/Editor/PrologueBuilder.cs) `BuildPanel_Cabine` | Retângulo roxo + glow |
| Aliens (prologue) | `Alien_Red`, `Alien_Blue`, `Alien_Yellow` | [PrologueBuilder.cs](../Assets/Editor/PrologueBuilder.cs) `BuildPanel_Aliens` | Quadrados RGB 0.8×0.8 |
| Woody (prologue, dormindo/sentado/cabine) | `Woody_Sleeping`, `Woody_Sitting`, `Woody` | [PrologueBuilder.cs](../Assets/Editor/PrologueBuilder.cs) | Marrom |
| Relógio (panel 7) | `WatchBody`, `WatchFace`, `WatchHand1/2` | [PrologueBuilder.cs](../Assets/Editor/PrologueBuilder.cs) `BuildPanel_Relogio` | Composto colorido |

## 3. Como trocar cada elemento

### Player (jovem ou adulto), Bully, HeavyBox, Star

A maneira mais simples: depois de re-rodar o builder pra montar a cena, no Inspector:

1. Selecione o GameObject filho `Body` dentro do prefab/hierarquia (ex: `PlayerYoung/Body`).
2. No componente `SpriteRenderer`, arraste o sprite autoral no campo **Sprite**.
3. Ajuste a `Scale` do `Body` se o sprite tiver outra resolução. Ex: se o sprite do jovem é 16×16 (1×1 unidade com PPU 16), use scale (1, 1, 1) — **não o (0.6, 1, 1) atual**, que era um esticamento de leitura do quadrado branco.

Pra fazer isso persistir após re-rodar o builder, edite o builder direto e troque o `SolidSprite()` por um `AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/young_idle.png")`.

### Pedra (Stone, runtime)

Em [PlayerAttack.cs](../Assets/Scripts/PlayerAttack.cs), o método `StoneSprite()` gera um quadrado branco em runtime. Troque por:

```csharp
static Sprite cachedStone;
static Sprite StoneSprite()
{
    if (cachedStone == null)
        cachedStone = Resources.Load<Sprite>("stone"); // ou AssetDatabase.LoadAssetAtPath em editor
    return cachedStone;
}
```

Se quiser sem `Resources/`, prefira expor um `public Sprite stoneSprite;` no `PlayerAttack` e setar via builder.

### Tilemap (chão / plataforma)

Cada tile vive em `Assets/Settings/Tiles/Ground_Dirt.asset`, `Ground_GrassTop.asset`, `Platform_Wood.asset`. Eles embedam `Texture2D` + `Sprite` como sub-assets.

**Opção A — substituir o sprite do tile direto:** abra o `.asset` no Inspector (modo Debug) e arraste o sprite autoral em `m_Sprite`.

**Opção B (recomendada) — Tile Palette nova:**

1. `Window → 2D → Tile Palette`. Crie uma palette nova (`Create New Palette` → escolha grid Rectangle, cell 1×1).
2. Arraste seus sprites de chão pra palette — cada sprite vira um tile asset.
3. Selecione `Tilemap_Ground` na hierarquia, escolha o brush, repinte por cima dos tiles antigos. Os colliders são reaproveitados pelo `CompositeCollider2D`.
4. (Opcional) Apague os `.asset` antigos em `Assets/Settings/Tiles/` se não usar mais.

### Aliens, Woody, cabine (Prologue)

Mesmo padrão dos players: cada `CreateSprite("Nome", ...)` em `PrologueBuilder.cs` cria um GameObject com `SpriteRenderer` apontando pro quadrado branco. Pra trocar, substitua o asset no campo `Sprite` do componente após gerar a cena, **ou** edite a função `CreateSprite`/builder pra carregar via `AssetDatabase`.

Os componentes de animação (`IdleBob`, `BreathingScale`) continuam funcionando — eles mexem em `transform`/`scale`, não no sprite.

### Cabine + glow (MainMenu)

`CabinGlow` é o sprite que pulsa via `CabinPulse`. Se você desenhar um glow já com transparência embutida, pode desligar o `CabinPulse` (ou ajustar `minAlpha`/`maxAlpha`) pra não exagerar.

## 4. Animação real (caminho futuro)

Hoje o player anima via `PlayerAnimator` (squash/stretch + tint placeholder). Quando os sprites forem entrar com frames de idle/walk/jump/attack, o caminho é:

1. Crie um `AnimatorController` em `Assets/Animations/Player.controller`.
2. States: `Idle`, `Walk`, `Jump`, `Fall`, `Attack`.
3. Parâmetros (Bool/Float):
   - `IsGrounded` (Bool) — escreve via script lendo `PlayerController.IsGrounded`.
   - `Speed` (Float) — `Mathf.Abs(controller.MoveX)`.
   - `VerticalVel` (Float) — `controller.Velocity.y`.
   - `Attack` (Trigger) — chamar quando `PlayerAttack.LastAttackTime` mudar.
4. Transitions: Idle ↔ Walk via `Speed`; Idle/Walk → Jump via `!IsGrounded && VerticalVel > 0`; Jump → Fall via `VerticalVel <= 0`; Any → Attack via Trigger.
5. Adicione um `Animator` no `PlayerYoung` (e outro no `PlayerAdult`, se for animação separada). Coloque um script ponte que copia os valores do `PlayerController` pros parâmetros do `Animator` no `Update`.
6. Remova ou desligue o `PlayerAnimator` placeholder quando o `Animator` real estiver tocando.

Pro bully o caminho é parecido — `BullyController` já expõe estado (chase via cor), basta adicionar `chasing` como parâmetro.

## 5. Tilemap Palette workflow rápido

Se for fazer todo um conjunto novo de tiles (chão, parede, plataforma, escada, decoração):

1. Importe o tileset PNG com **Sprite Mode = Multiple** e abra o Sprite Editor → `Slice → Grid By Cell Size 16×16` (ou o tamanho que você desenhou).
2. `Window → 2D → Tile Palette` → New Palette → grid Rectangle, cell size **1×1** (em unidades, pq nosso PPU é 16).
3. Arraste o atlas pra dentro da palette — Unity gera um `.asset` por sub-sprite automaticamente em `Assets/Tiles/` (escolhe a pasta na hora).
4. Selecione `Tilemap_Ground` e use os brushes da palette pra repintar o nível.
5. **Não esqueça** de re-checar que `Tilemap_Ground` ainda tem `CompositeCollider2D` + `Rigidbody2D Static` + `compositeOperation = Merge`. Sem isso, as costuras entre tiles voltam a prender o player.

## 6. Áudio

Mesmo princípio: `Assets/Audio/` está vazio. Pra adicionar:

1. Importe seus `.wav`/`.ogg`/`.mp3`. Em settings: Load Type **Decompress on Load** (curtinhos) ou **Streaming** (música), Compression **Vorbis** pra OGG.
2. Slot principal: `MenuMusicSource` (no MainMenu) e `BlipSource` (no Prologue) — arraste o clip no campo `AudioSource.clip`.
3. Pra SFX de impacto/pulo/pedra, expor `public AudioClip` nos scripts (`PlayerController`, `PlayerAttack`, `BullyController`) e tocar com `AudioSource.PlayClipAtPoint`.

## 7. Re-rodar builders

Depois de qualquer troca de placeholder, dois caminhos:

- **Edição direta na cena**: abra a cena em Unity, troque os sprites no Inspector, salve. **Evite** re-rodar o builder — ele recria a cena do zero.
- **Edição no builder**: troque o sprite carregado dentro do builder (via `AssetDatabase.LoadAssetAtPath`) e re-rode o menu **Retroself → Build *** pra regenerar a cena já com os sprites finais.

Recomendado pro fluxo final: editar os builders pra carregar sprites autorais. Assim, qualquer regen mantém o estado certo.
