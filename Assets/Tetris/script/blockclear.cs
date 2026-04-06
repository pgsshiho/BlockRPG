using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class blockclear : MonoBehaviour
{
    // --- 그리드 설정 ---
    public const int width = 20;
    public const int height = 60;
    public static Transform[,] grid = new Transform[width, height];
    public static bool[,] isDamagedHole = new bool[width, height];

    [Header("Grid Position Settings")]
    public float visualYOffset = -19f;

    [Header("Line Detection Settings")]
    public int lineCheckStartX = 6;
    public int lineCheckEndX = 16;
    public int requiredBlocks = 10;

    [Header("Hole Detection (Damage)")]
    public int topOffset = 1;

    [Header("Score & Stats")]
    public static int ScoreForSpeed = 0;
    public static int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public int combo = 0;
    public bool isB2B = false;

    // 내부 상태 및 참조 변수
    Enemybase eb;
    BlockBase bb;
    Sound sd;
    Stat st;
    string displayMessage = "";

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
        st = FindAnyObjectByType<Stat>();

        // 씬 시작 시 데이터 초기화
        grid = new Transform[width, height];
        isDamagedHole = new bool[width, height];
        UpdateScoreUI();
    }

    // 좌표 -> 그리드 인덱스 변환
    public static Vector2Int PosToIndex(Vector3 pos)
    {
        blockclear instance = FindAnyObjectByType<blockclear>();
        float offset = (instance != null) ? instance.visualYOffset : -19f;
        int x = Mathf.RoundToInt((pos.x + 4.5f) * 2f);
        int y = Mathf.RoundToInt((pos.y - offset) * 2f);
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
                y--; // 줄이 내려왔으므로 현재 줄 다시 검사
                linesCleared++;
            }
        }

        if (linesCleared > 0)
        {
            combo++;
            AddScore(linesCleared);
            PlayClearSound();
        }
        else
        {
            if (bb != null) bb.Tspin = false; // 줄 안 지워지면 Tspin 무효
            ResetCombo();
        }

        CheckForEnclosedHoles();
        CheckGameOver();
    }

    bool IsLineFull(int y)
    {
        int count = 0;
        for (int x = lineCheckStartX; x < lineCheckEndX; x++)
        {
            if (x >= 0 && x < width && grid[x, y] != null) count++;
        }
        return count >= requiredBlocks;
    }

    void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null) { Destroy(grid[x, y].gameObject); grid[x, y] = null; }
            isDamagedHole[x, y] = false;
        }
    }

    void DecreaseRowsAbove(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
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

    void AddScore(int lines)
    {
        displayMessage = "";
        bool isTSpin = (bb != null && bb.Tspin);
        bool isDifficult = (lines == 4) || isTSpin;

        int bonusScore = 0;
        int bonusDamage = 0;

        // --- T-Spin & Mini 판정 (각 -10 반영) ---
        if (isTSpin)
        {
            if (lines == 1)
            {
                displayMessage += "T-Spin Mini!\n";
                bonusScore += 90;   // 100 -> 90
                bonusDamage += 5;   // 15 -> 5
            }
            else
            {
                displayMessage += $"T-Spin {lines}Line!\n";
                bonusScore += 190 * lines;  // 200 -> 190
                bonusDamage += 20 * lines;  // 30 -> 20
            }
            bb.Tspin = false;
        }
        else if (lines == 4)
        {
            displayMessage += "TETRIS!\n";
        }

        // --- B2B 판정 (각 -10 반영) ---
        if (isDifficult)
        {
            if (isB2B)
            {
                displayMessage += "Back-to-Back!\n";
                bonusScore += 140;  // 150 -> 140
                bonusDamage += 10;  // 20 -> 10
            }
            isB2B = true;
        }
        else
        {
            isB2B = false;
        }

        int comboScore = (combo > 1) ? (combo - 1) * 50 : 0;
        int comboDamage = (combo > 1) ? (combo - 1) * 5 : 0;

        int[] baseLineScore = { 0, 50, 100, 200, 400 };
        int[] baseLineDamage = { 0, 5, 25, 50, 80 };

        int finalScore = baseLineScore[Mathf.Min(lines, 4)] + bonusScore + comboScore;
        int finalDamage = baseLineDamage[Mathf.Min(lines, 4)] + bonusDamage + comboDamage;

        currentScore += finalScore;
        StartCoroutine(ProcessDamageSequence(finalDamage));
        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            string comboStr = (combo > 1) ? $"{combo} COMBO!\n" : "";
            scoreText.text = $"Score: {currentScore}\n<size=80%>{displayMessage}{comboStr}</size>";
        }
    }

    #region [구멍 감지 로직]
    public void CheckForEnclosedHoles()
    {
        bool[,] isAccessible = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        int startHeight = Mathf.Clamp(height - 1 - topOffset, 0, height - 1);

        for (int x = 0; x < width; x++)
        {
            if (grid[x, startHeight] == null)
            {
                isAccessible[x, startHeight] = true;
                queue.Enqueue(new Vector2Int(x, startHeight));
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

        bool[,] currentLoopVisit = new bool[width, height];
        int newHoleGroupCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null && !isAccessible[x, y] && !currentLoopVisit[x, y] && !isDamagedHole[x, y])
                {
                    newHoleGroupCount++;
                    MarkAndRegisterHole(x, y, isAccessible, currentLoopVisit);
                }
            }
        }

        if (newHoleGroupCount > 0 && st != null)
        {
            int damage = newHoleGroupCount * (3 * st.difficult);
            st.hp -= damage;
            st.diecheck(damage, "Gem");
            st.hpcal();
        }
    }

    void MarkAndRegisterHole(int startX, int startY, bool[,] accessible, bool[,] currentVisit)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(startX, startY));
        currentVisit[startX, startY] = true;
        isDamagedHole[startX, startY] = true;

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
                        isDamagedHole[next.x, next.y] = true;
                        q.Enqueue(next);
                    }
                }
            }
        }
    }
    #endregion

    #region [기타 기능]
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
                int enemyCurrentHp = target.hp;
                float potentialDamage = remainingDamage * multiplier;
                if (potentialDamage >= enemyCurrentHp)
                {
                    int damageConsumed = Mathf.CeilToInt(enemyCurrentHp / multiplier);
                    target.hit(remainingDamage);
                    remainingDamage -= damageConsumed;
                    yield return new WaitForSeconds(1.1f);
                }
                else { target.hit(remainingDamage); remainingDamage = 0; }
            }
            else { yield return new WaitForSeconds(0.1f); }
        }
    }

    public void CheckGameOver()
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, 56] != null)
            {
                Time.timeScale = 1;
                ScoreForSpeed = 0;
                Gameover.killerName = "Gem";
                SceneManager.LoadScene("Gameover");
                return;
            }
        }
    }

    void ResetCombo()
    {
        combo = 0;
        displayMessage = "";
        if (sd != null && sd.blockclear != null) sd.blockclear.pitch = 1f;
        UpdateScoreUI();
    }

    void PlayClearSound()
    {
        if (sd != null && sd.blockclear != null)
        {
            if (sd.blockclear.pitch < 2f) sd.blockclear.pitch += 0.1f;
            sd.blockclear.Play();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.2f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float wx = x * 0.5f - 4.5f;
                float wy = y * 0.5f + visualYOffset;
                Gizmos.DrawWireCube(new Vector3(wx, wy, 0), new Vector3(0.5f, 0.5f, 0.1f));
            }
        }
        Gizmos.color = Color.red;
        float dY = 56 * 0.5f + visualYOffset;
        Gizmos.DrawLine(new Vector3(-10, dY, 0), new Vector3(10, dY, 0));
    }
    #endregion
}