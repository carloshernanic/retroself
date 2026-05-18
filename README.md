# Retroself

> *"E se você pudesse voltar e ajudar a si mesmo a não desistir?"*

## Sobre o jogo

Retroself é um puzzle-platformer narrativo de pixel art 8-bit, co-op assimétrico local (mesmo teclado, dois Woodys controláveis por Tab), com tema de auto-compaixão. Side-scroller 2D em Unity, target Web (itch.io WebGL). Campanha de ~45–60 min cobrindo quatro memórias da vida de Woody.

Woody adulto (~30 anos) é encontrado por alienígenas numa cabine cyberpunk escondida atrás de uma árvore. Eles oferecem um acordo: voltar a quatro momentos da própria vida e ajudar seu eu mais jovem a atravessá-los. Os dois Woodys coexistem na mesma cena, o jovem é menor, ágil e pode jogar pedrinhas, o adulto é maior, mais pesado e empurra blocos. O jogador alterna entre os dois com Tab. A cooperação é puramente espacial-assimétrica.

**Tema central:** Auto-compaixão. Não é apagar o passado — é voltar a ele com o cuidado que você não recebeu na época.

**Classificação:** 12+ (sem violência gráfica, sem palavrões — diálogos vetam linguagem pesada por regra do projeto).

**Público-alvo:** indie 18–35, fãs de Celeste/Braid/GRIS/Inside, jogador brasileiro adulto que se identifica com "segunda chance".

**Equipe:** Alex Chequer · Carlos Hernani · Lucas Ikawa — Insper, Jogos Digitais 2026.

---

## Como jogar

| Ação | Tecla |
|---|---|
| Mover | `A` `D` / `←` `→` |
| Pular | `Espaço` `W` `↑` |
| Arremessar pedra (Young) | `K` |
| Trocar Woody | `Tab` |
| Entrar em portal | `E` (dentro do trigger) |
| Abrir arcade | `Espaço` (dentro do trigger) |
| Snake | Setas direcionais |
| Guitar Hero | `A` `S` `D` (ou setas); `Tab` troca guitarra |
| Avançar diálogo | `Espaço` / `Enter` |
| Pular cutscene | `Esc` |

Fluxo: **MainMenu → Prologue → Memory_01 (Pátio) → Memory_02 (Domingo) → Memory_03 (Floresta) → Memory_04 (Mercado) → Cutscene final + créditos → MainMenu**.

---



---

## Créditos

| Função | Responsável |
|---|---|
| Direção & Game Design | Alex Chequer · Carlos Hernani · Lucas Ikawa |
| Programação | Alex Chequer · Carlos Hernani · Lucas Ikawa |
| Narrativa & Roteiro | Alex Chequer · Carlos Hernani · Lucas Ikawa |
| Música & Som | **Carlos Hernani** |
| Pixel Art | Craftpix.net (pacotes licenciados — ver §Assets) |

---

## Assets

### Pixel Art — [Craftpix.net](https://craftpix.net/) (licenciados)

Todos os tilesets, props, backgrounds e sprites de personagem são pacotes oficiais Craftpix. Cada pasta de sprites em `Assets/Sprites/` corresponde a um pack:

| Pack | Pasta | Uso |
|---|---|---|
| Residential Area Tileset | `residential-area-tileset-pixel-art/` | Memory_01_Patio (tilemap + props: bench, lamp, trashcan, boxes) |
| Basement Tileset | `basement-tileset-pixel-art/` | Memory_02_Domingo (interior, pipes, decor) |
| Green Zone Tileset | `green-zone-tileset-pixel-art/` | Memory_03_Floresta (chão, árvores, pedras, switches coloridos) |
| Cyberpunk Market Street | `cyberpunk-market-street-pixel-art/` | Memory_04 (vending machines, sinais, props de mercado) |
| Cyberpunk Bar/Cafe NPC | `cyberpunk-pixel-bar-cafe-npc-asset-pack/` | NPC do Guitar Hero (M_04) |
| Cyberpunk Skills Icons | `cyberpunk-skills-pixelated-icon-pack/` | Ícones de minigame |
| Cyberpunk Font Effects | `cyberpunk-pixel-art-font-effects/` | Efeitos de texto |
| Doors & Portals | `doors-and-portals-pixel-art-asset-pack/` | Portais animados (M_03 ReturnPad, M_02 ReturnPad) |
| Business Center | `business-center-tileset-pixel-art/` | Coin (HeavyBox da catraca M_04), interiores |
| Beach Cyberpunk | `beach-pixel-art-tileset-for-cyberpunk-topic/` | Variantes de BG |
| Ghetto Tileset | `ghetto-tileset-pixel-art/` | Props alternativos |
| Bar Street Tileset | `bar-street-tileset-pixel-art-pack/` | Props alternativos |
| Free Scrolling City BG | `free-scrolling-city-backgrounds-pixel-art/` | BG paralax noturno (MainMenu, Prologue, M_01) |
| Free Tiny Hero Sprites | `free-pixel-art-tiny-hero-sprites/` | 3 alienígenas do Prologue (Pink/Owlet/Dude Monster) |
| Street Food 32×32 Icons | `street-food-for-cyberpunk-pixel-art-32x32-icons/` | Comidas do errand M_04 |
| Street Snacks 32×32 Icons | `street-snacks-pixel-art-32x32-icon-pack/` | Frutas do minigame Snake M_04 |
| Crianças | `criancas/Child_3/` | Sprites do Woody jovem (Idle, Walk) |
| Mendigos | `mendigos/Homeless_1/` | Sprites do Woody adulto (Idle, Walk, Jump, Idle_2 sentado) |
| Gangsters | `gangsters/Gangsters_2/` | Sprites do porteiro/bully do M_01 |

### Fontes

| Fonte | Licença | Uso |
|---|---|---|
| [Press Start 2P](https://fonts.google.com/specimen/Press+Start+2P) | OFL (Open Font License) | Logo "RETROSELF", títulos, diálogos, créditos |
| [VT323](https://fonts.google.com/specimen/VT323) | OFL | Fallback / textos secundários |

Os `.ttf` originais estão em `Assets/Fonts/`. Os `.asset` SDF correspondentes são gerados via menu **Retroself → Build Pixel Font Asset** (usa `TMP_FontAsset.CreateFontAsset` com atlas 512×512, padding 5, samplingPointSize 64). Pré-popula ASCII + acentos pt-BR + travessão/aspas tipográficas.

### Engine & Bibliotecas

| Tech | Versão | Uso |
|---|---|---|
| Unity | 6000.3.13f1 | Engine |
| URP (Universal Render Pipeline) | 2D Renderer | Light2D + post-processing |
| New Input System | latest | Todos os reads via `Keyboard.current` |
| TextMeshPro | latest | UI text + world-space text |
| 2D Tilemap | latest | Memory_01_Patio (Tilemap_Ground + CompositeCollider2D) |

---

## Estrutura do código

Tudo em `Assets/` direto (flat layout, sem subfolder `_Retroself/`).

```
Assets/
├── Audio/            (música autoral)
├── Editor/           (builders idempotentes — gated com #if UNITY_EDITOR)
├── Fonts/            (PressStart2P + VT323 + SDF .assets)
├── Scenes/           (MainMenu, Prologue, Memory_01..04, Memory_04_Cutscene_Placeholder)
├── Scripts/          (runtime MonoBehaviours, flat sem subpastas)
├── Settings/         (URP renderer + post-processing Volume profile + Tiles + BeatMap_M04)
└── Sprites/          (packs Craftpix listados acima)
```

---

## Inspirações

*Braid* · *Celeste* · *Brothers: A Tale of Two Sons* · *Inside / Limbo* · *Undertale* · *Pico Park* · *Portal 2 (co-op)* · Filme *About Time*.

---

## Licença

Código original do projeto: uso acadêmico (Insper — Jogos Digitais 2026). Assets de terceiros mantêm suas licenças originais (Craftpix EULA, OFL pras fontes).
