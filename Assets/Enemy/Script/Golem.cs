using System.Collections;
using UnityEngine;

public class Golem : Enemybase
{
    private Animator anim;
    public float shakeDuration = 1.5f; // 흔들릴 시간
    public float shakeMagnitude = 2.0f; // 흔들릴 각도/강도

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override void Attack()
    {
        if (isattack) return; // 중복 공격 방지
        isattack = true;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 화면 흔들기 코루틴 시작
        StartCoroutine(ShakeCameraRoutine());

        // 부모의 공격 로직(데미지 등) 호출
        base.Attack();
    }

    IEnumerator ShakeCameraRoutine()
    {
        Quaternion originalRotation = Camera.main.transform.rotation;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // 지그재그로 회전값 조절
            float zRotation = Mathf.Sin(Time.time * 20f) * shakeMagnitude;
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, zRotation);

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 흔들기 종료 후 카메라 원래대로 복구
        Camera.main.transform.rotation = originalRotation;
        isattack = false;
    }

    public override void dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("dead");

        StopAllCoroutines(); // 죽을 때 흔들기 중단

        // 카메라 복구 (흔들리는 도중 죽었을 때 대비)
        Camera.main.transform.rotation = Quaternion.identity;

        StartCoroutine(WaitDeadAnimation());
    }

    IEnumerator WaitDeadAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        base.dead();
        if (es != null)
        {
            es.isSpawning = false;
            es.spawn();
        }
        Destroy(gameObject);
    }
}