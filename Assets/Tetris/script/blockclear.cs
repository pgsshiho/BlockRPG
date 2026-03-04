using UnityEngine;
using TMPro; // TextMeshPro를 사용한다면 추가, 일반 Text라면 삭제

public class blockclear : MonoBehaviour
{
    public const int width = 20;
    public const int height = 60;
    public static Transform[,] grid = new Transform[width, height];
    public static int ScoreForSpeed = 0;
    [Header("Score Settings")]
    public int currentScore = 0;
    public TextMeshProUGUI scoreText; // 유니티 에디터에서 TextUI를 드래그해서 넣어주세요

    void Awake()
    {
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
        int linesClearedInStep = 0; // 이번에 동시에 지워진 줄 수

        for (int y = 0; y < height; y++)
        {
            if (IsLineFull(y))
            {
                DeleteLine(y);
                DecreaseRowsAbove(y + 1);
                y--;
                linesClearedInStep++;
            }
        }

        if (linesClearedInStep > 0)
        {
            AddScore(linesClearedInStep);
        }
    }

    // 점수 계산 공식 (지운 줄 수에 따라 차등 지급)
    void AddScore(int lines)
    {
        switch (lines)
        {
            case 1: currentScore += 100; break;   // 1줄: 100점
            case 2: currentScore += 300; break;   // 2줄: 300점
            case 3: currentScore += 500; break;   // 3줄: 500점
            case 4: currentScore += 800; break;   // 4줄(테트리스): 800점
            default: currentScore += lines * 200; break;
        }
        ScoreForSpeed = currentScore; // 현재 점수를 업데이트
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    // IsLineFull, DeleteLine, DecreaseRowsAbove 함수는 기존과 동일하게 유지...
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
}