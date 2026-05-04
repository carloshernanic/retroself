# Setup do Menu Principal — Retroself

Guia passo a passo para montar a cena `MainMenu` no Unity Editor.

---

## 1. Criar a cena

1. No **Project window**: `Assets/Scenes/` → clique direito → **Create → Scene** → nome: `MainMenu`
2. Abra a cena (duplo clique)
3. Em **File → Build Profiles → Scene List**, adicione `MainMenu` e `Prologue` (criar `Prologue` em branco por enquanto)
4. Garanta que `MainMenu` esteja em **índice 0**

---

## 2. Configurar a câmera

Selecione a **Main Camera** e ajuste:
- **Projection:** Orthographic
- **Size:** 5
- **Background:** preto sólido (`#0a0a14`)
- **Position:** `(0, 0, -10)`

---

## 3. Criar o background da praça

> Você vai colocar seus sprites depois. Por enquanto, monte a estrutura com placeholders.

### Hierarquia sugerida (vazia → placeholders)

```
MainMenuRoot (GameObject vazio na origem)
├── Background
│   ├── Sky (Sprite Renderer)
│   ├── BuildingsBack (Sprite Renderer)
│   ├── Tree (Sprite Renderer)
│   ├── Cabin (Sprite Renderer)
│   │   └── CabinGlow (Sprite Renderer com aditivo, alpha animado)
│   ├── Bench (Sprite Renderer)
│   └── StreetLamp (Sprite Renderer com Light 2D)
├── Effects
│   ├── Rain (Particle System)
│   └── Lightning (Light 2D, esporádica via script)
└── Audio
    ├── Music (AudioSource — toca tema da praça em loop)
    └── Ambience (AudioSource — chuva loop)
```

### Configuração do Particle System "Rain"

- **Shape:** Box, Scale `(20, 1, 1)`, posição acima da câmera
- **Start Speed:** -8
- **Start Lifetime:** 2
- **Start Size:** 0.05
- **Emission Rate over Time:** 200
- **Color:** branco com alpha 80
- **Renderer → Material:** Default-Particle

---

## 4. Criar o Canvas do menu

### 4.1. Canvas principal

- **GameObject → UI → Canvas** → renomeie para `MenuCanvas`
- **Render Mode:** Screen Space - Overlay
- **Canvas Scaler → UI Scale Mode:** Scale With Screen Size
- **Reference Resolution:** 1920 × 1080
- **Match:** 0.5

### 4.2. Título

- Dentro do Canvas: **UI → Text - TextMeshPro**
- Nome: `Title`
- Texto: `RETROSELF`
- Fonte: pixel font (sugestão: importar **VT323** ou **Press Start 2P** via Window → TextMeshPro → Font Asset Creator)
- Tamanho: ~120
- Cor: ciano `#7DF9FF`
- **Posição:** centro-superior, aproximadamente `(0, 250)`
- **Outline:** preto, espessura 0.2

### 4.3. Subtítulo

- Dentro do Canvas: **UI → Text - TextMeshPro**
- Nome: `Subtitle`
- Texto: `"E se você pudesse voltar e ajudar a si mesmo a não desistir?"`
- Fonte: pixel font, itálico
- Tamanho: ~28
- Cor: branco com alpha 70
- **Posição:** abaixo do título, aproximadamente `(0, 130)`

### 4.4. Botões

Para cada botão (`Comecar`, `Creditos`, `Sair`):

- **GameObject → UI → Button - TextMeshPro**
- Nome: `Btn_Comecar` (e respectivos)
- Remover Image (deixar o botão sem fundo) **ou** dar uma cor sólida com alpha 30
- Texto do filho:
  - `Btn_Comecar` → `Começar`
  - `Btn_Creditos` → `Créditos`
  - `Btn_Sair` → `Sair`
- **Cor do texto:** branco
- **Hover:** trocar cor para ciano via `Button → Color Tint`
- **Posição:** alinhados verticalmente no canto inferior esquerdo

Sugestão de posições:
- `Btn_Comecar` → `(-700, -200)`
- `Btn_Creditos` → `(-700, -280)`
- `Btn_Sair` → `(-700, -360)`

### 4.5. Painel de créditos (oculto)

- Dentro do Canvas: **UI → Panel** → renomeie para `CreditsPanel`
- Cor de fundo: preto com alpha 90
- Tamanho: full screen
- Adicione um **TextMeshPro** dentro com:

```
RETROSELF

Direção & Game Design
Alex Chequer
Carlos Hernani
Lucas Ikawa

Música & Som
[Seu nome]

Pixel Art
[Seu nome]

Inspirado em
Braid · Celeste · Brothers · Inside
Undertale · About Time

Insper · 2026
```

- Adicione um botão `Btn_FecharCreditos` com texto `Voltar` no canto inferior direito do painel
- **Desative o `CreditsPanel` no Inspector** (deixa ele oculto por padrão)

---

## 5. Conectar os scripts

### 5.1. MenuActions

1. Crie um GameObject vazio chamado `MenuController`
2. **Add Component → MenuActions**
3. No campo `Credits Panel`, arraste o `CreditsPanel` da hierarquia

### 5.2. Conectar os botões

Para cada botão, no Inspector:

1. Vá até **Button → On Click()**
2. Clique no `+`
3. Arraste o `MenuController` para o slot
4. Escolha:
   - `Btn_Comecar` → `MenuActions.Comecar`
   - `Btn_Creditos` → `MenuActions.AbrirCreditos`
   - `Btn_Sair` → `MenuActions.Sair`
   - `Btn_FecharCreditos` → `MenuActions.FecharCreditos`

### 5.3. TitleGlitch (efeito no título)

1. Selecione o GameObject `Title`
2. **Add Component → TitleGlitch**
3. Arraste o próprio TMP_Text para o campo `Title Text`

### 5.4. CabinPulse (luz da cabine pulsando)

1. Selecione o GameObject `Cabin`
2. **Add Component → CabinPulse**
3. Arraste o `CabinGlow` (Sprite Renderer com material aditivo) para o campo
4. Ajuste `Pulse Speed` a gosto

### 5.5. MenuMusic

1. Selecione o GameObject `Music` em Audio
2. **Add Component → MenuMusic**
3. Arraste seu clip da trilha da praça para o campo `Music Clip`
4. Ajuste `Target Volume` (0.6 sugerido)

---

## 6. Testar

1. Aperte **Play**
2. Você deve ver:
   - Cena com chuva, cabine pulsando, título com glitch ocasional
   - Música tocando com fade-in
   - Botões funcionais (Começar tenta carregar a cena `Prologue` — se ela não existir, cria uma vazia primeiro)

---

## 7. Próximos passos

- [ ] Criar a cena `Prologue` (mesmo cenário, mas com Woody andando)
- [ ] Importar fonte pixel art real (VT323 ou Press Start 2P)
- [ ] Substituir placeholders por sprites finais
- [ ] Adicionar SFX de hover/click nos botões
- [ ] Adicionar transição de fade ao trocar de cena
