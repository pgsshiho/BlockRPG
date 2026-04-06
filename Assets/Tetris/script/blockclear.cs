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
    [Tooltip("그리드 격자의 세로 시작점입니다. 배경 판에 맞춰 조절하세요 (-19f 등)")]
    public float visualYOffset = -19f;

    [Header("Line Detection Settings")]
    [Tooltip("가로 몇 번째 칸부터 검사할지")]
    public int lineCheckStartX = 6;
    [Tooltip("가로 몇 번째 칸까지 검사할지")]
    public int lineCheckEndX = 16;
    [Tooltip("한 줄이 터지기 위해 필요한 블록 개수")]
    public int requiredBlocks = 10;

    [Header("Hole Detection (Damage)")]
    [Tooltip("맨 위에서 몇 칸 아래부터 구멍을 찾을지 (천장 여백)")]
    public int topOffset = 1;

    [Header("Score & Stats")]
    public static int ScoreForSpeed = 0;
    public static int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public int combo = 0;

    // 참조 변수
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

        // 씬 전환 시 데이터 초기화
        grid = new Transform[width, height];
        isDamagedHole = new bool[width, height];
        UpdateScoreUI();
    }

    // 좌표를 그리드 인덱스로 변환 (visualYOffset 적용)
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
                y--;
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
            ResetCombo();
        }

        CheckForEnclosedHoles();
        CheckGameOver();
    }

    bool IsLineFull(int y)
    {
        int count = 0;
        // 설정한 X 범위 내에서만 체크
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
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
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

    #region [구멍 감지 및 데미지 로직]
    public void CheckForEnclosedHoles()
    {
        bool[,] isAccessible = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // topOffset을 적용하여 감지 시작 높이 결정
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
            int curruntdamage = newHoleGroupCount * (3 * st.difficult);
            st.hp -= curruntdamage;
            st.diecheck(curruntdamage, "Gem");
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
                else
                {
                    target.hit(remainingDamage);
                    remainingDamage = 0;
                }
            }
            else { yield return new WaitForSeconds(0.1f); }
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

    void PlayClearSound()
    {
        if (sd != null && sd.blockclear != null)
        {
            if (sd.blockclear.pitch < 2f) sd.blockclear.pitch += 0.1f;
            sd.blockclear.Play();
        }
    }

    void ResetCombo()
    {
        combo = 0;
        comboText = "";
        if (sd != null && sd.blockclear != null) sd.blockclear.pitch = 1f;
        UpdateScoreUI();
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

    // --- 에디터 시각화 (Gizmos) ---
    void OnDrawGizmos()
    {
        // 1. 전체 그리드 (회색) - visualYOffset 반영 확인
        Gizmos.color = new Color(1, 1, 1, 0.2f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 변수가 바뀔 때마다 위치가 계산되도록 함
                float wx = x * 0.5f - 4.5f;
                float wy = y * 0.5f + visualYOffset;
                Gizmos.DrawWireCube(new Vector3(wx, wy, 0), new Vector3(0.5f, 0.5f, 0.1f));
            }
        }

        // 2. 한 줄 감지 가로 범위 (노란색 박스)
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.3f);
        float sX = lineCheckStartX * 0.5f - 4.5f;
        float eX = (lineCheckEndX - 1) * 0.5f - 4.5f;
        float cX = (sX + eX) / 2f;

        // 박스 너비 계산 (StartX와 EndX 차이에 따라 변함)
        float bWidth = (lineCheckEndX - lineCheckStartX) * 0.5f;
        // 박스 세로 위치도 visualYOffset에 따라 같이 움직여야 함
        Vector3 bCenter = new Vector3(cX, (height * 0.25f) + visualYOffset, 0);
        Vector3 bSize = new Vector3(bWidth, height * 0.5f, 0.1f);
        Gizmos.DrawCube(bCenter, bSize);

        // 3. 게임오버 라인 (빨간색 선)
        Gizmos.color = Color.red;
        float dY = 56 * 0.5f + visualYOffset;
        Gizmos.DrawLine(new Vector3(-10, dY, 0), new Vector3(10, dY, 0));

        // 4. 구멍 감지 시작 높이 (초록색 선)
        Gizmos.color = Color.green;
        float hY = (height - 1 - topOffset) * 0.5f + visualYOffset;
        Gizmos.DrawLine(new Vector3(-10, hY, 0), new Vector3(10, hY, 0));
    }
}