using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NyarlathotepBoss : Enemybase
{
    private Animator anim;
    private Sound sd;
    private KeyBinding key;
    private Hold hold;
    private Conebase cb;

    // 패턴 관리를 위한 리스트
    private List<Action> patterns = new List<Action>();
    private bool isPatternActive = false;

    // 샤먼 패턴용 프리팹 (인스펙터에서 할당)
    public GameObject specialBlock;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = Sound.instance;
        key = FindAnyObjectByType<KeyBinding>();
        hold = FindAnyObjectByType<Hold>();
        cb = FindAnyObjectByType<Conebase>();

        // --- 8가지 패턴 등록 ---
        patterns.Add(Pattern_Siren);      // 1. 키 교체
        patterns.Add(Pattern_Slime);      // 2. 일반 강타
        patterns.Add(Pattern_Shaman);     // 3. 블록 변환
        patterns.Add(Pattern_Dragon);     // 4. 게임 모드 체인지
        patterns.Add(Pattern_Ouger);      // 5. 화면 반전
        patterns.Add(Pattern_Golem);      // 6. 화면 흔들기
        patterns.Add(Pattern_Goblin);     // 7. 홀드 삭제
        patterns.Add(Pattern_Ghost);      // 8. 투명 블록
    }

    public override void Attack()
    {
        if (isPatternActive || isDead) return;

        // 부모의 기본 데미지 처리 호출
        base.Attack();

        if (anim != null) anim.SetTrigger("Attack");

        // 랜덤하게 하나의 패턴 실행
        int randomIndex = UnityEngine.Random.Range(0, patterns.Count);
        patterns[randomIndex].Invoke();
    }

    #region [패턴 로직들]

    // 1. 사이렌 (키 교체)
    void Pattern_Siren() => StartCoroutine(SirenRoutine());
    IEnumerator SirenRoutine()
    {
        isPatternActive = true;
        SwapKeys(); // 키 꼬기
        yield return new WaitForSeconds(6.0f);
        SwapKeys(); // 복구
        FinishPattern();
    }

    // 2. 슬라임 (강타 및 사운드)
    void Pattern_Slime()
    {
        if (sd != null && sd.slimehit != null) sd.slimehit.Play();
        Invoke("FinishPattern", 1.0f);
    }

    // 3. 샤먼 (조종 블록 강제 변환)
    void Pattern_Shaman()
    {
        BlockBase target = GetActiveBlock();
        if (target != null && specialBlock != null)
        {
            Destroy(target.gameObject);
            GameObject newObj = Instantiate(specialBlock, cb.spawnpoint.transform.position, Quaternion.identity);
            cb.seeclone(specialBlock, newObj);
        }
        Invoke("FinishPattern", 1.0f);
    }

    // 4. 드래곤 (게임 환경 변경)
    void Pattern_Dragon() => StartCoroutine(DragonRoutine());
    IEnumerator DragonRoutine()
    {
        isPatternActive = true;
        GameManager.Instance?.change();
        yield return new WaitForSeconds(6.0f);
        GameManager.Instance?.dechange();
        FinishPattern();
    }

    // 5. 오우거 (화면 반전)
    void Pattern_Ouger() => StartCoroutine(OugerRoutine());
    IEnumerator OugerRoutine()
    {
        isPatternActive = true;
        GameManager.Instance?.SetMirrorMode(true);
        yield return new WaitForSeconds(6.0f);
        GameManager.Instance?.SetMirrorMode(false);
        FinishPattern();
    }

    // 6. 골렘 (화면 흔들기)
    void Pattern_Golem() => StartCoroutine(GolemRoutine());
    IEnumerator GolemRoutine()
    {
        isPatternActive = true;
        float elapsed = 0f;
        Quaternion originalRot = Camera.main.transform.rotation;
        while (elapsed < 1.5f)
        {
            float z = Mathf.Sin(Time.time * 20f) * 2.0f;
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.rotation = originalRot;
        FinishPattern();
    }

    // 7. 고블린 (홀드 아이템 파괴)
    void Pattern_Goblin()
    {
        if (hold != null)
        {
            if (hold.currentHoldVisual != null) Destroy(hold.currentHoldVisual);
            hold.hold = null;
            hold.ishave = false;
        }
        Invoke("FinishPattern", 0.8f);
    }

    // 8. 고스트 (블록 투명화)
    void Pattern_Ghost() => StartCoroutine(GhostRoutine());
    IEnumerator GhostRoutine()
    {
        isPatternActive = true;
        BlockBase target = GetActiveBlock();
        if (target != null)
        {
            // 바닥에 닿을 때까지 체크하는 로직 (기존 Ghost 코드 참조)
            while (target != null && target.GetComponent<Rigidbody2D>().simulated)
            {
                foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
                if (target.ghost != null)
                    foreach (var gsr in target.ghost.GetComponentsInChildren<SpriteRenderer>()) gsr.enabled = false;
                yield return new WaitForSeconds(0.1f);
            }
        }
        FinishPattern();
    }

    #endregion

    // 유틸리티 함수들
    private void FinishPattern() { isPatternActive = false; isattack = false; }

    private void SwapKeys()
    {
        if (key == null) return;
        KeyCode temp = key.hardDrop;
        key.hardDrop = key.down; key.down = temp;
    }

    private BlockBase GetActiveBlock()
    {
        foreach (var b in BlockBase.AllBlocks)
        {
            if (b.enabled && b.GetComponent<Rigidbody2D>().simulated && !b.CompareTag("Block") && b.transform.parent == null)
                return b;
        }
        return null;
    }

    public override void dead()
    {
        if (isDead) return;
        StopAllCoroutines();
        // 모든 효과 강제 해제
        GameManager.Instance?.dechange();
        GameManager.Instance?.SetMirrorMode(false);
        if (anim != null) anim.SetTrigger("dead");
        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.5f); // 죽는 애니메이션 대기
        if (es != null) { es.isSpawning = false; es.spawn(); }
        Destroy(gameObject);
    }
}