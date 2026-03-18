using System.Collections;
using UnityEngine;

public class ouger : Enemybase, ISpecialAttack
{
    private Animator anim;
    public float mirrorDuration = 6.0f;
    // 사운드 추가 (필요시)
    private Sound sd;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = Sound.instance; // 싱글톤 참조
    }

    public void ApplyEffect() => GameManager.Instance?.SetMirrorMode(true);
    public void RemoveEffect() => GameManager.Instance?.SetMirrorMode(false);

    public override void Attack()
    {
        // 1. 이미 공격 중이면 중단
        if (isattack) return;

        // 2. 부모의 Attack 호출 (isattack = true, frame = 0 처리)
        base.Attack();

        // 3. 애니메이션 및 사운드
        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null && sd.ouger != null) sd.ouger.Play();

        // 4. 특수 공격 시퀀스 시작
        StartCoroutine(SpecialAttackSequence());
    }

    IEnumerator SpecialAttackSequence()
    {
        // 화면 반전 효과 적용
        ApplyEffect();

        // 거울 모드 지속 시간 동안 대기
        yield return new WaitForSeconds(mirrorDuration);

        // 효과 해제
        RemoveEffect();

        // 공격 후 약간의 후딜레이 (고블린의 0.8초처럼)
        yield return new WaitForSeconds(0.8f);

        // 이제 다음 공격이 가능하도록 해제
        isattack = false;
    }

    // 사망 로직도 고블린처럼 애니메이션 후 스폰되도록 통일
    public override void dead()
    {
        if (isDead) return;

        // 공격 중이었다면 효과 강제 해제
        StopAllCoroutines();
        RemoveEffect();

        if (anim != null) anim.SetTrigger("Dead");

        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f); // 사망 애니메이션 시간
        if (es != null)
        {
            es.isSpawning = false;
            es.spawn();
        }
        Destroy(gameObject);
    }
}