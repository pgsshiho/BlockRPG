using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 사용을 위해 추가

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
            comboText = "";
            sd.blockclear.pitch = 1f;
            UpdateScoreUI();
        }
        CheckGameOver();
    }

    void AddScore(int lines)
    {
        int comboScoreBonus = (combo > 1) ? (combo - 1) * 50 : 0;
        int comboDamageBonus = (combo > 1) ? (combo - 1) * 5 : 0;
        int scoreBonus = 0;
        int damageBonus = 0;

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
        int totalDamageToApply = lineDamage + damageBonus + comboDamageBonus;

        // [중요] 코루틴으로 데미지 연쇄 처리 시작
        StartCoroutine(ProcessDamageSequence(totalDamageToApply));

        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    // 데미지 이월 처리 코루틴
    IEnumerator ProcessDamageSequence(int remainingDamage)
    {
        while (remainingDamage > 0)
        {
            // 현재 살아있는 적 찾기
            Enemybase target = null;
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);

            foreach (var e in enemies)
            {
                if (!e.IsDead) { target = e; break; }
            }

            if (target != null)
            {
                // 스탯 기반 최종 데미지 배율 적용 전 원본 데미지로 계산
                float multiplier = (Stat.instance != null) ? (1.0f + (Stat.instance.atk * 0.1f)) : 1f;
                int enemyCurrentHp = target.hp;

                // 이번 적에게 줄 수 있는 최대 실질 데미지
                float potentialDamage = remainingDamage * multiplier;

                if (potentialDamage >= enemyCurrentHp)
                {
                    // 적이 죽을 만큼 데미지가 충분함
                    int damageConsumed = Mathf.CeilToInt(enemyCurrentHp / multiplier);
                    target.hit(remainingDamage); // hit 내부에서 hp를 깎고 dead 호출
                    remainingDamage -= damageConsumed;

                    // 적이 죽고 다음 적이 스폰될 때까지 대기 (슬라임 애니메이션 시간 고려)
                    yield return new WaitForSeconds(1.1f);
                }
                else
                {
                    // 적이 죽지 않을 정도면 남은 데미지 다 주고 종료
                    target.hit(remainingDamage);
                    remainingDamage = 0;
                }
            }
            else
            {
                // 적이 아직 스폰 안 되었으면 잠시 대기
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // (기존 나머지 함수들 유지: IsLineFull, DeleteLine, DecreaseRowsAbove, CheckGameOver, UpdateScoreUI)
    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            comboText = (combo > 1) ? $"\n{combo} COMBO!" : "";
            scoreText.text = "Score: " + currentScore + comboText;
        }
    }

    bool IsLineFull(int y)
    {
        int count = 0;
        for (int x = 0; x < width; x++) { if (grid[x, y] != null) count++; }
        return count >= 10;
    }

    void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
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

    public void CheckGameOver()
    {
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
}