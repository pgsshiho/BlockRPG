using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class Golbin : Enemybase
{
    private Animator anim;
    Hold hold;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        hold = FindAnyObjectByType<Hold>();
    }

    public override void Attack()
    {
        if (isattack)
        {
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            StartCoroutine(WaitAttackAnimation());
            if (hold.holdob != null)
            {
                Destroy(hold.holdob);
                Destroy(hold.currentHoldVisual);
            }
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
        yield return new WaitForSeconds(3.0f);

        base.dead();
    }
}