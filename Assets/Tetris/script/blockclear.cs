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

    void Awake()
    {
        eb = FindAnyObjectByType<Enemybase>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
        st = FindAnyObjectByType<Stat>();
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

        if (twistPerformed)
        {
            if (lines == 1) { displayMessage += "Twist Single!\n"; bonusScore += 150; bonusPower += 10; }
            else if (lines == 2) { displayMessage += "Twist Double!\n"; bonusScore += 450; bonusPower += 35; }
            else { displayMessage += $"Twist Burst {lines}!\n"; bonusScore += 700 * lines; bonusPower += 50 * lines; }
            bb.IsTwist = false;
        }
        else if (lines >= 4) { displayMessage += "ULTRA BURST!\n"; }

        if (isSpecial)
        {
            if (isLinkActive) { displayMessage += "LINK BONUS!\n"; bonusScore += 200; bonusPower += 15; }
            isLinkActive = true;
        }
        else { isLinkActive = false; }

        if (!string.IsNullOrEmpty(displayMessage)) SpawnFloatingText(displayMessage.Replace("\n", " "), 35, Color.green);

        int comboBonus = (combo > 1) ? (combo * combo * 12) : 0;
        int comboPower = (combo > 1) ? (int)(combo * 7.5f) : 0;

        int[] baseLineScore = { 0, 70, 180, 350, 750 };
        int[] baseLinePower = { 0, 10, 30, 60, 110 };

        int finalScore = baseLineScore[Mathf.Min(lines, 4)] + bonusScore + comboBonus;
        int finalPower = baseLinePower[Mathf.Min(lines, 4)] + bonusPower + comboPower;

        currentScore += finalScore;
        StartCoroutine(ProcessAttackSequence(finalPower));
        ScoreForSpeed = currentScore;
        UpdateScoreUI();
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

    IEnumerator ProcessAttackSequence(int remainingAtk)
    {
        while (remainingAtk > 0)
        {
            Enemybase target = null;
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);
            foreach (var e in enemies) { if (!e.IsDead) { target = e; break; } }
            if (target != null)
            {
                float multiplier = (Stat.instance != null) ? (1.0f + (Stat.instance.atk * 0.13f)) : 1f;
                int enemyCurrentHp = target.hp;
                int potentialAtk = Mathf.CeilToInt(remainingAtk * multiplier);
                if (potentialAtk > 0) SpawnFloatingText("-" + potentialAtk.ToString(), 55, Color.red);
                if (potentialAtk >= enemyCurrentHp)
                {
                    int consumed = Mathf.CeilToInt(enemyCurrentHp / multiplier);
                    target.hit(remainingAtk);
                    remainingAtk -= consumed;
                    yield return new WaitForSeconds(0.7f);
                }
                else { target.hit(remainingAtk); remainingAtk = 0; }
            }
            else { yield return new WaitForSeconds(0.1f); }
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
    void OnDrawGizmos()

    {

        // 1. 전체 그리드 베이스 (연한 회색)

        Gizmos.color = new Color(1, 1, 1, 0.05f);

        for (int x = 0; x < width; x++)

        {

            for (int y = 0; y < height; y++)

            {

                float wx = x * 0.5f - 4.5f;

                float wy = y * 0.5f + visualYOffset;

                Gizmos.DrawWireCube(new Vector3(wx, wy, 0), new Vector3(0.5f, 0.5f, 0.1f));

            }

        }



        // 2. 실제 라인 클리어 감지 범위 (하늘색 영역)

        // lineCheckStartX ~ lineCheckEndX 범위를 강조합니다.

        Gizmos.color = new Color(0, 0.8f, 1f, 0.3f);

        float startXPos = (lineCheckStartX * 0.5f) - 4.5f - 0.25f;

        float endXPos = ((lineCheckEndX - 1) * 0.5f) - 4.5f + 0.25f;

        float centerX = (startXPos + endXPos) / 2f;

        float rangeWidth = endXPos - startXPos;



        // 감지 구역을 반투명한 박스로 표시

        Vector3 areaCenter = new Vector3(centerX, (height * 0.25f) + visualYOffset - 0.25f, 0);

        Vector3 areaSize = new Vector3(rangeWidth, height * 0.5f, 0.1f);

        Gizmos.DrawCube(areaCenter, areaSize);



        // 감지 구역 테두리

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(areaCenter, areaSize);



        // 3. 게임 오버 라인 (빨간색)

        Gizmos.color = Color.red;

        // 인덱스 56 혹은 60 등 실제 체크하는 높이에 맞춰 선을 그립니다.

        // 현재 코드의 CheckGameOver는 grid[x, 60]을 체크하지만 배열은 59가 최대이므로 56~58 정도가 적당합니다.

        float deadLineY = 61 * 0.5f + visualYOffset - 0.25f;

        Gizmos.DrawLine(new Vector3(-10, deadLineY, 0), new Vector3(10, deadLineY, 0));



        // "DEAD LINE" 표시용 구구조물

        Gizmos.DrawSphere(new Vector3(startXPos, deadLineY, 0), 0.1f);

        Gizmos.DrawSphere(new Vector3(endXPos, deadLineY, 0), 0.1f);

    }
}