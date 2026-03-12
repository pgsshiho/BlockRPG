using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemybase : MonoBehaviour,TakeDamage
{
    public float frame = 0f;
    public bool isattack = false;
    public float baseTargetFrame = 500f;
    public float targetFrame;
    public int maxhp = 40;
    public int hp = 40;
    public int damage;
    public int ex;
    public GameObject hpbar;
    public int baseHp = 40;
    public int baseDamage = 3;

    protected blockclear bc;
    protected EnemySpawn es;
    protected Stat st;
    protected bool isDead = false;

    // 외부에서 죽음 상태를 확인하기 위한 프로퍼티
    public bool IsDead => isDead;

    protected virtual void Start()
    {
        st = Stat.instance;
        es = FindAnyObjectByType<EnemySpawn>();
        bc = FindAnyObjectByType<blockclear>();
        maxhp = baseHp * st.difficult;
        hp = maxhp;
        damage = baseDamage;
        UpdateTargetFrame();
        hpcal();
    }

    void Update()
    {
        UpdateTargetFrame();
        if (frame >= targetFrame && !isattack && !isDead) Attack();
    }

    private void FixedUpdate()
    {
        if (!isattack && !isDead) frame++;
    }

    void UpdateTargetFrame()
    {
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;
        float calculatedFrame = baseTargetFrame + (3 - d) * 25f;
        targetFrame = Mathf.Max(100f, calculatedFrame);
    }

    public virtual void Attack()
    {
        isattack = true;
        frame = 0f;

        // Stat.instance(플레이어)가 인터페이스를 가지고 있는지 확인
        TakeDamage player = Stat.instance.GetComponent<TakeDamage>();

        if (player != null)
        {
            // 변수명을 player로 일치시킴
            player.TakeDamage(damage, gameObject.name);
        }
    }
    public void TakeDamage(int amount, string attackerName)
    {
        hit(amount); // 기존에 만들어둔 hit 로직 재사용 (아주 좋습니다!)
    }
    public virtual void dead()
    {
        if (isDead) return;
        isDead = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (bc != null) { bc.currentScore += 100; bc.UpdateScoreUI(); }
        st.GainExperience(ex);
        st.hp += 5;
        Debug.Log("피를채웁니다");
        st.hpcal();
    }

    public void hit(int damageAmount)
    {
        if (isDead) return;

        float finalDamage = damageAmount * (1.0f + (st.atk * 0.1f));
        hp -= (int)finalDamage;

        if (hp <= 0)
        {
            hp = 0; // UI를 위해 0으로 고정
            hpcal();
            dead();
        }
        else
        {
            hpcal();
        }
    }

    public void hpcal()
    {
        if (hpbar == null) hpbar = GameObject.Find("enemyHPBar");
        if (hpbar == null) return;

        float hpRatio = (float)hp / maxhp;
        Image img = hpbar.GetComponent<Image>();
        if (img != null) img.fillAmount = hpRatio;
    }
}