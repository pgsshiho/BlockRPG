using UnityEngine;

public class Hold : MonoBehaviour
{
    public GameObject hold;
    public bool ishold = false;
    public bool ishave = false;
    BlockBase bb;
    Conebase cb;
    public GameObject holdob;
    public GameObject currentHoldVisual;
    public GameObject grayhold;
    KeyBinding kb;
    void Awake()
    {
        cb = FindAnyObjectByType<Conebase>();
        kb = FindAnyObjectByType<KeyBinding>();
    }

    void Update()
    {
        if (ishold == true && Input.GetKeyDown(kb.hold) || ishold == true && Input.GetKeyDown(kb.hold2))
        {
            Holding();  
        }
    }

    void DisableBlockFunctions(GameObject obj)
    {
        obj.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        BlockBase script = obj.GetComponent<BlockBase>();
        if (script != null) Destroy(script);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Collider2D mainCol = obj.GetComponent<Collider2D>();
        if (mainCol != null) mainCol.enabled = false;

        foreach (var c in obj.GetComponentsInChildren<Collider2D>())
        {
            c.enabled = false;
        }
    }
    public void Holding()
    {
        if (currentHoldVisual != null) Destroy(currentHoldVisual);

        if (ishave == true)
        {
            if (cb.currentBlock != null)
            {
                BlockBase currentBB = cb.currentBlock.GetComponent<BlockBase>();
                if (currentBB != null && currentBB.ghost != null) Destroy(currentBB.ghost);
                Destroy(cb.currentBlock);
            }

            GameObject temp = hold;
            hold = cb.blockBag[cb.c - 1];

            cb.currentBlock = Instantiate(temp, cb.spawnpoint.transform.position, Quaternion.identity);
            cb.seeclone(temp, cb.currentBlock);

            currentHoldVisual = Instantiate(hold, holdob.transform.position, Quaternion.identity);
            DisableBlockFunctions(currentHoldVisual);

            ishold = false;
            Grayhold();
        }
        else
        {
            hold = cb.blockBag[cb.c - 1];

            if (cb.currentBlock != null)
            {
                BlockBase currentBB = cb.currentBlock.GetComponent<BlockBase>();
                if (currentBB != null && currentBB.ghost != null) Destroy(currentBB.ghost);
                Destroy(cb.currentBlock);
            }
            Vector3 spawnPos = holdob.transform.position + new Vector3(-0.2f, 0, 0);

            currentHoldVisual = Instantiate(hold, spawnPos, Quaternion.identity);
            DisableBlockFunctions(currentHoldVisual);

            cb.Clone();
            ishold = false;
            Grayhold();
        }
        ishave = true;
    }
    public void Grayhold()
    {
        if (ishold == true)
        {
            grayhold.SetActive(false);
        }
        else
        {
            grayhold.SetActive(true);
        }
    }

}