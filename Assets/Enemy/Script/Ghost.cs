using System.Collections;
using UnityEngine;

public class Ghost : Enemybase, ISpecialAttack
{
    private Animator anim;
    private BlockBase bb;
    private Sound sd;

    private bool isEffectActive = false;

    [Header("Ghost Settings")]
    public float hideDuration = 5.0f; // 만약 시간 제한을 쓰고 싶다면 이 시간을 조절

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        bb = FindAnyObjectByType<BlockBase>();
        sd = FindAnyObjectByType<Sound>();
    }

    // [ISpecialAttack 구현] 블록 숨기기
    public void ApplyEffect()
    {
        if (bb == null || isEffectActive) return;

        foreach (Transform child in bb.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
        isEffectActive = true;
        Debug.Log("고스트: 블록 투명화 발동");
    }

    // [ISpecialAttack 구현] 블록 다시 보이기 (죽거나 시간이 다 되면 호출)
    public void RemoveEffect()
    {
        if (bb == null || !isEffectActive) return;

        foreach (Transform child in bb.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }
        isEffectActive = false;
        Debug.Log("고스트: 블록 투명화 해제");
    }

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null) sd.Goblin.Play();

        StartCoroutine(GhostAttackSequence());
    }

    IEnumerator GhostAttackSequence()
    {
        ApplyEffect();

        // ---------------------------------------------------------
        // [옵션: 시간 지나면 다시 보이게 하고 싶을 때 아래 주석 해제]
        /*
        yield return new WaitForSeconds(hideDuration);
        RemoveEffect(); 
        */
        // ---------------------------------------------------------

        yield return new WaitForSeconds(0.8f); // 공격 애니메이션 후딜레이
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;

        // Enemybase.dead()에서 special.RemoveEffect()를 호출하므로 
        // 죽을 때 자동으로 블록이 다시 나타납니다.
        base.dead();

        if (anim != null) anim.SetTrigger("Dead");
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        if (es != null)
        {
            es.isSpawning = false;
            es.spawn();
        }
        Destroy(gameObject);
    }
}