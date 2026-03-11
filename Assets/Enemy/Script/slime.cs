using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class slime : Enemybase
{
    private Animator anim;
    Sound sd;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = FindAnyObjectByType<Sound>();
    }

    public override void Attack()
    {
        base.Attack();

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        sd.slimehit.Play();
        StartCoroutine(WaitAttackAnimation());
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }
    public override void dead()
    {
        if (isDead) return;

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }
        base.dead();

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }
        Destroy(gameObject);
    }
}