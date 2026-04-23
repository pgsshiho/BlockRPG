using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class blockclear : MonoBehaviour
{
    public const int width = 20;
    public const int height = 80;
    public static Transform[,] grid = new Transform[width, height];
    public static bool[,] isDamagedHole = new bool[width, height];

    [Header("Grid Position Settings")]
    public float visualYOffset = -19f;
    public int topOffset = 2;

    [Header("Line Detection Settings")]
    public int lineCheckStartX = 6;
    public int lineCheckEndX = 16;
    public int requiredBlocks = 10;

    [Header("Score & Combo System")]
    public static int ScoreForSpeed = 0;
    public static int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public int combo = 0; // 명칭 유지
    public bool isLinkActive = false;

    [Header("UI Effects")]
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint;

    Enemybase eb;
    BlockBase bb;
    Sound sd;
    Stat st;
    string displayMessage = "";
    reincarnationSkill skill;
    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
        st = FindAnyObjectByType<Stat>();
        skill = FindAnyObjectByType<reincarnationSkill>();
        grid = new Transform[width, height];
        isDamagedHole = new bool[width, height];
        UpdateScoreUI();
    }

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
                PushDownRows(y + 1);
                y--;
                linesCleared++;
            }
        }

        if (linesCleared > 0)
        {
            combo++;
            CalculateReward(linesCleared);
            PlaySuccessSound();
        }
        else
        {
            if (bb != null) bb.IsTwist = false;
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

    void PushDownRows(int startY)
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

    void CalculateReward(int lines)
    {
        displayMessage = "";
        bool twistPerformed = (bb != null && bb.IsTwist);
        bool isSpecial = (lines >= 4) || twistPerformed;

        int bonusScore = 0;
        int bonusPower = 0;

        // 1. 특수 제거 보너스 하향
        if (twistPerformed)
        {
            if (lines == 1) { displayMessage += "Twist Single!\n"; bonusScore += 100; bonusPower += 5; } // 150 -> 100
            else if (lines == 2) { displayMessage += "Twist Double!\n"; bonusScore += 300; bonusPower += 20; } // 450 -> 300
            else { displayMessage += $"Twist Burst {lines}!\n"; bonusScore += 500 * lines; bonusPower += 30 * lines; } // 700 -> 500
            bb.IsTwist = false;
        }
        else if (lines >= 4) { displayMessage += "ULTRA BURST!\n"; }

        if (isSpecial)
        {
            if (isLinkActive) { displayMessage += "LINK BONUS!\n"; bonusScore += 100; bonusPower += 10; } // 200 -> 100
            isLinkActive = true;
        }
        else { isLinkActive = false; }

        if (!string.IsNullOrEmpty(displayMessage)) SpawnFloatingText(displayMessage.Replace("\n", " "), 35, Color.green);

        // 2. 콤보 보너스 하향 (제곱에서 배수로 변경하여 폭주 방지)
        int comboBonus = (combo > 1) ? (combo * 25) : 0; // combo*combo*12 -> combo*25
        int comboPower = (combo > 1) ? (int)(combo * 4f) : 0; // 7.5f -> 4f

        // 3. 기본 라인 점수/데미지 하향
        int[] baseLineScore = { 0, 40, 100, 250, 500 }; // 70, 180, 350, 750에서 하향
        int[] baseLinePower = { 0, 5, 15, 35, 70 };      // 10, 30, 60, 110에서 하향

        int finalScore = baseLineScore[Mathf.Min(lines, 4)] + bonusScore + comboBonus;
        int finalPower = baseLinePower[Mathf.Min(lines, 4)] + bonusPower + comboPower;

        currentScore += finalScore;
        skill.TryActivateSelfHeal(lines);
        skill.TrySkill();
        StartCoroutine(ProcessAttackSequence(finalPower));
        ScoreForSpeed = currentScore;
        UpdateScoreUI();
    }

    IEnumerator ProcessAttackSequence(int remainingAtk)
    {
        while (remainingAtk > 0)
        {
            Enemybase target = null;
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);
            foreach (var e in enemies) { if (!e.IsDead) { target = e; break; } }

            if (target != null)
            {
                // 4. 스탯에 따른 공격력 배율 하향 (13% -> 7%)
                float multiplier = (Stat.instance != null) ? (1.0f + (Stat.instance.atk * 0.07f)) : 1f;

                // 워리어 버프 시 데미지 50% 증가
                float warriorMult = RoleSkill.isWarrior ? 1.5f : 1f;

                int enemyCurrentHp = target.hp;
                int potentialAtk = Mathf.CeilToInt(remainingAtk * multiplier * warriorMult);

                if (potentialAtk > 0) SpawnFloatingText("-" + potentialAtk.ToString(), 55, Color.red);

                if (potentialAtk >= enemyCurrentHp)
                {
                    // 적 체력을 깎는데 소모된 순수 공격력 계산 (워리어 보정 포함)
                    int consumed = Mathf.CeilToInt(enemyCurrentHp / (multiplier * warriorMult));
                    target.hit(potentialAtk); // 실제 데미지 전달
                    if (RoleSkill.isArcher)
                    {
                        yield return new WaitForSeconds(0.1f);
                        target.hit(potentialAtk);
                    }
                    remainingAtk -= consumed;
                    yield return new WaitForSeconds(0.5f); // 대기 시간 단축 (속도감)
                }
                else
                {
                    target.hit(potentialAtk);
                    if (RoleSkill.isArcher)
                    {
                        yield return new WaitForSeconds(0.1f);
                        target.hit(potentialAtk);
                    }
                    remainingAtk = 0;
                }
            }
            else { yield return new WaitForSeconds(0.1f); }
        }
    }
    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            string comboStr = (combo > 1) ? $"{combo} COMBO!\n" : ""; // 명칭 유지
            scoreText.text = $"SCORE: {currentScore:N0}\n<size=70%>{displayMessage}{comboStr}</size>";
        }
    }

    public void CheckForEnclosedHoles()
    {
        bool[,] isAccessible = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        int startHeight = Mathf.Clamp(height - 1 - topOffset, 0, height - 1);

        for (int x = 0; x < width; x++)
        {
            if (grid[x, startHeight] == null) { isAccessible[x, startHeight] = true; queue.Enqueue(new Vector2Int(x, startHeight)); }
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
                    if (!isAccessible[next.x, next.y] && grid[next.x, next.y] == null) { isAccessible[next.x, next.y] = true; queue.Enqueue(next); }
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
            int penalty = newHoleGroupCount * (4 * st.difficult);
            st.hp -= penalty;
            st.diecheck(penalty, "Pressure");
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

    public void CheckGameOver()
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, 61] != null)
            {
                Time.timeScale = 1;
                ScoreForSpeed = 0;
                Gameover.killerName = "GEM"; // 명칭 유지 가능
                SceneManager.LoadScene("Gameover"); // 명칭 유지
                return;
            }
        }
    }

    void ResetCombo() { combo = 0; displayMessage = ""; if (sd != null && sd.blockclear != null) sd.blockclear.pitch = 1f; UpdateScoreUI(); }
    void PlaySuccessSound() { if (sd != null && sd.blockclear != null) { if (sd.blockclear.pitch < 2.8f) sd.blockclear.pitch += 0.14f; sd.blockclear.Play(); } }
    void SpawnFloatingText(string message, float fontSize, Color textColor)
    {
        if (floatingTextPrefab == null || textSpawnPoint == null) return;
        GameObject obj = Instantiate(floatingTextPrefab, textSpawnPoint);
        obj.transform.localPosition = new Vector3(Random.Range(-50f, 50f), Random.Range(-20f, 20f), 0);
        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null) ft.Setup(message, fontSize, textColor);
    }
    // 에디터에서 범위를 시각적으로 표시해주는 Gizmos 함수
    private void OnDrawGizmos()
    {
        // 1. 라인 감지 범위 표시 (Line Detection Range)
        // 설정된 startX, endX, 그리고 y축 전체 범위를 사각형으로 표시
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 투명한 빨간색

        // 인덱스를 실제 월드 좌표로 변환하기 위한 계산
        float worldStartX = (lineCheckStartX / 2f) - 4.5f;
        float worldEndX = (lineCheckEndX / 2f) - 4.5f;
        float widthSize = worldEndX - worldStartX;

        // 중심점과 사이즈 계산
        Vector3 center = new Vector3(worldStartX + (widthSize / 2f) - 0.25f, visualYOffset + (height / 4f), 0);
        Vector3 size = new Vector3(widthSize, height / 2f, 0.1f);

        Gizmos.DrawWireCube(center, size);
        Gizmos.DrawCube(center, size);

        // 2. 게임 오버 감지선 표시 (GameOver Line)
        // y = 61 인덱스를 월드 좌표로 변환
        float gameOverY = (61 / 2f) + visualYOffset;
        Gizmos.color = Color.yellow; // 게임오버 선은 노란색
        Vector3 lineStart = new Vector3(-5f, gameOverY, 0);
        Vector3 lineEnd = new Vector3(5f, gameOverY, 0);

        Gizmos.DrawLine(lineStart, lineEnd);

        // 게임오버 텍스트 표시 (선택 사항)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(lineStart + Vector3.up * 0.2f, "GAME OVER LINE (Y:61)");
#endif
    }
}