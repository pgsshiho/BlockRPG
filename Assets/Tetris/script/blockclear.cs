using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class blockclear : MonoBehaviour
{
    public const int width = 30;
    public const int height = 80;
    public static Transform[,] grid = new Transform[width, height];
    public static int ScoreForSpeed = 0;

    [Header("Score Settings")]
    public int currentScore = 0;
    public int nowdamage = 0;
    public TextMeshProUGUI scoreText;
    Enemybase eb;
    BlockBase bb;

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        grid = new Transform[width, height];
        UpdateScoreUI();
    }

    public static Vector2Int PosToIndex(Vector3 pos)
    {
        int x = Mathf.RoundToInt((pos.x + 4.5f+ 0.01f) * 2f);
        int y = Mathf.RoundToInt((pos.y + 19f+ 0.01f) * 2f);
        return new Vector2Int(x, y);
    }

    public void DeleteFullLines()
    {
        int linesCleared = 0;
        for (int y = 0; y < height; y++)
        {
            if (IsLineFull(y))
            {
                DeleteLine(y);
                DecreaseRowsAbove(y + 1);
                y--;
                linesCleared++;
            }
        }
        if (linesCleared > 0) AddScore(linesCleared);
        CheckGameOver();
    }

    public void CheckGameOver()
    {
        // 19번 라인에 블록이 하나라도 있으면 즉시 게임오버
        for (int x = 0; x < width; x++)
        {
            if (grid[x, 55] != null)
            {
                Time.timeScale = 1;
                ScoreForSpeed = 0;
                Gameover.killerName = "블럭";
                SceneManager.LoadScene("Gameover");
                return;
            }
        }
    }

    void AddScore(int lines)
    {
        int scoreBonus = 0;
        int damageBonus = 0;

        // 1. T-Spin 여부 먼저 확인 및 변수 저장
        bool isTspinActive = false;
        if (bb != null && bb.Tspin)
        {
            isTspinActive = true;
            bb.Tspin = false; // 여기서 한 번만 초기화
            scoreBonus = 100;
            damageBonus = 15;
            Debug.Log("T-Spin Bonus Applied!");
        }

        // 2. 기본 점수/데미지 배열
        int[] bonusScore = { 0, 50, 100, 200, 400 };
        int[] damageAmount = { 0, 10, 30, 55, 100 };

        // 3. 최종 계산 (T-Spin 보너스 합산)
        int lineScore = (lines <= 4) ? bonusScore[lines] : lines * 100;
        int lineDamage = (lines <= 4) ? damageAmount[lines] : lines * 25;

        currentScore += lineScore + scoreBonus;
        nowdamage = lineDamage + damageBonus;

        // 4. 적 공격
        if (eb == null) eb = FindAnyObjectByType<Enemybase>();
        if (eb != null) eb.hit(nowdamage);

        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
    }

    bool IsLineFull(int y)
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null) count++;
        }
        // 이 로그를 통해 실제 인덱스당 몇 칸이 인식되는지 확인하세요.
        if (count > 0) Debug.Log($"Line {y} check: {count} blocks found.");
        return count >= 10;
    }

    void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            // [수정] grid[x, y]가 null이 아닐 때만 Destroy를 호출합니다.
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null; // 삭제 후 배열 칸을 비워줍니다.
            }
        }
    }

    void DecreaseRowsAbove(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].position += new Vector3(0, -0.5f, 0);
                }
            }
        }
    }
}