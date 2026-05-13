using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Snake game embedded num MinigameOverlay. Grid retangular dentro do `gridArea`
// (RectTransform). Snake e comida são UI Images criados em runtime, pooled
// internamente (lista de segmentos é reusada). Movement em Time.unscaledDeltaTime
// porque o jogo principal tem Time.timeScale=0.
//
// Win: comer `targetScore` comidas (default 10).
// Lose/Esc: snake bate em parede/si mesma, ou jogador aperta Esc.
// Lose mostra "GAME OVER — Espaço pra retry" — Espaço reinicia, Esc fecha.
public class SnakeMinigame : MinigameOverlay
{
    [Header("Grid")]
    public RectTransform gridArea;
    public int cols = 16;
    public int rows = 12;
    public float moveInterval = 0.16f;
    public int targetScore = 10;

    [Header("Visual")]
    public Color snakeHeadColor = new Color(0.9f, 1f, 0.4f, 1f);
    public Color snakeBodyColor = new Color(0.6f, 0.8f, 0.3f, 1f);
    public List<Sprite> foodSprites = new List<Sprite>();

    [Header("UI Refs")]
    public TMP_Text scoreText;
    public TMP_Text statusText;

    Vector2Int dir = new Vector2Int(1, 0);
    Vector2Int nextDir = new Vector2Int(1, 0);
    readonly List<Vector2Int> body = new List<Vector2Int>();
    Vector2Int foodCell;
    float moveTimer;
    int score;
    bool gameOver;

    readonly List<Image> bodyTiles = new List<Image>();
    Image foodTile;
    float cellSize;

    protected override void OnStart()
    {
        ResetGame();
    }

    protected override void OnEnd(bool won)
    {
        // Limpar tiles do pool — ao reabrir, recria do zero.
        foreach (var t in bodyTiles) if (t != null) Destroy(t.gameObject);
        bodyTiles.Clear();
        if (foodTile != null) { Destroy(foodTile.gameObject); foodTile = null; }
    }

    void ResetGame()
    {
        body.Clear();
        body.Add(new Vector2Int(cols / 2, rows / 2));
        body.Add(new Vector2Int(cols / 2 - 1, rows / 2));
        body.Add(new Vector2Int(cols / 2 - 2, rows / 2));
        dir = new Vector2Int(1, 0);
        nextDir = dir;
        score = 0;
        moveTimer = 0f;
        gameOver = false;
        PlaceFood();
        UpdateScoreText();
        if (statusText != null) statusText.text = "Setas pra mover. Esc pra desistir.";
        RebuildTiles();
    }

    protected override void TickGame()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            Lose();
            return;
        }

        if (gameOver)
        {
            if (kb.spaceKey.wasPressedThisFrame) ResetGame();
            return;
        }

        // Input: setas (não permite reversão direta).
        if (kb.leftArrowKey.wasPressedThisFrame  && dir.x ==  0) nextDir = new Vector2Int(-1, 0);
        if (kb.rightArrowKey.wasPressedThisFrame && dir.x ==  0) nextDir = new Vector2Int( 1, 0);
        if (kb.upArrowKey.wasPressedThisFrame    && dir.y ==  0) nextDir = new Vector2Int( 0, 1);
        if (kb.downArrowKey.wasPressedThisFrame  && dir.y ==  0) nextDir = new Vector2Int( 0,-1);

        moveTimer += Time.unscaledDeltaTime;
        if (moveTimer < moveInterval) return;
        moveTimer = 0f;

        dir = nextDir;
        var head = body[0];
        var next = new Vector2Int(head.x + dir.x, head.y + dir.y);

        if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows)
        {
            EndGame();
            return;
        }
        // Auto-colisão: ignora a cauda última porque ela vai sair desse frame, EXCETO se a snake comeu agora.
        bool willEat = next == foodCell;
        int checkEnd = willEat ? body.Count : body.Count - 1;
        for (int i = 0; i < checkEnd; i++)
        {
            if (body[i] == next) { EndGame(); return; }
        }

        body.Insert(0, next);
        if (willEat)
        {
            score++;
            SfxBeep.PlayFoodPickup();
            UpdateScoreText();
            if (score >= targetScore) { Win(); return; }
            PlaceFood();
        }
        else
        {
            body.RemoveAt(body.Count - 1);
            SfxBeep.PlaySnakeMove();
        }
        RebuildTiles();
    }

    void EndGame()
    {
        gameOver = true;
        SfxBeep.PlayGameOver();
        if (statusText != null) statusText.text = "GAME OVER\nEspaço pra tentar de novo, Esc pra sair.";
    }

    void PlaceFood()
    {
        // Tenta achar uma célula livre random.
        for (int tries = 0; tries < 64; tries++)
        {
            var c = new Vector2Int(Random.Range(0, cols), Random.Range(0, rows));
            bool collides = false;
            for (int i = 0; i < body.Count; i++) if (body[i] == c) { collides = true; break; }
            if (!collides) { foodCell = c; break; }
        }
        // Nova comida = sprite novo (zera pra RebuildTiles re-randomizar).
        if (foodTile != null) foodTile.sprite = null;
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = $"{score} / {targetScore}";
    }

    void RebuildTiles()
    {
        if (gridArea == null) return;
        cellSize = Mathf.Min(gridArea.rect.width / cols, gridArea.rect.height / rows);

        // Pool: garante body tiles >= body.Count.
        while (bodyTiles.Count < body.Count) bodyTiles.Add(CreateTile(Color.white));
        for (int i = body.Count; i < bodyTiles.Count; i++) bodyTiles[i].gameObject.SetActive(false);

        for (int i = 0; i < body.Count; i++)
        {
            var img = bodyTiles[i];
            img.gameObject.SetActive(true);
            img.color = i == 0 ? snakeHeadColor : snakeBodyColor;
            img.sprite = null;
            img.rectTransform.sizeDelta = new Vector2(cellSize - 1f, cellSize - 1f);
            img.rectTransform.anchoredPosition = CellToLocal(body[i]);
        }

        if (foodTile == null) foodTile = CreateTile(new Color(1f, 0.6f, 0.4f, 1f));
        foodTile.gameObject.SetActive(true);
        // Sprite random da lista; se vazia, fica como blob colorido.
        if (foodSprites != null && foodSprites.Count > 0 && foodTile.sprite == null)
        {
            foodTile.sprite = foodSprites[Random.Range(0, foodSprites.Count)];
            foodTile.color = Color.white;
        }
        foodTile.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        foodTile.rectTransform.anchoredPosition = CellToLocal(foodCell);
    }

    Vector2 CellToLocal(Vector2Int c)
    {
        // Origem no canto inferior-esquerdo do gridArea.
        float w = cellSize * cols;
        float h = cellSize * rows;
        float ox = -w * 0.5f + cellSize * 0.5f;
        float oy = -h * 0.5f + cellSize * 0.5f;
        return new Vector2(ox + c.x * cellSize, oy + c.y * cellSize);
    }

    Image CreateTile(Color col)
    {
        var go = new GameObject("Tile", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(gridArea, false);
        var img = go.GetComponent<Image>();
        img.color = col;
        img.raycastTarget = false;
        return img;
    }
}
