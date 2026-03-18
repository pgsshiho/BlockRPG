using System.Collections;
using UnityEngine;

public class Chraken : Enemybase, ISpecialAttack
{
    private Animator anim;
    public float holdDuration = 6.0f;
    public FIndEnemy enemyData;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        if (enemyData != null) enemyData.chraken = true;

    }

    public void ApplyEffect() => GameManager.Instance?.closehold();
    public void RemoveEffect() => GameManager.Instance?.openhold();

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
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
}