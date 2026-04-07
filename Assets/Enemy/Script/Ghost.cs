using System.Collections;
using UnityEngine;

public class Ghost : Enemybase, ISpecialAttack
{
    private Animator anim;
    private Sound sd;
    private Conebase cb;

    // 현재 공격 대상이 된 블록과 그 고스트를 기억합니다.
    private BlockBase targetBB;
    private GameObject targetGhost;
    public FindEnemy enemyData;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = Sound.instance;
        cb = FindAnyObjectByType<Conebase>();
        if (enemyData != null) enemyData.ghost = true;
    }

    public void ApplyEffect()
    {
        // 1. 조종 중인 블록을 정밀하게 찾습니다.
        targetBB = GetActiveControlBlock();

        if (targetBB != null)
        {
            // 2. BlockBase 코드에 이미 연결된 ghost 오브젝트를 가져옵니다.
            targetGhost = targetBB.ghost;

            // 3. 해당 블록이 바닥에 닿을 때까지 숨기는 코루틴 시작
            StartCoroutine(KeepHidingUntilSettle(targetBB, targetGhost));
        }
    }

    // [중요] 세 가지 블록을 구분하여 '조종 중인' 것만 반환하는 함수
    private BlockBase GetActiveControlBlock()
    {
        BlockBase[] allBlocks = FindObjectsByType<BlockBase>(FindObjectsSortMode.None);
        foreach (var b in allBlocks)
        {
            if (b.enabled && b.GetComponent<Rigidbody2D>().simulated && !b.CompareTag("Block"))
            {
                // 추가 필터링: 부모가 없는 최상위 객체 (Next/Hold 시각화 블록 제외)
                if (b.transform.parent == null)
                {
                    return b;
                }
            }
        }
        return null;
    }

    // 블록이 바닥에 닿아 고정될 때까지 독립적으로 투명화를 유지합니다.
    IEnumerator KeepHidingUntilSettle(BlockBase bb, GameObject ghost)
    {
        // bb가 파괴되지 않았고, 여전히 조종 중(simulated)일 때만 반복
        while (bb != null && bb.enabled && bb.GetComponent<Rigidbody2D>().simulated)
        {
            // 본체 숨기기
            foreach (var sr in bb.GetComponentsInChildren<SpriteRenderer>())
                sr.enabled = false;

            // 가이드라인 숨기기
            if (ghost != null)
            {
                foreach (var gsr in ghost.GetComponentsInChildren<SpriteRenderer>())
                    gsr.enabled = false;
            }

            yield return new WaitForSeconds(0.1f); // 0.1초마다 상태 갱신
        }

        // 블록이 설치되었거나(bb.enabled = false) 파괴되었다면 투명화 로직 종료
        // (설치된 블록은 어차피 새로 생성된 자식들로 구성되므로 여기서 끝내면 됩니다)
    }

    public void RemoveEffect()
    {
        // Independent 루프가 스스로 종료되므로 특별한 처리가 필요 없으나,
        // 필요 시 현재 타겟들을 강제로 보이게 할 수 있습니다.
    }

    public override void Attack()
    {
        if (isattack) return;
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");
        if (sd != null && sd.Goblin != null) sd.Ghost.Play();
        StartCoroutine(GhostAttackSequence());
    }

    IEnumerator GhostAttackSequence()
    {
        ApplyEffect();
        yield return new WaitForSeconds(0.8f);
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;

        // 고스트가 죽어도 이미 투명해진 블록은 끝까지 안 보이게 하고 싶다면 
        // StopAllCoroutines()를 호출하지 마세요. 
        // 만약 죽었을 때 블록이 다시 보여야 한다면 아래 줄의 주석을 푸세요.
        // StopAllCoroutines(); 

        if (anim != null) anim.SetTrigger("Dead");

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