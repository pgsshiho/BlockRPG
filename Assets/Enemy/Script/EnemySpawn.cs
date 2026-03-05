using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[System.Serializable]
public class EnemyGroup
{
    public GameObject[] prefabs;
}

public class EnemySpawn : MonoBehaviour
{
    public EnemyGroup[] enemy;

    public int randomnenemy = 0;
    public int nowdif = 0;
    public GameObject spawnpoint;
    blockclear bc;

    void Start()
    {
        bc = FindAnyObjectByType<blockclear>();

        // 게임 시작 시 첫 번째 적 소환! (이거 없으면 몬스터가 아예 안 나옵니다)
        spawn();
    }

    void Update()
    {
    }

    public void spawn()
    {
        int s = blockclear.ScoreForSpeed;
        for (int i = 0; i < 19; i++)
        {
            if (s < (i + 1) * 2000)
            {
                nowdif = i;
                break;
            }
        }
        if (enemy.Length > 0)
        {
            nowdif = Mathf.Clamp(nowdif, 0, enemy.Length - 1);
        }

        if (enemy[nowdif].prefabs.Length > 0)
        {
            randomnenemy = Random.Range(0, enemy[nowdif].prefabs.Length);
            Instantiate(enemy[nowdif].prefabs[randomnenemy], spawnpoint.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("현재 난이도(" + nowdif + ")에 설정된 적 프리팹이 없습니다!");
        }
    }
}