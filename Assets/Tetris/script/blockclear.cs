using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class blockclear : MonoBehaviour
{
    public const int width = 30;
    public const int height = 80;
    public static Transform[,] grid = new Transform[width, height];
    public static int ScoreForSpeed = 0;

    public int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public int combo = 0;

    Sound sd;

    void Awake()
    {
        sd = FindAnyObjectByType<Sound>();
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
                sd.blockclear.Play();
            }
        }

        if (linesCleared > 0)
        {
            combo++;
            AddScore(linesCleared);
            if (sd.blockclear.pitch < 2) sd.blockclear.pitch += 0.1f;
        }
        else
        {
            combo = 0;
            sd.blockclear.pitch = 1f;
            UpdateScoreUI();
        }
        CheckGameOver();
    }

    void AddScore(int lines)
    {
        int comboScoreBonus = (combo > 1) ? (combo - 1) * 50 : 0;
        int comboDamageBonus = (combo > 1) ? (combo - 1) * 5 : 0;

        // T-Spin 체크 (BlockBase 참조)
        int scoreBonus = 0;
        int damageBonus = 0;
        BlockBase bb = FindAnyObjectByType<BlockBase>();
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

        currentScore += lineScore + scoreBonus + comboScoreBonus;
        ScoreForSpeed = currentScore;

        // 데미지 프로세스 시작
        StartCoroutine(ProcessDamageSequence(lineDamage + damageBonus + comboDamageBonus));

        // 점수에 따른 적 구성 변경 (Swap 호출)
        UpdateEnemyPoolByScore(currentScore);
        UpdateScoreUI();
    }

    void UpdateEnemyPoolByScore(int score)
    {
        CustomCloneBase es = FindAnyObjectByType<CustomCloneBase>();
        if (es == null) return;

        if (score < 5000) es.Swap(10, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        else if (score < 15000) es.Swap(5, 10, 5, 2, 0, 0, 2, 0, 0, 0, 0);
        else es.Swap(2, 5, 10, 5, 5, 3, 5, 1, 1, 1, 1);
    }

    IEnumerator ProcessDamageSequence(int remainingDamage)
    {
        while (remainingDamage > 0)
        {
            Enemybase target = null;
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);
            foreach (var e in enemies) { if (!e.IsDead) { target = e; break; } }

            if (target != null)
            {
                float multiplier = (Stat.instance != null) ? (1.0f + (Stat.instance.atk * 0.1f)) : 1f;
                int enemyHP = target.hp;
                float potentialDamage = remainingDamage * multiplier;

                if (potentialDamage >= enemyHP)
                {
                    int consumed = Mathf.CeilToInt(enemyHP / multiplier);
                    target.hit(remainingDamage);
                    remainingDamage -= consumed;
                    yield return new WaitForSeconds(1.1f); // 스폰 대기
                }
                else
                {
                    target.hit(remainingDamage);
                    remainingDamage = 0;
                }
            }
            else yield return new WaitForSeconds(0.2f);
        }
    }

    // --- 기존 유틸리티 함수들 ---
    bool IsLineFull(int y)
    {
        int count = 0;
        for (int x = 0; x < width; x++) if (grid[x, y] != null) count++;
        return count >= 10;
    }
    void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++) if (grid[x, y] != null) { Destroy(grid[x, y].gameObject); grid[x, y] = null; }
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
    public void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + currentScore + (combo > 1 ? $"\n{combo} COMBO!" : ""); }
    public void CheckGameOver() { if (grid[0, 56] != null) SceneManager.LoadScene("Gameover"); }
}