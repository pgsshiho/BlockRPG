using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class night_Knight : Enemybase
{
    private Animator anim;
    private Conebase cb;
    private Sound sd;
    public FIndEnemy enemyData;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        cb = FindAnyObjectByType<Conebase>();
        sd = FindAnyObjectByType<Sound>();
        if (enemyData != null) enemyData.knight_night = true;
    }

    public override void Attack()
    {
        if (isattack) return; // 중복 실행 방지
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");

        sd.night_knight.Play();
        BlockBase target = GetActiveBlock();
        if (target != null)
        {
            if (target.ghost != null) Destroy(target.ghost);
            Destroy(target.gameObject);

            cb?.Clone();
        }

        StartCoroutine(WaitAttackAnimation());
    }

    private BlockBase GetActiveBlock()
    {
        foreach (var b in BlockBase.AllBlocks)
        {
            // 조종 중인 블록 필터링 (활성화되어 있고 설치되지 않은 것)
            if (b.enabled && b.GetComponent<Rigidbody2D>().simulated && !b.CompareTag("Block"))
                return b;
        }
        return null;
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;
        isDead = true;
        if (anim != null) anim.SetTrigger("dead");
        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        if (es != null) { es.isSpawning = false; es.spawn(); }
        Destroy(gameObject);
    }
}