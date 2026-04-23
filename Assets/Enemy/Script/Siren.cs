using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Siren : Enemybase
{
    private Animator anim;
    private KeyBinding key;
    private bool isConfusionActive = false; // 중복 공격 및 키 꼬임 방지용
    private Sound sd;
    public FindEnemy enemyData;
    protected override void Start()
    {
        base.Start(); // 부모(Enemybase)의 Start 실행 (HP설정, 스폰매니저 찾기 등)
        anim = GetComponent<Animator>();

        // 키 설정을 바꾸기 위해 KeyBinding 스크립트를 찾습니다.
        key = Object.FindAnyObjectByType<KeyBinding>();
        sd = FindAnyObjectByType<Sound>();
        if (enemyData != null){ enemyData.knight_night = true;
            enemyData.Save();
        }
    }

    public override void Attack()
    {
        if (isConfusionActive) return;

        base.Attack();

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // --- 사운드 재생 부분 수정 ---
        if (sd != null && sd.siren != null)
        {
            StopCoroutine("PlaySirenShort");
            StartCoroutine(PlaySirenShort(3.0f)); // 3초만 재생하는 코루틴 호출
        }

        StartCoroutine(ConfusionRoutine());
    }

    // [추가] 지정된 시간만큼만 사운드를 재생하는 코루틴
    IEnumerator PlaySirenShort(float duration)
    {
        sd.night_knight.Play();

        yield return new WaitForSeconds(duration); // 3초 대기

        sd.siren.Stop(); // 사운드 정지
    }

    IEnumerator ConfusionRoutine()
    {
        isConfusionActive = true;

        // [키 바꾸기] 하드드롭과 아래 방향키를 교체
        SwapKeys();

        // 4초 동안 유지
        yield return new WaitForSeconds(8.0f);

        // [키 복구] 다시 한번 교체해서 원래대로 돌림
        if (isConfusionActive) // 죽어서 이미 복구된 게 아닐 때만 실행
        {
            SwapKeys();
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

    public override void dead()
    {
        if (isDead) return;
        isDead = true; // 중복 실행 방지 필수!

        // 1. 키 복구 (사이렌 전용 로직)
        if (isConfusionActive)
        {
            SwapKeys();
            isConfusionActive = false;
        }

        // 2. 애니메이션
        if (anim != null) anim.SetTrigger("dead");

        // 3. 부모 로직 실행 (점수 등)
        base.dead();

        // 4. 스폰 관리는 코루틴에게 맡김
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        base.enemyspawn();

        Destroy(gameObject);
    }
    void OnEnable()
    {
        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 이벤트 해제 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SwapKeys();
        isConfusionActive = false;
    }
}