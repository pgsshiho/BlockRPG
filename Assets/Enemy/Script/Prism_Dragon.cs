using System.Collections;
using UnityEngine;

public class Prism_Dragon : Enemybase
{
    private Animator anim;
    GameManager GM;
    public float changeDuration = 6.0f;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        GM = GetComponent<GameManager>();
    }

    public override void Attack()
    {
        // 1. 이미 공격 중이면 중복 실행 방지
        if (isattack) return;

        // 2. 부모의 Attack 호출 (isattack = true, frame = 0 처리)
        base.Attack();

        // 3. 드래곤 전용 공격 연출
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 4. 변환 및 후딜레이 코루틴 시작 (오우거 스타일)
        StartCoroutine(ChangeAttackSequence());
    }

    IEnumerator ChangeAttackSequence()
    {
        // 블록 상태 변환 발동
        if (GM != null) 
        Debug.Log("프리즘 드래곤: 블록 변환 시작!");

        // 지속 시간 동안 대기
        yield return new WaitForSeconds(changeDuration);

        // 지속 시간 종료 후 원래대로 복구
        if (GM != null) GM.dechange();
        Debug.Log("프리즘 드래곤: 블록 복구 완료");

        // 공격 후딜레이 마무리 (애니메이션 마무리 대기)
        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }

    public override void dead()
    {
        // 1. 중복 사망 처리 방지
        if (isDead) return;
        isDead = true;

        if (GM != null) GM.dechange();

        // 3. 애니메이션 및 콜라이더 처리
        if (anim != null)
        {
            anim.SetTrigger("dead");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. 부모의 dead 로직 호출
        base.dead();

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        // 사망 애니메이션 연출 대기
        yield return new WaitForSeconds(1.0f);

        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }

        Destroy(gameObject);
    }
}