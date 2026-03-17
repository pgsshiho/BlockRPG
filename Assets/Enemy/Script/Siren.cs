using System.Collections;
using UnityEngine;

public class Siren : Enemybase
{
    private Animator anim;
    private KeyBinding key;
    private bool isConfusionActive = false; // 중복 공격 및 키 꼬임 방지용

    protected override void Start()
    {
        base.Start(); // 부모(Enemybase)의 Start 실행 (HP설정, 스폰매니저 찾기 등)
        anim = GetComponent<Animator>();

        // 키 설정을 바꾸기 위해 KeyBinding 스크립트를 찾습니다.
        key = Object.FindAnyObjectByType<KeyBinding>();
    }

    public override void Attack()
    {
        // 이미 혼란을 걸고 있다면 중복 공격 방지
        if (isConfusionActive) return;

        base.Attack(); // 부모의 Attack 실행 (플레이어에게 데미지 전달)

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 세이렌만의 특수 능력: 키 혼란 코루틴 시작
        StartCoroutine(ConfusionRoutine());
    }

    IEnumerator ConfusionRoutine()
    {
        isConfusionActive = true;

        // [키 바꾸기] 하드드롭과 아래 방향키를 교체
        SwapKeys();
        Debug.Log("Siren: Keys Swapped!");

        // 4초 동안 유지
        yield return new WaitForSeconds(8.0f);

        // [키 복구] 다시 한번 교체해서 원래대로 돌림
        if (isConfusionActive) // 죽어서 이미 복구된 게 아닐 때만 실행
        {
            SwapKeys();
            Debug.Log("Siren: Keys Restored!");
            isConfusionActive = false;
        }

        isattack = false; // 공격 대기 해제
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
        if (isDead) return;

        // 1. 만약 키가 바뀐 상태에서 죽는다면 즉시 복구
        if (isConfusionActive)
        {
            SwapKeys();
            isConfusionActive = false;
        }

        // 2. 애니메이션 실행
        if (anim != null)
        {
            anim.SetTrigger("dead");
        }

        // 3. 부모의 dead()를 호출하여 점수/경험치/스폰 신호를 즉시 처리
        // base.dead() 안에서 newEs.spawn() 또는 es.spawn()이 호출됩니다.
        base.dead();

        // 4. 애니메이션이 끝날 시간을 벌어준 뒤 오브젝트 파괴
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        // 콜라이더를 꺼서 죽은 애니메이션 중에 다시 맞지 않게 함
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}