using System.Collections.Generic;
using UnityEngine;

public class Conebase : MonoBehaviour
{
    private GameObject ghostBlock;
    public GameObject[] prefabs;
    public GameObject spawnpoint;
    public List<GameObject> blockBag = new List<GameObject>();
    public int c = 0;
    Hold hhold;
    public GameObject currentBlock;

    public GameObject[] nextHoldObs;
    Stat st;
    private List<GameObject> nextVisuals = new List<GameObject>();

    void Awake()
    {
        hhold = FindAnyObjectByType<Hold>();
    }

    void Start()
    {

        st = FindAnyObjectByType<Stat>();
        fillbag();
        Clone();
    }

    public void Clone()
    {
        if (c + 5 >= blockBag.Count) fillbag();

        // 1. 블록 생성
        currentBlock = Instantiate(blockBag[c], spawnpoint.transform.position, Quaternion.identity);

        // 2. 고스트 생성 및 설정
        seeclone(blockBag[c], currentBlock);

        c++;
        UpdateNextVisuals();

        // Hold 기능 활성화
        if (hhold != null) hhold.ishold = true;
    }

    public void fillbag()
    {
        List<GameObject> newBag = new List<GameObject>(prefabs);
        if (prefabs.Length > 0)
        {
            int extra = Random.Range(0, prefabs.Length);
            newBag.Add(prefabs[extra]);
        }
        for (int i = newBag.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            GameObject temp = newBag[k];
            newBag[k] = newBag[i];
            newBag[i] = temp;
        }
        blockBag.AddRange(newBag);
    }

    void UpdateNextVisuals()
    {
        foreach (GameObject obj in nextVisuals) Destroy(obj);
        nextVisuals.Clear();

        for (int i = 0; i < st.it; i++)
        {
            if (i >= nextHoldObs.Length) break;

            int nextIndex = c + i;
            if (nextIndex < blockBag.Count)
            {
                Vector3 spawnPos = nextHoldObs[i].transform.position + new Vector3(-0.2f, 0, 0);
                GameObject visual = Instantiate(blockBag[nextIndex], spawnPos, Quaternion.identity);
                visual.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

                // 시각화용 블록이므로 모든 기능 정지
                DisableBlockFunctions(visual);
                nextVisuals.Add(visual);
            }
        }
    }

    public void seeclone(GameObject prefab, GameObject owner)
    {
        if (ghostBlock != null) Destroy(ghostBlock);

        // 고스트 생성
        ghostBlock = Instantiate(prefab, spawnpoint.transform.position, Quaternion.identity);

        // [중요] BlockBase의 디자인 변경 기능과 충돌하지 않도록 스크립트 먼저 제거
        BlockBase bBase = ghostBlock.GetComponent<BlockBase>();
        if (bBase != null) Destroy(bBase);

        // 고스트는 물리 연산 제외
        Rigidbody2D rb = ghostBlock.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 투명도 조절 (자식들의 모든 SpriteRenderer 대상)
        foreach (SpriteRenderer sr in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sr.color;
            color.a = 0.3f;
            sr.color = color;
        }

        // 본체에 고스트 연결
        owner.GetComponent<BlockBase>().ghost = ghostBlock;
    }

    void DisableBlockFunctions(GameObject obj)
    {
        // 1. 메인 스크립트 제거
        BlockBase script = obj.GetComponent<BlockBase>();
        if (script != null) Destroy(script);

        // 2. 물리 및 충돌체 완전히 비활성화
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 3. 모든 자식 콜라이더 끄기 (본체 이동 방해 금지)
        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }
}