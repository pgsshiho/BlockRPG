using System.Collections;
using UnityEngine;

public class ouger : Enemybase
{
    private Animator anim;
    GameManager gm;
    public float mirrorDuration = 3.0f; // 디버프 지속 시간

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

        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }

    public override void dead()
    {
        if (gm != null) gm.SetMirrorMode(false);

        if (anim != null) anim.SetTrigger("dead");
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        base.dead();
    }
}