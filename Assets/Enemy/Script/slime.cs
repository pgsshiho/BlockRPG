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


    // --- [죽음 파트] ---
    public override void dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }

        // 추가 공격 방지
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