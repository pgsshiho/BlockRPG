using System.Collections;
using UnityEngine;

public class Shaman : Enemybase
{
    private Animator anim;
    Conebase cb;
    public GameObject block;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        cb = GetComponent<Conebase>();
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
                    Instantiate(block, cb.spawnpoint.transform.position, Quaternion.identity);
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

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        base.dead();
    }
}
