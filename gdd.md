# retroself — Game Design Document

> *"E se você pudesse voltar e ajudar a si mesmo a não desistir?"*

**Versão:** 0.1 (Draft)
**Autor:** Carlos
**Data:** abril de 2026

---

## 1. Visão Geral

**Título:** retroself
**Gênero:** Puzzle-platformer narrativo com mecânica de manipulação do tempo
**Estilo visual:** Pixel art 8-bits (paleta restrita, sprites de 16×16 / 16×32)
**Perspectiva:** 2D side-scroller
**Modo:** Single-player
**Plataformas-alvo (MVP):** PC (Steam, itch.io). Stretch goal: Nintendo Switch e mobile (iOS/Android)
**Engine recomendada:** Godot 4 (alternativas: Unity 2D ou GB Studio para um look mais autêntico de Game Boy)
**Classificação indicativa:** 12+ (temas de fracasso, despejo, melancolia — sem violência gráfica)
**Duração estimada da campanha:** 3 a 5 horas (4 fases + hub + epílogo)

### 1.1. Conceito (pitch de uma frase)
Um homem fracassado e despejado encontra uma máquina alienígena que o leva de volta para quatro momentos decisivos da sua infância e adolescência, onde ele precisa cooperar com seu eu mais novo — usando a diferença de tamanho entre os dois e a habilidade de congelar o tempo — para reescrever a própria história.

### 1.2. Pitch longo
Woody tem 30 anos, foi despejado e dorme num banco de praça. Numa noite chuvosa, encontra uma máquina estranha escondida atrás de uma árvore. Ao entrar, descobre que alienígenas vinham observando ele há anos: não como cobaia, mas como candidato. Eles concluíram que Woody não é um homem mau, apenas alguém que tropeçou nas piores horas. E oferecem um acordo: ele pode escolher **quatro momentos da própria vida**, entre os 5 e os 18 anos, e voltar para ajudar seu eu mais jovem a atravessá-los.

A mágica do jogo está no encontro entre os dois Woodys. O Woody-criança é pequeno, ágil, mas frágil e ingênuo. O Woody-adulto é o dobro do tamanho, pesa, alcança coisas que a criança não alcança, e ainda recebe dos alienígenas um relógio que **congela o tempo**. Cada fase é um quebra-cabeça em movimento: o jogador controla o Woody adulto, posiciona o cenário no momento certo, congela o tempo para preparar o caminho, e libera para o Woody-criança seguir adiante.

### 1.3. Pilares de design
1. **Cooperação assimétrica entre eu-de-agora e eu-de-antes.** A mecânica nunca esconde a metáfora: você está literalmente carregando o seu eu mais novo em alguns momentos.
2. **Tempo como ferramenta de cuidado, não de poder.** Congelar o tempo não serve para combate — serve para preparar o terreno antes que sua versão jovem tropece.
3. **Estética 8-bits com peso emocional.** A simplicidade visual contrasta com a profundidade temática, no espírito de *Undertale*, *Celeste* e *To the Moon*.
4. **Fracasso reversível, mas com consequência.** Cada fase pode ser refeita, mas o jogador sente o peso de ter falhado — não há game over agressivo, e sim uma volta ao início da fase com um pequeno diálogo do Woody-adulto consigo mesmo.

### 1.4. Inspirações
| Jogo / Obra | O que inspira |
|---|---|
| *Braid* | Manipulação do tempo como narrativa, não só mecânica |
| *Celeste* | Plataforma com tema de saúde mental e crescimento pessoal |
| *Brothers: A Tale of Two Sons* | Dois personagens, dois corpos, uma história |
| *The Swapper* | Quebra-cabeças baseados em duplicar/coordenar personagens |
| *Inside / Limbo* | Atmosfera melancólica em plataforma 2D |
| *Undertale* | Estilo 8/16-bits com peso emocional |
| *Filme "About Time"* | Premissa de viagem ao passado para corrigir o eu |

### 1.5. Público-alvo
- **Primário:** Jogadores indie de 18–35 anos que gostam de puzzle-platformers narrativos (público de Celeste, Braid, GRIS).
- **Secundário:** Público brasileiro adulto que se identifica com a temática de "começar do zero", "segunda chance", "lidar com o eu do passado" — tema que ressoa especialmente em momentos de crise econômica.
- **Plataforma de descoberta principal:** Steam, TikTok e YouTube (gameplay + storytelling).

---

## 2. Narrativa

### 2.1. Sinopse
Woody, 30 anos, perdeu o emprego, foi despejado e dorme numa praça da periferia de uma cidade grande. Sua vida é uma sequência de pequenas desistências acumuladas. Numa madrugada, ele encontra uma cabine retrô meio escondida na praça, parecida com um fliperama velho. Quando entra, é abduzido. Os alienígenas — uma espécie observadora, não invasora — explicam que ele foi escolhido para receber uma segunda chance. Não porque ele é especial: porque é comum, e merece. Eles lhe dão um relógio capaz de congelar o tempo e o enviam de volta para quatro momentos da própria vida, à sua escolha.

A cada fase concluída, o presente de Woody se altera sutilmente: a praça começa a clarear, o banco onde ele dormia ganha um cobertor, depois um casaco, depois desaparece. O jogo termina quando Woody acorda — talvez na praça, talvez não.

### 2.2. Tema central
**Auto-compaixão.** O jogo não é sobre apagar o passado, é sobre voltar a ele com o cuidado que você não recebeu na época. A mecânica do tempo congelado é a metáfora central: você não pode mudar o que sua versão jovem fez, mas pode parar o mundo por um instante e arrumar o cenário para que ela tenha alguma chance.

### 2.3. Tom
Melancólico mas esperançoso. Humor seco do Woody-adulto contrastando com a sinceridade do Woody-criança. Diálogos curtos, em texto 8-bits com efeito de máquina de escrever. Nada de cinemáticas longas — toda a narrativa cabe em pequenos diálogos antes e depois de cada fase, mais detalhes ambientais (pôsteres, bilhetes, fotos) durante.

### 2.4. Estrutura narrativa
1. **Prólogo (10 min).** Praça, chuva, cabine, abdução, conversa com os alienígenas, escolha das 4 memórias.
2. **Hub: A Sala dos Alienígenas.** Espaço de seleção entre as 4 fases. O jogador escolhe a ordem em que joga.
3. **Fase 1, 2, 3, 4.** Cada uma com prólogo curto (Woody-adulto narra em off por que aquele momento marcou) e epílogo curto (impacto da fase no presente).
4. **Epílogo.** Woody acorda. O jogador vê o presente reescrito.

### 2.5. As 4 memórias (ordem sugerida, mas livre)
| # | Idade | Local | Tema do momento |
|---|---|---|---|
| 1 | 7 anos | Pátio da escola pública | Bullying e a primeira vez que ele aprendeu a se calar |
| 2 | 12 anos | Casa da família, num domingo | A briga dos pais e a primeira vez que ele se sentiu invisível |
| 3 | 15 anos | Festa de aniversário de um colega | Pressão social, primeira escolha covarde |
| 4 | 18 anos | Manhã do vestibular | Auto-sabotagem e a primeira grande desistência |

(Essas memórias podem ser ajustadas — o que importa é que cada uma tenha um cenário visualmente forte e um obstáculo emocional concreto traduzível em puzzle.)

---

## 3. Personagens

### 3.1. Woody Adulto (jogável)
- **Idade:** 30 anos
- **Aparência:** Sprite de 16×32 (dobro da altura do jovem). Casaco surrado, barba por fazer, olheiras. Paleta apagada (cinzas, marrom).
- **Personalidade:** Cansado, irônico, mas com um fundo de ternura que aparece quando fala com seu eu mais novo.
- **Habilidades:**
  - Pulo médio (não tão alto pelo peso)
  - Empurrar / puxar caixas pesadas
  - Carregar o Woody-criança no colo
  - **Congelar o tempo** (relógio alienígena, recurso central)
  - Alcançar plataformas e interruptores altos

### 3.2. Woody Criança/Adolescente (companheiro / parcialmente jogável)
- **Idade:** 7, 12, 15 ou 18 (varia por fase)
- **Aparência:** Sprite de 16×16 (metade do tamanho do adulto). Roupa colorida, sprite mais vivo. Em cada fase ele tem uma aparência diferente — mochila escolar aos 7, fone de ouvido aos 12, skate aos 15, uniforme escolar aos 18.
- **Personalidade:** Muda por fase. Aos 7, curioso e medroso. Aos 12, fechado. Aos 15, debochado. Aos 18, ansioso. Ele **não vê** o Woody-adulto como ele mesmo — vê como um "tio estranho" que apareceu pra ajudar. (Os alienígenas explicam isso no prólogo.)
- **Habilidades:**
  - Pulo alto (relativo ao tamanho)
  - Cabe em passagens estreitas
  - Corre mais rápido
  - **Não pode congelar o tempo** nem empurrar objetos pesados
  - Em algumas fases, segue o Woody-adulto automaticamente; em outras, é controlado diretamente após troca de personagem

### 3.3. Os Alienígenas (NPCs do hub)
- Trio de criaturas de pixel, cada uma com uma cor primária (vermelho, azul, amarelo).
- Falam pouco, em frases curtas e enigmáticas.
- Funcionam como "tutorial difuso" — explicam o relógio, a regra das 4 memórias, e dão pistas no hub se o jogador empacar.
- Visualmente: simples, sem boca, com expressões mostradas só pelos olhos.

### 3.4. NPCs antagonistas (por fase)
Eles **não são vilões clássicos**. São obstáculos cotidianos da vida do Woody jovem: o bully da escola, o pai irritado, o amigo que pressiona, o tempo do relógio do vestibular. Mecanicamente, são "inimigos" — patrulham, perseguem, bloqueiam o caminho — mas narrativamente, são pessoas reais. O Woody-adulto tem diálogos internos sobre cada um.

---

## 4. Gameplay

### 4.1. Loop principal
1. **Selecionar memória** no hub alienígena.
2. **Prólogo curto** (Woody-adulto narra).
3. **Resolver a fase**: avançar pela plataforma, usar a diferença de tamanho e o congelamento do tempo para que **os dois Woodys** cheguem ao final.
4. **Epílogo curto** (impacto no presente, mostrado visualmente no hub).
5. Voltar ao hub. Repetir até completar as 4. Desbloquear epílogo final.

### 4.2. Mecânicas core

#### 4.2.1. Dualidade de tamanho
- O Woody-adulto tem **o dobro da altura** do jovem.
- Isso cria um sistema de afinidades opostas:
  - **Só o adulto** alcança plataformas altas, empurra blocos, ativa interruptores de pressão pesados.
  - **Só o jovem** passa em frestas, cabe em dutos, ativa interruptores delicados, corre por baixo de obstáculos.
- Em vários momentos, o adulto **carrega** o jovem (sprite combinado de 16×40, movimento mais lento) para atravessar zonas perigosas.

#### 4.2.2. Congelamento do tempo
- Botão dedicado. Ao apertar, **tudo congela** exceto o Woody-adulto: o jovem para no ar, inimigos param, plataformas móveis param, projéteis param.
- Enquanto está congelado, o adulto pode:
  - Reposicionar caixas
  - Abrir portas
  - Bloquear inimigos com objetos
  - Pegar o jovem no colo e movê-lo
- O congelamento tem **medidor de energia** (barra que recarrega lentamente fora do uso). Não é infinito — exige planejamento.
- Visual: tela ganha leve filtro azul-acinzentado e granulação. SFX de zumbido elétrico baixo.

#### 4.2.3. Troca de personagem
- O jogador pode trocar quem está controlando ativamente (botão dedicado).
- O personagem inativo fica parado ou segue um padrão simples (idle / patrulha curta).
- Em algumas fases, há restrições: por exemplo, na fase do vestibular (18 anos), o Woody jovem é controlado mais frequentemente, porque o adulto é mais "ajuda externa".

#### 4.2.4. Sem morte permanente
- Se o jovem cai num buraco ou é "pego" por um obstáculo, a fase reinicia do último checkpoint, com um diálogo curto do adulto: *"Ok, de novo. A gente acerta dessa vez."* — reforçando o tema.
- Sem contador de mortes visível.

### 4.3. Controles (gamepad / teclado)
| Ação | Gamepad | Teclado |
|---|---|---|
| Mover | Analógico esquerdo | A / D ou ←/→ |
| Pular | A / X | Espaço |
| Interagir / pegar | B / Círculo | E |
| Congelar tempo | RT / R2 | Shift |
| Trocar personagem | Y / Triângulo | Tab |
| Pausar | Start | Esc |

### 4.4. Progressão
- Não há sistema de XP, loot ou árvore de habilidades. Toda a progressão é **narrativa e mecânica** — cada fase introduz uma nova variação da mecânica principal:
  - **Fase 1 (7 anos):** introduz dualidade de tamanho.
  - **Fase 2 (12 anos):** introduz congelamento do tempo.
  - **Fase 3 (15 anos):** introduz troca livre de personagem (até aqui ela era restrita).
  - **Fase 4 (18 anos):** combina tudo, com timing apertado.
- **Coletáveis opcionais por fase:** 3 "fotografias" escondidas em cada fase, que desbloqueiam um álbum no hub. Ele revela detalhes da vida do Woody que não cabem nos diálogos. Pura recompensa narrativa, zero efeito mecânico.

---

## 5. Design de Níveis

### 5.1. Hub: A Sala dos Alienígenas
Pequena sala com 4 portais (um por memória) e 1 portal central trancado (epílogo, abre quando os 4 forem resolvidos). Os alienígenas circulam pelo espaço. Há uma janela mostrando a praça do presente — ela vai mudando à medida que as fases são resolvidas. Funciona como feedback visual constante de progresso.

### 5.2. Fase 1 — "O Pátio" (7 anos)
- **Cenário:** Pátio de escola pública, com quadra, bebedouro, árvores, muro do fundo.
- **Objetivo:** Atravessar o pátio durante o recreio sem que o Woody-criança seja pego pelo bully.
- **Mecânicas introduzidas:** Movimentação básica, dualidade de tamanho, primeiro uso de carregar o jovem.
- **Puzzles típicos:** O adulto empurra um banco de madeira para fazer o jovem passar por cima sem ser visto. O jovem se esconde atrás da árvore enquanto o adulto distrai o bully com uma bola. O jovem cabe na fresta sob a quadra; o adulto tem que dar a volta.
- **Antagonista:** Um bully maior que o jovem, mas menor que o adulto. Visualmente caricato, sem ser cruel.
- **Trilha:** Recreio em chiptune — barulho de criança brincando ao fundo, traduzido em pixel.

### 5.3. Fase 2 — "Domingo" (12 anos)
- **Cenário:** Casa de dois andares. Cozinha, sala, quarto do Woody, garagem.
- **Objetivo:** O Woody jovem precisa atravessar a casa, pegar a mochila, e sair pela porta dos fundos sem ouvir a briga dos pais.
- **Mecânicas introduzidas:** Congelamento do tempo. Congelar é necessário para passar por cômodos onde a "voz" dos pais (representada como ondas sonoras visuais 8-bits) preencheria o espaço.
- **Puzzles típicos:** Congelar uma onda sonora a meio caminho do corredor para que o jovem passe por baixo. Empurrar a estante para abafar o som. Subir o adulto para pegar a mochila do alto do armário enquanto o jovem distrai o gato (sim, tem um gato).
- **Antagonista:** As "ondas sonoras" da briga. Não há violência mostrada — só o efeito sonoro/visual sobre a casa.
- **Trilha:** Música de domingo apagado, melancólica, lo-fi 8-bits.

### 5.4. Fase 3 — "A Festa" (15 anos)
- **Cenário:** Casa de festa adolescente, com piscina, garagem, telhado.
- **Objetivo:** O Woody jovem precisa **sair da festa** sem aceitar uma proposta de um amigo (não detalhada — fica ambígua de propósito).
- **Mecânicas introduzidas:** Troca livre entre personagens. O jogador alterna ativamente entre os dois.
- **Puzzles típicos:** O jovem tem que atravessar a multidão (sprites de NPCs em movimento que bloqueiam passagem), enquanto o adulto, do telhado, congela o tempo para criar brechas. O adulto baixa uma escada pelo lado de fora, o jovem desce. Há uma seção em que o jogador precisa fazer os dois andarem **em paralelo** em duas faixas (telhado e térreo).
- **Antagonista:** O "amigo", representado como um NPC que persegue o jovem com diálogo em balão. Não pode ser combatido, só evitado.
- **Trilha:** Festa pulsante 8-bits com bateria distorcida.

### 5.5. Fase 4 — "A Sala" (18 anos)
- **Cenário:** Sala de prova do vestibular. Mesa, relógio gigante na parede, fileiras de carteiras.
- **Objetivo:** O Woody jovem precisa **terminar a prova**. Mas o cenário é abstrato — paredes se movem, o relógio acelera, fantasmas das fases anteriores aparecem como obstáculos (o bully de novo, o pai irritado, o amigo da festa).
- **Mecânicas introduzidas:** Combinação de tudo. Timing apertado.
- **Puzzles típicos:** O adulto congela o relógio gigante para dar tempo ao jovem. Carrega o jovem por seções caóticas. Empurra os "fantasmas" para fora da sala.
- **Antagonista:** O próprio tempo (relógio gigante) e os ecos das fases anteriores.
- **Trilha:** Versão remixada e mais densa dos temas das 3 fases anteriores. É a fase em que a música conta tanto quanto o gameplay.

### 5.6. Epílogo — "A Praça"
- Curto, sem puzzle. Woody-adulto acorda na praça. O cenário ao redor pode ter mudado bastante, pouco, ou nada — depende de **quantas fases o jogador completou perfeitamente** (com todos os coletáveis). Três finais possíveis, sem julgamento moral, mas com mudanças visuais finas. O jogo termina com Woody se levantando do banco e caminhando para fora da tela.

---

## 6. Direção de Arte

### 6.1. Estilo visual
- **Pixel art 8-bits** com paleta restrita (estilo NES + influência Game Boy Color).
- Sprites do protagonista: 16×32 (adulto), 16×16 (jovem).
- Tiles de cenário: 16×16.
- Resolução interna do jogo: 320×180, escalonada para a resolução do monitor.

### 6.2. Paleta de cores
| Cena | Paleta dominante |
|---|---|
| Praça (presente) | Cinzas, azul-marinho, laranja apagado dos postes |
| Sala alienígena | Roxos, ciano, preto profundo |
| Fase 1 (Pátio) | Verde-amarelo escolar, marrom de tijolo |
| Fase 2 (Casa) | Bege, marrom, vermelho desbotado |
| Fase 3 (Festa) | Magenta, azul neon, preto |
| Fase 4 (Vestibular) | Branco frio, cinza, vermelho do relógio |

A ideia é que **cada fase tenha uma identidade cromática própria**, o que ajuda o jogador a se localizar emocionalmente.

### 6.3. HUD / Interface
- HUD minimalista: apenas a barra de energia do congelamento do tempo (canto superior esquerdo, estilo retrô).
- Sem mapa, sem minimapa, sem missões em texto. Tudo é orientação espacial direta.
- Diálogos: caixa de texto pixelada com retrato do personagem em 24×24 (estilo Pokémon clássico). Texto em pt-BR, fonte custom de 8 pixels.

### 6.4. Animações-chave
- Idle do adulto: ele suspira a cada ~5 segundos.
- Idle do jovem: muda por fase (chuta uma pedra aos 7, mexe no fone aos 12, anda no skate aos 15, mexe a perna nervosamente aos 18).
- Carregar: o adulto pega o jovem no colo com cuidado, transição suave.
- Congelamento: tela ganha um quadro de pulsação ciano por 3 frames, depois entra o filtro azul.

---

## 7. Som e Música

### 7.1. Trilha sonora
- **Direção:** Chiptune com camadas modernas. Cada fase tem um tema próprio, e a trilha do epílogo é um remix de todos.
- **Referências sonoras:** *Celeste OST* (Lena Raine), *Undertale OST* (Toby Fox), *Hyper Light Drifter*, e a estética chiptune do *Shovel Knight*.
- **Faixas previstas:**
  1. Tema da praça (prólogo + epílogo)
  2. Tema da sala alienígena (hub)
  3. Tema do pátio (fase 1)
  4. Tema do domingo (fase 2)
  5. Tema da festa (fase 3)
  6. Tema da sala de prova (fase 4)
  7. Tema final (epílogo)

### 7.2. SFX
- Pulo, passos (diferentes para adulto e jovem), congelamento, descongelamento, pegar item, abrir porta, ondas sonoras dos pais (fase 2), passos do bully (fase 1), música distorcida da festa (fase 3), tique-taque do relógio gigante (fase 4).
- Vozes: **sem voice acting**. Apenas blips de texto em diferentes tons (mais grave para o adulto, mais agudo para o jovem, sintético para os alienígenas).

---

## 8. Tecnologia e Produção

### 8.1. Engine e ferramentas
- **Godot 4** (motor principal). Open-source, leve, ótimo para 2D.
- **Aseprite** para pixel art e animações.
- **FamiStudio** ou **DefleMask** para a trilha chiptune.
- **Tiled** para mapas de níveis (se não usar o editor nativo da Godot).

### 8.2. Plataformas-alvo
- **MVP:** Steam (Windows + Linux), itch.io.
- **Stretch:** Nintendo Switch (via Godot console export), Mac, mobile com ajustes de controle.

### 8.3. Equipe mínima ideal
- 1 game designer / programador (você?)
- 1 pixel artist / animador
- 1 compositor chiptune
- 1 escritor / revisor narrativo (pode ser o próprio designer)
- Total: 2–4 pessoas para um escopo de 6–9 meses.

### 8.4. Cronograma (alto nível, 9 meses)
| Mês | Marco |
|---|---|
| 1 | Pré-produção: protótipo da mecânica core (dualidade + tempo) |
| 2 | Vertical slice: 30% da fase 1 jogável, com arte e som finais |
| 3–4 | Produção da fase 1 e 2 |
| 5–6 | Produção da fase 3 e 4 |
| 7 | Hub, epílogo, polimento de transições |
| 8 | Playtest fechado, balanceamento |
| 9 | Polimento final, marketing pré-lançamento, lançamento |

---

## 9. Monetização e Escopo

- **Modelo:** Pago, preço único. Faixa sugerida: **US$ 9,99 / R$ 29,90**.
- **Sem DLC, sem microtransações, sem season pass.** O jogo é uma obra fechada.
- **Possíveis expansões pós-lançamento (gratuitas):** modo "speedrun", modo "+1 memória" (5ª fase opcional, talvez baseada em escolhas do jogador via comunidade).

---

## 10. Riscos e Próximos Passos

### 10.1. Riscos
| Risco | Mitigação |
|---|---|
| Mecânica do tempo congelado pode ficar trivial ou frustrante | Fazer protótipo cedo (mês 1) e iterar com playtesters externos |
| Tema melancólico pode afastar parte do público | Reforçar humor e ternura nos diálogos; trailer focado na esperança |
| Escopo das 4 fases pode crescer demais | Definir limite rígido de 30 min por fase no documento de level design |
| Carregamento do jovem pelo adulto pode ficar técnico chato | Animar com peso, dar SFX gostoso, tornar a ação prazerosa em si |

### 10.2. Próximos passos imediatos
1. **Protótipo de uma tela única** com dualidade de tamanho + congelamento do tempo (sem arte final, retângulos coloridos). Meta: 2 semanas.
2. **Fechar o roteiro** das 4 memórias com diálogos completos.
3. **Mood board visual** por fase.
4. **Definir paleta** e fazer o sprite final do Woody-adulto e Woody-criança (todas as 4 idades).
5. **Música:** compor o tema da praça primeiro — é o que define o tom de tudo.

---

## Apêndice A — Glossário rápido

- **Woody-adulto / Woody-presente:** Protagonista jogável principal, 30 anos, dobro da altura do jovem.
- **Woody-jovem:** Versão de 7, 12, 15 ou 18 anos do mesmo personagem. Companheiro, parcialmente jogável.
- **Memória:** Cada uma das 4 fases. Internamente, equivale a um nível.
- **Hub:** Sala dos alienígenas, espaço entre fases.
- **Congelamento:** Habilidade do adulto que para tudo no cenário menos ele.
- **Dualidade:** Princípio de design que estrutura todas as fases — o que um Woody pode, o outro não.