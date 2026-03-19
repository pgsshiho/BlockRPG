using UnityEngine;
using UnityEngine.UI;

public class Enemybase : MonoBehaviour, TakeDamage
{
    [Header("Status")]
    public float frame = 0f;
    public bool isattack = false;
    public float baseTargetFrame = 500f;
    public float targetFrame;
    public int maxhp = 40;
    public int hp = 40;
    public int damage;
    public int ex;

    [Header("Settings")]
    public GameObject hpbar;
    public int baseHp = 40;
    public int baseDamage = 3;

    protected blockclear bc;

    // [기존/신규 스폰 시스템 모두 대응]
    protected EnemySpawn es;
    protected CustomCloneBase newEs;

    protected Stat st;
    protected bool isDead = false;

    public bool IsDead => isDead;

    protected virtual void Start()
    {
        st = Stat.instance;
        bc = FindAnyObjectByType<blockclear>();

        // 두 종류의 스폰 매니저를 모두 찾아봅니다.
        es = FindAnyObjectByType<EnemySpawn>();
        newEs = FindAnyObjectByType<CustomCloneBase>();

        if (hpbar == null) hpbar = GameObject.Find("enemyHPBar");

        maxhp = baseHp * (st != null ? st.difficult : 1);
        hp = maxhp;
        damage = baseDamage;
        UpdateTargetFrame();
        hpcal();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isON) return;
        if (isDead) return;

        UpdateTargetFrame();
        if (frame >= targetFrame && !isattack) Attack();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isON) return;
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

        TakeDamage player = Stat.instance.GetComponent<TakeDamage>();
        if (player != null)
        {
            player.TakeDamage(damage, gameObject.name);
        }
    }

    public void TakeDamage(int amount, string attackerName)
    {
        hit(amount);
    }

    public virtual void dead()
    {
        if (isDead) return;
        isDead = true;

        // 물리 충돌 끄기 (시체에 블록이 닿지 않게)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 점수 및 경험치 처리
        if (bc != null) { bc.currentScore += 100; bc.UpdateScoreUI(); }
        if (st != null)
        {
            st.GainExperience(ex);
            st.hp += 5;
            if (st.hp >= st.maxhp) {
                st.hp = st.maxhp;
            }
            st.hpcal();
        }

    }

    public void hit(int damageAmount)
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isON)) return;

        float multiplier = (st != null) ? (1.0f + (st.atk * 0.1f)) : 1f;
        float finalDamage = damageAmount * multiplier;
        hp -= (int)finalDamage;

        if (hp <= 0)
        {
            hp = 0;
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
        if (hpbar == null) return;
        float hpRatio = (float)hp / maxhp;
        Image img = hpbar.GetComponent<Image>();
        if (img != null) img.fillAmount = hpRatio;
    }
}