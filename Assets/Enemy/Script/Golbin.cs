using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class Golbin : Enemybase
{
    private Animator anim;
    Hold hold;
    Sound sd;
    public FindEnemy enemyData;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        hold = FindAnyObjectByType<Hold>();
        sd = FindAnyObjectByType<Sound>();
        if (enemyData != null) enemyData.goblin = true;
    }

    public override void Attack()
{
    if (isattack) return;

    int baseattack = damage;
    if(hold.hold == null)
        {
            base.damage = baseattack * 2; // 홀드가 비어있으면 공격력 2배
        }
    base.Attack(); 
    base.damage = baseattack; // 공격 후 원래 공격력으로 복원
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
    yield return new WaitForSeconds(0.8f);
    isattack = false; 
    // 이제 다시 FixedUpdate에서 frame이 0부터 쌓이기 시작합니다.
}
    public override void dead()
    {
        if (isDead) return;

        if (anim != null)
        {
            anim.SetTrigger("Dead");
        }
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