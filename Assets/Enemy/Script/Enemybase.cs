using UnityEngine;
using UnityEngine.UI;
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

    public int damage;
    public int ex;
    public GameObject hpbar;
    public int baseHp = 40;     // 유니티 인스펙터에서 수정할 원본 체력
    public int baseDamage = 3;  // 유니티 인스펙터에서 수정할 원본 데미지
    protected bool isDead = false; // 죽음 상태 추가
    protected virtual void Start()
    {
        st = Stat.instance;
        es = FindAnyObjectByType<EnemySpawn>();
        bc = FindAnyObjectByType<blockclear>();
        // 인스펙터에 입력한 원본 값에 난이도를 딱 한 번만 곱해서 할당합니다.
        maxhp = baseHp * st.difficult;
        hp = maxhp;
        damage = baseDamage; // 데미지는 아래 Stat에서 곱해주므로 여기선 원본만 전달

        UpdateTargetFrame();
        hpcal();
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
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " 사망. 새 적 스폰 요청");

        if (bc != null)
        {
            bc.currentScore += 100;
            bc.UpdateScoreUI();
        }

        // 스폰 요청
        if (es != null) es.spawn();

        st.GainExperience(ex);
        st.hp += 5;
        st.hpcal();

        // 골블린의 경우 Coroutine에서 Destroy하므로 여기선 일단 비활성화만 하거나
        // 콜라이더를 꺼서 중복 히트를 방지해야 합니다.
        GetComponent<Collider2D>().enabled = false;

        if (gameObject.name.Contains("Golbin")) return;
        Destroy(gameObject);
    }

    public void hit(int damage)
    {
        if (isDead) return;

        float finalDamage = damage * (1.0f + (st.atk * 0.1f));
        hp -= (int)finalDamage;
        Debug.Log(gameObject.name + "가 데미지를 입음. 남은 HP: " + hp); // 로그 추가

        hpcal();

        if (hp <= 0)
        {
            dead();
        }
    }
    public void hpcal()
    {
        if (hpbar == null)
        {
            GameObject found = GameObject.Find("enemyHPBar");

            if (found != null)
            {
                hpbar = found;
            }
            else
            {
                Debug.LogError("오류: 하이어라키에 'enemyHPBar' 이름의 오브젝트가 없거나 비활성화 상태입니다!");
                return; 
            }
        }
        Debug.Log("데미지 계산 진입 성공 - 현재 HP: " + hp);

        float hpRatio = (float)hp / maxhp;
        Image img = hpbar.GetComponent<Image>();

        if (img != null)
        {
            img.fillAmount = hpRatio;
        }
        else
        {
            Debug.LogError("오류: enemyHPBar 오브젝트에 Image 컴포넌트가 없습니다!");
        }
    }
}