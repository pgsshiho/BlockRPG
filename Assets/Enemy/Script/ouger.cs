using System.Collections;
using UnityEngine;

public class ouger : Enemybase, ISpecialAttack
{
    private Animator anim;
    public float mirrorDuration = 6.0f;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public void ApplyEffect() => GameManager.Instance?.SetMirrorMode(true);
    public void RemoveEffect() => GameManager.Instance?.SetMirrorMode(false);

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
        yield return new WaitForSeconds(mirrorDuration);
        RemoveEffect();

        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }
}