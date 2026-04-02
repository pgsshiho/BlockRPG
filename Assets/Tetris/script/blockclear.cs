using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class blockclear : MonoBehaviour
{
    public const int width = 30;
    public const int height = 80;
    public static Transform[,] grid = new Transform[width, height];

    // [중요] 이미 데미지를 입힌 구멍 좌표를 기억하는 배열
    public static bool[,] isDamagedHole = new bool[width, height];

    public static int ScoreForSpeed = 0;

    [Header("Score Settings")]
    public int currentScore = 0;
    public int nowdamage = 0;
    public TextMeshProUGUI scoreText;
    public int combo = 0;

    Enemybase eb;
    BlockBase bb;
    Sound sd;
    Stat st;
    string comboText = "";

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
        st = FindAnyObjectByType<Stat>();
        grid = new Transform[width, height];
        isDamagedHole = new bool[width, height]; // 배열 초기화
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

        // 라인 처리가 완전히 끝난 후 "새로운" 구멍이 생겼는지 체크
        CheckForEnclosedHoles();

        CheckGameOver();
    }

    #region [핵심 로직: 중복 방지 구멍 감지]
    public void CheckForEnclosedHoles()
    {
        bool[,] isAccessible = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // 1. 하늘에서 물 붓기 (접근 가능한 빈 공간 찾기)
        for (int x = 0; x < width; x++)
        {
            if (grid[x, height - 1] == null)
            {
                isAccessible[x, height - 1] = true;
                queue.Enqueue(new Vector2Int(x, height - 1));
            }
        }

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = curr + dir;
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                {
                    if (!isAccessible[next.x, next.y] && grid[next.x, next.y] == null)
                    {
                        isAccessible[next.x, next.y] = true;
                        queue.Enqueue(next);
                    }
                }
            }
        }

        // 2. 새로운 구멍 덩어리 판정
        bool[,] currentLoopVisit = new bool[width, height];
        int newHoleGroupCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // [조건] 비어있음 + 하늘과 연결 안됨 + 이번 루프 미방문 + **이전에 데미지 안 입힘**
                if (grid[x, y] == null && !isAccessible[x, y] && !currentLoopVisit[x, y] && !isDamagedHole[x, y])
                {
                    newHoleGroupCount++;
                    // 해당 덩어리 전체를 '데미지 입음' 상태로 마킹
                    MarkAndRegisterHole(x, y, isAccessible, currentLoopVisit);
                }
            }
        }

        // 3. 데미지 처리
        if (newHoleGroupCount > 0)
        {
            st.hp -= newHoleGroupCount * (3 * st.difficult);
            Debug.Log($"새로운 구멍 덩어리 {newHoleGroupCount}개 발견! 데미지 적용.");
            st.hpcal();
        }
    }

    void MarkAndRegisterHole(int startX, int startY, bool[,] accessible, bool[,] currentVisit)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(startX, startY));
        currentVisit[startX, startY] = true;
        isDamagedHole[startX, startY] = true; // 영구 기록에 등록

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            Vector2Int curr = q.Dequeue();
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = curr + dir;
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                {
                    if (!accessible[next.x, next.y] && grid[next.x, next.y] == null && !currentVisit[next.x, next.y])
                    {
                        currentVisit[next.x, next.y] = true;
                        isDamagedHole[next.x, next.y] = true; // 덩어리 전체 등록
                        q.Enqueue(next);
                    }
                }
            }
        }
    }
    #endregion

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
        int[] damageAmount = { 0, 5, 25, 50, 80 };

        int lineScore = (lines <= 4) ? bonusScore[lines] : lines * 100;
        int lineDamage = (lines <= 4) ? damageAmount[lines] : lines * 25;

        currentScore += lineScore + scoreBonus + comboScoreBonus;
        int totalDamageToApply = lineDamage + damageBonus + comboDamageBonus;

        StartCoroutine(ProcessDamageSequence(totalDamageToApply));

        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    IEnumerator ProcessDamageSequence(int remainingDamage)
    {
        while (remainingDamage > 0)
        {
            Enemybase target = null;
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);

            foreach (var e in enemies)
            {
                if (!e.IsDead) { target = e; break; }
            }

            if (target != null)
            {
                float multiplier = (Stat.instance != null) ? (1.0f + (Stat.instance.atk * 0.1f)) : 1f;
                int enemyCurrentHp = target.hp;
                float potentialDamage = remainingDamage * multiplier;

                if (potentialDamage >= enemyCurrentHp)
                {
                    int damageConsumed = Mathf.CeilToInt(enemyCurrentHp / multiplier);
                    target.hit(remainingDamage);
                    remainingDamage -= damageConsumed;
                    yield return new WaitForSeconds(1.1f);
                }
                else
                {
                    target.hit(remainingDamage);
                    remainingDamage = 0;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

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
            // 라인이 지워진 자리는 구멍 상태 해제
            isDamagedHole[x, y] = false;
        }
    }

    void DecreaseRowsAbove(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 구멍 기록도 블록과 함께 아래로 한 칸 이동
                isDamagedHole[x, y - 1] = isDamagedHole[x, y];
                isDamagedHole[x, y] = false;

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
            if (grid[x, 57] != null)
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