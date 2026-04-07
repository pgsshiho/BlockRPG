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

    [Header("Ghost Settings")]
    public Sprite ghostInnerSprite; 

    public GameObject GetGhostBlock()
    {
        return ghostBlock;
    }

    void Awake()
    {
        hhold = FindAnyObjectByType<Hold>();
    }

    void Start()
    {
        st = Stat.instance;
        if (st != null)
        {
            st.OnStatChanged += UpdateNextVisuals;
        }
        fillbag();
        Clone();
    }

    public void Clone()
    {
        if (c + 5 >= blockBag.Count) fillbag();

        currentBlock = Instantiate(blockBag[c], spawnpoint.transform.position, Quaternion.identity);
        seeclone(blockBag[c], currentBlock);

        c++;
        UpdateNextVisuals();

        if (hhold != null) hhold.ishold = true;
        hhold.Grayhold();
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

        if (st == null) return;

        for (int i = 0; i < st.it; i++)
        {
            if (i >= nextHoldObs.Length) break;

            int nextIndex = c + i;
            if (nextIndex < blockBag.Count)
            {
                Vector3 spawnPos = nextHoldObs[i].transform.position + new Vector3(-0.2f, 0, 0);
                GameObject visual = Instantiate(blockBag[nextIndex], spawnPos, Quaternion.identity);
                visual.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

                DisableBlockFunctions(visual);
                nextVisuals.Add(visual);
            }
        }
    }

    public void seeclone(GameObject prefab, GameObject owner)
    {
        if (ghostBlock != null) Destroy(ghostBlock);

        // 1. 고스트 생성
        ghostBlock = Instantiate(prefab, spawnpoint.transform.position, Quaternion.identity);
        ghostBlock.name = "Ghost_" + prefab.name;

        // 2. 기능 제거
        BlockBase bBase = ghostBlock.GetComponent<BlockBase>();
        if (bBase != null) Destroy(bBase);

        Rigidbody2D rb = ghostBlock.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 3. 스프라이트 교체 및 색상 설정
        foreach (SpriteRenderer sr in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            // 테두리 스프라이트로 교체
            if (ghostInnerSprite != null)
            {
                sr.sprite = ghostInnerSprite;
            }
            Color color = sr.color;
            color.a = 1.0f;
            sr.color = color;

            // 본체보다 뒤에 그려지도록 설정
            sr.sortingOrder = 10;
        }

        // 4. 레이어 변경 (Ghost 레이어가 있다면)
        int ghostLayer = LayerMask.NameToLayer("Ghost");
        if (ghostLayer != -1) ghostBlock.layer = ghostLayer;

        owner.GetComponent<BlockBase>().ghost = ghostBlock;
    }

    void DisableBlockFunctions(GameObject obj)
    {
        BlockBase script = obj.GetComponent<BlockBase>();
        if (script != null) Destroy(script);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }
}