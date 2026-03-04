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

    public int it = 5;
    public GameObject[] nextHoldObs;
    private List<GameObject> nextVisuals = new List<GameObject>();

    void Awake()
    {
        hhold = FindAnyObjectByType<Hold>();
    }

    void Start()
    {
        fillbag();
        Clone();
    }

    public void Clone()
    {
        if (c + it >= blockBag.Count) fillbag();

        currentBlock = Instantiate(blockBag[c], spawnpoint.transform.position, Quaternion.identity);
        seeclone(blockBag[c], currentBlock);

        c++;
        UpdateNextVisuals();
        hhold.ishold = true;
    }

    public void fillbag()
    {
        List<GameObject> newBag = new List<GameObject>(prefabs);
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

        for (int i = 0; i < it; i++)
        {
            if (i >= nextHoldObs.Length) break;

            int nextIndex = c + i;
            if (nextIndex < blockBag.Count)
            {
                // 위치를 왼쪽으로 0.5만큼 보정한 좌표 계산
                Vector3 spawnPos = nextHoldObs[i].transform.position + new Vector3(-0.2f, 0, 0);

                // 계산된 위치에 생성
                GameObject visual = Instantiate(blockBag[nextIndex], spawnPos, Quaternion.identity);

                // 다시 부모의 전체 크기를 조절하는 방식
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

        Destroy(ghostBlock.GetComponent<BlockBase>());
        if (ghostBlock.GetComponent<Rigidbody2D>()) ghostBlock.GetComponent<Rigidbody2D>().simulated = false;

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

        Collider2D mainCol = obj.GetComponent<Collider2D>();
        if (mainCol != null) mainCol.enabled = false;

        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }
}