# retroself — Game Design Document

> *"E se você pudesse voltar e ajudar a si mesmo a não desistir?"*

**Versão:** 0.2 (Draft)
**Autor:** Carlos
**Data:** abril de 2026

---

## 1. Game Concept

> **[IMAGEM REPRESENTANDO O JOGO]**
> *Prompt para gerar:* "Pixel art game cover for an indie puzzle-platformer. A tired 30-year-old man in a worn-out coat stands in a rainy city square at night, looking down at a small 7-year-old boy — both are clearly the same person at different ages. Between them, a glowing alien pocket watch floats, freezing a single raindrop mid-air. A retro arcade cabinet glows behind a tree in the background. Cold blue-gray palette with one warm amber glow. 8-bit pixel art, CRT scanlines, melancholic but hopeful mood. --ar 16:9"

### História + Visão Geral

**Retroself** é um puzzle-platformer narrativo de pixel art 8-bits, single-player, com mecânica de manipulação do tempo. Jogado em 2D side-scroller, tem entre **45 e 60 minutos** de campanha (4 fases + hub + epílogo) e é desenvolvido em **Unity 2D** para **Web (itch.io)**.

Woody tem 30 anos, foi despejado e dorme num banco de praça. Numa noite chuvosa, encontra uma cabine alienígena escondida atrás de uma árvore. Ao entrar, descobre que alienígenas vinham observando ele há anos: não como cobaia, mas como candidato. Eles concluíram que Woody não é um homem mau, apenas alguém que tropeçou nas piores horas. E oferecem um acordo: ele pode voltar para **quatro momentos da própria vida**, entre os 7 e os 18 anos, e ajudar seu eu mais jovem a atravessá-los.

A mágica do jogo está no encontro entre os dois Woodys. O Woody-criança é pequeno, ágil, frágil. O Woody-adulto é o dobro do tamanho, alcança o que a criança não alcança, e recebe dos alienígenas um relógio que **congela o tempo**. Cada fase é um quebra-cabeça em movimento: o adulto prepara o cenário, congela o tempo, e o jovem segue adiante.

A cada fase concluída, o presente de Woody se altera sutilmente: a praça começa a clarear, o banco onde ele dormia ganha um cobertor, depois um casaco, depois desaparece. O jogo termina quando Woody acorda — talvez na praça, talvez não.

**Tema central:** Auto-compaixão. O jogo não é sobre apagar o passado, é sobre voltar a ele com o cuidado que você não recebeu na época.

**Classificação indicativa:** 12+ (temas de fracasso, despejo, melancolia — sem violência gráfica)

**Público-alvo:** Jogadores indie de 18–35 anos que gostam de puzzle-platformers narrativos (público de Celeste, Braid, GRIS) e público brasileiro adulto que se identifica com a temática de "segunda chance".

---

## 2. Design

> **[IMAGEM ILUSTRANDO O DESIGN DO JOGO]**
> *Prompt para gerar:* "Pixel art level design mockup, side-scroller perspective. A cross-section of four different scenes stacked: a school courtyard, a family home interior, a teen party at night, and a bright exam room. Two characters visible — a small kid and a tall adult silhouette. Glowing pocket watch icon on a HUD bar. 8-bit pixel art, restricted palette, design document style. --ar 16:9"

### 2.1. Blueprint

#### Art Style

> **[IMAGEM]**
> *Prompt:* "Pixel art style reference sheet. NES + Game Boy Color inspired palette. Tile examples (16x16) of grass, brick, wood, metal. Side-scroller environment chunk with 4 distinct color zones: warm school yellow, melancholic home brown, neon party magenta, cold exam white. CRT scanline effect overlay. --ar 16:9"

- **Pixel art 8-bits** com paleta restrita (estilo NES + Game Boy Color)
- Sprites: 16×32 (Woody adulto), 16×16 (Woody jovem), tiles de 16×16
- Resolução interna: 320×180, escalonada
- Cada fase tem **identidade cromática própria** para localizar o jogador emocionalmente
- HUD minimalista: apenas barra de energia do congelamento (canto superior esquerdo)
- Diálogos: caixa pixelada com retrato 24×24 (estilo Pokémon clássico), fonte custom de 8 pixels

#### Narrative Style

> **[IMAGEM]**
> *Prompt:* "Pixel art dialogue box mockup, retro RPG style. Two character portraits side by side — a tired 30-year-old man with stubble and a curious 7-year-old boy. Typewriter text effect mid-sentence. Bottom of screen, classic Pokemon-inspired UI frame. 8-bit pixel art, low resolution. --ar 16:9"

- **Tom:** melancólico mas esperançoso. Humor seco do adulto contra a sinceridade do jovem.
- **Diálogos curtos**, em texto 8-bits com efeito de máquina de escrever.
- **Sem cinemáticas longas** — narrativa cabe em pequenos diálogos antes/depois das fases.
- **Detalhes ambientais**: pôsteres, bilhetes, fotos durante as fases.
- **Sem voice acting** — apenas blips de texto (grave para o adulto, agudo para o jovem, sintético para os alienígenas).

#### Sound Style

> **[IMAGEM]**
> *Prompt:* "Pixel art retro audio waveform visualization. Stylized 8-bit chiptune sheet music floating over a moody pixel art landscape. Old NES-style sound chip in foreground. Cold purple and amber accent colors. 8-bit aesthetic, atmospheric. --ar 16:9"

- **Direção:** Chiptune com camadas modernas, lo-fi melancólico.
- **Referências:** Celeste OST (Lena Raine), Undertale (Toby Fox), Hyper Light Drifter, Shovel Knight.
- **Faixas previstas:** tema da praça, da sala alienígena, dos 4 cenários, e remix final no epílogo.
- **SFX:** pulo, passos diferentes por personagem, zumbido elétrico do congelamento, ondas sonoras dos pais (fase 2), tique-taque do relógio gigante (fase 4).

#### Characters

> **[IMAGEM — Woody Adulto]**
> *Prompt:* "Pixel art character sprite sheet, 16x32 pixels. A tired 30-year-old man with stubble, dark circles under eyes, worn brown coat, slumped posture. Walk cycle, idle pose, jump frame. Muted gray-brown palette. White background, sprite reference. 8-bit pixel art. --ar 1:1"

> **[IMAGEM — Woody Jovem (4 versões)]**
> *Prompt:* "Pixel art character sprite sheet showing the same boy at 4 ages, all 16x16. At 7 (school uniform, backpack, curious), at 12 (headphones, closed-off), at 15 (skateboard under arm, smug grin), at 18 (school uniform, anxious posture). Bright contrasting palette. 8-bit pixel art, white background. --ar 16:9"

> **[IMAGEM — Alienígenas]**
> *Prompt:* "Pixel art alien creature trio sprite sheet. Three small round beings, 16x16 each, no mouths, expressive eyes only. One red, one blue, one yellow. Minimalist friendly design. 8-bit pixel art, white background. --ar 16:9"

- **Woody Adulto (jogável):** 30 anos, sprite 16×32. Casaco surrado, barba, olheiras. Cansado, irônico, com ternura escondida.
- **Woody Jovem (companheiro / parcialmente jogável):** 7, 12, 15 ou 18 anos por fase. Sprite 16×16. Mochila aos 7, fones aos 12, skate aos 15, uniforme aos 18. Não reconhece o adulto como ele mesmo — vê como "tio estranho".
- **Alienígenas (NPCs do hub):** trio de criaturas, cada uma com cor primária. Tutorial difuso, falam pouco.
- **NPCs antagonistas:** bully, pais brigando, amigo da festa, relógio do vestibular. Não são vilões — são obstáculos cotidianos.

#### Locations

> **[IMAGEM — Mapa geral / hub estilo mood board]**
> *Prompt:* "Pixel art map overview showing six interconnected scenes arranged like memory fragments: a rainy city square, an alien ship interior with portals, a school courtyard, a 2-story house at sunset, a teen party house with a pool at night, and a stark exam room. Side-scroller perspective tiles. 8-bit pixel art, varied color palettes per scene. --ar 16:9"

| Local | Paleta | Descrição |
|---|---|---|
| **Praça (presente)** | Cinzas, azul-marinho, laranja apagado | Praça chuvosa, banco, cabine alienígena |
| **Hub — Sala dos Alienígenas** | Roxos, ciano, preto profundo | 4 portais + janela para a praça que vai mudando |
| **Fase 1 — O Pátio (7 anos)** | Verde-amarelo escolar, marrom de tijolo | Pátio de escola pública com quadra, bebedouro |
| **Fase 2 — Domingo (12 anos)** | Bege, marrom, vermelho desbotado | Casa de dois andares, cozinha, sala, quarto |
| **Fase 3 — A Festa (15 anos)** | Magenta, azul neon, preto | Casa de festa adolescente com piscina e telhado |
| **Fase 4 — A Sala (18 anos)** | Branco frio, cinza, vermelho do relógio | Sala de prova de vestibular, paredes que se movem |

#### Heroes

- **Woody Adulto** — protagonista principal, herói da auto-compaixão. Carrega o relógio alienígena.
- **Woody Jovem** — co-protagonista, ele mesmo no passado. O herói que precisa ser cuidado.

### 2.2. Mechanics

#### Game Objects
- **Caixas pesadas** — só o adulto empurra
- **Frestas e dutos** — só o jovem passa
- **Interruptores de pressão pesados** — só o adulto ativa
- **Interruptores delicados** — só o jovem ativa
- **Relógio alienígena** — equipado pelo adulto, congela o tempo
- **Fotografias coletáveis** — 3 escondidas por fase, desbloqueiam álbum no hub
- **Inimigos contextuais** — bully, ondas sonoras, NPCs da festa, fantasmas das fases anteriores
- **Ataque do jovem** — arremesso de pedras / objetos pequenos (curto alcance, atordoa)
- **Ataque do adulto** — golpe corpo-a-corpo (empurra / derruba; reaproveita a animação de empurrar caixa)

#### Regras de Interação
- O adulto tem **dobro da altura** do jovem (sistema de afinidades opostas)
- O adulto pode **carregar** o jovem (sprite 16×40, movimento mais lento)
- O congelamento de tempo **só funciona com o adulto** ativo
- O congelamento tem **medidor de energia** (recarrega lentamente fora de uso)
- Troca de personagem por botão dedicado; o inativo fica em idle ou patrulha curta

#### Condições de Vitória e Derrota
- **Vitória da fase:** ambos os Woodys chegam ao ponto final
- **Derrota:** barra de vida do personagem ativo zerada, ou jovem cai/é pego → fase reinicia do último checkpoint
- **Sem morte permanente, sem contador de mortes** — diálogo do adulto: *"Ok, de novo. A gente acerta dessa vez."*
- **Vitória do jogo:** completar as 4 memórias → epílogo

#### Comportamentos
- **Personagem inativo:** idle ou patrulha curta
- **Inimigos:** patrulham, perseguem ou bloqueiam. Podem ser **combatidos (atordoados/empurrados, sem morte gráfica)**, evitados ou manipulados — combate é último recurso, não objetivo central
- **Congelamento ativo:** filtro azul-acinzentado na tela, granulação, zumbido elétrico baixo

#### Sistemas
- **Progressão por desbloqueio mecânico** (sem XP, sem loot, sem skill tree)
  - Fase 1: dualidade de tamanho
  - Fase 2: + congelamento do tempo
  - Fase 3: + troca livre de personagem
  - Fase 4: combina tudo, timing apertado
- **Feedback de progresso:** janela do hub mostra a praça mudando

### 2.3. Interface

> **[IMAGEM — HUD mockup]**
> *Prompt:* "Pixel art HUD mockup for a retro side-scroller. Top-left corner shows a small energy bar with a clock icon (time-freeze meter). Bottom of screen has dialogue box with character portrait. Otherwise minimal — no map, no quest log. 8-bit pixel art, restricted palette. --ar 16:9"

- HUD minimalista: barra de energia do congelamento (canto superior esquerdo) e barra de vida do personagem ativo (logo abaixo)
- Sem mapa, sem minimapa, sem missões em texto
- Diálogos: caixa pixelada estilo Pokémon clássico
- Animações-chave: idle do adulto suspira a cada ~5s, idle do jovem muda por fase, transição suave do "carregar"

#### Controles
| Ação | Gamepad | Teclado |
|---|---|---|
| Mover | Analógico esquerdo | A / D ou ←/→ |
| Pular | A / X | Espaço |
| Interagir / pegar | B / Círculo | E |
| Atacar | X / Quadrado | K |
| Congelar tempo | RT / R2 | Shift |
| Trocar personagem | Y / Triângulo | Tab |
| Pausar | Start | Esc |

---

## 3. Dynamics

### 3.1. Player-Player
**Não se aplica diretamente** — Retroself é single-player.

Porém, há uma **dinâmica simbólica entre dois "jogadores"**: o jogador do presente (adulto) e o jogador-criança (passado). O jogador real ocupa os dois papéis em momentos diferentes.

**Cooperação interna:** o jogador precisa pensar como adulto (planejamento, recursos, paciência) e como jovem (agilidade, ingenuidade, vulnerabilidade) — alternando entre as duas mentalidades.

### 3.2. Player-Game

#### Curva de aprendizado
- **Fase 1 (7 anos):** ensina movimento e dualidade de tamanho. Puzzle simples, foco em narrativa.
- **Fase 2 (12 anos):** introduz congelamento do tempo. Curva sobe.
- **Fase 3 (15 anos):** introduz troca livre, gameplay paralelo. Maior complexidade espacial.
- **Fase 4 (18 anos):** combina tudo, timing apertado. Ponto alto de desafio.

#### Tipos de interação
- **Exploração:** descobrir os 3 colecionáveis (fotografias) escondidos em cada fase
- **Confronto:** combate não-letal (atordoar/empurrar), evasão ou manipulação — preferir manipulação quando possível
- **Estratégia:** decidir quando congelar, quando carregar, quando trocar personagem
- **Resolução de puzzle:** o cenário inteiro é o quebra-cabeça

### 3.3. Game-Game

#### Sistemas que conversam entre si
- **Tempo congelado ↔ Movimento de inimigos:** congelar abre janelas seguras
- **Dualidade de tamanho ↔ Geometria do nível:** cada nível é desenhado para forçar cooperação
- **Energia do relógio ↔ Tensão temporal:** recurso limitado força planejamento
- **Progresso nas fases ↔ Estado do hub:** janela do hub muda visualmente a cada fase concluída
- **Coletáveis ↔ Final:** 3 finais possíveis baseados em quantas fotos o jogador encontrou

---

## 4. Experience

### 4.1. Senses (feedback sensorial)

#### Audiovisual
- **Visual:** pixel art 8-bits, paleta restrita por fase, CRT scanline sutil
- **Filtro de congelamento:** azul-acinzentado, granulação leve, pulsação ciano de 3 frames na ativação
- **Áudio:** chiptune lo-fi, blips de texto por personagem, SFX de zumbido elétrico no congelamento
- **Háptica:** vibração leve no gamepad ao congelar/descongelar (se aplicável)

### 4.2. Cerebellum (emoções)
- **Nostalgia:** cenários reconhecíveis da infância brasileira
- **Culpa:** o adulto revive erros que nunca resolveu
- **Ternura:** carregar literalmente o seu eu mais novo
- **Tensão:** recurso de tempo limitado, antagonistas que se aproximam
- **Esperança:** o presente clareando a cada fase resolvida
- **Auto-compaixão:** sentimento culminante do epílogo

### 4.3. Cerebrum (cognição)

#### Desafios
- Planejamento espacial (qual Woody passa por onde)
- Timing (quando congelar, quanto tempo segurar)
- Gerenciamento de recurso (energia do relógio)

#### Descobertas
- Colecionáveis escondidos
- Detalhes ambientais que enriquecem a narrativa (pôsteres, bilhetes, fotos)
- Múltiplos finais

#### Auto-expressão
- Ordem livre de jogar as 4 memórias
- Estilo de jogo (perfeccionista coletando tudo vs. apenas atravessando)

### 4.4. Perception (interpretação do mundo)

O jogador deve perceber que:
- **Os dois personagens são a mesma pessoa em tempos diferentes** — mas o jovem não sabe disso (vê o adulto como "tio estranho")
- **O tempo congelado é uma metáfora de cuidado**, não de poder — você para o mundo para arrumar o caminho da sua versão jovem
- **Os "vilões" são pessoas reais** — bully, pais, amigos — não monstros. Podem ser combatidos quando necessário, mas o combate é não-letal (atordoa/empurra) e nunca é o caminho mais elegante: manipular o cenário continua sendo a graça do jogo
- **Falhar não é punido** — a fase reinicia com leveza, reforçando o tema de auto-compaixão
- **O presente reflete o esforço**: a praça muda visualmente, dando sentido emocional à progressão

---

## 5. Inspirações

| Jogo / Obra | O que inspira |
|---|---|
| *Braid* | Manipulação do tempo como narrativa |
| *Celeste* | Plataforma com tema de saúde mental |
| *Brothers: A Tale of Two Sons* | Dois personagens, dois corpos, uma história |
| *Inside / Limbo* | Atmosfera melancólica em 2D |
| *Undertale* | Estilo 8/16-bits com peso emocional |
| *Filme "About Time"* | Viagem ao passado para corrigir o eu |
