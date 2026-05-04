# retroself

> *"E se você pudesse voltar e ajudar a si mesmo a não desistir?"*

Puzzle-platformer narrativo de pixel art 8-bits sobre auto-compaixão. Woody, 30 anos, encontra uma cabine alienígena numa praça chuvosa e recebe a chance de voltar a quatro lembranças da própria vida — mas a única coisa que ele pode mudar é o quanto a versão jovem dele se sente sozinha.

Desenvolvido em **Unity 6 (6000.3.13f1)** para **WebGL** (jogável no navegador via itch.io).

---

## Estado da entrega (MVP — Projeto 2D 2026.1)

Escopo entregue:
- **Prólogo** (praça + cabine alienígena)
- **Hub** com 2 portais ativos
- **Fase 1 — "O Pátio" (7 anos)**: dualidade de tamanho (adulto/jovem), bully que patrulha
- **Fase 2 — "Domingo" (12 anos)**: congelamento do tempo + ondas sonoras dos pais
- **Epílogo** com 3 finais visuais (varia conforme coletáveis encontrados)

Mecânicas implementadas:
- Dois personagens jogáveis (Woody Adulto 16×32, Woody Jovem 16×16)
- Troca de personagem (TAB)
- Carregar o jovem no colo (E)
- Congelamento do tempo com medidor de energia (SHIFT)
- Caixas empurráveis (só o adulto)
- Frestas que só o jovem passa
- Placas de pressão
- Inimigos com patrulha (param quando o tempo congela)
- Ondas sonoras (param quando o tempo congela)
- Checkpoints e respawn sem game-over
- Coletáveis (1 fotografia por fase) que afetam o final

Cortes documentados (fora do escopo do MVP, previstos no GDD original):
- Fase 3 ("A Festa", 15 anos)
- Fase 4 ("A Sala", 18 anos)
- Múltiplos finais com texto único cada (atualmente 3 variantes visuais do mesmo epílogo)

## Controles

| Ação | Teclado |
|---|---|
| Mover | A/D ou ←/→ |
| Pular | Espaço (segurar = pulo mais alto) |
| Trocar de personagem | TAB ou Q |
| Pegar / soltar (Adulto carrega Jovem) | E |
| Congelar o tempo (só Adulto) | SHIFT (segurar) |
| Avançar diálogo | Enter / Espaço / E |
| Pausar | ESC |

Dica: para terminar uma fase, **os dois Woodys precisam estar na zona dourada do final**.

## Como rodar

### Jogar a build (recomendado)
Abra o link do itch.io na descrição da entrega. Funciona em qualquer navegador moderno.

### Rodar pelo código
1. Clone o repositório.
2. Abra a pasta na **Unity Hub** (Unity 6000.3.13f1).
3. Abra a cena `Assets/_Retroself/Scenes/MainMenu.unity` e dê Play, ou rode pelo menu **Retroself → Build WebGL** para gerar a build.

## Arquitetura

```
Assets/_Retroself/
├── Scenes/         — 6 cenas (cada uma é só um GameObject com SceneSetup)
├── Scripts/
│   ├── Core/       — GameManager, InputReader, Bootstrap, ProceduralSprites
│   ├── Player/     — CharacterMotor, WoodyController, PartyManager, CameraFollow
│   ├── Mechanics/  — TimeFreezeSystem, CarrySystem, PushableBox, PressurePlate, …
│   ├── Level/      — SceneBuilder + Scenes/* (cada fase se constrói via código)
│   ├── UI/         — HUD, PauseMenu, SceneFader, InteractPrompt
│   ├── Narrative/  — DialogueSystem (typewriter, blip de voz por personagem)
│   ├── Audio/      — AudioManager (chiptune/SFX gerados em runtime)
│   └── Editor/     — RetroselfBuild (menu para WebGL)
```

**Padrão de design:** as cenas .unity são minimalistas — apenas um `GameObject` com um componente `SceneSetup`. Toda a geometria, players, inimigos e UI são instanciados via `SceneBuilder` no `Start()`. Isso facilita iteração rápida sem mexer no editor e mantém o git limpo.

## Créditos

- **Game design / programação / level design / arte / áudio (placeholders):** equipe do projeto (4 pessoas — preencher nomes na entrega)
- **Engine:** Unity 6 (URP 2D)
- **Fonte UI:** LegacyRuntime.ttf (Unity built-in, livre para uso)
- **Sprites:** placeholders gerados proceduralmente em runtime (`ProceduralSprites.cs`); pixel art final fica como TODO pós-entrega
- **Música/SFX:** chiptune gerado proceduralmente em runtime (`AudioManager.cs`); composição própria — sem assets externos

**Inspirações** (apenas estética/temática, nenhum asset copiado): *Braid*, *Celeste*, *Brothers: A Tale of Two Sons*, *Inside / Limbo*, *Undertale*.

## Documento de Design (GDD)

Versão viva no Milanote (link na entrega Blackboard). Espelho local em [`gdd.md`](gdd.md).

## Riscos conhecidos

- WebGL: builds com compressão Brotli/Gzip exigem servidor com headers corretos; o `RetroselfBuild` desabilita compressão para máxima compatibilidade com itch.io.
- Áudio gerado em runtime usa `AudioClip.Create` — funciona em WebGL, mas latência de primeiros sons pode ser perceptível.

## Plágio

Nenhum trecho de código foi copiado de colegas ou da internet. Estruturas comuns (singleton, state machine simples) seguem padrões de domínio público sem cópia direta. Dois trechos de inspiração reconhecida — coyote-time e jump-buffer no `CharacterMotor` — são padrões clássicos de platformer (referência conceitual: GDC talks de Maddy Thorson sobre Celeste).
