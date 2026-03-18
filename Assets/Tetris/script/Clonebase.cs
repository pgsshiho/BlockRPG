using System.Collections.Generic;
using UnityEngine;

public class Conebase : MonoBehaviour
{
    private GameObject ghostBlock; // 외부 접근을 위해 아래 GetGhostBlock() 추가
    public GameObject[] prefabs;
    public GameObject spawnpoint;
    public List<GameObject> blockBag = new List<GameObject>();
    public int c = 0;
    Hold hhold;
    public GameObject currentBlock;

    public GameObject[] nextHoldObs;
    Stat st;
    private List<GameObject> nextVisuals = new List<GameObject>();

    // [추가] 고스트 적이 이 블록을 찾아 숨길 수 있도록 반환하는 함수
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

        ghostBlock = Instantiate(prefab, spawnpoint.transform.position, Quaternion.identity);

        BlockBase bBase = ghostBlock.GetComponent<BlockBase>();
        if (bBase != null) Destroy(bBase);

        Rigidbody2D rb = ghostBlock.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        foreach (SpriteRenderer sr in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sr.color;
            color.a = 0.3f;
            sr.color = color;
        }

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