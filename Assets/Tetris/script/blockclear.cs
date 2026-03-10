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
    public int combo = 0;
    Enemybase eb;
    BlockBase bb;
    Sound sd;
    string comboText = "";

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
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
                sd.blockclear.Play();
            }
        }

        if (linesCleared > 0)
        {
            combo++;
            AddScore(linesCleared);
            Debug.Log("Combo: " + combo);
            if(sd.blockclear.pitch < 2)
            {
                sd.blockclear.pitch += 0.1f;
            }

        }
        else
        {
            // 줄을 하나도 못 지웠다면 콤보 초기화
            combo = 0;
            comboText = "";
            sd.blockclear.pitch = 1f;
            UpdateScoreUI();
        }

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
        // 콤보 보너스 계산 (예: 콤보당 50점, 데미지 5씩 추가)
        // 첫 번째 줄 제거(combo 1)일 때는 보너스가 없게 하려면 (combo - 1)을 사용합니다.
        int comboScoreBonus = (combo > 1) ? (combo - 1) * 50 : 0;
        int comboDamageBonus = (combo > 1) ? (combo - 1) * 5 : 0;

        int scoreBonus = 0;
        int damageBonus = 0;

        // T-Spin 확인 로직 (기존 유지)
        if (bb != null && bb.Tspin)
        {
            bb.Tspin = false;
            scoreBonus = 100;
            damageBonus = 15;
        }

        int[] bonusScore = { 0, 50, 100, 200, 400 };
        int[] damageAmount = { 0, 10, 30, 55, 100 };

        int lineScore = (lines <= 4) ? bonusScore[lines] : lines * 100;
        int lineDamage = (lines <= 4) ? damageAmount[lines] : lines * 25;

        // 콤보 보너스까지 합산
        currentScore += lineScore + scoreBonus + comboScoreBonus;
        nowdamage = lineDamage + damageBonus + comboDamageBonus;

        if (eb == null) eb = FindAnyObjectByType<Enemybase>();
        if (eb != null) eb.hit(nowdamage);

        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            comboText = (combo > 1) ? $"\n{combo} COMBO!" : "";
            if(combo == 0)
            {
                comboText = "";
            }
            scoreText.text = "Score: " + currentScore + comboText;
        }
    }

    bool IsLineFull(int y)
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null) count++;
        }
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