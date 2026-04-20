using System.Collections;
using UnityEngine;

public class slime : Enemybase
{
    private Animator anim;
    private Sound sd;
    public FindEnemy enemyData;
    protected override void Start()
    {

        base.Start(); // 부모(Enemybase)의 Start 실행
        anim = GetComponent<Animator>();
        sd = FindAnyObjectByType<Sound>();
        if (enemyData != null){ enemyData.slime = true;
            enemyData.Save();
        }
    }

    public override void Attack()
    {
        base.Attack(); // 부모의 Attack(데미지 계산 등) 실행

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (sd != null && sd.slimehit != null)
        {
            sd.slimehit.Play();
        }

        StartCoroutine(WaitAttackAnimation());
    }

    IEnumerator WaitAttackAnimation()
    {
        // WaitForSeconds는 Time.timeScale이 0이면 같이 멈춥니다.
        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }
        Debug.Log("슬라임죽음");
        base.dead(); // 부모의 dead(점수, 경험치 등) 실행

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        base.enemyspawn();
        Destroy(gameObject);
    }
}