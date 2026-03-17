using UnityEngine;

public class CustomCloneBase : MonoBehaviour, Iswap
{
    public bool isSpawning = false;
    public GameObject spawnpoint;
    public GameObject[] kindenemy; // 인스펙터에서 11종 프리팹 할당
    public GameObject[] spawns = new GameObject[110];
    private int activeEnemyCount = 0;

    void Start()
    {
        // 씬 시작 시 데이터 로드
        if (DataHolder.instance != null)
        {
            int[] c = DataHolder.instance.monsterCounts;
            Swap(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7], c[8], c[9], c[10]);
        }
        else
        {
            Swap(10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0); // 기본값 슬라임
        }
        spawn();
    }

    public void Swap(int slime, int Goblin, int ouger, int siren, int Golem, int Chraken, int Ghost, int Prism_Dragon, int Crown, int Shaman, int Knight_night)
    {
        int currentIndex = 0;
        int[] counts = { slime, Goblin, ouger, siren, Golem, Chraken, Ghost, Prism_Dragon, Crown, Shaman, Knight_night };

        for (int k = 0; k < counts.Length; k++)
        {
            int countToFill = Mathf.Min(counts[k], 10);
            for (int j = 0; j < countToFill; j++)
            {
                if (currentIndex < spawns.Length)
                    spawns[currentIndex++] = kindenemy[k];
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