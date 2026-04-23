using UnityEngine;

public class CustomCloneBase : MonoBehaviour
{
    public bool isSpawning = false;
    public GameObject spawnpoint;

    [Header("프리팹 12종을 순서대로 넣어주세요")]
    public GameObject[] kindenemy; // 인스펙터에서 반드시 Size를 12로 설정해야 함

    public GameObject[] spawns = new GameObject[144];
    private int activeEnemyCount = 0;

    void Start()
    {
        // 0.1초 뒤에 LateStart 호출
        Invoke("LateStart", 0.1f);
    }

    void LateStart()
    {
        if (DataHolder.instance != null)
        {
            int[] c = DataHolder.instance.monsterCounts;

            Swap(c);
            spawn();
            
        }
        else
        {
        }
    }
    // 매개변수를 배열로 받도록 수정하여 관리가 편하게 변경
    public void Swap(int[] counts)
    {
        int currentIndex = 0;
        activeEnemyCount = 0;

        // 1. 넘어온 데이터 값 출력 (모두 0인지 확인용)
        string debugLog = "받은 데이터 값들: ";
        for (int n = 0; n < counts.Length; n++) debugLog += $"[{n}번:{counts[n]}] ";

        // 2. 프리팹 배열 크기 확인
        if (kindenemy == null || kindenemy.Length == 0)
        {
            return;
        }

        // 3. 실제 루프 (counts와 kindenemy 중 작은 값 기준)
        int limit = Mathf.Min(counts.Length, kindenemy.Length);

        for (int k = 0; k < limit; k++)
        {
            // 0마리라고 설정했으면 건너뜀
            if (counts[k] <= 0) continue;

            for (int j = 0; j < counts[k]; j++)
            {
                if (currentIndex < spawns.Length)
                {
                    if (kindenemy[k] != null)
                    {
                        spawns[currentIndex++] = kindenemy[k];
                    }
                    else
                    {
                    }
                }
            }
        }

        activeEnemyCount = currentIndex;
    }

    public void spawn()
    {
        if (activeEnemyCount <= 0 || isSpawning) return;

        int randomIndex = Random.Range(0, activeEnemyCount);
        GameObject selected = spawns[randomIndex];

        if (selected != null)
        {
            Instantiate(selected, spawnpoint.transform.position, Quaternion.identity);
        }
    }
}