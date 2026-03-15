using Unity.VisualScripting;
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
    protected EnemySpawn es;
    protected Stat st;
    protected bool isDead = false;

    public bool IsDead => isDead;

    protected virtual void Start()
    {
        st = Stat.instance;
        es = FindAnyObjectByType<EnemySpawn>();
        bc = FindAnyObjectByType<blockclear>();

        // HP바 캐싱 (매번 Update에서 찾지 않도록 Start로 이동)
        if (hpbar == null) hpbar = GameObject.Find("enemyHPBar");

        maxhp = baseHp * st.difficult;
        hp = maxhp;
        damage = baseDamage;
        UpdateTargetFrame();
        hpcal();
    }

    void Update()
    {
        // 1. 게임이 멈춰있거나 죽었다면 로직 중단
        if (GameManager.Instance != null && GameManager.Instance.isON) return;
        if (isDead) return;

        UpdateTargetFrame();
        if (frame >= targetFrame && !isattack) Attack();
    }

    private void FixedUpdate()
    {
        // 2. 물리/프레임 카운트 중단
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

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (bc != null) { bc.currentScore += 100; bc.UpdateScoreUI(); }

        if (st != null)
        {
            st.GainExperience(ex);
            st.hp += 5;
            st.hpcal();
        }
    }

    public void hit(int damageAmount)
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isON)) return;

        float finalDamage = damageAmount * (1.0f + (st.atk * 0.1f));
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