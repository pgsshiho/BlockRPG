using System.Collections;
using UnityEngine;

public class Shaman : Enemybase
{
    private Animator anim;
    private Conebase cb;
    public GameObject specialBlock; // 교체할 프리팹
    public FindEnemy enemyData;
    public Sound sd;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        cb = FindAnyObjectByType<Conebase>(); // 수정: GetComponent가 아닌 Find
        sd = FindAnyObjectByType<Sound>();
        if (enemyData != null){ enemyData.shaman = true;
            enemyData.Save();
        }
    }

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null) sd.Shamen.Play();

        BlockBase target = GetActiveBlock();
        if (target != null)
        {
            // 1. 기존 블록이 죽으면서 Clone()을 호출하지 못하게 막음
            target.skipSpawnNext = true;

            if (target.ghost != null) Destroy(target.ghost);
            Destroy(target.gameObject);

            if (cb != null && specialBlock != null)
            {
                // 2. 새로운 특수 블록 생성
                GameObject newObj = Instantiate(specialBlock, cb.spawnpoint.transform.position, Quaternion.identity);

                // 3. 중요!! 새로 생성된 블록은 정상적으로 다음 블록을 소환해야 하므로 false로 설정
                BlockBase newBlockScript = newObj.GetComponent<BlockBase>();
                if (newBlockScript != null)
                {
                    newBlockScript.skipSpawnNext = false; // 새 블록은 다시 소환 권한을 가짐
                }

                cb.seeclone(specialBlock, newObj);
                cb.currentBlock = newObj;
            }
        }

        StartCoroutine(WaitAttackAnimation());
    }

    private BlockBase GetActiveBlock()
    {
        foreach (var b in BlockBase.AllBlocks)
        {
            if (b.enabled && b.GetComponent<Rigidbody2D>().simulated && !b.CompareTag("Block"))
                return b;
        }
        return null;
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;
        isDead = true;
        if (anim != null) anim.SetTrigger("dead");
        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        base.enemyspawn();
        Destroy(gameObject);
    }
}