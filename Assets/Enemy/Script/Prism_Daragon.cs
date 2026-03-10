using System.Collections;
using UnityEngine;

public class Prism_Daragon : Enemybase
{
    private Animator anim;
    BlockBase bb;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        bb = GetComponent<BlockBase>();
    }

    public override void Attack()
    {
        base.Attack();

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        //bb.change();
        StartCoroutine(WaitAttackAnimation());
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(10.0f);
        //bb.dechange();
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
