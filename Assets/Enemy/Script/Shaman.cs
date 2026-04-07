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
        if (enemyData != null) enemyData.shaman = true;
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
            if (target.ghost != null) Destroy(target.ghost);
            Destroy(target.gameObject);

            // 지정된 특수 블록을 스폰 지점에 생성
            if (cb != null && specialBlock != null)
            {
                GameObject newObj = Instantiate(specialBlock, cb.spawnpoint.transform.position, Quaternion.identity);
                cb.seeclone(specialBlock, newObj);
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