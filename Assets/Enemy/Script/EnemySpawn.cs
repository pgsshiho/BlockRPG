using UnityEngine;
using TMPro;
[System.Serializable]
public class EnemyGroup
{
    public string groupName;
    public GameObject[] prefabs;
}

public class EnemySpawn : MonoBehaviour
{
    public bool isSpawning = false;
    public EnemyGroup[] enemy;
    public TextMeshProUGUI nowwave;
    [Header("Settings")]
    public int scoreThreshold = 1500;
    public GameObject spawnpoint;

    // GameManager에서 참조하기 위해 i 또는 nowdif를 사용
    [HideInInspector] public int i = 0;
    private blockclear bc;

    void Start()
    {
        bc = FindAnyObjectByType<blockclear>();
        isSpawning = false;
        spawn();
    }

    public void spawn()
    {
        if (isSpawning) return;
        isSpawning = true; // 소환 시작 시 true

        // 난이도 계산
        int currentScore = blockclear.ScoreForSpeed;
        i = currentScore / scoreThreshold;
        nowwave.text = $"Wave: {i + 1}";
        if (enemy.Length > 0)
        {
            i = Mathf.Clamp(i, 0, enemy.Length - 1);
        }
        else
        {
            Debug.LogError("Enemy 그룹이 설정되지 않았습니다!");
            isSpawning = false;
            return;
        }

        if (enemy[i].prefabs != null && enemy[i].prefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, enemy[i].prefabs.Length);
            GameObject selectedPrefab = enemy[i].prefabs[randomIndex];
            Instantiate(selectedPrefab, spawnpoint.transform.position, Quaternion.identity);
            isSpawning = false;
        }
        else
        {
            Debug.LogWarning($"난이도 {i}에 프리팹이 없습니다.");
            isSpawning = false;
        }
    }
}