using System.Collections;
using UnityEngine;

public class Crown : Enemybase
{
    private Animator anim;
    private Hold hold;
    private Sound sd;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        hold = FindAnyObjectByType<Hold>();
        sd = Sound.instance;
    }

    public override void Attack()
    {
        if (isattack) return;
        base.Attack(); // frame 초기화 및 isattack = true

        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null && sd.Goblin != null) sd.Goblin.Play();

        // 홀드 강제 실행
        if (hold != null) hold.Holding();

        StartCoroutine(WaitAttackAnimation());
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;
        if (anim != null) anim.SetTrigger("Dead");
        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }
    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        if (es != null) { es.isSpawning = false; es.spawn(); }
        Destroy(gameObject);
    }
}