using System.Collections;
using UnityEngine;

public class Golem : Enemybase, ISpecialAttack
{
    private Animator anim;
    private Sound sd;

    [Header("Shake Settings")]
    public float shakeDuration = 1.5f; // 흔들릴 시간
    public float shakeMagnitude = 2.0f; // 흔들릴 강도

    private Coroutine shakeCoroutine;
    private Quaternion originalRotation;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        sd = Sound.instance;
        originalRotation = Camera.main.transform.rotation;
    }

    // ISpecialAttack 인터페이스 구현
    public void ApplyEffect()
    {
        // 이미 흔들리고 있다면 멈추고 새로 시작
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    public void RemoveEffect()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        Camera.main.transform.rotation = originalRotation;
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // 지그재그 회전 (Sin파 활용)
            float zRotation = Mathf.Sin(Time.time * 20f) * shakeMagnitude;
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, zRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.rotation = originalRotation;
        shakeCoroutine = null;
    }

    public override void Attack()
    {
        // 1. 중복 공격 방지
        if (isattack) return;

        // 2. 부모 공격 로직 (isattack = true 등)
        base.Attack();

        // 3. 애니메이션 및 사운드 (골렘 사운드가 있다면 sd.golem 등으로 교체)
        if (anim != null) anim.SetTrigger("Attack");
        // if (sd != null && sd.golem != null) sd.golem.Play(); 

        // 4. 특수 공격 시퀀스 시작
        StartCoroutine(SpecialAttackSequence());
    }

    IEnumerator SpecialAttackSequence()
    {
        // 화면 흔들기 효과 적용
        ApplyEffect();

        // 흔들기가 끝날 때까지 대기 (혹은 특정 시간 대기)
        yield return new WaitForSeconds(shakeDuration);

        // 공격 후 후딜레이
        yield return new WaitForSeconds(0.8f);

        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;

        // 공격 및 흔들기 즉시 중단
        StopAllCoroutines();
        RemoveEffect();

        if (anim != null) anim.SetTrigger("dead");

        base.dead();
        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        if (es != null)
        {
            es.isSpawning = false;
            es.spawn();
        }
        Destroy(gameObject);
    }
}