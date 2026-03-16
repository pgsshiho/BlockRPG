using System.Collections;
using UnityEngine;

public class ouger : Enemybase
{
    private Animator anim;
    GameManager gm;
    public float mirrorDuration = 6.0f; // 디버프 지속 시간

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        gm = FindAnyObjectByType<GameManager>();
    }

    public override void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        StartCoroutine(MirrorAttackSequence());

        base.Attack();
    }

    IEnumerator MirrorAttackSequence()
    {
        gm.SetMirrorMode(true);
        Debug.Log("좌우 반전 디버프 시작!");

        yield return new WaitForSeconds(mirrorDuration);

        gm.SetMirrorMode(false);
        Debug.Log("좌우 반전 디버프 종료");

        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }

    public override void dead()
    {
        if (gm != null) gm.SetMirrorMode(false);

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
        yield return new WaitForSeconds(0.8f);

        base.dead();
        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }
        Destroy(gameObject);
    }
}