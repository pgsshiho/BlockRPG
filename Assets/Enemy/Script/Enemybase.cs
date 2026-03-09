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

    public int damage = 3;
    public int ex;
    public GameObject hpbar;
    protected virtual void Start()
    {
        bc = FindAnyObjectByType<blockclear>();
        es = FindAnyObjectByType<EnemySpawn>();
        st = FindAnyObjectByType<Stat>();
        UpdateTargetFrame();
        hp = hp * st.difficult;
        maxhp = maxhp * st.difficult;
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
        hpcal();
    }

    public void hit(int damage)
    {
        hp -= damage;
        Debug.Log("공격성공");
        hpcal();
        if (hp <= 0) dead();
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