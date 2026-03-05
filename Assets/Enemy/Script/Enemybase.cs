using UnityEngine;

public class Enemybase : MonoBehaviour
{
    public float frame = 0f;
    public bool isattack = false;

    public float baseTargetFrame = 500f;
    public float targetFrame;
    public int maxhp = 40;
    public int hp = 40;

    protected blockclear bc; // 자식에서도 접근 가능하게 protected
    protected EnemySpawn es;
    protected Stat st;

    public int damage = 3;
    public int ex;

    protected virtual void Start()
    {
        bc = FindAnyObjectByType<blockclear>();
        es = FindAnyObjectByType<EnemySpawn>();
        st = FindAnyObjectByType<Stat>();
        UpdateTargetFrame();
        hp = hp * st.difficult;
        maxhp = maxhp * st.difficult;
    }

    void Update()
    {
        
        UpdateTargetFrame();

        if (frame >= targetFrame && !isattack)
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        if (!isattack)
        {
            frame++;
        }
    }

    void UpdateTargetFrame()
    {
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;
        float calculatedFrame = baseTargetFrame + (3 - d) * 25f;
        targetFrame = Mathf.Max(100f, calculatedFrame);
    }

    // 자식(slime)에서 덮어쓸 수 있도록 virtual 선언
    public virtual void Attack()
    {
        isattack = true; // 공격 시작! (이제 FixedUpdate에서 frame이 멈춤)
        frame = 0f;      // 프레임 초기화

        if (st != null)
        {
            st.damage(damage, gameObject.name);
        }

    }

    public virtual void dead()
    {
        if (bc != null)
        {
            bc.currentScore += 100;
            bc.UpdateScoreUI();
        }
        if (es != null) es.spawn();
        st.GainExperience(ex);
        st.hp += 5;
        st.hpcal();
        Destroy(gameObject);
        
    }

    public void hit(int damage)
    {
        hp -= damage;
        if (hp <= 0) dead();
    }
}