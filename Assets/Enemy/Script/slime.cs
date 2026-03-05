using System.Collections; // 코루틴을 위해 필요
using UnityEngine;

public class slime : Enemybase
{
    private Animator anim;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override void Attack()
    {
        base.Attack();

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        StartCoroutine(WaitAnimation());
    }

    IEnumerator WaitAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        isattack = false;
    }
}