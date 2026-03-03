using System.Collections.Generic;
using UnityEngine;

public class Conebase : MonoBehaviour
{
    private GameObject ghostBlock;
    public GameObject[] prefabs;
    public GameObject spawnpoint;
    List<GameObject> blockBag = new List<GameObject>();
    int c = 0;

    void Start()
    {
        fillbag();
        Clone();
    }


    public void Clone()
    {
        if (c >= blockBag.Count) fillbag();

        // 1. 실제 조작할 블록 생성
        GameObject currentBlock = Instantiate(blockBag[c], spawnpoint.transform.position, Quaternion.identity);

        // 2. 고스트 블록 생성 (seeclone 호출)
        seeclone(blockBag[c], currentBlock);

        c++;
    }
    public void fillbag()
    {
        blockBag.Clear();
        // 배열(prefabs)의 내용물을 리스트(blockBag)에 하나씩 추가
        blockBag.AddRange(prefabs);

        // Fisher-Yates Shuffle
        for (int i = blockBag.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            GameObject temp = blockBag[k];
            blockBag[k] = blockBag[i];
            blockBag[i] = temp;
        }
        c = 0;
    }
    public void seeclone(GameObject prefab, GameObject owner)
    {
        // 기존 고스트가 있다면 삭제
        if (ghostBlock != null) Destroy(ghostBlock);

        // 프리팹 복제
        ghostBlock = Instantiate(prefab, spawnpoint.transform.position, Quaternion.identity);

        // 고스트 블록의 기능들 제거 (조작되지 않게)
        Destroy(ghostBlock.GetComponent<BlockBase>());
        if (ghostBlock.GetComponent<Rigidbody2D>()) ghostBlock.GetComponent<Rigidbody2D>().simulated = false;

        // 반투명 처리
        foreach (SpriteRenderer sr in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sr.color;
            color.a = 0.3f; // 투명도 30%
            sr.color = color;
        }

        // 실제 블록(BlockBase)에게 내 고스트가 누구인지 알려줌
        owner.GetComponent<BlockBase>().ghost = ghostBlock;
    }
}