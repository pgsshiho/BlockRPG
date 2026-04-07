using System.Collections;
using UnityEngine;

public class Crown : Enemybase
{
    private Animator anim;
    private Hold hold;
    private Sound sd;
    public FindEnemy enemyData;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        hold = FindAnyObjectByType<Hold>();
        sd = Sound.instance;
        if (enemyData != null) enemyData.crown = true;
    }

    public override void Attack()
    {
        if (isattack) return;

        Holdc();
        base.Attack();
        base.damage = base.baseDamage;
        if (anim != null) anim.SetTrigger("Attack");

        if (sd != null && sd.Goblin != null) sd.Jester.Play();
        

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
        base.enemyspawn();
        Destroy(gameObject);
    }public void Holdc()
    {
        if (hold != null) {
            if (!hold.ishold == false) hold.Holding();
            else base.damage += 3;
        }
    }
}