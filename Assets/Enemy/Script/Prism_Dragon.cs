using System.Collections;
using UnityEngine;

public class Prism_Dragon : Enemybase, ISpecialAttack
{
    private Animator anim;
    private Sound sd; // 사운드 추가
    public float changeDuration = 6.0f;
    public FindEnemy enemyData;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = Sound.instance;
        if (enemyData != null) enemyData.dragon = true;
    }

    public void ApplyEffect() => GameManager.Instance?.change();
    public void RemoveEffect() => GameManager.Instance?.dechange();

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
        // 드래곤 전용 사운드가 있다면 여기에 추가
        // if (sd != null && sd.dragonAttack != null) sd.dragonAttack.Play();

        StartCoroutine(SpecialAttackSequence());
    }

    IEnumerator SpecialAttackSequence()
    {
        ApplyEffect();
        yield return new WaitForSeconds(changeDuration);
        RemoveEffect();

        yield return new WaitForSeconds(1.0f); // 후딜레이
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;

        // 공격 중 죽었을 때를 대비해 모든 코루틴 중단 및 효과 해제
        StopAllCoroutines();
        RemoveEffect();

        if (anim != null) anim.SetTrigger("Dead");

        base.dead(); // Enemybase의 dead 호출 (보상 처리)

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f); // 사망 애니메이션 대기
        if (es != null)
        {
            es.isSpawning = false;
            es.spawn();
        }
        Destroy(gameObject);
    }
}