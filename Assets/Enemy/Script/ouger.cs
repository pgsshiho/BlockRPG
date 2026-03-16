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
        if (isattack) return;

        // 1. 부모의 Attack 호출 (isattack = true, frame = 0 처리)
        base.Attack();

        // 2. 오우거 전용 공격 연출
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 3. 디버프 및 후딜레이 코루틴 시작
        StartCoroutine(MirrorAttackSequence());
    }

    IEnumerator MirrorAttackSequence()
    {
        // 디버프 발동
        if (gm != null) gm.SetMirrorMode(true);
        Debug.Log("좌우 반전 디버프 시작!");

        // 디버프 지속 시간 대기 (고블린처럼 일정 시간 후 해제)
        yield return new WaitForSeconds(mirrorDuration);

        if (gm != null) gm.SetMirrorMode(false);
        Debug.Log("좌우 반전 디버프 종료");

        // 공격 애니메이션 및 후딜레이 마무리 대기 (고블린 로직 적용)
        // 만약 애니메이션이 mirrorDuration보다 짧다면 시간을 조절하세요.
        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;
        isDead = true;

        // 죽을 때 디버프 강제 해제 (중요!)
        if (gm != null) gm.SetMirrorMode(false);

        if (anim != null)
        {
            anim.SetTrigger("dead"); // 고블린은 "Dead", 오우거는 "dead" 대소문자 확인 필수
        }

        // 추가 공격 방지 (콜라이더 끄기)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 부모의 dead 로직 (점수 등) 호출
        base.dead();

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f);

        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }

        Destroy(gameObject);
    }
}