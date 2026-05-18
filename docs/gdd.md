# retroself — Game Design Document

> *"E se você pudesse voltar e ajudar a si mesmo a não desistir?"*

**Versão:** 0.4 — sprint final
**Equipe:** Alex Chequer, Carlos Hernani, Lucas Ikawa
**Curso:** Insper — Jogos Digitais 2026
**Data:** maio de 2026

---

## 1. Game Concept

### História + Visão Geral

**Retroself** é um puzzle-platformer narrativo de pixel art 8-bit, **co-op assimétrico local** (mesmo teclado, dois Woodys controláveis por Tab), com tema de **auto-compaixão**. Side-scroller 2D em Unity, target **Web (itch.io WebGL)**. Campanha de ~45–60 min cobrindo quatro memórias da vida de Woody.

Woody adulto (homeless, ~30 anos) é encontrado por alienígenas numa cabine cyberpunk escondida atrás de uma árvore. Eles oferecem um acordo: voltar a quatro momentos da própria vida e ajudar seu eu mais jovem a atravessá-los. **Os dois Woodys coexistem na mesma cena** — o jovem é menor e ágil, o adulto é maior, mais pesado e empurra blocos. O jogador alterna entre os dois com `Tab`. Não há mecânica de congelamento de tempo no estado atual; a cooperação é puramente **espacial-assimétrica** (Pico Park-style).

A cada fase resolvida o presente de Woody muda sutilmente. O jogo termina numa **cutscene de auto-perdão** seguida dos créditos rolando.

**Tema central:** Auto-compaixão. Não é apagar o passado — é voltar a ele com o cuidado que você não recebeu na época.

**Classificação:** 12+ (sem violência gráfica, sem palavrões — diálogos vetam linguagem pesada por regra do projeto).

**Público-alvo:** indie 18–35, fãs de Celeste/Braid/GRIS/Inside, jogador brasileiro adulto que se identifica com "segunda chance".

---

## 2. Design

### 2.1. Blueprint

#### Art Style

- **Pixel art 8-bit** com paleta restrita; cada fase tem identidade cromática (escolar quente, domingo basement marrom, floresta verde noturna, mercado cyberpunk neon).
- Sprites de personagem: **PlayerYoung** ~16×16 (jaqueta amarela), **PlayerAdult** ~16×32 (sobretudo marrom). Tiles 32×32 PPU 32.
- BG paralax 5 layers (CityNight, basement, floresta, mercado) parented na Main Camera.
- Pós-processamento URP por cena (Bloom + Vignette + ColorAdjustments + Light2D), tunado manualmente — builders criam só o pré-requisito.
- HUD minimalista: título da fase + barra de vida do ativo + hint de controles + slot do inventário de chaves/comidas quando aplicável.

#### Narrative Style

- Tom melancólico-esperançoso; humor seco do adulto contra a sinceridade do jovem.


#### Sound Style

- Chiptune lo-fi melancólico (Lena Raine, Toby Fox como referência).
- **Música**: Carlos Hernani.
- **SFX procedural** (`SfxBeep.BuildBlip`): pulo, pedra, plate on/off, beat hit/miss, snake move/food, game over, dialog blip, plank break.
- AudioManager pausa/retoma trilha de fundo durante minigames (Snake / Guitar Hero).

#### Characters

- **Woody Adulto** (jogável, sprite `mendigos/Homeless_1/`, 16×32): 30 anos. Casaco surrado, barba. Cansado, irônico, com ternura escondida. Ancora puzzles — empurra `HeavyBox`, pisa em `PressurePlate.Requirement.Adult`, alcança alto.
- **Woody Jovem** (jogável, sprite `criancas/Child_3/`, 16×16): idade varia por fase — **7** (M_01 pátio), **12** (M_02 domingo), **15** (M_03 floresta), **15–16** (M_04 mercado). Sprite reusado em todas as fases — sem variação visual de adereços (mochila/fones/skate/uniforme não foram implementados). Cabe em frestas, **arremessa pedras** (`K`) que destroem `BreakablePlank`, ativam `StoneSwitch` e derrotam o porteiro. Salto não inclui frame de jump (fallback pra Idle).
- **Porteiro** (Memory_01, sprite `gangsters/Gangsters_2/`): único inimigo com IA no jogo. `BullyController` patrulha + persegue + leva pedrada → cai → dropa chave (`KeyDropper` + `KeyPickup`).
- **Alienígenas** (Prologue, sprites `tiny-hero-sprites/Pink/Owlet/Dude_Monster`): trio decorativo nos painéis 5–6 (`IdleBob` em fase offset). **O Hub dos alienígenas foi dropado** — a cutscene final de auto-perdão cumpre a função narrativa (ver §2.1).
- **Pais brigando** (Memory_02): referenciados apenas na fala de abertura (`IntroDialogue`); não aparecem em cena. ~~Amigo da festa, relógio do vestibular~~ cortados — M_04 virou mercado cyberpunk.

#### Locations

| Fase / Cena | Setting | Idade | Mecânica-foco |
|---|---|---|---|
| **MainMenu** | Praça noturna (rain + lightning) | — | Botões + créditos |
| **Prologue** | 7 painéis (praça → cabine → flash → aliens → relógio) | 30 (presente) | Cutscene |
| **Memory_01_Patio** | Pátio escolar, paleta verde-amarelo | 7 | Dualidade Woody + porteiro |
| **Memory_02_Domingo** | Interior basement (madeira marrom) | 12 | Co-op assimétrico estilo Pico Park |
| **Memory_03_Floresta** | Floresta noturna, paleta verde-azul | 15 | Portais (`ReturnPad` com `E`) |
| **Memory_04_Sala (mercado)** | Mercado cyberpunk noturno | 15-16 | Errands + arcades (Snake, Guitar Hero) |
| **Memory_04_Cutscene_Placeholder** | Fundo preto + créditos | — | Cutscene outro + créditos rolando |

*Hub (sala dos alienígenas) e Epílogo planejados originalmente foram dropados — a cutscene final cumpre a função.*

##### Paletas e ambientação visual

| Local | Paleta | Descrição |
|---|---|---|
| **Praça (presente)** | Cinzas, azul-marinho, laranja apagado | MainMenu + Prologue. Chuva + lightning flash, banco, vending machine cyberpunk como portal alienígena |
| ~~**Hub — Sala dos Alienígenas**~~ | — | **Dropado.** Alienígenas aparecem só nos painéis 5–6 do Prologue (trio decorativo `IdleBob`); a cutscene final cumpre a função narrativa |
| **Memory_01 — Pátio (7 anos)** | Verde-amarelo escolar, marrom de tijolo, dourado quente | Tilemap `residential-area`, bench / lamp post / trashcan, BG night paralax 5 layers |
| **Memory_02 — Domingo (12 anos)** | Marrom de madeira, bege, cinza escuro | Interior `basement-tileset`. Chão solid-color, pipes / boxes / decor decorativos. Pais brigando só no diálogo de abertura |
| **Memory_03 — Floresta (15 anos)** | Verde-azul noturno, ciano dos portais | Floresta com portais como verbo principal. Switches acima dos portais (jovem pula pra acertar) |
| **Memory_04 — Mercado (15–16 anos)** | Magenta, ciano neon, preto, vermelho de letreiro | Mercado cyberpunk noturno (`cyberpunk-market-street`). Vending machines, foodtruck, cabines arcade (Snake, Guitar Hero), NPCs cyberpunk |
| **Cutscene final + créditos** | Preto profundo + texto branco | Auto-perdão em 5 painéis → créditos rolando → volta ao MainMenu |

*Fases 3 e 4 mudaram completamente vs concept original: M_03 deixou de ser "festa adolescente com piscina" → floresta noturna com portais; M_04 deixou de ser "sala de vestibular com paredes que se movem" → mercado cyberpunk com arcades, idade 18 → 15–16 (conexão narrativa com M_02).*

### 2.2. Mechanics

#### Game Objects

- **Caixas pesadas** — só o adulto empurra.
- **Frestas baixas** — só o jovem passa (adulto bate a cabeça).
- **Placas de pressão** — algumas só com adulto em cima, outras só com caixa, outras com qualquer um.
- **Alvos de pedra** — o jovem mira e arremessa pra ativar.
- **Cofre de senha** — três alvos coloridos que precisam ser acertados na ordem certa.
- **Tábuas frágeis** — pedra do jovem destrói.
- **Portas com regra** — só abrem quando todas as condições estão satisfeitas ao mesmo tempo.
- **Chave** — derrubar o porteiro da fase 1 faz ele dropar a chave da escola.
- **Portais** — teleporte entre dois pontos da fase, exigem apertar `E` pra entrar.
- **Cabines arcade** — minigames embedded (Snake e Guitar Hero) que destravam a saída.
- **Comidas coletáveis** — o jovem pega, entrega na barraca pra liberar passagem.
- **Porteiro** — único inimigo do jogo, na fase 1: patrulha, persegue, leva pedrada e cai.

#### Regras de Interação

- O adulto tem **dobro da altura** do jovem.
- **`Tab` troca quem você controla.** O outro continua na cena parado, mas ainda **pesa em placas** — é assim que se resolve a maioria dos puzzles ("um segura, o outro age").
- Só o jovem **arremessa pedra** (`K`).
- Durante minigames o jogo congela; o overlay do minigame continua funcionando normal.

#### Condições de Vitória e Derrota

- **Vitória da fase**: os dois Woodys juntos na porta de saída (+ chave quando exigida).
- **Derrota**: vida zerada ou cair no buraco → respawn no ponto inicial. Sem game over, sem contador de mortes.
- **Vitória do jogo**: terminar a última fase → cutscene de auto-perdão + créditos rolando.

#### Comportamentos

- **Personagem inativo**: fica parado onde foi deixado (não anda, não patrulha).
- **Porteiro**: patrulha um trecho, persegue quando o jogador chega perto, leva 3 pedradas até cair.
- **Feedback visual**: poeirinha ao pousar, flash branco quando inimigo apanha, placa muda de cor quando ativada, tábua vira partículas ao quebrar.

#### Sistemas

- **Progressão por mecânica** (sem XP, sem loot, sem skill tree):
  - **Fase 1 (Pátio)** — aprende dualidade Woody, fresta, caixa-como-degrau, ataque com pedra.
  - **Fase 2 (Domingo)** — pedra como ferramenta de puzzle, cofre de senha, coordenação espacial.
  - **Fase 3 (Floresta)** — portais como elevadores.
  - **Fase 4 (Mercado)** — combina tudo + minigames + entrega de comidas + cofre de cores.
- **Hub e congelamento de tempo foram cortados** — a cutscene final substitui o hub.

### 2.3. Interface

- **HUD minimalista**: barra de vida do Woody ativo, título da fase, hint de controles, inventário (chave ou comidas) quando aplicável.
- **Sem mapa, sem minimapa, sem missões em texto, sem skill tree.**
- **Diálogos**: caixa preta no rodapé com portrait do personagem, texto digitando (typewriter) e indicador piscante `[Espaço/Enter]` pra avançar.
- **Animações-chave**: poeirinha ao pousar, flash branco quando o porteiro apanha, placas mudam de cor on/off, tábua frágil vira partículas ao quebrar.

*Barra de energia do congelamento, idle do adulto suspirando, idle do jovem variando por fase e animação de "carregar o jovem" foram cortados — congelamento de tempo e mecânica de carregar não foram implementados; o jovem usa o mesmo sprite em todas as fases.*

#### Controles

| Ação | Teclado |
|---|---|
| Mover | `A` / `D` ou `←` / `→` |
| Pular | `Espaço` / `W` / `↑` |
| Arremessar pedra (Young) | `K` |
| Trocar Woody | `Tab` |
| Entrar em portal / arcade | `E` (portal) / `Espaço` (arcade) |
| Snake | Setas direcionais |
| Guitar Hero | `A`/`S`/`D` ou `←`/`↓`/`→` ; `Tab` troca guitarra |
| Avançar diálogo | `Espaço` / `Enter` |
| Pular cutscene | `Esc` |

*Sem suporte a gamepad no estado atual (todos os reads usam `Keyboard.current` do New Input System).*

---

## 3. Dynamics

### 3.1. Player-Player

Single-player com **dois corpos co-presentes**. Toda a tensão vem da assimetria entre eles: o Adult ancora (pesa em plate, empurra caixa, alcança alto), o Young executa (cabe em fresta, atira pedra, pula em cima da caixa). Como o inativo continua na cena, o jogador aprende a **deixar um Woody numa posição-chave** antes de trocar.

### 3.2. Player-Game

**Curva de aprendizado**:
- **M_01**: ensina movimento, `Tab`, fresta, `HeavyBox` como degrau, ataque com pedra.
- **M_02**: introduz `K` como verbo de puzzle (Stone como chave), cofre de senha, dependência espacial Adult↔Young, `ReturnPad` como portal de revisita.
- **M_03**: portais como elevadores, switches **acima dos portais** (precisa pular pra acertar), portal exige `E` (anti-acidente).
- **M_04**: combina tudo + minigames embedded (Snake, Guitar Hero) + inventário cross-puzzle (comidas) + senha 3 cores via `SequenceLock`.

**Tipos de interação**: plataforma, arremesso de pedra, empurrar caixa, ler pistas (números 1/2/3 espalhados pra deduzir senhas), minigames arcade.

### 3.3. Game-Game

- **Stone ↔ EnemyHealth ↔ BreakablePlank/StoneSwitch/Bully**: uma única primitiva (Stone como trigger) interage com 3 alvos diferentes via componente compartilhado (`EnemyHealth` ou `StoneSwitch`).
- **PressurePlate.latch ↔ HeavyBox**: caixa "encaixa" visualmente quando pisa pela 1ª vez (snap X + RB Static + componente HeavyBox desativado).
- **ArcadeMachine ↔ GateSource**: cabine vira fonte de gate quando o minigame é vencido (sem novos tipos de gate).
- **`SceneStartReset` ↔ KeyPickup/FoodInventory**: zera flags estáticas no Awake da cena (evita coleta cross-cena).

---

## 4. Experience

### 4.1. Senses

- **Visual**: pixel art 8-bit, paleta restrita por fase, scanlines URP, Light2D manuais. Press Start 2P (logo + diálogos) + VT323 (fallback).
- **Audio**: chiptune autoral (Carlos Hernani) + SFX procedural via `SfxBeep`.
- **Feedback de hit**: flash branco no Bully, dust puff no pouso, plank vira partículas ao quebrar, plate troca de cor on/off.

### 4.2. Cerebellum (emoções)

- **Nostalgia**: cenários reconhecíveis (pátio escolar, basement, mercado de noite).
- **Ternura**: ver o Young pular em cima da caixa que o Adult empurrou.
- **Tensão**: minigame com `Time.timeScale=0` cria foco isolado.
- **Auto-compaixão**: pico emocional na cutscene final (5 painéis de fala do Adult).

### 4.3. Cerebrum

- **Planejamento espacial**: qual Woody usa qual alavanca, em que ordem.
- **Memória/observação**: senhas de 3 cores espalhadas como pistas numeradas (M_02 P4, M_04 P5).
- **Ritmo/coordenação**: Guitar Hero co-op (Tab troca guitarra em runtime).

### 4.4. Perception

O jogador percebe que:
- **Os dois são a mesma pessoa em tempos diferentes**.
- **Cooperar = aceitar quem você foi**. O Adult que rejeita o Young não atravessa as fases — só quando o adulto **espera, segura, empurra a caixa pro filho** o caminho abre.
- **Falhar não é punido**. Sem contagem de mortes; respawn imediato. Reforça o tom de auto-compaixão.
- **A cutscene final é a recompensa narrativa**, não uma boss fight.

---

## 5. Inspirações

| Obra | O que inspira |
|---|---|
| *Braid* | Manipulação do tempo como narrativa (mantido no Prologue como motivo simbólico) |
| *Celeste* | Plataforma com tema de saúde mental |
| *Brothers: A Tale of Two Sons* | Dois personagens, dois corpos, uma história |
| *Inside / Limbo* | Atmosfera melancólica em 2D |
| *Undertale* | Estilo retrô com peso emocional |
| *Pico Park* | Co-op assimétrico em puzzles de plate/gate |
| *Portal 2 (co-op)* | Cooperação espacial com ferramentas diferentes por personagem |
| *Filme "About Time"* | Voltar ao passado pra corrigir o eu |

---

## 6. Estado atual vs. visão original

Diferenças intencionais em relação à v0.2 do GDD:

- **Congelamento de tempo** virou motivo narrativo (mencionado na cutscene final), **não mecânica implementada**. A coop assimétrica espacial substituiu o time-freeze como verbo principal.
- **Carregar o jovem** foi dropado. Caixas (`HeavyBox`) cumprem a função de degrau.
- **Hub dos alienígenas** dropado. Fluxo linear MainMenu → Prologue → M_01 → M_02 → M_03 → M_04 → Cutscene → MainMenu.
- **Cenários reescritos**: festa adolescente (M_03 original) virou floresta noturna; sala de prova (M_04 original) virou mercado cyberpunk.
- **Idade da Memory_04**: 18 → 15-16 anos (Woody adolescente fugindo de casa pro mercado, ligando narrativamente com M_02).
- **3 fotografias coletáveis por fase + múltiplos finais** dropados. Final único (cutscene + créditos).
- **Combate** restrito ao M_01 (porteiro). M_02/M_03/M_04 não têm inimigos AI — só hazards estáticos.
- **Minigames embedded** (Snake + Guitar Hero co-op) adicionados na M_04 — não previstos no GDD original.
