using System.Collections;
using UnityEngine;

public class Chraken : Enemybase, ISpecialAttack
{
    private Animator anim;
    public float holdDuration = 6.0f;
    public FindEnemy enemyData;
    private Sound sd;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        if (enemyData != null) enemyData.chraken = true;
        sd = FindAnyObjectByType<Sound>();
    }

    public void ApplyEffect() => GameManager.Instance?.closehold();
    public void RemoveEffect() => GameManager.Instance?.openhold();

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null && sd.Kraken != null) sd.Kraken.Play();
        StartCoroutine(SpecialAttackSequence());
    }

    IEnumerator SpecialAttackSequence()
    {
        ApplyEffect();
        yield return new WaitForSeconds(holdDuration);
        RemoveEffect();

        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }
    public override void dead()
    {
        if (isDead) return;

        isDead = true;
        StopAllCoroutines();
        RemoveEffect();

        if (anim != null) anim.SetTrigger("dead");

        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        base.enemyspawn();

        Destroy(gameObject);
    }
}