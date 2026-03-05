using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class blockclear : MonoBehaviour
{
    public const int width = 20;
    public const int height = 80;
    public static Transform[,] grid = new Transform[width, height];
    public static int ScoreForSpeed = 0;

    [Header("Score Settings")]
    public int currentScore = 0;
    public int nowdamage = 0;
    public TextMeshProUGUI scoreText;
    Enemybase eb;

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        grid = new Transform[width, height];
        UpdateScoreUI();
    }

    public static Vector2Int PosToIndex(Vector3 pos)
    {
        int x = Mathf.RoundToInt((pos.x + 4.5f) * 2f);
        int y = Mathf.RoundToInt((pos.y + 19f) * 2f);
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
                SceneManager.LoadScene("Mainmenu");
                return;
            }
        }
    }

    void AddScore(int lines)
    {
        int[] bonusScore = { 0, 50, 100, 200, 400 };
        currentScore += (lines <= 4) ? bonusScore[lines] : lines * 100;

        int[] damageAmount = { 0, 10, 30, 55, 100 };
        nowdamage = (lines <= 4) ? damageAmount[lines] : lines * 25;

        if (eb != null) eb.hit(nowdamage);
        else
        {
            eb = FindAnyObjectByType<Enemybase>();
            if (eb != null) eb.hit(nowdamage);
        }

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
        // 실제 게임 화면 가로 칸 수에 맞춰 이 숫자를 조절하세요.
        // 화면이 10칸 너비라면 10, 20칸 너비라면 20으로 설정합니다.
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