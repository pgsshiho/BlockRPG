using System.Collections.Generic; // 리스트 사용을 위해 필요
using UnityEngine;

public class createblock : MonoBehaviour
{
    public GameObject[] blockPrefabs; // 7종류의 블록 프리팹을 담는 배열
    public GameObject spawnpoint;
    
    // 셔플된 블록 순서를 저장할 리스트 (이것이 우리의 'Bag'입니다)
    private List<int> blockBag = new List<int>();

    void Start()
    {
        FillBag(); // 시작할 때 가방을 한 번 채웁니다.
        spawn();
    }

    // 7개의 블록 번호를 가방에 넣고 무작위로 섞는 함수
    void FillBag()
    {
        // 1. 일단 0부터 6까지 순서대로 넣습니다.
        for (int i = 0; i < 7; i++)
        {
            blockBag.Add(i);
        }

        // 2. 섞기 (Fisher-Yates Shuffle 알고리즘의 간략 버전)
        for (int i = 0; i < blockBag.Count; i++)
        {
            int temp = blockBag[i];
            int randomIndex = Random.Range(i, blockBag.Count);
            blockBag[i] = blockBag[randomIndex];
            blockBag[randomIndex] = temp;
        }
        
        Debug.Log("가방이 새로 채워지고 섞였습니다!");
    }

    public void spawn()
    {
        // 가방이 비어있다면 다시 채웁니다.
        if (blockBag.Count == 0)
        {
            FillBag();
        }

        // 가방의 가장 앞에 있는(0번) 번호를 꺼냅니다.
        int nextBlockIndex = blockBag[0];
        blockBag.RemoveAt(0); // 꺼낸 번호는 가방에서 삭제

        // 해당 번호의 블록을 생성합니다.
        Instantiate(blockPrefabs[nextBlockIndex], spawnpoint.transform.position, Quaternion.identity);
    }
}