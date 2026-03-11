using System.Collections;
using UnityEngine;

public class Siren : Enemybase
{
    private Animator anim;
    private KeyBinding key;
    private bool isConfusionActive = false; // 중복 공격 방지용

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();

        // KeyBinding은 보통 하나만 존재하므로 Find 방식을 권장합니다.
        key = Object.FindAnyObjectByType<KeyBinding>();
    }

    public override void Attack()
    {
        // 이미 혼란 상태이거나 공격 중이 아닐 때는 실행하지 않음
        if (!isattack || isConfusionActive) return;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        StartCoroutine(ConfusionRoutine());
        base.Attack();
    }

    IEnumerator ConfusionRoutine()
    {
        isConfusionActive = true;
        isattack = false; // 공격 쿨타임 시작

        // [키 바꾸기]
        SwapKeys();
        Debug.Log("Siren Attack: Keys Swapped!");

        // 4초 대기
        yield return new WaitForSeconds(4.0f);

        // [키 복구]
        SwapKeys();
        Debug.Log("Siren Attack: Keys Restored!");

        isConfusionActive = false;
    }

    private void SwapKeys()
    {
        if (key == null) return;

        KeyCode temp = key.hardDrop;
        key.hardDrop = key.down;
        key.down = temp;
    }

    // --- [죽음 파트] ---
    public override void dead()
    {
        // 만약 죽을 때 키가 바뀐 상태라면 강제로 복구해주는 것이 매너!
        if (isConfusionActive)
        {
            SwapKeys();
            StopAllCoroutines();
        }
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("dead");
        }

        // 추가 공격 방지
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