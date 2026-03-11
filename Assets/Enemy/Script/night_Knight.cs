using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class night_Knight : Enemybase
{
    private Animator anim;
    BlockBase bb;
    Conebase cb;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        bb = FindAnyObjectByType<BlockBase>();
        cb = FindAnyObjectByType<Conebase>();
    }

    public override void Attack()
    {
        if (isattack)
        {
            anim?.SetTrigger("Attack");
            
            for (int i = BlockBase.AllBlocks.Count - 1; i >= 0; i--)
            {
                BlockBase targetBlock = BlockBase.AllBlocks[i];
                
                if (targetBlock != null)
                {
                    if (targetBlock.ghost != null) Destroy(targetBlock.ghost);

                    Destroy(targetBlock.gameObject);
                    cb?.Clone(); 
                }
            }

            StartCoroutine(WaitAttackAnimation());
            base.Attack();
        }
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }


    // --- [죽음 파트] ---
    public override void dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        base.dead();
        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }
        Destroy(gameObject);
    }
}