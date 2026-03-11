using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class Golbin : Enemybase
{
    private Animator anim;
    Hold hold;
    Sound sd;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        hold = FindAnyObjectByType<Hold>();
        sd = FindAnyObjectByType<Sound>();
    }

    public override void Attack()
{
    if (isattack) return;

    // 1. 부모의 Attack을 호출하여 frame = 0f와 isattack = true를 처리합니다.
    base.Attack(); 

    // 2. 골블린 전용 로직 수행
    if (anim != null)
    {
        anim.SetTrigger("Attack");
    }

    if (hold != null)
    {
        // 홀드 '칸'은 남기고 '내용물'만 지우기
        if (hold.currentHoldVisual != null) Destroy(hold.currentHoldVisual);
        hold.hold = null;
        hold.ishave = false;
        // hold.ishold = true; // 필요하다면 추가
    }
        sd.Goblin.Play();
    StartCoroutine(WaitAttackAnimation());
}

IEnumerator WaitAttackAnimation()
{
    // 여기서 대기하는 시간(1.0f) 동안은 isattack이 true라 frame이 쌓이지 않습니다.
    yield return new WaitForSeconds(1.0f);
    isattack = false; 
    // 이제 다시 FixedUpdate에서 frame이 0부터 쌓이기 시작합니다.
}


    // --- [죽음 파트] ---
    public override void dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Dead");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        base.dead();
        if (es != null)
        {
            es.isSpawning = false; // 방어막 해제
            es.spawn();            // 새 적 소환
        }
        Destroy(gameObject);
    }
}